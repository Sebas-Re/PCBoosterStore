namespace PCBooster.Dominio;

/* Cabecera de una compra: quién, cuándo, cómo pagó y adónde se entrega. */
public class Venta
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    /* Suma de los subtotales de los detalles, fijada al confirmar la compra. */
    public decimal Total { get; set; }
    public DateTime Fecha { get; set; }
    public bool PagoConCredito { get; set; }

    public string DireccionEntrega { get; set; } = "";

    /* Quién recibe el pedido. */
    public string Contacto { get; set; } = "";

    public List<DetalleVenta> Detalles { get; set; } = new();
}

/* Un producto dentro de una venta, con el precio que tenía al comprarlo. */
public class DetalleVenta
{
    public int VentaId { get; set; }
    public Venta? Venta { get; set; }

    public string ArticuloCodigo { get; set; } = "";
    public Articulo? Articulo { get; set; }

    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }

    public decimal Subtotal => PrecioUnitario * Cantidad;
}
