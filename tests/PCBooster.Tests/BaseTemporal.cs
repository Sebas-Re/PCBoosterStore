using Microsoft.Data.Sqlite;
using PCBooster.Datos;

namespace PCBooster.Tests;

/* Cada test arranca con una base SQLite nueva en una carpeta temporal, creada
   con el schema y los datos de prueba (sin el historial de demo, salvo que el
   test lo pida). */
public abstract class BaseTemporal : IDisposable
{
    readonly string carpeta = Path.Combine(Path.GetTempPath(), "pcbooster-tests-" + Guid.NewGuid().ToString("N"));

    protected BaseTemporal()
    {
        Contrasenas.Iteraciones = 1000; // el hash real tarda ~100 ms por usuario
        Ruta = Path.Combine(carpeta, "PCbooster.db");
    }

    protected string Ruta { get; }

    protected void CrearBase(bool conDemo = false) => InicializadorBase.Inicializar(Ruta, conDemo);

    /* Un contexto nuevo por llamada, como en un request de la app. Crea la base si no existe. */
    protected TiendaContext Contexto()
    {
        if (!File.Exists(Ruta))
        {
            CrearBase();
        }
        return new TiendaContext(TiendaContext.Opciones(Ruta));
    }

    protected object? Escalar(string sql)
    {
        using var cn = new SqliteConnection(InicializadorBase.CadenaConexion(Ruta));
        cn.Open();
        using SqliteCommand cmd = cn.CreateCommand();
        cmd.CommandText = sql;
        return cmd.ExecuteScalar();
    }

    protected void Ejecutar(string sql) => Escalar(sql);

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try { Directory.Delete(carpeta, true); } catch (IOException) { }
        GC.SuppressFinalize(this);
    }
}
