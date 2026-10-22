namespace PCBooster.Web.Tests;

/* Flujos del cliente: catálogo, carrito, compra, historial y cuenta. */
public class TiendaTests(SitioFixture sitio) : Pruebas(sitio)
{
    async Task<Pagina> Agregar(string rutaCatalogo, int cantidadProductos)
    {
        Pagina catalogo = await Navegador.Abrir(rutaCatalogo);
        for (int i = 0; i < cantidadProductos; i++)
        {
            var botones = catalogo.Documento.QuerySelectorAll(".producto button[title='Agregar al carrito']:not([disabled])");
            catalogo = await Navegador.Enviar(catalogo, botones[i]);
            Assert.Contains(catalogo.Alertas, a => a.StartsWith("Agregaste"));
        }
        return catalogo;
    }

    async Task<Pagina> GuardarTarjeta(Pagina carrito) =>
        await Navegador.Enviar(carrito, "#btnGuardarTarjeta", Campos(
            "Tarjeta.Numero", "4567123412340012", "Tarjeta.Titular", "Cliente E2E", "Tarjeta.Nombre", "Visa",
            "Tarjeta.EsCredito", "true", "Tarjeta.Mes", "12", "Tarjeta.Año", "2035"));

    [Fact]
    public async Task FlujoCompletoDeCompra()
    {
        await RegistrarClienteNuevo();
        Pagina catalogo = await Agregar("/Catalogo?cat=2", 2);
        Assert.Equal("2", catalogo.Elemento("#cantidadCarrito").TextContent.Trim());

        Pagina carrito = await Navegador.Abrir("/Carrito");
        Assert.Contains("Gabinete Gamemax", carrito.Texto);

        carrito = await GuardarTarjeta(carrito);
        Assert.Contains(carrito.Alertas, a => a.StartsWith("Tarjeta guardada"));
        Assert.Contains("Visa terminada en 0012 (Crédito)", carrito.Texto);
        Assert.DoesNotContain("4567123412340012", carrito.Html); // el número completo no llega al navegador

        Pagina compras = await Navegador.Enviar(carrito, "#btnConfirmar", Campos("Compra.Direccion", "Av. Siempreviva 742", "Compra.Contacto", "Homero"));

        Assert.Equal("/MisCompras", compras.Ruta);
        Assert.Contains(compras.Alertas, a => a.Contains("Registramos tu compra"));
        Assert.Contains("Gabinete Gamemax", compras.Elemento("#detalleCompra").TextContent);
        Assert.Contains("Av. Siempreviva 742", compras.Texto);
        Assert.Contains("Tu carrito está vacío", (await Navegador.Abrir("/Carrito")).Texto);

        Pagina cuenta = await Navegador.Abrir("/MiCuenta");
        cuenta = await Navegador.Enviar(cuenta, "button[title='Eliminar tarjeta']");
        Assert.Contains(cuenta.Alertas, a => a.Contains("Eliminaste la tarjeta"));
        Assert.Null(cuenta.Documento.QuerySelector("#tarjetas"));
    }

    [Fact]
    public async Task ConfirmarSinDatosValidaEnElServidorYNoRegistraLaCompra()
    {
        await RegistrarClienteNuevo();
        await Agregar("/Catalogo?cat=8", 1);
        Pagina carrito = await Navegador.Abrir("/Carrito");

        Pagina pagina = await Navegador.Enviar(carrito, "#btnConfirmar", Campos("Compra.Direccion", "", "Compra.Contacto", ""));

        Assert.Equal("/Carrito", pagina.Ruta);
        Assert.Contains("Elegí una tarjeta.", pagina.ErroresDeValidacion);
        Assert.Contains("Ingresá la dirección de entrega.", pagina.ErroresDeValidacion);
        Assert.Contains("Todavía no realizaste ninguna compra", (await Navegador.Abrir("/MisCompras")).Texto);
    }

    [Fact]
    public async Task NoSePuedePagarConLaTarjetaDeOtroCliente()
    {
        await RegistrarClienteNuevo();
        await Agregar("/Catalogo?cat=8", 1);
        Pagina carrito = await Navegador.Abrir("/Carrito");

        // Se manipula el select para mandar un id de tarjeta que no es del usuario.
        Pagina pagina = await Navegador.Enviar(carrito, "#btnConfirmar", Campos("Compra.TarjetaId", "1", "Compra.Direccion", "Calle 1", "Compra.Contacto", "Yo"));

        Assert.Contains(pagina.Alertas, a => a.Contains("tarjetas guardadas"));
    }

    [Fact]
    public async Task ElCarritoPermiteQuitarYVaciar()
    {
        await RegistrarClienteNuevo();
        await Agregar("/Catalogo?cat=3", 2);
        Pagina carrito = await Navegador.Abrir("/Carrito");
        Assert.Equal(2, carrito.Documento.QuerySelectorAll("#itemsCarrito tbody tr").Length);

        carrito = await Navegador.Enviar(carrito, "button[title='Quitar del carrito']");
        Assert.Single(carrito.Documento.QuerySelectorAll("#itemsCarrito tbody tr"));

        Pagina catalogo = await Navegador.Enviar(carrito, "#btnVaciar");
        Assert.Equal("/Catalogo", catalogo.Ruta);
        Assert.Contains(catalogo.Alertas, a => a.Contains("Vaciaste el carrito"));
    }

    [Fact]
    public async Task ElModalAgregaLaCantidadElegidaYRespetaElStock()
    {
        await RegistrarClienteNuevo();
        Pagina catalogo = await Navegador.Abrir("/Catalogo?q=Ryzen 7");
        var modal = catalogo.Elemento("#detalle-ps2");
        Assert.Contains("Especificaciones", modal.TextContent);
        Assert.Contains("Socket", modal.TextContent);

        catalogo = await Navegador.Enviar(catalogo, "#detalle-ps2 button[title='Agregar la cantidad elegida']", Campos("cantidad", "3"));
        Assert.Contains(catalogo.Alertas, a => a.StartsWith("Agregaste 3 unidades"));
        Assert.Equal("Ryzen 7", Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(catalogo.Url.Query)["q"]); // vuelve a la misma búsqueda

        // Una cantidad enorme se limita al stock que queda.
        int stock = int.Parse(catalogo.Elemento("#detalle-ps2 input[name='cantidad']").GetAttribute("max")!);
        catalogo = await Navegador.Enviar(catalogo, "#detalle-ps2 button[title='Agregar la cantidad elegida']", Campos("cantidad", "100000"));
        Assert.Contains(catalogo.Alertas, a => a.StartsWith("Solo había stock para agregar"));
        Assert.Equal(stock.ToString(), catalogo.Elemento("#cantidadCarrito").TextContent.Trim()); // todo el stock, ni una más
    }

    [Fact]
    public async Task ElCatalogoFiltraBuscaYPagina()
    {
        await IniciarSesion("Nom3", "ctr3");

        Pagina procesadores = await Navegador.Abrir("/Catalogo?cat=7");
        Assert.Equal(9, procesadores.Documento.QuerySelectorAll(".producto").Length);
        Assert.Equal("Procesador", procesadores.Elemento(".nav-pills .active").TextContent.Trim());

        Pagina busqueda = await Navegador.Abrir("/Catalogo?q=ryzen");
        Assert.Contains("Resultados para \"ryzen\": 2", busqueda.Elemento("#resultado").TextContent);

        Pagina segunda = await Navegador.Abrir("/Catalogo?pagina=2");
        Assert.Equal(12, segunda.Documento.QuerySelectorAll(".producto").Length);
        Assert.Equal("2", segunda.Elemento(".pagination .active").TextContent.Trim());
    }

    [Fact]
    public async Task LaBusquedaSeMuestraEncodeada()
    {
        await IniciarSesion("Nom3", "ctr3");

        Pagina pagina = await Navegador.Abrir("/Catalogo?q=" + Uri.EscapeDataString("<script>alert(1)</script>"));

        Assert.DoesNotContain("<script>alert(1)</script>", pagina.Html);
        Assert.Contains("&lt;script&gt;", pagina.Html);
    }

    [Fact]
    public async Task UnClienteNoVeElDetalleDeComprasAjenas()
    {
        await IniciarSesion("Nom3", "ctr3");

        // Las compras del historial de demo son de otros clientes.
        Pagina pagina = await Navegador.Abrir("/MisCompras?compra=1");

        Assert.DoesNotContain("Detalle de la compra N° 1", pagina.Texto);
    }

    [Fact]
    public async Task CambiarElEmailPideLaContraseñaActual()
    {
        await RegistrarClienteNuevo();
        Pagina cuenta = await Navegador.Abrir("/MiCuenta");
        string email = "cambiado" + Random.Shared.Next(100000, 999999) + "@test.com";

        Pagina sinClave = await Navegador.Enviar(cuenta, "#btnGuardar", Campos("Datos.Email", email));
        Assert.Contains(sinClave.Alertas, a => a.Contains("ingresá tu contraseña actual"));

        Pagina conClave = await Navegador.Enviar(cuenta, "#btnGuardar", Campos("Datos.Email", email, "Datos.ContraseñaActual", "clave1"));
        Assert.Contains(conClave.Alertas, a => a.Contains("Guardamos tus cambios"));
        Assert.Equal(email, conClave.Elemento("#Datos_Email").GetAttribute("value"));
    }

    [Fact]
    public async Task UnFormularioDeOtraSesionEsRechazado()
    {
        Pagina formularioAjeno;
        using (var atacante = Sitio.NuevoNavegador())
        {
            await IniciarSesion("Nom2", "ctr2", atacante);
            formularioAjeno = await atacante.Abrir("/MiCuenta");
        }
        await IniciarSesion("Nom4", "ctr4");

        // Como en un CSRF: la víctima postea un formulario (con token) que generó otra sesión.
        Pagina pagina = await Navegador.Enviar(formularioAjeno, "#btnGuardar", Campos("Datos.Telefono", "999"));

        Assert.Equal(400, pagina.Estado);
    }
}
