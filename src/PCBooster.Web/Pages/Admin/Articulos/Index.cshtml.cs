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

    public List<Articulo> Articulos { get; private set; } = new();
    public Paginacion Paginacion { get; private set; } = null!;

    public async Task OnGetAsync()
    {
        IEnumerable<Articulo> todos = await catalogo.ListarArticulosAsync();
        if (!string.IsNullOrWhiteSpace(Q))
        {
            string q = Q.Trim();
            todos = todos.Where(a => a.Codigo.Equals(q, StringComparison.OrdinalIgnoreCase) || a.Descripcion.Contains(q, StringComparison.OrdinalIgnoreCase));
        }
        List<Articulo> lista = todos.ToList();
        int paginas = Math.Max(1, (int)Math.Ceiling(lista.Count / (double)PorPagina));
        Pagina = Math.Clamp(Pagina, 1, paginas);
        Articulos = lista.Skip((Pagina - 1) * PorPagina).Take(PorPagina).ToList();
        var ruta = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(Q)) ruta["q"] = Q;
        Paginacion = new Paginacion(Pagina, paginas, "/Admin/Articulos/Index", "pagina", ruta);
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
