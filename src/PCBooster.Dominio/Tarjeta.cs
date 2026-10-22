namespace PCBooster.Dominio;

/* Tarjeta guardada por un cliente. No se guarda el código de seguridad. */
public class Tarjeta
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public string Titular { get; set; } = "";

    /* Marca o nombre que le da el cliente ("Visa", "Master débito"...). */
    public string Nombre { get; set; } = "";

    public bool EsCredito { get; set; }
    public long Numero { get; set; }

    /* Primer día del mes de vencimiento. */
    public DateOnly Vencimiento { get; set; }

    public string UltimosDigitos => Numero.ToString().PadLeft(4, '0')[^4..];
}
