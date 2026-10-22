namespace PCBooster.Dominio;

/* Cabecera de una compra. El total lo calcula la base (trigger tr_SumaTotal)
   a medida que se insertan los detalles. */
public class Venta
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public decimal Total { get; set; }
    public DateTime Fecha { get; set; }
    public bool PagoConCredito { get; set; }

    public List<DetalleVenta> Detalles { get; set; } = new();
}

/* Un producto dentro de una venta, con el precio que tenía al comprarlo. Al
   insertarlo, la base descuenta el stock (trigger tr_ControlStock). */
public class DetalleVenta
{
    public int VentaId { get; set; }
    public Venta? Venta { get; set; }

    /* Repetido de la venta: la clave foránea de la tabla es (venta, usuario). */
    public int UsuarioId { get; set; }

    public string ArticuloCodigo { get; set; } = "";
    public Articulo? Articulo { get; set; }

    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public string DireccionEntrega { get; set; } = "";
    public string Contacto { get; set; } = "";

    public decimal Subtotal => PrecioUnitario * Cantidad;
}
