namespace PCBooster.Dominio;

/* Carrito de compras. Vive en la sesión (serializado a JSON) mientras el
   cliente navega y se vacía al confirmar o cancelar la compra. */
public class Carrito
{
    /* Público y con set para poder serializarlo; se modifica con Agregar/Quitar. */
    public List<ItemCarrito> Items { get; set; } = new();

    public bool Vacio => Items.Count == 0;
    public int CantidadUnidades => Items.Sum(i => i.Cantidad);
    public decimal Total => Items.Sum(i => i.Subtotal);

    /* Suma una unidad del artículo. Devuelve false si no queda stock para otra. */
    public bool Agregar(string codigo, string descripcion, decimal precioUnitario, int stock)
    {
        ItemCarrito? item = Buscar(codigo);
        int cantidadNueva = (item?.Cantidad ?? 0) + 1;
        if (cantidadNueva > stock)
        {
            return false;
        }
        if (item == null)
        {
            Items.Add(new ItemCarrito { Codigo = codigo, Descripcion = descripcion, PrecioUnitario = precioUnitario, Cantidad = 1 });
        }
        else
        {
            item.Cantidad = cantidadNueva;
        }
        return true;
    }

    public bool Quitar(string codigo) => Items.RemoveAll(i => MismoCodigo(i.Codigo, codigo)) > 0;

    public void Vaciar() => Items.Clear();

    private ItemCarrito? Buscar(string codigo) => Items.FirstOrDefault(i => MismoCodigo(i.Codigo, codigo));

    private static bool MismoCodigo(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
}

public class ItemCarrito
{
    public string Codigo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; }

    public decimal Subtotal => PrecioUnitario * Cantidad;
}
