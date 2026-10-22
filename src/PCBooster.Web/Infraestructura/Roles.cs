using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using PCBooster.Dominio;

namespace PCBooster.Web.Infraestructura;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Cliente = "Cliente";
    public const string Politica = "SoloAdmin";

    /* Emite la cookie de autenticación. Cada login genera una cookie nueva, así que
       un id fijado antes por otro (session fixation) no sirve. En un login nuevo
       se descarta el carrito de la sesión anterior; al refrescar los datos del
       mismo usuario (cambió su nombre) se conserva. */
    public static async Task IniciarSesionAsync(HttpContext contexto, Usuario usuario, bool conservarCarrito = false)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nombre),
            new(ClaimTypes.Role, usuario.EsAdmin ? Admin : Cliente)
        };
        var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        if (!conservarCarrito)
        {
            contexto.Session.Clear();
        }
        await contexto.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identidad));
    }
}
