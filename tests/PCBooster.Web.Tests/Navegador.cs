using System.Net;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;

namespace PCBooster.Web.Tests;

/* Un navegador mínimo: mantiene cookies, sigue redirecciones y envía los
   formularios como lo haría el navegador (con el token antiforgery que trae
   el HTML). No ejecuta JavaScript, así que también prueba la validación del
   servidor. Usa https, como el sitio en producción. */
public sealed class Navegador : IDisposable
{
    public static readonly Uri Direccion = new("https://localhost/");

    readonly HttpClient cliente;
    readonly CookieContainer cookies = new();

    public Navegador(SitioFixture sitio)
    {
        cliente = sitio.CreateDefaultClient(Direccion, new RedirectHandler(), new CookieContainerHandler(cookies));
    }

    public Cookie? Cookie(string nombre) => cookies.GetCookies(Direccion)[nombre];

    public void PonerCookie(string nombre, string valor) => cookies.Add(Direccion, new Cookie(nombre, valor));

    public async Task<Pagina> Abrir(string ruta) => await Leer(await cliente.GetAsync(ruta));

    /* La respuesta sin procesar, para revisar headers. */
    public Task<HttpResponseMessage> Pedir(string ruta) => cliente.GetAsync(ruta);

    /* Envía el formulario que contiene el botón "selectorBoton" (o el primero de la
       página), con sus campos actuales pisados por "campos" (por nombre). */
    public Task<Pagina> Enviar(Pagina pagina, string? selectorBoton = null, IDictionary<string, string>? campos = null)
    {
        IElement? boton = selectorBoton == null ? null : pagina.Documento.QuerySelector(selectorBoton)
            ?? throw new InvalidOperationException("No está el botón " + selectorBoton);
        return Enviar(pagina, boton, campos);
    }

    public Task<Pagina> Enviar(Pagina pagina, IElement? boton, IDictionary<string, string>? campos = null)
    {
        var form = (IHtmlFormElement)(boton?.Closest("form") ?? pagina.Documento.Forms.FirstOrDefault(f => f.Method == "post")
            ?? throw new InvalidOperationException("La página no tiene formularios"));

        var valores = new List<KeyValuePair<string, string>>();
        foreach (IElement campo in form.Elements)
        {
            string? nombre = campo.GetAttribute("name");
            if (string.IsNullOrEmpty(nombre) || campo is IHtmlButtonElement) continue;
            if (campo is IHtmlInputElement input)
            {
                if ((input.Type == "checkbox" || input.Type == "radio") && !input.IsChecked) continue;
                if (input.Type == "submit") continue;
                valores.Add(new(nombre, input.Value));
            }
            else if (campo is IHtmlSelectElement select)
            {
                valores.Add(new(nombre, (select.Options.FirstOrDefault(o => o.IsSelected) ?? select.Options.FirstOrDefault())?.Value ?? ""));
            }
            else if (campo is IHtmlTextAreaElement area)
            {
                valores.Add(new(nombre, area.Value));
            }
        }
        if (boton?.GetAttribute("name") is { Length: > 0 } nombreBoton)
        {
            valores.Add(new(nombreBoton, boton.GetAttribute("value") ?? ""));
        }
        foreach (var campo in campos ?? new Dictionary<string, string>())
        {
            valores.RemoveAll(v => v.Key == campo.Key);
            valores.Add(new(campo.Key, campo.Value));
        }

        string accion = boton?.GetAttribute("formaction") ?? form.GetAttribute("action") ?? pagina.Url.PathAndQuery;
        return Postear(new Uri(pagina.Url, WebUtility.HtmlDecode(accion)), valores);
    }

    public async Task<Pagina> Postear(Uri url, IEnumerable<KeyValuePair<string, string>> valores) =>
        await Leer(await cliente.PostAsync(url, new FormUrlEncodedContent(valores)));

    static async Task<Pagina> Leer(HttpResponseMessage respuesta)
    {
        string html = await respuesta.Content.ReadAsStringAsync();
        return new Pagina(respuesta.RequestMessage!.RequestUri!, (int)respuesta.StatusCode, html);
    }

    public void Dispose() => cliente.Dispose();
}

public sealed class Pagina(Uri url, int estado, string html)
{
    public Uri Url { get; } = url;
    public int Estado { get; } = estado;
    public string Html { get; } = html;
    public IHtmlDocument Documento { get; } = new HtmlParser().ParseDocument(html);

    public string Ruta => Url.AbsolutePath;
    public string Texto => Documento.Body?.TextContent ?? "";

    public IReadOnlyList<string> Alertas => Documento.QuerySelectorAll(".alert").Select(a => a.TextContent.Trim()).ToList();

    public IReadOnlyList<string> ErroresDeValidacion => Documento.QuerySelectorAll(".validador").Select(e => e.TextContent.Trim()).Where(t => t.Length > 0).ToList();

    public IElement Elemento(string selector) => Documento.QuerySelector(selector) ?? throw new InvalidOperationException($"No está {selector} en {Ruta}");
}
