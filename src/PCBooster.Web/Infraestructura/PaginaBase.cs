using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace PCBooster.Web.Infraestructura;

public enum TipoMensaje
{
    Exito,
    Error,
    Aviso,
    Info
}

public record Mensaje(string Texto, TipoMensaje Tipo)
{
    public string ClaseBootstrap => Tipo switch
    {
        TipoMensaje.Exito => "success",
        TipoMensaje.Error => "danger",
        TipoMensaje.Aviso => "warning",
        _ => "info"
    };

    public string Icono => Tipo switch
    {
        TipoMensaje.Exito => "bi-check-circle-fill",
        TipoMensaje.Error => "bi-x-circle-fill",
        TipoMensaje.Aviso => "bi-exclamation-triangle-fill",
        _ => "bi-info-circle-fill"
    };
}

public static class Mensajes
{
    const string Clave = "Mensajes";

    /* Los mensajes van en TempData: se muestran en esta respuesta o, si la
       página redirige, en la siguiente (mensajes "flash"). */
    public static void Agregar(ITempDataDictionary tempData, string texto, TipoMensaje tipo)
    {
        List<Mensaje> lista = Leer(tempData, conservar: true);
        lista.Add(new Mensaje(texto, tipo));
        tempData[Clave] = JsonSerializer.Serialize(lista);
    }

    public static List<Mensaje> Leer(ITempDataDictionary tempData, bool conservar = false)
    {
        string? json = (conservar ? tempData.Peek(Clave) : tempData[Clave]) as string;
        return json == null ? new List<Mensaje>() : JsonSerializer.Deserialize<List<Mensaje>>(json) ?? new List<Mensaje>();
    }
}

/* Base de las páginas: datos del usuario logueado y mensajes. */
public abstract class PaginaBase : PageModel
{
    protected int UsuarioId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    protected void Avisar(string texto, TipoMensaje tipo) => Mensajes.Agregar(TempData, texto, tipo);
}
