using System.Globalization;
using Microsoft.EntityFrameworkCore;
using PCBooster.Datos;
using PCBooster.Dominio;

namespace PCBooster.Negocio;

/* Métricas de e-commerce para el panel y los reportes, con LINQ sobre los
   detalles de venta (cantidad x precio), que es el dato de origen: así se
   pueden desglosar por categoría o producto y siempre suman lo mismo que los
   totales. Los períodos son de días completos e incluyen hoy: "últimos 30
   días" = hoy y los 29 anteriores. */
public class MetricasService(TiendaContext db)
{
    static readonly CultureInfo Cultura = new("es-AR");

    /* Detalles de las ventas hechas entre "desde" (incluido) y "hasta" (excluido). */
    IQueryable<DetalleVenta> Lineas(DateTime desde, DateTime hasta) =>
        db.DetallesVenta.AsNoTracking().Where(d => d.Venta!.Fecha >= desde && d.Venta.Fecha < hasta);

    /* [hoy - (dias - 1), mañana): los últimos "dias" días completos. */
    static (DateTime Desde, DateTime Hasta) UltimosDias(int dias) =>
        (DateTime.Today.AddDays(1 - dias), DateTime.Today.AddDays(1));

    /* Últimos "dias" días contra los "dias" días anteriores. */
    public async Task<Comparativa> CompararAsync(int dias)
    {
        var (desde, hasta) = UltimosDias(dias);
        return new Comparativa(await ResumenAsync(desde, hasta), await ResumenAsync(desde.AddDays(-dias), desde));
    }

    async Task<ResumenPeriodo> ResumenAsync(DateTime desde, DateTime hasta)
    {
        IQueryable<DetalleVenta> lineas = Lineas(desde, hasta);
        return new ResumenPeriodo
        {
            Ingresos = await lineas.SumAsync(d => d.Cantidad * d.PrecioUnitario),
            Pedidos = await lineas.Select(d => d.VentaId).Distinct().CountAsync(),
            Unidades = await lineas.SumAsync(d => d.Cantidad),
            Compradores = await lineas.Select(d => d.Venta!.UsuarioId).Distinct().CountAsync(),
            CompradoresRecurrentes = await lineas.GroupBy(d => d.Venta!.UsuarioId)
                .CountAsync(g => g.Select(d => d.VentaId).Distinct().Count() >= 2),
            ClientesNuevos = await db.Usuarios.CountAsync(u => !u.EsAdmin && u.FechaAlta >= desde && u.FechaAlta < hasta)
        };
    }

    /* Ingresos (Valores) y pedidos (Valores2) de los últimos "meses" meses
       calendario, incluido el actual; los meses sin ventas van en 0. */
    public async Task<Serie> IngresosPorMesAsync(int meses)
    {
        DateTime primerMes = PrimerMes(meses);
        var porMes = await Lineas(primerMes, DateTime.Today.AddDays(1))
            .GroupBy(d => new { d.Venta!.Fecha.Year, d.Venta.Fecha.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Ingresos = g.Sum(d => d.Cantidad * d.PrecioUnitario), Pedidos = g.Select(d => d.VentaId).Distinct().Count() })
            .ToListAsync();
        return SeriePorMes(meses, mes =>
        {
            var fila = porMes.SingleOrDefault(f => f.Year == mes.Year && f.Month == mes.Month);
            return (fila?.Ingresos ?? 0, fila?.Pedidos ?? 0);
        });
    }

    public async Task<Serie> ClientesNuevosPorMesAsync(int meses)
    {
        DateTime primerMes = PrimerMes(meses);
        var porMes = await db.Usuarios.AsNoTracking()
            .Where(u => !u.EsAdmin && u.FechaAlta >= primerMes)
            .GroupBy(u => new { u.FechaAlta.Year, u.FechaAlta.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Clientes = g.Count() })
            .ToListAsync();
        return SeriePorMes(meses, mes => (porMes.SingleOrDefault(f => f.Year == mes.Year && f.Month == mes.Month)?.Clientes ?? 0, 0));
    }

    /* Ingresos (Valores) y pedidos (Valores2) por día; los días sin ventas van en 0. */
    public async Task<Serie> IngresosPorDiaAsync(int dias)
    {
        var (desde, hasta) = UltimosDias(dias);
        var porDia = await Lineas(desde, hasta)
            .GroupBy(d => d.Venta!.Fecha.Date)
            .Select(g => new { Dia = g.Key, Ingresos = g.Sum(d => d.Cantidad * d.PrecioUnitario), Pedidos = g.Select(d => d.VentaId).Distinct().Count() })
            .ToDictionaryAsync(f => f.Dia);

        var serie = new Serie();
        for (DateTime dia = desde; dia < hasta; dia = dia.AddDays(1))
        {
            porDia.TryGetValue(dia, out var fila);
            serie.Agregar(dia.ToString("dd/MM", Cultura), fila?.Ingresos ?? 0, fila?.Pedidos ?? 0);
        }
        return serie;
    }

    /* Ingresos (Valores) y unidades (Valores2) por categoría, de mayor a menor. */
    public async Task<Serie> IngresosPorCategoriaAsync(int dias)
    {
        var (desde, hasta) = UltimosDias(dias);
        var filas = await Lineas(desde, hasta)
            .GroupBy(d => d.Articulo!.Categoria!.Descripcion)
            .Select(g => new { Categoria = g.Key, Ingresos = g.Sum(d => d.Cantidad * d.PrecioUnitario), Unidades = g.Sum(d => d.Cantidad) })
            .OrderByDescending(f => f.Ingresos)
            .ToListAsync();
        var serie = new Serie();
        filas.ForEach(f => serie.Agregar(f.Categoria, f.Ingresos, f.Unidades));
        return serie;
    }

    /* Unidades (Valores) e ingresos (Valores2) de los productos más vendidos. */
    public async Task<Serie> TopProductosAsync(int dias, int cantidad)
    {
        var (desde, hasta) = UltimosDias(dias);
        var filas = await Lineas(desde, hasta)
            .GroupBy(d => new { d.ArticuloCodigo, d.Articulo!.Descripcion })
            .Select(g => new { g.Key.Descripcion, Unidades = g.Sum(d => d.Cantidad), Ingresos = g.Sum(d => d.Cantidad * d.PrecioUnitario) })
            .OrderByDescending(f => f.Unidades).ThenByDescending(f => f.Ingresos)
            .Take(cantidad)
            .ToListAsync();
        var serie = new Serie();
        filas.ForEach(f => serie.Agregar(f.Descripcion, f.Unidades, f.Ingresos));
        return serie;
    }

    /* Pedidos (Valores) e ingresos (Valores2) con débito y con crédito. */
    public async Task<Serie> MetodosDePagoAsync(int dias)
    {
        var (desde, hasta) = UltimosDias(dias);
        var filas = await Lineas(desde, hasta)
            .GroupBy(d => d.Venta!.PagoConCredito)
            .Select(g => new { Credito = g.Key, Pedidos = g.Select(d => d.VentaId).Distinct().Count(), Ingresos = g.Sum(d => d.Cantidad * d.PrecioUnitario) })
            .ToListAsync();
        var serie = new Serie();
        foreach (bool credito in new[] { false, true })
        {
            var fila = filas.SingleOrDefault(f => f.Credito == credito);
            serie.Agregar(credito ? "Crédito" : "Débito", fila?.Pedidos ?? 0, fila?.Ingresos ?? 0);
        }
        return serie;
    }

    public async Task<List<ClienteDestacado>> TopClientesAsync(int dias, int cantidad)
    {
        var (desde, hasta) = UltimosDias(dias);
        var filas = await Lineas(desde, hasta)
            .GroupBy(d => new { d.Venta!.UsuarioId, d.Venta.Usuario!.Nombre })
            .Select(g => new { g.Key.UsuarioId, g.Key.Nombre, Pedidos = g.Select(d => d.VentaId).Distinct().Count(), Gasto = g.Sum(d => d.Cantidad * d.PrecioUnitario) })
            .OrderByDescending(f => f.Gasto)
            .Take(cantidad)
            .ToListAsync();
        return filas.Select(f => new ClienteDestacado(f.UsuarioId, f.Nombre, f.Pedidos, f.Gasto)).ToList();
    }

    /* Artículos activos con stock por debajo del umbral, de menor a mayor. */
    public Task<List<Articulo>> StockBajoAsync(int umbral) =>
        db.Articulos.AsNoTracking().Where(a => a.Activo && a.Stock < umbral)
            .OrderBy(a => a.Stock).ThenBy(a => a.Descripcion).ToListAsync();

    static DateTime PrimerMes(int meses) => new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1 - meses);

    static Serie SeriePorMes(int meses, Func<DateTime, (decimal Valor, decimal Valor2)> valores)
    {
        var serie = new Serie();
        DateTime mes = PrimerMes(meses);
        for (int i = 0; i < meses; i++, mes = mes.AddMonths(1))
        {
            var (valor, valor2) = valores(mes);
            serie.Agregar(Cultura.TextInfo.ToTitleCase(mes.ToString("MMM yy", Cultura).Replace(".", "")), valor, valor2);
        }
        return serie;
    }
}

public record ClienteDestacado(int Id, string Nombre, int Pedidos, decimal Gasto);

public class ResumenPeriodo
{
    public decimal Ingresos { get; init; }
    public int Pedidos { get; init; }
    public int Unidades { get; init; }
    public int Compradores { get; init; }
    public int CompradoresRecurrentes { get; init; }
    public int ClientesNuevos { get; init; }

    public decimal TicketPromedio => Pedidos == 0 ? 0 : Ingresos / Pedidos;

    /* Porcentaje de compradores del período que compraron más de una vez. */
    public decimal TasaRecompra => Compradores == 0 ? 0 : 100m * CompradoresRecurrentes / Compradores;
}

public record Comparativa(ResumenPeriodo Actual, ResumenPeriodo Anterior)
{
    /* Variación porcentual; null si en el período anterior no hubo nada contra qué comparar. */
    public static decimal? Variacion(decimal actual, decimal anterior) =>
        anterior == 0 ? null : Math.Round((actual - anterior) / anterior * 100, 1);
}

/* Serie para un gráfico: una etiqueta por punto y hasta dos valores. */
public class Serie
{
    public List<string> Etiquetas { get; } = new();
    public List<decimal> Valores { get; } = new();
    public List<decimal> Valores2 { get; } = new();

    public void Agregar(string etiqueta, decimal valor, decimal valor2)
    {
        Etiquetas.Add(etiqueta);
        Valores.Add(valor);
        Valores2.Add(valor2);
    }
}
