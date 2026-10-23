using Microsoft.AspNetCore.Mvc;
using PCBooster.Dominio;
using PCBooster.Negocio;
using PCBooster.Web.Infraestructura;

namespace PCBooster.Web.Pages.Admin.Articulos;

public class IndexModel(CatalogoService catalogo) : PaginaBase
{
    const int PorPagina = 15;

    /* Filtro por código o descripción (sin distinguir mayúsculas). */
    [BindProperty(SupportsGet = true)] public string? Q { get; set; }
    [BindProperty(SupportsGet = true)] public int Pagina { get; set; } = 1;

    public IReadOnlyList<Articulo> Articulos { get; private set; } = [];
    public Paginacion Paginacion { get; private set; } = null!;

    public async Task OnGetAsync()
    {
        Pagina<Articulo> resultado = await catalogo.ListarArticulosAsync(Q, Pagina, PorPagina);
        Articulos = resultado.Items;
        Pagina = resultado.Numero;
        Paginacion = Paginacion.De(resultado, "/Admin/Articulos/Index", "pagina", Paginacion.Parametros(("q", Q)));
    }

    public async Task<IActionResult> OnPostBajaAsync(string codigo)
    {
        if (await catalogo.DarDeBajaArticuloAsync(codigo))
        {
            Avisar($"Artículo {codigo} dado de baja.", TipoMensaje.Exito);
        }
        return RedirectToPage(new { q = Q, pagina = Pagina });
    }
}
