namespace PCBooster.Dominio;

/* Cliente o administrador de la tienda (tabla Usuarios). */
public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";

    /* Hash PBKDF2 ("pbkdf2$iteraciones$salt$hash"), nunca la contraseña. */
    public string HashContraseña { get; set; } = "";

    public string Email { get; set; } = "";
    /* null si no lo cargó. */
    public string? Telefono { get; set; }
    public bool EsAdmin { get; set; }

    /* Baja lógica: un usuario inactivo no puede iniciar sesión. */
    public bool Activo { get; set; } = true;

    public DateTime FechaAlta { get; set; }

    public List<Venta> Ventas { get; set; } = new();
    public List<Tarjeta> Tarjetas { get; set; } = new();
}
