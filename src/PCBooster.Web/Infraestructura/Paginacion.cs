using PCBooster.Negocio;

namespace PCBooster.Web.Infraestructura;

/* Datos para el partial _Paginacion: página actual, total y cómo armar los links
   (página de Razor, nombre del parámetro de la página y el resto de la querystring). */
public record Paginacion(int Actual, int Total, string Pagina, string Parametro, IDictionary<string, string> Ruta, bool Chica = true)
{
    public static Paginacion De<T>(Pagina<T> resultado, string pagina, string parametro, IDictionary<string, string> ruta, bool chica = true) =>
        new(resultado.Numero, resultado.TotalPaginas, pagina, parametro, ruta, chica);

    /* Querystring con los valores que no están vacíos. */
    public static Dictionary<string, string> Parametros(params (string Clave, object? Valor)[] valores) =>
        valores.Where(v => !string.IsNullOrWhiteSpace(v.Valor?.ToString()))
            .ToDictionary(v => v.Clave, v => v.Valor!.ToString()!);
}
