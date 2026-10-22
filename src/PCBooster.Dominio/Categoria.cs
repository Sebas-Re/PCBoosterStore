namespace PCBooster.Dominio;

public class Categoria
{
    public int Id { get; set; }
    public string Descripcion { get; set; } = "";

    /* Una categoría inactiva oculta todos sus artículos del catálogo. */
    public bool Activa { get; set; } = true;

    public List<Articulo> Articulos { get; set; } = new();
}
