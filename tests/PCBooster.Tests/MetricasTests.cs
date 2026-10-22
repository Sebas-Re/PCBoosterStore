using PCBooster.Dominio;
using PCBooster.Negocio;

namespace PCBooster.Tests;

public class MetricasTests : BaseTemporal
{
    MetricasService Metricas() => new(Contexto());

    /* El usuario compra 2 Gabinete Gamemax ($3.500) y 1 Fuente Deepcool ($7.300) con crédito. */
    async Task Comprar(int usuarioId = 5)
    {
        var tarjetas = new TarjetasService(Contexto());
        if ((await tarjetas.ListarAsync(usuarioId)).Count == 0)
        {
            await tarjetas.AgregarAsync(new Tarjeta { UsuarioId = usuarioId, Titular = "T", Nombre = "Visa", EsCredito = true, Numero = 4567123412340000 + usuarioId, Vencimiento = new DateOnly(2099, 1, 1) });
        }
        int tarjeta = (await tarjetas.ListarAsync(usuarioId))[0].Id;
        var carrito = new Carrito();
        carrito.Agregar("gb1", "Gabinete Gamemax", 3500m, 10);
        carrito.Agregar("gb1", "Gabinete Gamemax", 3500m, 10);
        carrito.Agregar("fnt1", "Fuente Deepcool 700W", 7300m, 100);
        ResultadoCompra resultado = await new ComprasService(Contexto()).ConfirmarAsync(usuarioId, carrito, "Calle 1", "Yo", tarjeta);
        Assert.True(resultado.Ok, resultado.Error);
    }

    [Fact]
    public async Task ResumenDelPeriodoContraElAnterior()
    {
        await Comprar();
        await new UsuariosService(Contexto()).RegistrarAsync("nuevo", "n@x.com", "1", "clave1");

        Comparativa c = await Metricas().CompararAsync(30);

        Assert.Equal(14300m, c.Actual.Ingresos);
        Assert.Equal(1, c.Actual.Pedidos);
        Assert.Equal(3, c.Actual.Unidades);
        Assert.Equal(14300m, c.Actual.TicketPromedio);
        Assert.Equal(1, c.Actual.ClientesNuevos); // los usuarios del seed tienen 210 días
        Assert.Equal(0m, c.Anterior.Ingresos);
        Assert.Null(Comparativa.Variacion(c.Actual.Ingresos, c.Anterior.Ingresos));
    }

    [Fact]
    public async Task TasaDeRecompra()
    {
        await Comprar(5);
        await Comprar(5);
        await Comprar(6);

        ResumenPeriodo r = (await Metricas().CompararAsync(7)).Actual;

        Assert.Equal(2, r.Compradores);
        Assert.Equal(1, r.CompradoresRecurrentes);
        Assert.Equal(50m, r.TasaRecompra);
    }

    [Fact]
    public async Task LasSeriesRellenanLosPeriodosSinVentas()
    {
        await Comprar();

        Serie meses = await Metricas().IngresosPorMesAsync(6);
        Serie dias = await Metricas().IngresosPorDiaAsync(30);

        Assert.Equal(6, meses.Etiquetas.Count);
        Assert.Equal(new[] { 0m, 0m, 0m, 0m, 0m, 14300m }, meses.Valores);
        Assert.Equal(1m, meses.Valores2.Last());
        Assert.Equal(30, dias.Valores.Count);
        Assert.Equal(14300m, dias.Valores.Last());
        Assert.Equal(DateTime.Today.ToString("dd/MM"), dias.Etiquetas.Last());
    }

    [Fact]
    public async Task IngresosPorCategoriaProductosYMetodosDePago()
    {
        await Comprar();

        Serie categorias = await Metricas().IngresosPorCategoriaAsync(30);
        Serie productos = await Metricas().TopProductosAsync(30, 5);
        Serie pagos = await Metricas().MetodosDePagoAsync(30);
        var clientes = await Metricas().TopClientesAsync(30, 5);

        Assert.Equal(new[] { "Fuente", "Gabinete" }, categorias.Etiquetas);
        Assert.Equal(new[] { 7300m, 7000m }, categorias.Valores);
        Assert.Equal("Gabinete Gamemax", productos.Etiquetas[0]);
        Assert.Equal(2m, productos.Valores[0]);
        Assert.Equal(new[] { "Débito", "Crédito" }, pagos.Etiquetas);
        Assert.Equal(new[] { 0m, 1m }, pagos.Valores);
        Assert.Equal(new ClienteDestacado(5, "Nom1", 1, 14300m), Assert.Single(clientes));
    }

    [Fact]
    public async Task StockBajoSoloIncluyeArticulosActivos()
    {
        await new CatalogoService(Contexto()).DarDeBajaArticuloAsync("gb8");

        var codigos = (await Metricas().StockBajoAsync(10)).Select(a => a.Codigo).ToList();

        Assert.Contains("pdv4", codigos);
        Assert.DoesNotContain("gb8", codigos);
        Assert.DoesNotContain("gb1", codigos); // stock 10: no es menor al umbral
    }

    [Fact]
    public async Task ConElHistorialDeDemoLasMetricasSonConsistentes()
    {
        CrearBase(conDemo: true);
        var metricas = Metricas();

        decimal ingresos180 = (await metricas.CompararAsync(180)).Actual.Ingresos;
        decimal ingresos30 = (await metricas.CompararAsync(30)).Actual.Ingresos;

        Assert.True(ingresos180 > 0);
        Assert.Equal(ingresos180, (await metricas.IngresosPorCategoriaAsync(180)).Valores.Sum());
        Assert.Equal(ingresos30, (await metricas.IngresosPorDiaAsync(30)).Valores.Sum());
        Assert.All((await metricas.IngresosPorMesAsync(6)).Valores.Take(5), v => Assert.True(v > 0));
        Assert.True((await metricas.ClientesNuevosPorMesAsync(6)).Valores.Sum() > 0);
    }

    [Fact]
    public void VariacionPorcentual()
    {
        Assert.Equal(50m, Comparativa.Variacion(150, 100));
        Assert.Equal(-25m, Comparativa.Variacion(75, 100));
        Assert.Null(Comparativa.Variacion(10, 0));
    }
}
