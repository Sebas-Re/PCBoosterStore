namespace PCBooster.Web.Tests;

/* Base de los tests del sitio: un navegador nuevo por test y atajos comunes. */
[Collection("Sitio")]
public abstract class Pruebas(SitioFixture sitio) : IDisposable
{
    static readonly Random azar = new();

    protected SitioFixture Sitio { get; } = sitio;
    protected Navegador Navegador { get; } = sitio.NuevoNavegador();

    protected static Dictionary<string, string> Campos(params string[] nombreYValor)
    {
        var campos = new Dictionary<string, string>();
        for (int i = 0; i < nombreYValor.Length; i += 2)
        {
            campos[nombreYValor[i]] = nombreYValor[i + 1];
        }
        return campos;
    }

    protected async Task<Pagina> IniciarSesion(string usuario, string contraseña, Navegador? navegador = null)
    {
        navegador ??= Navegador;
        Pagina inicio = await navegador.Abrir("/");
        return await navegador.Enviar(inicio, "#btnIngresar", Campos("Nombre", usuario, "Contraseña", contraseña));
    }

    /* Cada test que cambia datos usa un cliente nuevo, para no depender del orden. */
    protected async Task<(Pagina Pagina, string Nombre)> RegistrarClienteNuevo()
    {
        string nombre = "e2e" + azar.Next(100000, 999999);
        Pagina registro = await Navegador.Abrir("/Cuenta/Registro");
        Pagina pagina = await Navegador.Enviar(registro, "#btnRegistrarse", Campos(
            "Datos.Nombre", nombre, "Datos.Email", nombre + "@test.com",
            "Datos.Contraseña", "clave1", "Datos.RepetirContraseña", "clave1", "Datos.Telefono", "1122334455"));
        return (pagina, nombre);
    }

    public void Dispose()
    {
        Navegador.Dispose();
        GC.SuppressFinalize(this);
    }
}
