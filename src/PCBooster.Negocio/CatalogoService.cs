using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using PCBooster.Datos;
using PCBooster.Dominio;

namespace PCBooster.Negocio;

/* Artículos y categorías: lo que ve el cliente y la administración. */
public partial class CatalogoService(TiendaContext db)
{
    /* Código de 1 a 4 letras o números, como los de los datos de prueba. */
    public static bool CodigoValido(string? codigo) => codigo != null && RegexCodigo().IsMatch(codigo);

    /* La imagen se muestra en el catálogo: solo rutas del sitio ("/img/...") o
       direcciones http(s). Nada de "javascript:", "data:", etc. */
    public static bool ImagenValida(string? url)
    {
        if (string.IsNullOrWhiteSpace(url) || url.Length > 100)
        {
            return false;
        }
        if (url.StartsWith('/') && !url.StartsWith("//"))
        {
            return true;
        }
        return Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    public const string ErrorImagen = "La imagen tiene que ser una ruta del sitio (/img/productos/...) o una dirección http(s).";

    // ------------------------------------------------------------------------
    // Tienda
    // ------------------------------------------------------------------------

    /* Artículos activos de categorías activas. La búsqueda (por descripción, sin
       distinguir mayúsculas) tiene prioridad sobre la categoría. */
    public Task<Pagina<Articulo>> BuscarAsync(string? busqueda, int? categoriaId, int pagina, int tamañoPagina)
    {
        IQueryable<Articulo> consulta = db.Articulos.AsNoTracking().Include(a => a.Categoria)
            .Where(a => a.Activo && a.Categoria!.Activa);

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            string patron = Consultas.Contiene(busqueda);
            consulta = consulta.Where(a => EF.Functions.Like(a.Descripcion, patron, Consultas.Escape));
        }
        else if (categoriaId != null)
        {
            consulta = consulta.Where(a => a.CategoriaId == categoriaId);
        }
        return consulta.OrderBy(a => a.CategoriaId).ThenBy(a => a.Codigo).PaginarAsync(pagina, tamañoPagina);
    }

    public Task<Articulo?> ObtenerAsync(string codigo) =>
        db.Articulos.AsNoTracking().Include(a => a.Categoria).SingleOrDefaultAsync(a => a.Codigo == codigo);

    public Task<List<Categoria>> CategoriasActivasAsync() =>
        db.Categorias.AsNoTracking().Where(c => c.Activa).OrderBy(c => c.Descripcion).ToListAsync();

    // ------------------------------------------------------------------------
    // Administración de artículos
    // ------------------------------------------------------------------------
    /* Todos los artículos, incluidos los dados de baja. "filtro": el código exacto o parte de la descripción. */
    public Task<Pagina<Articulo>> ListarArticulosAsync(string? filtro, int pagina, int tamañoPagina)
    {
        IQueryable<Articulo> consulta = db.Articulos.AsNoTracking().Include(a => a.Categoria);
        if (!string.IsNullOrWhiteSpace(filtro))
        {
            string codigo = filtro.Trim(), patron = Consultas.Contiene(filtro);
            consulta = consulta.Where(a => a.Codigo == codigo || EF.Functions.Like(a.Descripcion, patron, Consultas.Escape));
        }
        return consulta.OrderBy(a => a.CategoriaId).ThenBy(a => a.Codigo).PaginarAsync(pagina, tamañoPagina);
    }

    public async Task<string?> AgregarArticuloAsync(Articulo articulo)
    {
        articulo.Codigo = articulo.Codigo.Trim();
        if (!CodigoValido(articulo.Codigo))
        {
            return "El código tiene que tener de 1 a 4 letras o números.";
        }
        if (await db.Articulos.AnyAsync(a => a.Codigo == articulo.Codigo))
        {
            return $"Ya existe un artículo con el código {articulo.Codigo}.";
        }
        string? error = await ValidarArticulo(articulo);
        if (error != null)
        {
            return error;
        }
        articulo.Activo = true;
        db.Articulos.Add(articulo);
        await db.SaveChangesAsync();
        return null;
    }

    /* Modifica los datos editables (no el código). Activo = true reactiva un artículo dado de baja. */
    public async Task<string?> ModificarArticuloAsync(Articulo cambios)
    {
        Articulo? articulo = await db.Articulos.SingleOrDefaultAsync(a => a.Codigo == cambios.Codigo);
        if (articulo == null)
        {
            return "El artículo no existe.";
        }
        string? error = await ValidarArticulo(cambios);
        if (error != null)
        {
            return error;
        }
        articulo.Descripcion = cambios.Descripcion.Trim();
        articulo.CategoriaId = cambios.CategoriaId;
        articulo.Stock = cambios.Stock;
        articulo.Precio = cambios.Precio;
        articulo.Imagen = cambios.Imagen?.Trim();
        articulo.Activo = cambios.Activo;
        articulo.Marca = cambios.Marca;
        articulo.DescripcionLarga = cambios.DescripcionLarga;
        articulo.Especificaciones = cambios.Especificaciones;
        articulo.GarantiaMeses = cambios.GarantiaMeses;
        await db.SaveChangesAsync();
        return null;
    }

    /* Baja lógica: deja de mostrarse y venderse, pero sigue en las ventas viejas. */
    public async Task<bool> DarDeBajaArticuloAsync(string codigo) =>
        await db.Articulos.Where(a => a.Codigo == codigo).ExecuteUpdateAsync(s => s.SetProperty(a => a.Activo, false)) == 1;

    async Task<string?> ValidarArticulo(Articulo articulo)
    {
        if (string.IsNullOrWhiteSpace(articulo.Descripcion))
        {
            return "Ingresá una descripción.";
        }
        if (articulo.Stock < 0 || articulo.Precio <= 0)
        {
            return "El stock no puede ser negativo y el precio tiene que ser mayor a cero.";
        }
        if (!ImagenValida(articulo.Imagen))
        {
            return ErrorImagen;
        }
        if (!await db.Categorias.AnyAsync(c => c.Id == articulo.CategoriaId))
        {
            return "Elegí una categoría.";
        }
        string descripcion = articulo.Descripcion.Trim();
        if (await db.Articulos.AnyAsync(a => a.Descripcion == descripcion && a.Codigo != articulo.Codigo))
        {
            return "Ya existe otro artículo con esa descripción.";
        }
        return null;
    }

    // ------------------------------------------------------------------------
    // Administración de categorías
    // ------------------------------------------------------------------------
    public Task<List<Categoria>> CategoriasAsync() =>
        db.Categorias.AsNoTracking().OrderBy(c => c.Id).ToListAsync();

    public async Task<string?> AgregarCategoriaAsync(string descripcion)
    {
        descripcion = descripcion.Trim();
        if (descripcion.Length == 0 || descripcion.Length > 30)
        {
            return "La descripción tiene que tener entre 1 y 30 caracteres.";
        }
        if (await db.Categorias.AnyAsync(c => c.Descripcion == descripcion))
        {
            return "Ya existe una categoría con ese nombre.";
        }
        db.Categorias.Add(new Categoria { Descripcion = descripcion, Activa = true });
        await db.SaveChangesAsync();
        return null;
    }

    public async Task<string?> ModificarCategoriaAsync(int id, string descripcion, bool activa)
    {
        descripcion = descripcion.Trim();
        Categoria? categoria = await db.Categorias.SingleOrDefaultAsync(c => c.Id == id);
        if (categoria == null)
        {
            return "La categoría no existe.";
        }
        if (descripcion.Length == 0 || descripcion.Length > 30)
        {
            return "La descripción tiene que tener entre 1 y 30 caracteres.";
        }
        if (await db.Categorias.AnyAsync(c => c.Descripcion == descripcion && c.Id != id))
        {
            return "Ya existe una categoría con ese nombre.";
        }
        categoria.Descripcion = descripcion;
        categoria.Activa = activa;
        await db.SaveChangesAsync();
        return null;
    }

    /* Baja lógica: oculta sus artículos del catálogo. */
    public async Task<bool> DesactivarCategoriaAsync(int id) =>
        await db.Categorias.Where(c => c.Id == id).ExecuteUpdateAsync(s => s.SetProperty(c => c.Activa, false)) == 1;

    [GeneratedRegex("^[A-Za-z0-9]{1,4}$")]
    private static partial Regex RegexCodigo();
}
