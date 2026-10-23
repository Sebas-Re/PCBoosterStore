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

    public IReadOnlyList<Usuario> Usuarios { get; private set; } = [];
    public Paginacion Paginacion { get; private set; } = null!;

    public async Task OnGetAsync()
    {
        Pagina<Usuario> resultado = await usuarios.BuscarAsync(Id, Q, Pagina, PorPagina);
        Usuarios = resultado.Items;
        Pagina = resultado.Numero;
        Paginacion = Paginacion.De(resultado, "/Admin/Usuarios/Index", "pagina", Paginacion.Parametros(("id", Id), ("q", Q)));
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
