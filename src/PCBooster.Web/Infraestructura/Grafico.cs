using System.Text.Json;
using PCBooster.Negocio;

namespace PCBooster.Web.Infraestructura;

/* Arma la definición de un gráfico como JSON para wwwroot/js/graficos.js, que lo
   dibuja con Chart.js. Va en un atributo data-grafico (encodeado por Razor):
   new Grafico("bar", serie.Etiquetas).Serie("Ingresos", serie.Valores, Grafico.Moneda).Json() */
public class Grafico(string tipo, IList<string> etiquetas)
{
    public const string Moneda = "moneda";
    public const string Numero = "numero";

    readonly List<object> series = new();
    bool horizontal;

    public Grafico Serie(string nombre, IList<decimal> valores, string formato, string? tipoSerie = null, bool ejeDerecho = false)
    {
        series.Add(new { nombre, valores, formato, tipo = tipoSerie, ejeDerecho });
        return this;
    }

    public Grafico Horizontal()
    {
        horizontal = true;
        return this;
    }

    public string Json() => JsonSerializer.Serialize(new { tipo, etiquetas, series, horizontal });
}

/* Tarjeta de indicador (KPI) con la variación contra el período anterior. */
public record Indicador(string Titulo, string Valor, decimal? Variacion, string Icono, string Color, string Comparacion)
{
    /* Los tres indicadores principales de un período (ingresos, pedidos y ticket promedio). */
    public static List<Indicador> Principales(Comparativa c, string comparacion) =>
    [
        new("Ingresos", c.Actual.Ingresos.ToString("C0"), Comparativa.Variacion(c.Actual.Ingresos, c.Anterior.Ingresos), "bi-cash-stack", "success", comparacion),
        new("Pedidos", c.Actual.Pedidos.ToString("N0"), Comparativa.Variacion(c.Actual.Pedidos, c.Anterior.Pedidos), "bi-bag-check", "primary", comparacion),
        new("Ticket promedio", c.Actual.TicketPromedio.ToString("C0"), Comparativa.Variacion(c.Actual.TicketPromedio, c.Anterior.TicketPromedio), "bi-receipt", "warning", comparacion),
    ];
}
