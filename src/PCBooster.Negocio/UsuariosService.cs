using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using PCBooster.Datos;
using PCBooster.Dominio;

namespace PCBooster.Negocio;

/* Login, registro y datos de los usuarios. Los métodos que pueden fallar por
   una regla devuelven el mensaje para mostrar, o null si salió bien. */
public class UsuariosService(TiendaContext db)
{
    public const int LargoMinimoContraseña = 6;
    public const int LargoMaximoContraseña = 64;

    public static bool ContraseñaValida(string? contraseña) =>
        contraseña != null && contraseña.Length >= LargoMinimoContraseña && contraseña.Length <= LargoMaximoContraseña;

    public static string ErrorLargoContraseña =>
        $"La contraseña tiene que tener entre {LargoMinimoContraseña} y {LargoMaximoContraseña} caracteres.";

    // ------------------------------------------------------------------------
    // Freno a la fuerza bruta: después de MaxIntentosFallidos seguidos, el usuario
    // queda bloqueado por DuracionBloqueo. Vive en memoria: alcanza para una
    // instancia y se reinicia con la aplicación.
    // ------------------------------------------------------------------------
    public const int MaxIntentosFallidos = 5;
    public static TimeSpan DuracionBloqueo { get; set; } = TimeSpan.FromMinutes(5);

    sealed class Intentos
    {
        public int Fallidos;
        public DateTime Ultimo;
    }

    static readonly ConcurrentDictionary<string, Intentos> intentosFallidos = new(StringComparer.OrdinalIgnoreCase);

    public bool LoginBloqueado(string? nombre)
    {
        if (nombre == null || !intentosFallidos.TryGetValue(nombre, out Intentos? intentos))
        {
            return false;
        }
        lock (intentos)
        {
            return intentos.Fallidos >= MaxIntentosFallidos && DateTime.UtcNow - intentos.Ultimo < DuracionBloqueo;
        }
    }

    static void RegistrarFallo(string nombre)
    {
        // Que nombres inventados no hagan crecer el diccionario sin límite.
        if (intentosFallidos.Count > 10_000)
        {
            foreach (var par in intentosFallidos)
            {
                if (DateTime.UtcNow - par.Value.Ultimo >= DuracionBloqueo)
                {
                    intentosFallidos.TryRemove(par.Key, out _);
                }
            }
        }
        Intentos intentos = intentosFallidos.GetOrAdd(nombre, _ => new Intentos());
        lock (intentos)
        {
            if (DateTime.UtcNow - intentos.Ultimo >= DuracionBloqueo)
            {
                intentos.Fallidos = 0; // pasado el bloqueo se vuelve a empezar la cuenta
            }
            intentos.Fallidos++;
            intentos.Ultimo = DateTime.UtcNow;
        }
    }

    static void OlvidarFallos(string nombre) => intentosFallidos.TryRemove(nombre, out _);

    /* El usuario activo si las credenciales son válidas; null si no lo son o si
       está bloqueado por intentos fallidos. */
    public async Task<Usuario?> LoginAsync(string? nombre, string? contraseña)
    {
        nombre = (nombre ?? "").Trim();
        if (LoginBloqueado(nombre))
        {
            return null;
        }
        Usuario? usuario = await db.Usuarios.AsNoTracking().SingleOrDefaultAsync(u => u.Nombre == nombre && u.Activo);
        if (usuario == null)
        {
            Contrasenas.SimularVerificacion(contraseña); // mismo tiempo que con un usuario existente
        }
        if (usuario == null || !Contrasenas.Verificar(contraseña, usuario.HashContraseña))
        {
            RegistrarFallo(nombre);
            return null;
        }
        OlvidarFallos(nombre);
        return usuario;
    }

    public Task<Usuario?> ObtenerAsync(int id) => db.Usuarios.AsNoTracking().SingleOrDefaultAsync(u => u.Id == id);

    /* Alta de un cliente (registro) o, desde la administración, de un admin. */
    public async Task<string?> RegistrarAsync(string nombre, string email, string? telefono, string contraseña, bool esAdmin = false)
    {
        nombre = nombre.Trim();
        email = email.Trim();
        if (!ContraseñaValida(contraseña))
        {
            return ErrorLargoContraseña;
        }
        if (await db.Usuarios.AnyAsync(u => u.Nombre == nombre))
        {
            return "Ese nombre de usuario ya está en uso. Probá con otro.";
        }
        if (await db.Usuarios.AnyAsync(u => u.Email == email))
        {
            return "Ya existe una cuenta con ese e-mail.";
        }
        db.Usuarios.Add(new Usuario
        {
            Nombre = nombre,
            Email = email,
            Telefono = string.IsNullOrWhiteSpace(telefono) ? "Sin telefono" : telefono.Trim(),
            HashContraseña = Contrasenas.Hashear(contraseña),
            EsAdmin = esAdmin,
            Activo = true,
            FechaAlta = DateTime.Now
        });
        await db.SaveChangesAsync();
        return null;
    }

    /* El usuario edita su propia cuenta. Cambiar nombre, e-mail o contraseña exige
       la contraseña actual: con una sesión ajena abierta no se puede tomar la
       cuenta. Los errores cuentan para el mismo bloqueo que el login. El tipo y
       el estado no se pueden cambiar desde acá. "nuevaContraseña" vacía = no cambiarla. */
    public async Task<string?> ActualizarDatosPropiosAsync(int id, string nombre, string email, string? telefono,
        string? nuevaContraseña, string? contraseñaActual)
    {
        Usuario usuario = await db.Usuarios.SingleAsync(u => u.Id == id);
        nombre = nombre.Trim();
        email = email.Trim();
        bool cambiaCredenciales = nombre != usuario.Nombre
            || !string.Equals(email, usuario.Email, StringComparison.OrdinalIgnoreCase)
            || !string.IsNullOrEmpty(nuevaContraseña);

        if (cambiaCredenciales)
        {
            if (LoginBloqueado(usuario.Nombre))
            {
                return "Demasiados intentos fallidos. Esperá unos minutos y volvé a probar.";
            }
            if (!Contrasenas.Verificar(contraseñaActual, usuario.HashContraseña))
            {
                RegistrarFallo(usuario.Nombre);
                return "Para cambiar el nombre, el e-mail o la contraseña, ingresá tu contraseña actual correcta.";
            }
            OlvidarFallos(usuario.Nombre);
        }

        string? error = await ValidarNombreEmailYContraseña(id, nombre, email, nuevaContraseña);
        if (error != null)
        {
            return error;
        }
        Aplicar(usuario, nombre, email, telefono, nuevaContraseña);
        await db.SaveChangesAsync();
        return null;
    }

    // ------------------------------------------------------------------------
    // Administración
    // ------------------------------------------------------------------------
    public Task<List<Usuario>> ListarAsync(int? id = null) =>
        db.Usuarios.AsNoTracking().Where(u => id == null || u.Id == id).OrderBy(u => u.Id).ToListAsync();

    /* El admin no necesita la contraseña actual del usuario. */
    public async Task<string?> ModificarAsync(int id, string nombre, string email, string? telefono, bool esAdmin, bool activo, string? nuevaContraseña)
    {
        Usuario? usuario = await db.Usuarios.SingleOrDefaultAsync(u => u.Id == id);
        if (usuario == null)
        {
            return "El usuario no existe.";
        }
        string? error = await ValidarNombreEmailYContraseña(id, nombre.Trim(), email.Trim(), nuevaContraseña);
        if (error != null)
        {
            return error;
        }
        Aplicar(usuario, nombre.Trim(), email.Trim(), telefono, nuevaContraseña);
        usuario.EsAdmin = esAdmin;
        usuario.Activo = activo;
        await db.SaveChangesAsync();
        return null;
    }

    /* Baja lógica: el usuario ya no puede iniciar sesión. */
    public async Task<bool> DesactivarAsync(int id)
    {
        return await db.Usuarios.Where(u => u.Id == id).ExecuteUpdateAsync(s => s.SetProperty(u => u.Activo, false)) == 1;
    }

    async Task<string?> ValidarNombreEmailYContraseña(int id, string nombre, string email, string? nuevaContraseña)
    {
        if (await db.Usuarios.AnyAsync(u => u.Id != id && (u.Nombre == nombre || u.Email == email)))
        {
            return "Ese nombre de usuario o e-mail ya lo usa otra cuenta.";
        }
        if (!string.IsNullOrEmpty(nuevaContraseña) && !ContraseñaValida(nuevaContraseña))
        {
            return ErrorLargoContraseña;
        }
        return null;
    }

    static void Aplicar(Usuario usuario, string nombre, string email, string? telefono, string? nuevaContraseña)
    {
        usuario.Nombre = nombre;
        usuario.Email = email;
        usuario.Telefono = string.IsNullOrWhiteSpace(telefono) ? "Sin telefono" : telefono.Trim();
        if (!string.IsNullOrEmpty(nuevaContraseña))
        {
            usuario.HashContraseña = Contrasenas.Hashear(nuevaContraseña);
        }
    }
}
