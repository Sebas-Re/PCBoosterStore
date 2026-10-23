using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using PCBooster.Dominio;
using PCBooster.Negocio;
using PCBooster.Web.Infraestructura;

namespace PCBooster.Web.Pages.Admin.Articulos;

/* Alta (sin código en la ruta) o modificación de un artículo, con su detalle para el catálogo. */
public class EditarModel(CatalogoService catalogo) : PaginaBase
{
    public class Formulario
    {
        [Required(ErrorMessage = "Ingresá un código."), StringLength(4, ErrorMessage = "Hasta 4 caracteres.")]
        public string Codigo { get; set; } = "";

        [Required(ErrorMessage = "Ingresá una descripción."), StringLength(300)]
        public string Descripcion { get; set; } = "";

        [Required(ErrorMessage = "Elegí una categoría.")]
        public int? CategoriaId { get; set; }

        [Required(ErrorMessage = "Ingresá el stock."), Range(0, 1_000_000, ErrorMessage = "El stock no puede ser negativo.")]
        public int? Stock { get; set; }

        /* Texto: se acepta "1500,50" o "1500.50" sin depender de la cultura. */
        [Required(ErrorMessage = "Ingresá el precio.")]
        public string Precio { get; set; } = "";

        [Required(ErrorMessage = "Ingresá la imagen."), StringLength(100)]
        public string Imagen { get; set; } = "";

        public bool Activo { get; set; } = true;

        [StringLength(50)] public string? Marca { get; set; }
        [StringLength(2000)] public string? DescripcionLarga { get; set; }
        [StringLength(4000)] public string? Especificaciones { get; set; }

        [Range(0, 120, ErrorMessage = "Entre 0 y 120 meses.")]
        public int? GarantiaMeses { get; set; }
    }

    /* Vacío = artículo nuevo. */
    [BindProperty(SupportsGet = true)]
    public string? Codigo { get; set; }

    [BindProperty]
    public Formulario Datos { get; set; } = new();

    public List<Categoria> Categorias { get; private set; } = new();
    public bool EsNuevo => string.IsNullOrEmpty(Codigo);

    public async Task<IActionResult> OnGetAsync()
    {
        Categorias = await catalogo.CategoriasAsync();
        if (EsNuevo)
        {
            Datos.Imagen = "/img/productos/";
            return Page();
        }
        Articulo? articulo = await catalogo.ObtenerAsync(Codigo!);
        if (articulo == null)
        {
            return NotFound();
        }
        Datos = new Formulario
        {
            Codigo = articulo.Codigo,
            Descripcion = articulo.Descripcion,
            CategoriaId = articulo.CategoriaId,
            Stock = articulo.Stock,
            Precio = articulo.Precio.ToString(CultureInfo.InvariantCulture),
            Imagen = articulo.Imagen ?? "",
            Activo = articulo.Activo,
            Marca = articulo.Marca,
            DescripcionLarga = articulo.DescripcionLarga,
            Especificaciones = articulo.Especificaciones,
            GarantiaMeses = articulo.GarantiaMeses
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!EsNuevo)
        {
            Datos.Codigo = Codigo!; // el código no se modifica
            ModelState.Remove("Datos.Codigo");
        }
        if (!decimal.TryParse(Datos.Precio.Trim().Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal precio) || precio <= 0)
        {
            ModelState.AddModelError("Datos.Precio", "Ingresá un precio mayor a cero.");
        }
        if (ModelState.IsValid)
        {
            var articulo = new Articulo
            {
                Codigo = Datos.Codigo.Trim(),
                Descripcion = Datos.Descripcion.Trim(),
                CategoriaId = Datos.CategoriaId!.Value,
                Stock = Datos.Stock!.Value,
                Precio = precio,
                Imagen = Datos.Imagen.Trim(),
                Activo = Datos.Activo,
                Marca = Vacio(Datos.Marca),
                DescripcionLarga = Vacio(Datos.DescripcionLarga),
                // Se normalizan los saltos de línea del textarea ("\r\n" → "\n").
                Especificaciones = Vacio(Datos.Especificaciones)?.Replace("\r\n", "\n"),
                GarantiaMeses = Datos.GarantiaMeses
            };
            string? error = EsNuevo ? await catalogo.AgregarArticuloAsync(articulo) : await catalogo.ModificarArticuloAsync(articulo);
            if (error == null)
            {
                Avisar($"Artículo {articulo.Codigo} {(EsNuevo ? "agregado" : "modificado")}.", TipoMensaje.Exito);
                return RedirectToPage("Index", new { q = articulo.Codigo });
            }
            Avisar(error, TipoMensaje.Error);
        }
        Categorias = await catalogo.CategoriasAsync();
        return Page();
    }

    static string? Vacio(string? texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
}
