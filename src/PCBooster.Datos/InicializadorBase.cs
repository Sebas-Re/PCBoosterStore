using Microsoft.EntityFrameworkCore;

namespace PCBooster.Datos;

/* Prepara el archivo SQLite al arrancar la app: aplica las migraciones de EF
   pendientes y, si la base está vacía, carga los datos de prueba. */
public static class InicializadorBase
{
    /* Clave de configuración (o variable de entorno) con la ruta de la base. */
    public const string ClaveRuta = "PCBOOSTER_DB";

    /* "cargarDemo" agrega el historial de clientes y ventas de los últimos 6 meses. */
    public static void Inicializar(string ruta, bool cargarDemo = true)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(ruta))!);
        using var db = new TiendaContext(TiendaContext.Opciones(ruta));
        db.Database.Migrate();
        if (db.Usuarios.Any())
        {
            return;
        }

        // En una transacción: si algo falla, la base queda vacía y se vuelve a cargar al próximo arranque.
        using var transaccion = db.Database.BeginTransaction();
        EjecutarScript(db, "seed.sql");
        EjecutarScript(db, "articulos_detalle.sql");
        if (cargarDemo)
        {
            EjecutarScript(db, "demo.sql");
        }
        // Los scripts traen las contraseñas en texto plano (así se leen las
        // credenciales de prueba); se guardan hasheadas.
        foreach (var usuario in db.Usuarios)
        {
            usuario.HashContraseña = Contrasenas.Hashear(usuario.HashContraseña);
        }
        db.SaveChanges();
        transaccion.Commit();
    }

    static void EjecutarScript(TiendaContext db, string nombre)
    {
        using Stream recurso = typeof(InicializadorBase).Assembly.GetManifestResourceStream("Sql." + nombre)
            ?? throw new InvalidOperationException("Falta el script embebido " + nombre);
        using var lector = new StreamReader(recurso);
        db.Database.ExecuteSqlRaw(lector.ReadToEnd());
    }
}
