using Microsoft.EntityFrameworkCore;
using PCBooster.Datos;
using PCBooster.Dominio;

namespace PCBooster.Negocio;

/* Resultado de confirmar una compra: el id de la venta o el motivo del rechazo. */
public record ResultadoCompra(int IdVenta, string? Error)
{
    public bool Ok => Error == null;
}

/* Fila del reporte de artículos vendidos. */
public record ArticuloVendido(string Codigo, string Descripcion, int Unidades, decimal Total);

public enum OrdenArticulosVendidos
{
    Codigo,
    Unidades,
    Recaudacion
}

public class ComprasService(TiendaContext db)
{
    /* Registra la compra del carrito con una tarjeta guardada del usuario, en una
       sola transacción: venta y detalles se guardan juntos o no se guarda nada.
       Precio y medio de pago se toman de la base (o del carrito, que se armó con
       precios de la base), nunca de lo que mande la página. */
    public async Task<ResultadoCompra> ConfirmarAsync(int usuarioId, Carrito? carrito, string? direccion, string? contacto, int tarjetaId)
    {
        if (carrito == null || carrito.Vacio)
        {
            return new(0, "El carrito está vacío.");
        }
        if (string.IsNullOrWhiteSpace(direccion) || string.IsNullOrWhiteSpace(contacto))
        {
            return new(0, "Completá la dirección de entrega y el contacto.");
        }

        // SQLite abre la transacción con BEGIN IMMEDIATE: otra compra espera hasta
        // que esta termine, así el stock que se verifica es el que se descuenta.
        await using var transaccion = await db.Database.BeginTransactionAsync();

        Tarjeta? tarjeta = await db.Tarjetas.SingleOrDefaultAsync(t => t.Id == tarjetaId && t.UsuarioId == usuarioId);
        if (tarjeta == null || !TarjetasService.Vigente(tarjeta.Vencimiento))
        {
            return new(0, "Elegí una de tus tarjetas guardadas que no esté vencida.");
        }

        var venta = new Venta
        {
            UsuarioId = usuarioId,
            Fecha = DateTime.Now,
            PagoConCredito = tarjeta.EsCredito,
            DireccionEntrega = direccion.Trim(),
            Contacto = contacto.Trim()
        };
        foreach (ItemCarrito item in carrito.Items)
        {
            Articulo? articulo = await db.Articulos.SingleOrDefaultAsync(a => a.Codigo == item.Codigo);
            if (articulo == null || !articulo.Activo)
            {
                return new(0, $"\"{articulo?.Descripcion ?? item.Descripcion}\" ya no está disponible. Quitalo del carrito para continuar.");
            }
            if (articulo.Stock < item.Cantidad)
            {
                return new(0, $"Solo quedan {articulo.Stock} unidades de \"{articulo.Descripcion}\". Ajustá el carrito para continuar.");
            }
            articulo.Stock -= item.Cantidad;
            venta.Detalles.Add(new DetalleVenta { ArticuloCodigo = articulo.Codigo, Cantidad = item.Cantidad, PrecioUnitario = item.PrecioUnitario });
        }
        venta.Total = venta.Detalles.Sum(d => d.Subtotal);

        db.Ventas.Add(venta);
        await db.SaveChangesAsync();
        await transaccion.CommitAsync();
        return new(venta.Id, null);
    }

    /* Compras del usuario, de la más reciente a la más vieja. */
    public Task<List<Venta>> ComprasDeAsync(int usuarioId) =>
        db.Ventas.AsNoTracking().Where(v => v.UsuarioId == usuarioId).OrderByDescending(v => v.Id).ToListAsync();

    /* Una venta con sus productos. Con usuarioId, solo si es de ese usuario. */
    public Task<Venta?> DetalleAsync(int ventaId, int? usuarioId = null) =>
        db.Ventas.AsNoTracking().Include(v => v.Usuario).Include(v => v.Detalles).ThenInclude(d => d.Articulo)
            .SingleOrDefaultAsync(v => v.Id == ventaId && (usuarioId == null || v.UsuarioId == usuarioId));

    // ------------------------------------------------------------------------
    // Administración
    // ------------------------------------------------------------------------
    /* Ventas de la más reciente a la más vieja; con usuarioId, solo las de ese cliente. */
    public Task<Pagina<Venta>> VentasAsync(int? usuarioId, int pagina, int tamañoPagina) =>
        db.Ventas.AsNoTracking().Include(v => v.Usuario)
            .Where(v => usuarioId == null || v.UsuarioId == usuarioId)
            .OrderByDescending(v => v.Id).PaginarAsync(pagina, tamañoPagina);

    /* Unidades y monto vendidos de cada artículo (o solo del código pedido), en toda la historia. */
    public async Task<Pagina<ArticuloVendido>> ArticulosVendidosAsync(string? codigo, OrdenArticulosVendidos orden, int pagina, int tamañoPagina)
    {
        IQueryable<DetalleVenta> detalles = db.DetallesVenta.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(codigo))
        {
            codigo = codigo.Trim();
            detalles = detalles.Where(d => d.ArticuloCodigo == codigo);
        }
        // Se ordena y pagina el tipo anónimo: EF no traduce un orden por propiedades de un record.
        var filas = detalles
            .GroupBy(d => new { d.ArticuloCodigo, d.Articulo!.Descripcion })
            .Select(g => new { g.Key.ArticuloCodigo, g.Key.Descripcion, Unidades = g.Sum(d => d.Cantidad), Total = g.Sum(d => d.Cantidad * d.PrecioUnitario) });
        var ordenadas = orden switch
        {
            OrdenArticulosVendidos.Unidades => filas.OrderByDescending(f => f.Unidades).ThenBy(f => f.ArticuloCodigo),
            OrdenArticulosVendidos.Recaudacion => filas.OrderByDescending(f => f.Total).ThenBy(f => f.ArticuloCodigo),
            _ => filas.OrderBy(f => f.ArticuloCodigo)
        };
        var resultado = await ordenadas.PaginarAsync(pagina, tamañoPagina);
        return new Pagina<ArticuloVendido>(
            resultado.Items.Select(f => new ArticuloVendido(f.ArticuloCodigo, f.Descripcion, f.Unidades, f.Total)).ToList(),
            resultado.Total, resultado.Numero, resultado.TamañoPagina);
    }
}
