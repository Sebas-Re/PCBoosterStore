using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using PCBooster.Dominio;
using PCBooster.Negocio;
using PCBooster.Web.Infraestructura;

namespace PCBooster.Web.Pages;

/* Iniciar sesión. Es la página de inicio del sitio. */
public class IndexModel(UsuariosService usuarios) : PaginaBase
{
    [BindProperty, Required(ErrorMessage = "Ingresá tu usuario."), StringLength(30)]
    public string Nombre { get; set; } = "";

    [BindProperty, Required(ErrorMessage = "Ingresá tu contraseña."), StringLength(UsuariosService.LargoMaximoContraseña)]
    [DataType(DataType.Password)]
    public string Contraseña { get; set; } = "";

    /* Página a la que se quería entrar antes de que pidiera el login. */
    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public IActionResult OnGet() => User.Identity?.IsAuthenticated == true ? IrAlInicio(User.IsInRole(Roles.Admin)) : Page();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        Usuario? usuario = await usuarios.LoginAsync(Nombre, Contraseña);
        if (usuario == null)
        {
            Avisar(usuarios.LoginBloqueado(Nombre.Trim())
                ? "Demasiados intentos fallidos. Esperá unos minutos y volvé a probar."
                : "Usuario o contraseña incorrectos. Si todavía no tenés cuenta, registrate.", TipoMensaje.Error);
            return Page();
        }
        await Roles.IniciarSesionAsync(HttpContext, usuario);
        // Solo direcciones de este sitio: nada de redirigir a otro dominio (open redirect).
        return Url.IsLocalUrl(ReturnUrl) ? LocalRedirect(ReturnUrl) : IrAlInicio(usuario.EsAdmin);
    }

    IActionResult IrAlInicio(bool esAdmin) => RedirectToPage(esAdmin ? "/Admin/Index" : "/Catalogo");
}
