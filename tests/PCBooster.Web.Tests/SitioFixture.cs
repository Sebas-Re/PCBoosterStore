using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using PCBooster.Datos;

namespace PCBooster.Web.Tests;

/* El sitio real en memoria (TestServer), en entorno Production y con una base
   temporal con los datos de prueba y el historial de demo. Se comparte entre
   todos los tests: cada test que cambia datos usa un cliente nuevo. */
public class SitioFixture : WebApplicationFactory<Program>
{
    readonly string carpeta = Path.Combine(Path.GetTempPath(), "pcbooster-web-" + Guid.NewGuid().ToString("N"));

    public SitioFixture()
    {
        Contrasenas.Iteraciones = 1000; // el hash real tarda ~100 ms por usuario
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Production");
        builder.UseSetting(InicializadorBase.VariableEntorno, Path.Combine(carpeta, "PCbooster.db"));
    }

    public Navegador NuevoNavegador() => new(this);

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        SqliteConnection.ClearAllPools();
        try { Directory.Delete(carpeta, true); } catch (IOException) { }
    }
}

[CollectionDefinition("Sitio")]
public class ColeccionSitio : ICollectionFixture<SitioFixture>;
