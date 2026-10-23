using Microsoft.AspNetCore.Mvc;
using PCBooster.Dominio;
using PCBooster.Negocio;
using PCBooster.Web.Infraestructura;

namespace PCBooster.Web.Pages.Admin;

/* Reportes del período elegido. Todo va por querystring (período, filtros,
   páginas y venta seleccionada): los links se pueden compartir. */
public class ReportesModel(MetricasService metricas, ComprasService compras) : PaginaBase
{
    public static readonly int[] PeriodosDisponibles = [7, 30, 90, 180];
    const int VentasPorPagina = 8;
    const int ArticulosPorPagina = 10;

    [BindProperty(SupportsGet = true)] public int Dias { get; set; } = 30;
    [BindProperty(SupportsGet = true)] public int? Usuario { get; set; }
    [BindProperty(SupportsGet = true)] public int? Venta { get; set; }
    [BindProperty(SupportsGet = true)] public int Pv { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public string? Codigo { get; set; }
    [BindProperty(SupportsGet = true)] public string Orden { get; set; } = "codigo";
    [BindProperty(SupportsGet = true)] public int Pa { get; set; } = 1;

    public List<Indicador> Indicadores { get; private set; } = new();
    public string TasaRecompra { get; private set; } = "";
    public string GraficoDias { get; private set; } = "";
    public string GraficoProductos { get; private set; } = "";
    public string GraficoPagos { get; private set; } = "";
    public List<ClienteDestacado> TopClientes { get; private set; } = new();

    public IReadOnlyList<Venta> Ventas { get; private set; } = [];
    public Paginacion PaginacionVentas { get; private set; } = null!;
    public Venta? DetalleVenta { get; private set; }

    public IReadOnlyList<ArticuloVendido> ArticulosVendidos { get; private set; } = [];
    public Paginacion PaginacionArticulos { get; private set; } = null!;

    public async Task OnGetAsync()
    {
        if (!PeriodosDisponibles.Contains(Dias))
        {
            Dias = 30;
        }
        await CargarMetricasAsync();
        await CargarVentasAsync();
        await CargarArticulosVendidosAsync();
    }

    async Task CargarMetricasAsync()
    {
        string comparacion = $"vs. {Dias} días anteriores";
        Comparativa comparativa = await metricas.CompararAsync(Dias);
        Indicadores = Indicador.Principales(comparativa, comparacion);
        Indicadores.Add(new Indicador("Unidades vendidas", comparativa.Actual.Unidades.ToString("N0"),
            Comparativa.Variacion(comparativa.Actual.Unidades, comparativa.Anterior.Unidades), "bi-box-seam", "info", comparacion));
        TasaRecompra = comparativa.Actual.TasaRecompra.ToString("0") + "%";

        Serie dias = await metricas.IngresosPorDiaAsync(Dias);
        GraficoDias = new Grafico("line", dias.Etiquetas)
            .Serie("Ingresos", dias.Valores, Grafico.Moneda)
            .Serie("Pedidos", dias.Valores2, Grafico.Numero, "bar", ejeDerecho: true)
            .Json();

        Serie productos = await metricas.TopProductosAsync(Dias, 10);
        GraficoProductos = new Grafico("bar", productos.Etiquetas).Serie("Unidades", productos.Valores, Grafico.Numero).Horizontal().Json();

        Serie pagos = await metricas.MetodosDePagoAsync(Dias);
        GraficoPagos = new Grafico("doughnut", pagos.Etiquetas).Serie("Pedidos", pagos.Valores, Grafico.Numero).Json();

        TopClientes = await metricas.TopClientesAsync(Dias, 8);
    }

    async Task CargarVentasAsync()
    {
        Pagina<Venta> ventas = await compras.VentasAsync(Usuario, Pv, VentasPorPagina);
        Ventas = ventas.Items;
        Pv = ventas.Numero;
        PaginacionVentas = Paginacion.De(ventas, "/Admin/Reportes", "pv", Ruta(sin: "pv"));
        if (Venta != null)
        {
            DetalleVenta = await compras.DetalleAsync(Venta.Value);
        }
    }

    async Task CargarArticulosVendidosAsync()
    {
        OrdenArticulosVendidos orden = Orden switch
        {
            "unidades" => OrdenArticulosVendidos.Unidades,
            "recaudacion" => OrdenArticulosVendidos.Recaudacion,
            _ => OrdenArticulosVendidos.Codigo
        };
        Pagina<ArticuloVendido> articulos = await compras.ArticulosVendidosAsync(Codigo, orden, Pa, ArticulosPorPagina);
        ArticulosVendidos = articulos.Items;
        Pa = articulos.Numero;
        PaginacionArticulos = Paginacion.De(articulos, "/Admin/Reportes", "pa", Ruta(sin: "pa"));
    }

    /* Los parámetros actuales, para armar links que cambian solo uno. */
    public Dictionary<string, string> Ruta(string sin = "", params (string Clave, object? Valor)[] cambios)
    {
        var ruta = new Dictionary<string, string>
        {
            ["dias"] = Dias.ToString(),
            ["usuario"] = Usuario?.ToString() ?? "",
            ["venta"] = Venta?.ToString() ?? "",
            ["pv"] = Pv.ToString(),
            ["codigo"] = Codigo ?? "",
            ["orden"] = Orden,
            ["pa"] = Pa.ToString()
        };
        foreach (var (clave, valor) in cambios)
        {
            ruta[clave] = valor?.ToString() ?? "";
        }
        ruta.Remove(sin);
        return ruta.Where(p => p.Value != "").ToDictionary(p => p.Key, p => p.Value);
    }
}
