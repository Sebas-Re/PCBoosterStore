using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PCBooster.Web.Pages;

/* Error genérico, sin detalles internos (el stack trace solo se ve en Development). */
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int? Codigo { get; set; }

    public string Titulo => Codigo switch
    {
        404 => "No encontramos esa página",
        400 => "La solicitud no es válida",
        403 => "No tenés acceso a esta página",
        _ => "Algo salió mal"
    };
}
