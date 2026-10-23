using PCBooster.Dominio;
using PCBooster.Negocio;
using PCBooster.Web.Infraestructura;

namespace PCBooster.Web.Pages.Admin;

/* Panel: los últimos 30 días contra los 30 anteriores, tendencias de 6 meses y stock bajo. */
public class IndexModel(MetricasService metricas) : PaginaBase
{
    const int Dias = 30;
    const int Meses = 6;
    public const int UmbralStock = 10;

    public List<Indicador> Indicadores { get; private set; } = new();
    public string GraficoMeses { get; private set; } = "";
    public string GraficoCategorias { get; private set; } = "";
    public string GraficoClientes { get; private set; } = "";
    public List<Articulo> StockBajo { get; private set; } = new();

    public async Task OnGetAsync()
    {
        const string comparacion = "vs. 30 días anteriores";
        Comparativa comparativa = await metricas.CompararAsync(Dias);
        Indicadores = Indicador.Principales(comparativa, comparacion);
        Indicadores.Add(new Indicador("Clientes nuevos", comparativa.Actual.ClientesNuevos.ToString(),
            Comparativa.Variacion(comparativa.Actual.ClientesNuevos, comparativa.Anterior.ClientesNuevos), "bi-person-plus", "info", comparacion));

        Serie meses = await metricas.IngresosPorMesAsync(Meses);
        GraficoMeses = new Grafico("bar", meses.Etiquetas)
            .Serie("Ingresos", meses.Valores, Grafico.Moneda)
            .Serie("Pedidos", meses.Valores2, Grafico.Numero, "line", ejeDerecho: true)
            .Json();

        Serie categorias = await metricas.IngresosPorCategoriaAsync(180);
        GraficoCategorias = new Grafico("doughnut", categorias.Etiquetas).Serie("Ingresos", categorias.Valores, Grafico.Moneda).Json();

        Serie clientes = await metricas.ClientesNuevosPorMesAsync(Meses);
        GraficoClientes = new Grafico("bar", clientes.Etiquetas).Serie("Clientes nuevos", clientes.Valores, Grafico.Numero).Json();

        StockBajo = await metricas.StockBajoAsync(UmbralStock);
    }
}
