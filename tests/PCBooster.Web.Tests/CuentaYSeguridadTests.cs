namespace PCBooster.Web.Tests;

/* Login, registro, permisos y protecciones del sitio. */
public class CuentaYSeguridadTests(SitioFixture sitio) : Pruebas(sitio)
{
    [Fact]
    public async Task LoginInvalidoMuestraElMensajeEnLaPagina()
    {
        Pagina pagina = await IniciarSesion("Nom1", "incorrecta");

        Assert.Equal("/", pagina.Ruta);
        Assert.Contains(pagina.Alertas, a => a.Contains("Usuario o contraseña incorrectos"));
    }

    [Fact]
    public async Task LoginSinDatosMuestraLosErroresDeValidacion()
    {
        Pagina pagina = await IniciarSesion("", "");

        Assert.Contains("Ingresá tu usuario.", pagina.ErroresDeValidacion);
        Assert.Contains("Ingresá tu contraseña.", pagina.ErroresDeValidacion);
    }

    [Fact]
    public async Task DemasiadosIntentosFallidosBloqueanElLogin()
    {
        Pagina pagina = null!;
        for (int i = 0; i < 6; i++)
        {
            pagina = await IniciarSesion("Nom5", "incorrecta");
        }
        Assert.Contains(pagina.Alertas, a => a.Contains("Demasiados intentos fallidos"));

        pagina = await IniciarSesion("Nom5", "ctr5");
        Assert.Equal("/", pagina.Ruta); // sigue bloqueado aunque la contraseña sea correcta
    }

    [Fact]
    public async Task ElRegistroCreaLaCuentaYEntraAlCatalogo()
    {
        var (pagina, nombre) = await RegistrarClienteNuevo();

        Assert.Equal("/Catalogo", pagina.Ruta);
        Assert.Contains(pagina.Alertas, a => a.Contains($"¡Bienvenido/a, {nombre}!"));
    }

    [Fact]
    public async Task ElRegistroValidaEnElServidor()
    {
        Pagina registro = await Navegador.Abrir("/Cuenta/Registro");

        Pagina pagina = await Navegador.Enviar(registro, "#btnRegistrarse", Campos(
            "Datos.Nombre", "Nom1", "Datos.Email", "x@test.com", "Datos.Contraseña", "123", "Datos.RepetirContraseña", "456", "Datos.Telefono", "abc"));

        Assert.Equal("/Cuenta/Registro", pagina.Ruta);
        Assert.Contains("La contraseña tiene que tener entre 6 y 64 caracteres.", pagina.ErroresDeValidacion);
        Assert.Contains("Las contraseñas no coinciden.", pagina.ErroresDeValidacion);
        Assert.Contains("Ingresá solo números.", pagina.ErroresDeValidacion);

        pagina = await Navegador.Enviar(registro, "#btnRegistrarse", Campos(
            "Datos.Nombre", "NOM1", "Datos.Email", "x@test.com", "Datos.Contraseña", "clave1", "Datos.RepetirContraseña", "clave1"));
        Assert.Contains(pagina.Alertas, a => a.Contains("ya está en uso"));
    }

    [Fact]
    public async Task SinSesionLasPaginasPidenLoginYVuelvenDespues()
    {
        Pagina pagina = await Navegador.Abrir("/MisCompras");
        Assert.Equal("/", pagina.Ruta);
        Assert.Contains("ReturnUrl=%2FMisCompras", pagina.Url.Query);

        Pagina despues = await Navegador.Enviar(pagina, "#btnIngresar", Campos("Nombre", "Nom3", "Contraseña", "ctr3"));
        Assert.Equal("/MisCompras", despues.Ruta);
    }

    [Fact]
    public async Task ElLoginNoRedirigeAOtroSitio()
    {
        Pagina inicio = await Navegador.Abrir("/?ReturnUrl=https%3A%2F%2Fsitio-malicioso.com%2F");

        Pagina pagina = await Navegador.Enviar(inicio, "#btnIngresar", Campos("Nombre", "Nom3", "Contraseña", "ctr3"));

        Assert.Equal("localhost", pagina.Url.Host);
        Assert.Equal("/Catalogo", pagina.Ruta);
    }

    [Fact]
    public async Task CerrarSesionSoloFuncionaPorPostConToken()
    {
        await IniciarSesion("Nom3", "ctr3");

        // Un link o un form de otro sitio (sin token) no cierra la sesión.
        await Navegador.Abrir("/Cuenta/Salir");
        Pagina sinToken = await Navegador.Postear(new Uri(Navegador.Direccion, "/Cuenta/Salir"), Array.Empty<KeyValuePair<string, string>>());
        Assert.Equal(400, sinToken.Estado);
        Assert.Equal("/MisCompras", (await Navegador.Abrir("/MisCompras")).Ruta);

        Pagina compras = await Navegador.Abrir("/MisCompras");
        Pagina salida = await Navegador.Enviar(compras, "form[action='/Cuenta/Salir'] button");
        Assert.Equal("/", salida.Ruta);
        Assert.Equal("/", (await Navegador.Abrir("/MisCompras")).Ruta);
    }

    [Fact]
    public async Task UnFormularioSinTokenAntiforgeryEsRechazado()
    {
        // Como en un CSRF: un formulario armado en otro sitio no trae el token.
        Pagina pagina = await Navegador.Postear(new Uri(Navegador.Direccion, "/"), new Dictionary<string, string> { ["Nombre"] = "Nom1", ["Contraseña"] = "ctr1" });

        Assert.Equal(400, pagina.Estado);
        Assert.Null(Navegador.Cookie("PCBooster.Auth"));
    }

    [Fact]
    public async Task LaCookieDeSesionCambiaEnCadaLogin()
    {
        await IniciarSesion("Nom3", "ctr3");
        string primera = Navegador.Cookie("PCBooster.Auth")!.Value;

        using var otro = Sitio.NuevoNavegador();
        await IniciarSesion("Nom3", "ctr3", otro);

        Assert.NotEqual(primera, otro.Cookie("PCBooster.Auth")!.Value);
    }

    [Fact]
    public async Task LasRespuestasTraenLosHeadersDeSeguridadYCookiesProtegidas()
    {
        await IniciarSesion("Nom3", "ctr3");
        using HttpResponseMessage respuesta = await Navegador.Pedir("/MisCompras");
        string? Header(string nombre) => respuesta.Headers.TryGetValues(nombre, out var valores) ? string.Join(",", valores) : null;

        Assert.Equal("nosniff", Header("X-Content-Type-Options"));
        Assert.Equal("DENY", Header("X-Frame-Options"));
        string csp = Header("Content-Security-Policy")!;
        Assert.Contains("script-src 'self';", csp); // sin 'unsafe-inline'
        Assert.Contains("frame-ancestors 'none'", csp);
        Assert.Null(Header("Server"));
        Assert.Null(Header("X-Powered-By"));
        // HSTS está activo en producción, pero ASP.NET Core no lo manda para localhost.
        Assert.True(Navegador.Cookie("PCBooster.Auth")!.HttpOnly);
        Assert.True(Navegador.Cookie("PCBooster.Auth")!.Secure);
    }

    [Fact]
    public async Task LasPaginasNoTienenScriptsInline()
    {
        await IniciarSesion("admAmador", "admpsw2");

        foreach (string ruta in new[] { "/", "/Cuenta/Registro" })
        {
            Pagina pagina = await Sitio.NuevoNavegador().Abrir(ruta);
            Assert.All(pagina.Documento.Scripts, s => Assert.False(string.IsNullOrEmpty(s.Source), $"script inline en {ruta}"));
            Assert.DoesNotContain(" onerror=", pagina.Html);
            Assert.DoesNotContain(" onclick=", pagina.Html);
        }
    }
}
