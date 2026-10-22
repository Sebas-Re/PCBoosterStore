namespace PCBooster.Web.Infraestructura;

/* Headers de seguridad en todas las respuestas.
   La CSP no permite scripts inline: todo el JavaScript está en wwwroot/js y los
   comportamientos del markup (confirmaciones, imagen de reemplazo) se declaran
   con atributos data-*. Los estilos inline sí se permiten (atributos style del
   markup y de Bootstrap). Las imágenes pueden venir de https porque el admin
   puede cargar URLs externas. */
public class HeadersSeguridad(RequestDelegate siguiente)
{
    public const string Csp =
        "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; " +
        "font-src 'self'; connect-src 'self'; object-src 'none'; base-uri 'self'; form-action 'self'; frame-ancestors 'none'";

    public Task InvokeAsync(HttpContext contexto)
    {
        IHeaderDictionary headers = contexto.Response.Headers;
        headers.ContentSecurityPolicy = Csp;
        headers.XContentTypeOptions = "nosniff";
        headers.XFrameOptions = "DENY";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        return siguiente(contexto);
    }
}
