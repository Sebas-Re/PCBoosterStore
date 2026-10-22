using Microsoft.Data.Sqlite;

namespace PCBooster.Datos;

/* Crea o actualiza el archivo SQLite al arrancar la app. No usa migraciones de
   EF: el schema viene del sitio original (con triggers y datos de prueba) y su
   versión se guarda en PRAGMA user_version, así una base vieja se actualiza
   con los scripts pendientes sin perder datos. */
public static class InicializadorBase
{
    public const string VariableEntorno = "PCBOOSTER_DB";
    const string NombreArchivo = "PCbooster.db";

    /* Versión del schema. Una base nueva nace en esta versión. */
    public const int VersionSchema = 5;

    /* Scripts que llevan una base de la versión N-1 a la N (índice 0 = versión 1). */
    static readonly string[][] Migraciones =
    {
        new[] { "migracion_1.sql" },
        new[] { "migracion_2.sql", "articulos_detalle.sql" },
        new[] { "migracion_3.sql" },
        Array.Empty<string>(), // versión 4: hash de contraseñas (HashearContraseñas, no se puede en SQL)
        new[] { "migracion_5.sql" },
    };

    /* PCBOOSTER_DB si está definida (la usan los tests); si no, App_Data. */
    public static string RutaPorDefecto(string carpetaBase)
    {
        string? desdeEntorno = Environment.GetEnvironmentVariable(VariableEntorno);
        return string.IsNullOrWhiteSpace(desdeEntorno)
            ? Path.Combine(carpetaBase, "App_Data", NombreArchivo)
            : desdeEntorno;
    }

    public static string CadenaConexion(string ruta) =>
        new SqliteConnectionStringBuilder { DataSource = ruta, ForeignKeys = true }.ToString();

    /* Si el archivo no existe lo crea con el schema y los datos de prueba (y el
       historial de demo si "cargarDemo"); si existe, aplica las migraciones pendientes. */
    public static void Inicializar(string ruta, bool cargarDemo = true)
    {
        if (File.Exists(ruta))
        {
            Migrar(ruta);
        }
        else
        {
            Crear(ruta, cargarDemo);
        }
    }

    public static int Version(SqliteConnection cn) => Convert.ToInt32(Escalar(cn, "pragma user_version"));

    static void Migrar(string ruta)
    {
        using var cn = new SqliteConnection(CadenaConexion(ruta));
        cn.Open();
        for (int siguiente = Version(cn) + 1; siguiente <= VersionSchema; siguiente++)
        {
            using SqliteTransaction tx = cn.BeginTransaction();
            foreach (string script in Migraciones[siguiente - 1])
            {
                EjecutarScript(cn, script);
            }
            if (siguiente == 4)
            {
                HashearContraseñas(cn);
            }
            Escalar(cn, "pragma user_version = " + siguiente);
            tx.Commit();
        }
    }

    static void Crear(string ruta, bool cargarDemo)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(ruta))!);
        // Se arma en un temporal y se mueve al final: nunca queda una base a medio crear.
        string temporal = ruta + ".tmp";
        File.Delete(temporal);
        using (var cn = new SqliteConnection(CadenaConexion(temporal)))
        {
            cn.Open();
            using SqliteTransaction tx = cn.BeginTransaction();
            EjecutarScript(cn, "schema.sql");
            EjecutarScript(cn, "seed.sql");
            EjecutarScript(cn, "articulos_detalle.sql");
            if (cargarDemo)
            {
                EjecutarScript(cn, "demo.sql");
            }
            HashearContraseñas(cn);
            Escalar(cn, "pragma user_version = " + VersionSchema);
            tx.Commit();
        }
        SqliteConnection.ClearAllPools();
        File.Move(temporal, ruta);
    }

    /* Los scripts de datos traen las contraseñas en texto plano (así se leen las
       credenciales de prueba); acá se reemplazan por su hash. */
    static void HashearContraseñas(SqliteConnection cn)
    {
        var pendientes = new List<(long Id, string Clave)>();
        using (SqliteCommand cmd = cn.CreateCommand())
        {
            cmd.CommandText = "select IdUsuario_us, Contraseña_us from Usuarios";
            using SqliteDataReader fila = cmd.ExecuteReader();
            while (fila.Read())
            {
                if (!Contrasenas.EstaHasheada(fila.GetString(1)))
                {
                    pendientes.Add((fila.GetInt64(0), fila.GetString(1)));
                }
            }
        }
        foreach (var (id, clave) in pendientes)
        {
            using SqliteCommand cmd = cn.CreateCommand();
            cmd.CommandText = "update Usuarios set Contraseña_us = @Hash where IdUsuario_us = @Id";
            cmd.Parameters.AddWithValue("@Hash", Contrasenas.Hashear(clave));
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
        }
    }

    static object? Escalar(SqliteConnection cn, string sql)
    {
        using SqliteCommand cmd = cn.CreateCommand();
        cmd.CommandText = sql;
        return cmd.ExecuteScalar();
    }

    static void EjecutarScript(SqliteConnection cn, string nombre)
    {
        using Stream recurso = typeof(InicializadorBase).Assembly.GetManifestResourceStream("Sql." + nombre)
            ?? throw new InvalidOperationException("Falta el script embebido " + nombre);
        using var lector = new StreamReader(recurso);
        using SqliteCommand cmd = cn.CreateCommand();
        cmd.CommandText = lector.ReadToEnd();
        cmd.ExecuteNonQuery();
    }
}
