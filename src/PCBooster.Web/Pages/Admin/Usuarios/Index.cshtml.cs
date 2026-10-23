using Microsoft.AspNetCore.Mvc;
using PCBooster.Dominio;
using PCBooster.Negocio;
using PCBooster.Web.Infraestructura;

namespace PCBooster.Web.Pages.Admin.Usuarios;

public class IndexModel(UsuariosService usuarios) : PaginaBase
{
    const int PorPagina = 15;

    [BindProperty(SupportsGet = true)] public int? Id { get; set; }

    /* Búsqueda por nombre o e-mail. */
    [BindProperty(SupportsGet = true)] public string? Q { get; set; }
    [BindProperty(SupportsGet = true)] public int Pagina { get; set; } = 1;

    public List<Usuario> Usuarios { get; private set; } = new();
    public Paginacion Paginacion { get; private set; } = null!;

    public async Task OnGetAsync()
    {
        List<Usuario> todos = await usuarios.ListarAsync(Id);
        if (!string.IsNullOrWhiteSpace(Q))
        {
            string q = Q.Trim();
            todos = todos.Where(u => u.Nombre.Contains(q, StringComparison.OrdinalIgnoreCase) || u.Email.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        int paginas = Math.Max(1, (int)Math.Ceiling(todos.Count / (double)PorPagina));
        Pagina = Math.Clamp(Pagina, 1, paginas);
        Usuarios = todos.Skip((Pagina - 1) * PorPagina).Take(PorPagina).ToList();
        var ruta = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(Q)) ruta["q"] = Q;
        Paginacion = new Paginacion(Pagina, paginas, "/Admin/Usuarios/Index", "pagina", ruta);
    }

    public async Task<IActionResult> OnPostDesactivarAsync(int usuarioId)
    {
        if (usuarioId == UsuarioId)
        {
            Avisar("No podés desactivar tu propia cuenta.", TipoMensaje.Aviso);
        }
        else if (await usuarios.DesactivarAsync(usuarioId))
        {
            Avisar("Usuario desactivado.", TipoMensaje.Exito);
        }
        return RedirectToPage(new { q = Q, pagina = Pagina });
    }
}
