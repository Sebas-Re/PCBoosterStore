using System.Text.Json;

namespace PCBooster.Web.Tests;

/* Panel, reportes y ABM de la administración. */
public class AdminTests(SitioFixture sitio) : Pruebas(sitio)
{
    static readonly Random azar = new();

    Task<Pagina> EntrarComoAdmin() => IniciarSesion("admAmador", "admpsw2");

    static List<JsonElement> Graficos(Pagina pagina) =>
        pagina.Documento.QuerySelectorAll("canvas[data-grafico]").Select(c => JsonDocument.Parse(c.GetAttribute("data-grafico")!).RootElement).ToList();

    [Fact]
    public async Task UnClienteNoEntraALaAdministracion()
    {
        await IniciarSesion("Nom3", "ctr3");

        foreach (string ruta in new[] { "/Admin", "/Admin/Reportes", "/Admin/Usuarios", "/Admin/Articulos/Editar/gb1" })
        {
            Assert.Equal("/Catalogo", (await Navegador.Abrir(ruta)).Ruta);
        }
    }

    [Fact]
    public async Task ElPanelMuestraIndicadoresGraficosYStockBajo()
    {
        Pagina panel = await EntrarComoAdmin();

        Assert.Equal("/Admin", panel.Ruta);
        Assert.Equal(4, panel.Documento.QuerySelectorAll("#indicadores .indicador").Length);
        List<JsonElement> graficos = Graficos(panel);
        Assert.Equal(3, graficos.Count);
        Assert.Equal(6, graficos[0].GetProperty("etiquetas").GetArrayLength()); // 6 meses
        Assert.True(graficos[0].GetProperty("series")[0].GetProperty("valores").EnumerateArray().Sum(v => v.GetDecimal()) > 0);
        Assert.NotEmpty(panel.Documento.QuerySelectorAll("#stockBajo li"));
    }

    [Fact]
    public async Task LosReportesCambianDePeriodoYFiltranVentas()
    {
        await EntrarComoAdmin();

        Pagina semana = await Navegador.Abrir("/Admin/Reportes?dias=7");
        Assert.Equal("7 días", semana.Elemento("#periodos .active").TextContent.Trim());
        Assert.Equal(7, Graficos(semana)[0].GetProperty("etiquetas").GetArrayLength());
        Assert.Equal(30, Graficos(await Navegador.Abrir("/Admin/Reportes?dias=999"))[0].GetProperty("etiquetas").GetArrayLength());

        Pagina deNom2 = await Navegador.Abrir("/Admin/Reportes?usuario=6");
        var filas = deNom2.Documento.QuerySelectorAll("#ventas tbody tr");
        Assert.NotEmpty(filas);
        Assert.All(filas, f => Assert.StartsWith("6 ·", f.Children[1].TextContent.Trim()));

        string idVenta = filas[0].Children[0].TextContent.Trim();
        Pagina detalle = await Navegador.Abrir($"/Admin/Reportes?usuario=6&venta={idVenta}");
        Assert.Contains($"Detalle de la venta N° {idVenta}", detalle.Elemento("#detalleVenta").TextContent);

        Pagina porUnidades = await Navegador.Abrir("/Admin/Reportes?orden=unidades");
        var unidades = porUnidades.Documento.QuerySelectorAll("#articulosVendidos tbody tr").Select(f => int.Parse(f.Children[2].TextContent)).ToList();
        Assert.Equal(unidades.OrderByDescending(u => u), unidades);
    }

    [Fact]
    public async Task AltaEdicionYBajaDeUnArticulo()
    {
        await EntrarComoAdmin();
        string codigo = "t" + azar.Next(100, 999);

        Pagina nuevo = await Navegador.Abrir("/Admin/Articulos/Editar");
        Pagina lista = await Navegador.Enviar(nuevo, "#btnGuardar", Campos(
            "Datos.Codigo", codigo, "Datos.Descripcion", "Monitor de prueba " + codigo, "Datos.CategoriaId", "8",
            "Datos.Stock", "5", "Datos.Precio", "1500,50", "Datos.Imagen", "~/Imagenes_ar/SDD/sdd1.png",
            "Datos.Marca", "Marca E2E", "Datos.Especificaciones", "Tamaño: 27\"\r\nPanel: IPS"));
        Assert.Contains(lista.Alertas, a => a.Contains($"Artículo {codigo} agregado"));

        Pagina edicion = await Navegador.Abrir($"/Admin/Articulos/Editar/{codigo}");
        Assert.Equal("1500.5", edicion.Elemento("#Datos_Precio").GetAttribute("value"));
        await Navegador.Enviar(edicion, "#btnGuardar", Campos("Datos.CategoriaId", "3", "Datos.GarantiaMeses", "24"));

        Pagina catalogo = await Navegador.Abrir("/Catalogo?cat=3");
        var modal = catalogo.Elemento($"#detalle-{codigo}");
        Assert.Contains("Marca E2E", modal.TextContent);
        Assert.Contains("Panel", modal.TextContent);
        Assert.Contains("2 años", modal.TextContent);

        lista = await Navegador.Abrir($"/Admin/Articulos?q={codigo}");
        lista = await Navegador.Enviar(lista, "button[title='Dar de baja']");
        Assert.Contains(lista.Alertas, a => a.Contains($"Artículo {codigo} dado de baja"));
        Assert.Null((await Navegador.Abrir("/Catalogo?cat=3")).Documento.QuerySelector($"#detalle-{codigo}"));
    }

    [Fact]
    public async Task NoSeGuardaUnaImagenQueNoSeaUnaUrlDelSitioOHttp()
    {
        await EntrarComoAdmin();
        Pagina edicion = await Navegador.Abrir("/Admin/Articulos/Editar/hdd8");

        Pagina pagina = await Navegador.Enviar(edicion, "#btnGuardar", Campos("Datos.Imagen", "x\" onerror=\"alert(1)"));

        Assert.Contains(pagina.Alertas, a => a.Contains("La imagen tiene que ser"));
        Assert.DoesNotContain("onerror=\"alert", pagina.Html); // el valor reenviado sale encodeado
        Assert.Contains("~/Imagenes_ar/", (await Navegador.Abrir("/Admin/Articulos/Editar/hdd8")).Elemento("#Datos_Imagen").GetAttribute("value"));
    }

    [Fact]
    public async Task UnaCategoriaInactivaDesapareceDelCatalogo()
    {
        await EntrarComoAdmin();
        string nombre = "Cat" + azar.Next(1000, 9999);

        Pagina categorias = await Navegador.Enviar(await Navegador.Abrir("/Admin/Categorias"), "#btnAgregarCategoria", Campos("descripcion", nombre));
        Assert.Contains(categorias.Alertas, a => a.Contains($"\"{nombre}\" agregada"));
        Assert.Contains(nombre, (await Navegador.Abrir("/Catalogo")).Elemento(".nav-pills").TextContent);

        var fila = categorias.Documento.QuerySelectorAll("#categorias tbody tr").Single(f => f.QuerySelector("input")!.GetAttribute("value") == nombre);
        string formulario = fila.QuerySelector("form")!.Id!;
        categorias = await Navegador.Enviar(categorias, $"#{formulario} button", Campos("activa", "false", "descripcion", nombre));

        Assert.Contains(categorias.Alertas, a => a.Contains("Categoría modificada"));
        Assert.DoesNotContain(nombre, (await Navegador.Abrir("/Catalogo")).Elemento(".nav-pills").TextContent);
    }

    [Fact]
    public async Task AlDesactivarUnUsuarioSuSesionAbiertaDejaDeValer()
    {
        // El cliente se registra y queda logueado en su navegador.
        var (_, nombre) = await RegistrarClienteNuevo();
        Assert.Equal("/MisCompras", (await Navegador.Abrir("/MisCompras")).Ruta);

        using var admin = Sitio.NuevoNavegador();
        await IniciarSesion("admNicolas", "admpsw2", admin);
        Pagina usuarios = await admin.Abrir("/Admin/Usuarios?q=" + nombre);
        var fila = usuarios.Documento.QuerySelectorAll("#usuarios tbody tr").Single(f => f.Children[1].TextContent == nombre);
        usuarios = await admin.Enviar(usuarios, fila.QuerySelector("button[title='Desactivar']"));
        Assert.Contains(usuarios.Alertas, a => a.Contains("Usuario desactivado"));

        Assert.Equal("/", (await Navegador.Abrir("/MisCompras")).Ruta);
        Assert.Contains((await IniciarSesion(nombre, "clave1")).Alertas, a => a.Contains("incorrectos"));
    }

    [Fact]
    public async Task ElAdminCreaUnAdministradorPeroNoSePuedeDesactivarASiMismo()
    {
        Pagina panel = await IniciarSesion("admDiamela", "admpsw1");
        string nombre = "adm" + azar.Next(1000, 9999);

        Pagina lista = await Navegador.Enviar(await Navegador.Abrir("/Admin/Usuarios/Editar"), "#btnGuardar", Campos(
            "Datos.Nombre", nombre, "Datos.Email", nombre + "@adm.com", "Datos.Contraseña", "clave123", "Datos.EsAdmin", "true"));
        Assert.Contains(lista.Alertas, a => a.Contains($"Administrador {nombre} agregado"));
        using (var nuevo = Sitio.NuevoNavegador())
        {
            Assert.Equal("/Admin", (await IniciarSesion(nombre, "clave123", nuevo)).Ruta);
        }

        Pagina propia = await Navegador.Abrir("/Admin/Usuarios?id=2");
        propia = await Navegador.Enviar(propia, "button[title='Desactivar']");
        Assert.Contains(propia.Alertas, a => a.Contains("No podés desactivar tu propia cuenta"));
        Assert.Equal("/Admin", (await Navegador.Abrir("/Admin")).Ruta);
    }

    [Fact]
    public async Task NingunaPaginaTieneScriptsInline()
    {
        await EntrarComoAdmin();

        foreach (string ruta in new[] { "/Admin", "/Admin/Reportes", "/Admin/Articulos", "/Admin/Articulos/Editar/gb1", "/Admin/Categorias",
                                        "/Admin/Usuarios", "/Admin/Usuarios/Editar/5", "/Catalogo", "/Carrito", "/MisCompras", "/MiCuenta" })
        {
            Pagina pagina = await Navegador.Abrir(ruta);
            Assert.Equal(200, pagina.Estado);
            Assert.All(pagina.Documento.Scripts, s => Assert.False(string.IsNullOrEmpty(s.Source), $"script inline en {ruta}"));
            Assert.DoesNotContain(" onerror=", pagina.Html);
            Assert.DoesNotContain(" onclick=", pagina.Html);
            Assert.DoesNotContain(" onchange=", pagina.Html);
        }
    }
}
