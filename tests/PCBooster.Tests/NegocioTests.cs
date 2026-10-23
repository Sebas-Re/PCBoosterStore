using Microsoft.EntityFrameworkCore;
using PCBooster.Datos;
using PCBooster.Dominio;
using PCBooster.Negocio;

namespace PCBooster.Tests;

/* Reglas de negocio contra una base temporal con los datos de prueba. Nom1 es
   el usuario 5, Nom2 el 6, Nom4 el 8. */
public class NegocioTests : BaseTemporal
{
    UsuariosService Usuarios() => new(Contexto());
    CatalogoService Catalogo() => new(Contexto());
    TarjetasService Tarjetas() => new(Contexto());
    ComprasService Compras() => new(Contexto());

    /* Id de una tarjeta vigente del usuario; la guarda si todavía no tiene. */
    async Task<int> TarjetaDe(int usuarioId)
    {
        var tarjetas = await Tarjetas().ListarAsync(usuarioId);
        if (tarjetas.Count == 0)
        {
            Assert.Null(await Tarjetas().AgregarAsync(new Tarjeta
            {
                UsuarioId = usuarioId, Titular = "Titular", Nombre = "Visa", EsCredito = true,
                Numero = 4567123412340000 + usuarioId, Vencimiento = new DateOnly(2099, 12, 1)
            }));
            tarjetas = await Tarjetas().ListarAsync(usuarioId);
        }
        return tarjetas[0].Id;
    }

    static Carrito CarritoConDosArticulos()
    {
        var carrito = new Carrito();
        carrito.Agregar("gb1", "Gabinete Gamemax", 3500m, 10);
        carrito.Agregar("gb1", "Gabinete Gamemax", 3500m, 10);
        carrito.Agregar("fnt1", "Fuente Deepcool 700W", 7300m, 100);
        return carrito;
    }

    async Task<int> Stock(string codigo) => (await Catalogo().ObtenerAsync(codigo))!.Stock;

    // ------------------------------------------------------------------------
    // Usuarios
    // ------------------------------------------------------------------------
    [Fact]
    public async Task LoginDevuelveElUsuarioConSuTipo()
    {
        Usuario? admin = await Usuarios().LoginAsync("admAmador", "admpsw2");
        Usuario? cliente = await Usuarios().LoginAsync("nom1", "ctr1"); // nombre sin distinguir mayúsculas

        Assert.Equal(1, admin!.Id);
        Assert.True(admin.EsAdmin);
        Assert.Equal(5, cliente!.Id);
        Assert.False(cliente.EsAdmin);
    }

    [Fact]
    public async Task LoginConCredencialesInvalidasDevuelveNull()
    {
        Assert.Null(await Usuarios().LoginAsync("Nom1", "otra"));
        Assert.Null(await Usuarios().LoginAsync("noexiste", "ctr1"));
        Assert.Null(await Usuarios().LoginAsync("Nom1", "' or '1'='1"));
        Assert.Null(await Usuarios().LoginAsync("' or 1=1 --", "x"));
    }

    [Fact]
    public async Task BloqueaElLoginDespuesDeVariosIntentosFallidos()
    {
        var usuarios = Usuarios();
        try
        {
            for (int i = 0; i < UsuariosService.MaxIntentosFallidos; i++)
            {
                Assert.Null(await usuarios.LoginAsync("Nom2", "incorrecta"));
            }

            Assert.True(usuarios.LoginBloqueado("NOM2"));
            Assert.Null(await usuarios.LoginAsync("Nom2", "ctr2")); // ni con la contraseña correcta
            Assert.False(usuarios.LoginBloqueado("Nom3"));

            UsuariosService.DuracionBloqueo = TimeSpan.Zero; // pasó el tiempo de bloqueo
            Assert.NotNull(await usuarios.LoginAsync("Nom2", "ctr2"));
        }
        finally
        {
            UsuariosService.DuracionBloqueo = TimeSpan.FromMinutes(5);
        }
    }

    [Fact]
    public async Task UnLoginCorrectoReiniciaLosIntentosFallidos()
    {
        var usuarios = Usuarios();
        for (int i = 0; i < UsuariosService.MaxIntentosFallidos - 1; i++)
        {
            await usuarios.LoginAsync("Nom3", "incorrecta");
        }
        Assert.NotNull(await usuarios.LoginAsync("Nom3", "ctr3"));
        await usuarios.LoginAsync("Nom3", "incorrecta");

        Assert.False(usuarios.LoginBloqueado("Nom3"));
        Assert.NotNull(await usuarios.LoginAsync("Nom3", "ctr3"));
    }

    [Fact]
    public async Task UnUsuarioDesactivadoNoPuedeIniciarSesion()
    {
        Assert.True(await Usuarios().DesactivarAsync(5));

        Assert.Null(await Usuarios().LoginAsync("Nom1", "ctr1"));
    }

    [Fact]
    public async Task RegistraUnClienteSinRepetirNombreNiEmail()
    {
        Assert.Null(await Usuarios().RegistrarAsync("nuevo", "n@mail.com", "", "clave1"));

        Usuario? nuevo = await Usuarios().LoginAsync("nuevo", "clave1");
        Assert.NotNull(nuevo);
        Assert.False(nuevo.EsAdmin);
        Assert.Null(nuevo.Telefono);
        Assert.True(nuevo.FechaAlta > DateTime.Now.AddMinutes(-1));
        Assert.Contains("nombre de usuario", await Usuarios().RegistrarAsync("NUEVO", "otro@mail.com", "", "clave1"));
        Assert.Contains("e-mail", await Usuarios().RegistrarAsync("otro", "N@MAIL.COM", "", "clave1"));
    }

    [Fact]
    public async Task ExigeUnLargoMinimoDeContraseña()
    {
        Assert.Contains("entre 6 y 64", await Usuarios().RegistrarAsync("corta", "c@mail.com", "", "12345"));
        Assert.Null(await Usuarios().LoginAsync("corta", "12345"));
        Assert.Contains("entre 6 y 64", await Usuarios().ModificarAsync(5, "Nom1", "ema1", "", false, true, "123"));
        Assert.Null(await Usuarios().ModificarAsync(5, "Nom1", "ema1", "", false, true, "")); // vacía = conservar
        Assert.NotNull(await Usuarios().LoginAsync("Nom1", "ctr1"));
    }

    [Fact]
    public async Task CambiarNombreOEmailExigeLaContraseñaActual()
    {
        Assert.Contains("contraseña actual", await Usuarios().ActualizarDatosPropiosAsync(8, "Nom4", "nuevo@mail.com", "1", null, ""));
        Assert.Contains("contraseña actual", await Usuarios().ActualizarDatosPropiosAsync(8, "Intruso", "ema4", "1", null, "incorrecta"));
        Assert.Equal("Nom4", (await Usuarios().ObtenerAsync(8))!.Nombre);

        Assert.Null(await Usuarios().ActualizarDatosPropiosAsync(8, "Nom4b", "nuevo@mail.com", "1", null, "ctr4"));
        Usuario guardado = (await Usuarios().ObtenerAsync(8))!;
        Assert.Equal("Nom4b", guardado.Nombre);
        Assert.Equal("nuevo@mail.com", guardado.Email);
        Assert.False(guardado.EsAdmin);
        Assert.True(guardado.Activo);
    }

    [Fact]
    public async Task CambiarSoloElTelefonoNoPideContraseña()
    {
        Assert.Null(await Usuarios().ActualizarDatosPropiosAsync(8, "Nom4", "ema4", "1199887766", null, null));
        Assert.Equal("1199887766", (await Usuarios().ObtenerAsync(8))!.Telefono);
    }

    [Fact]
    public async Task CambiarLaContraseñaExigeLaActual()
    {
        Assert.NotNull(await Usuarios().ActualizarDatosPropiosAsync(8, "Nom4", "ema4", "1", "otra123", ""));
        Assert.NotNull(await Usuarios().LoginAsync("Nom4", "ctr4"));

        Assert.Null(await Usuarios().ActualizarDatosPropiosAsync(8, "Nom4", "ema4", "1", "otra123", "ctr4"));
        Assert.Null(await Usuarios().LoginAsync("Nom4", "ctr4"));
        Assert.NotNull(await Usuarios().LoginAsync("Nom4", "otra123"));
    }

    [Fact]
    public async Task LosErroresDeContraseñaActualCuentanParaElBloqueo()
    {
        for (int i = 0; i < UsuariosService.MaxIntentosFallidos; i++)
        {
            await Usuarios().ActualizarDatosPropiosAsync(8, "Nom4", "robado@mail.com", "1", null, "incorrecta");
        }

        Assert.Contains("Demasiados intentos", await Usuarios().ActualizarDatosPropiosAsync(8, "Nom4", "robado@mail.com", "1", null, "ctr4"));
        Assert.True(Usuarios().LoginBloqueado("Nom4"));
        Assert.Equal("ema4", (await Usuarios().ObtenerAsync(8))!.Email);

        UsuariosService.DuracionBloqueo = TimeSpan.Zero;
        try
        {
            Assert.NotNull(await Usuarios().LoginAsync("Nom4", "ctr4")); // reinicia la cuenta de intentos
        }
        finally
        {
            UsuariosService.DuracionBloqueo = TimeSpan.FromMinutes(5);
        }
    }

    [Fact]
    public async Task ElAdminModificaTipoYEstado()
    {
        Assert.Null(await Usuarios().ModificarAsync(6, "Nom2", "ema2", "", esAdmin: true, activo: false, nuevaContraseña: null));

        Usuario nom2 = (await Usuarios().ObtenerAsync(6))!;
        Assert.True(nom2.EsAdmin);
        Assert.False(nom2.Activo);
        Assert.Contains("otra cuenta", await Usuarios().ModificarAsync(6, "Nom1", "ema2", "", false, true, null));
    }

    [Fact]
    public async Task ElAdminBuscaUsuariosPorNombreEmailOId()
    {
        Assert.Equal(["admAmador", "admDiamela", "admNicolas", "admSebasti"], (await Usuarios().BuscarAsync(null, "ADM", 1, 10)).Items.Select(u => u.Nombre));
        Assert.Equal("Nom1", (await Usuarios().BuscarAsync(null, "ema1", 1, 10)).Items.Single().Nombre);
        Assert.Equal("Nom2", (await Usuarios().BuscarAsync(6, null, 1, 10)).Items.Single().Nombre);

        Pagina<Usuario> segunda = await Usuarios().BuscarAsync(null, null, 2, 5);
        Assert.Equal(9, segunda.Total);
        Assert.Equal([6, 7, 8, 9], segunda.Items.Select(u => u.Id));
    }

    // ------------------------------------------------------------------------
    // Catálogo
    // ------------------------------------------------------------------------
    [Fact]
    public async Task ElCatalogoSoloMuestraArticulosActivosDeCategoriasActivas()
    {
        Assert.True(await Catalogo().DarDeBajaArticuloAsync("gb1"));
        Assert.True(await Catalogo().DesactivarCategoriaAsync(1)); // Fuente: 9 artículos

        Pagina<Articulo> todo = await Catalogo().BuscarAsync(null, null, 1, 100);
        Assert.Equal(68 - 1 - 9, todo.Total);
        Assert.DoesNotContain(todo.Items, a => a.Codigo == "gb1" || a.CategoriaId == 1);
        Assert.Equal(8, (await Catalogo().BuscarAsync(null, 2, 1, 100)).Total);
        Assert.Equal(0, (await Catalogo().BuscarAsync(null, 1, 1, 100)).Total);
        Assert.DoesNotContain(await Catalogo().CategoriasActivasAsync(), c => c.Id == 1);
    }

    [Fact]
    public async Task BuscaSinDistinguirMayusculasYPagina()
    {
        Assert.Equal(2, (await Catalogo().BuscarAsync("RYZEN", null, 1, 12)).Total);
        Assert.Equal(0, (await Catalogo().BuscarAsync("%", null, 1, 12)).Total); // el comodín se busca literal

        Pagina<Articulo> segunda = await Catalogo().BuscarAsync(null, null, 2, 12);
        Assert.Equal(12, segunda.Items.Count);
        Assert.Equal(6, segunda.TotalPaginas);
        Assert.Equal(6, (await Catalogo().BuscarAsync(null, null, 99, 12)).Numero); // fuera de rango: la última
    }

    [Fact]
    public async Task LaAdministracionListaTambienLosArticulosDadosDeBaja()
    {
        await Catalogo().DarDeBajaArticuloAsync("gb1");

        Assert.Equal(68, (await Catalogo().ListarArticulosAsync(null, 1, 100)).Total);
        Assert.Equal("gb1", (await Catalogo().ListarArticulosAsync("GB1", 1, 10)).Items.Single().Codigo); // código exacto
        Assert.Equal(9, (await Catalogo().ListarArticulosAsync("gabinete", 1, 10)).Total); // parte de la descripción
    }

    Articulo NuevoArticulo(string codigo, string imagen = "/img/productos/gb1.png") => new()
    {
        Codigo = codigo, CategoriaId = 2, Descripcion = "Nuevo " + codigo, Stock = 1, Precio = 100, Imagen = imagen
    };

    [Fact]
    public async Task AltaYModificacionDeArticulos()
    {
        Assert.Contains("Ya existe un artículo", await Catalogo().AgregarArticuloAsync(NuevoArticulo("gb1")));
        Assert.Null(await Catalogo().AgregarArticuloAsync(NuevoArticulo("gb99")));

        Articulo cambios = NuevoArticulo("gb99");
        cambios.CategoriaId = 8;
        cambios.Precio = 1500.5m;
        cambios.Activo = false;
        Assert.Null(await Catalogo().ModificarArticuloAsync(cambios));
        Articulo guardado = (await Catalogo().ObtenerAsync("GB99"))!;
        Assert.Equal("Ssd", guardado.Categoria!.Descripcion);
        Assert.Equal(1500.5m, guardado.Precio);
        Assert.False(guardado.Activo);

        cambios.Descripcion = (await Catalogo().ObtenerAsync("gb1"))!.Descripcion;
        Assert.Contains("otro artículo con esa descripción", await Catalogo().ModificarArticuloAsync(cambios));
    }

    [Theory]
    [InlineData("/img/productos/gb1.png", true)]
    [InlineData("https://cdn.ejemplo.com/a.png", true)]
    [InlineData("javascript:alert(1)", false)]
    [InlineData("data:image/svg+xml,<svg onload=alert(1)>", false)]
    [InlineData("//otro-sitio.com/a.png", false)]
    [InlineData("", false)]
    public void SoloAceptaImagenesDelSitioOHttp(string url, bool valida)
    {
        Assert.Equal(valida, CatalogoService.ImagenValida(url));
    }

    [Fact]
    public async Task RechazaArticulosConCodigoOImagenInvalidos()
    {
        Assert.NotNull(await Catalogo().AgregarArticuloAsync(NuevoArticulo("x'><")));
        Assert.Equal(CatalogoService.ErrorImagen, await Catalogo().AgregarArticuloAsync(NuevoArticulo("ok1", "javascript:alert(1)")));
        Assert.Equal(CatalogoService.ErrorImagen, await Catalogo().ModificarArticuloAsync(NuevoArticulo("gb1", "' onerror='alert(1)")));
    }

    [Fact]
    public async Task CategoriasAltaModificacionYBajaLogica()
    {
        Assert.Null(await Catalogo().AgregarCategoriaAsync("Monitores"));
        Assert.NotNull(await Catalogo().AgregarCategoriaAsync("MONITORES"));
        Categoria monitores = (await Catalogo().CategoriasAsync()).Single(c => c.Descripcion == "Monitores");

        Assert.Null(await Catalogo().ModificarCategoriaAsync(monitores.Id, "Pantallas", true));
        Assert.True(await Catalogo().DesactivarCategoriaAsync(monitores.Id));
        Assert.False((await Catalogo().CategoriasAsync()).Single(c => c.Id == monitores.Id).Activa);
        Assert.Contains("Ya existe", await Catalogo().ModificarCategoriaAsync(monitores.Id, "Fuente", true));
    }

    // ------------------------------------------------------------------------
    // Tarjetas
    // ------------------------------------------------------------------------
    [Fact]
    public async Task AgregarTarjetaRechazaVencidasYRepetidas()
    {
        Tarjeta Tarjeta(DateOnly vencimiento) => new()
        {
            UsuarioId = 5, Titular = "Nom Uno", Nombre = "Visa", EsCredito = true, Numero = 4567123412340012, Vencimiento = vencimiento
        };

        Assert.Contains("vencida", await Tarjetas().AgregarAsync(Tarjeta(new DateOnly(2020, 1, 1))));
        Assert.Null(await Tarjetas().AgregarAsync(Tarjeta(new DateOnly(2099, 12, 1))));
        Assert.Contains("Ya tenías", await Tarjetas().AgregarAsync(Tarjeta(new DateOnly(2099, 12, 1))));
        Assert.Single(await Tarjetas().ListarAsync(5));
    }

    [Fact]
    public async Task SoloSePuedeEliminarUnaTarjetaPropia()
    {
        int id = await TarjetaDe(5);

        Assert.False(await Tarjetas().EliminarAsync(6, id));
        Assert.True(await Tarjetas().EliminarAsync(5, id));
        Assert.Empty(await Tarjetas().ListarAsync(5));
    }

    // ------------------------------------------------------------------------
    // Compras
    // ------------------------------------------------------------------------
    [Fact]
    public async Task ConfirmarCompraRegistraVentaDetallesYDescuentaStock()
    {
        ResultadoCompra resultado = await Compras().ConfirmarAsync(5, CarritoConDosArticulos(), "Calle 123", "1122334455", await TarjetaDe(5));

        Assert.True(resultado.Ok, resultado.Error);
        Venta venta = (await Compras().DetalleAsync(resultado.IdVenta, 5))!;
        Assert.Equal(14300m, venta.Total);
        Assert.Equal("Calle 123", venta.DireccionEntrega);
        Assert.True(venta.PagoConCredito); // tomado de la tarjeta
        Assert.Equal(2, venta.Detalles.Count);
        Assert.Equal(8, await Stock("gb1"));
        Assert.Single(await Compras().ComprasDeAsync(5));
    }

    [Fact]
    public async Task CadaCompraConservaSuPropioTotalYSuId()
    {
        ResultadoCompra primera = await Compras().ConfirmarAsync(5, CarritoConDosArticulos(), "Calle 123", "11", await TarjetaDe(5));
        var carrito = new Carrito();
        carrito.Agregar("gb2", "Gabinete Aerocool", 4750m, 100);
        ResultadoCompra segunda = await Compras().ConfirmarAsync(6, carrito, "Calle 9", "22", await TarjetaDe(6));

        Assert.Equal(primera.IdVenta + 1, segunda.IdVenta);
        Assert.Equal(14300m, (await Compras().DetalleAsync(primera.IdVenta))!.Total);
        Venta deNom2 = (await Compras().DetalleAsync(segunda.IdVenta))!;
        Assert.Equal(4750m, deNom2.Total);
        Assert.Equal(6, deNom2.UsuarioId);
    }

    [Fact]
    public async Task SiFaltaStockDeUnArticuloNoSeRegistraNadaDeLaCompra()
    {
        int stockFuente = await Stock("fnt1"), stockGabinete = await Stock("gb1");
        // El carrito se armó cuando había más stock (otro cliente compró después).
        var carrito = new Carrito();
        carrito.Agregar("fnt1", "Fuente Deepcool 700W", 7300m, 1000);
        for (int i = 0; i <= stockGabinete; i++)
        {
            carrito.Agregar("gb1", "Gabinete Gamemax", 3500m, 1000);
        }

        ResultadoCompra resultado = await Compras().ConfirmarAsync(5, carrito, "Calle 123", "11", await TarjetaDe(5));

        Assert.Equal(0, resultado.IdVenta);
        Assert.Contains($"Solo quedan {stockGabinete} unidades", resultado.Error);
        Assert.Empty(await Compras().ComprasDeAsync(5));
        Assert.Equal(stockFuente, await Stock("fnt1"));
    }

    [Fact]
    public async Task NoSePuedeComprarUnArticuloDadoDeBaja()
    {
        await Catalogo().DarDeBajaArticuloAsync("fnt1");

        ResultadoCompra resultado = await Compras().ConfirmarAsync(5, CarritoConDosArticulos(), "Calle 123", "11", await TarjetaDe(5));

        Assert.Contains("ya no está disponible", resultado.Error);
        Assert.Empty(await Compras().ComprasDeAsync(5));
    }

    [Fact]
    public async Task ConfirmarCompraSoloAceptaUnaTarjetaVigenteDelUsuario()
    {
        int deOtroCliente = await TarjetaDe(6);

        Assert.Contains("tarjetas guardadas", (await Compras().ConfirmarAsync(5, CarritoConDosArticulos(), "Calle 1", "11", deOtroCliente)).Error);
        Assert.Contains("tarjetas guardadas", (await Compras().ConfirmarAsync(5, CarritoConDosArticulos(), "Calle 1", "11", 0)).Error);
        Assert.Empty(await Compras().ComprasDeAsync(5));
    }

    [Fact]
    public async Task ConfirmarCompraValidaCarritoYDatosDeEntrega()
    {
        int tarjeta = await TarjetaDe(5);

        Assert.Contains("vacío", (await Compras().ConfirmarAsync(5, new Carrito(), "Calle 123", "11", tarjeta)).Error);
        Assert.Contains("dirección", (await Compras().ConfirmarAsync(5, CarritoConDosArticulos(), " ", "11", tarjeta)).Error);
        Assert.Empty(await Compras().ComprasDeAsync(5));
    }

    [Fact]
    public async Task MientrasUnaTransaccionEstaAbiertaOtraConexionNoPuedeEscribir()
    {
        // ConfirmarAsync verifica el stock y después lo descuenta: eso es seguro solo si
        // la transacción toma el lock de escritura al empezar (BEGIN IMMEDIATE).
        await using TiendaContext compra = Contexto();
        await using var transaccion = await compra.Database.BeginTransactionAsync();

        var cadena = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder(TiendaContext.CadenaConexion(Ruta)) { DefaultTimeout = 1 };
        await using var otra = new TiendaContext(new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<TiendaContext>().UseSqlite(cadena.ToString()).Options);
        otra.Database.SetCommandTimeout(1);

        await Assert.ThrowsAsync<Microsoft.Data.Sqlite.SqliteException>(() =>
            otra.Articulos.Where(a => a.Codigo == "gb1").ExecuteUpdateAsync(s => s.SetProperty(a => a.Stock, 0)));
    }

    [Fact]
    public async Task ElDetalleDeUnaCompraSoloLoVeSuDueño()
    {
        ResultadoCompra compra = await Compras().ConfirmarAsync(5, CarritoConDosArticulos(), "Calle 123", "11", await TarjetaDe(5));

        Assert.NotNull(await Compras().DetalleAsync(compra.IdVenta, 5));
        Assert.Null(await Compras().DetalleAsync(compra.IdVenta, 6));
    }

    [Fact]
    public async Task ElReporteDeArticulosVendidosFiltraYOrdena()
    {
        await Compras().ConfirmarAsync(5, CarritoConDosArticulos(), "Calle 123", "11", await TarjetaDe(5)); // 2 gb1 + 1 fnt1

        ArticuloVendido gabinete = (await Compras().ArticulosVendidosAsync("GB1", OrdenArticulosVendidos.Codigo, 1, 10)).Items.Single();
        Assert.Equal(2, gabinete.Unidades);
        Assert.Equal(7000m, gabinete.Total);
        Assert.Equal(["fnt1", "gb1"], (await Compras().ArticulosVendidosAsync(null, OrdenArticulosVendidos.Codigo, 1, 10)).Items.Select(a => a.Codigo));
        Assert.Equal(["gb1", "fnt1"], (await Compras().ArticulosVendidosAsync(null, OrdenArticulosVendidos.Unidades, 1, 10)).Items.Select(a => a.Codigo));
        Assert.Equal(["fnt1", "gb1"], (await Compras().ArticulosVendidosAsync(null, OrdenArticulosVendidos.Recaudacion, 1, 10)).Items.Select(a => a.Codigo));
    }
}
