using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using PCBooster.Dominio;
using PCBooster.Negocio;
using PCBooster.Web.Infraestructura;

namespace PCBooster.Web.Pages;

public partial class CatalogoModel(CatalogoService catalogo, CarritoSesion carritoSesion) : PaginaBase
{
    const int ArticulosPorPagina = 12;

    /* Filtros y página por querystring: las URLs se pueden compartir y "Atrás" funciona. */
    [BindProperty(SupportsGet = true)] public int? Cat { get; set; }
    [BindProperty(SupportsGet = true)] public string? Q { get; set; }
    [BindProperty(SupportsGet = true)] public int Pagina { get; set; } = 1;

    public Pagina<Articulo> Articulos { get; private set; } = null!;
    public List<Categoria> Categorias { get; private set; } = new();
    public Carrito Carrito { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Q = Q?.Trim();
        Categorias = await catalogo.CategoriasActivasAsync();
        Articulos = await catalogo.BuscarAsync(Q, Cat, Pagina, ArticulosPorPagina);
        Carrito = carritoSesion.Obtener();
    }

    /* Agrega "cantidad" unidades (1 desde la tarjeta, la elegida desde el modal).
       Precio y stock salen de la base, no de lo que mostró la página. */
    public async Task<IActionResult> OnPostAgregarAsync(string codigo, int cantidad = 1)
    {
        cantidad = Math.Clamp(cantidad, 1, 99);
        Articulo? articulo = await catalogo.ObtenerAsync(codigo);
        if (articulo == null || !articulo.Activo || articulo.Categoria?.Activa != true)
        {
            Avisar("El producto ya no está disponible.", TipoMensaje.Error);
            return VolverAlCatalogo();
        }

        Carrito carrito = carritoSesion.Obtener();
        int agregadas = 0;
        while (agregadas < cantidad && carrito.Agregar(articulo.Codigo, articulo.Descripcion, articulo.Precio, articulo.Stock))
        {
            agregadas++;
        }
        carritoSesion.Guardar(carrito);

        if (agregadas == cantidad)
        {
            Avisar(cantidad == 1
                ? $"Agregaste \"{articulo.Descripcion}\" al carrito."
                : $"Agregaste {cantidad} unidades de \"{articulo.Descripcion}\" al carrito.", TipoMensaje.Exito);
        }
        else if (agregadas > 0)
        {
            Avisar($"Solo había stock para agregar {agregadas} unidades de \"{articulo.Descripcion}\".", TipoMensaje.Aviso);
        }
        else
        {
            Avisar($"No hay más stock de \"{articulo.Descripcion}\".", TipoMensaje.Aviso);
        }
        return VolverAlCatalogo();
    }

    /* Después de un POST se vuelve con un GET a la misma vista (sin reenvíos al recargar). */
    IActionResult VolverAlCatalogo() => RedirectToPage(new { cat = Cat, q = Q, pagina = Pagina });

    /* Id HTML a partir del código del artículo, solo con caracteres seguros. */
    public static string IdHtml(string prefijo, string codigo) => prefijo + "-" + CaracteresInseguros().Replace(codigo, "_");

    public static string TextoGarantia(int? meses)
    {
        int valor = meses ?? 12;
        return valor % 12 == 0 && valor >= 24 ? $"{valor / 12} años" : $"{valor} meses";
    }

    [GeneratedRegex("[^A-Za-z0-9_-]")]
    private static partial Regex CaracteresInseguros();
}
