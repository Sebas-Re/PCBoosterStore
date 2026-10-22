using System.Text.Json;
using PCBooster.Dominio;

namespace PCBooster.Web.Infraestructura;

/* El carrito del usuario, guardado como JSON en la sesión. */
public class CarritoSesion(IHttpContextAccessor accesor)
{
    const string Clave = "Carrito";

    ISession Sesion => accesor.HttpContext!.Session;

    public Carrito Obtener()
    {
        string? json = Sesion.GetString(Clave);
        return json == null ? new Carrito() : JsonSerializer.Deserialize<Carrito>(json) ?? new Carrito();
    }

    public void Guardar(Carrito carrito) => Sesion.SetString(Clave, JsonSerializer.Serialize(carrito));

    public void Vaciar() => Sesion.Remove(Clave);
}
