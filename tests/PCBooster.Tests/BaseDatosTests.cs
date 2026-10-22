using Microsoft.EntityFrameworkCore;
using PCBooster.Datos;
using PCBooster.Dominio;

namespace PCBooster.Tests;

/* El mapeo de EF Core contra el schema heredado y los triggers de la base. */
public class BaseDatosTests : BaseTemporal
{
    [Fact]
    public void CreaLaBaseConLosDatosDePrueba()
    {
        using var db = Contexto();

        Assert.Equal(9, db.Usuarios.Count());
        Assert.Equal(8, db.Categorias.Count());
        Assert.Equal(68, db.Articulos.Count());
        Assert.Empty(db.Ventas);
        Assert.Equal(InicializadorBase.VersionSchema, Convert.ToInt32(Escalar("pragma user_version")));
    }

    [Fact]
    public void LaRutaPorDefectoRespetaLaVariableDeEntorno()
    {
        Assert.Equal(Path.Combine("sitio", "App_Data", "PCbooster.db"), InicializadorBase.RutaPorDefecto("sitio"));
        Environment.SetEnvironmentVariable(InicializadorBase.VariableEntorno, @"C:\otra\base.db");
        try
        {
            Assert.Equal(@"C:\otra\base.db", InicializadorBase.RutaPorDefecto("sitio"));
        }
        finally
        {
            Environment.SetEnvironmentVariable(InicializadorBase.VariableEntorno, null);
        }
    }

    [Fact]
    public void NoVuelveACargarDatosSiLaBaseYaExiste()
    {
        using (var db = Contexto())
        {
            db.Categorias.Add(new Categoria { Descripcion = "Monitores" });
            db.SaveChanges();
        }

        CrearBase(); // como al reiniciar la app

        using var otra = Contexto();
        Assert.Equal(9, otra.Categorias.Count());
        Assert.Equal(9, otra.Usuarios.Count());
    }

    [Fact]
    public void ElMapeoLeeTodosLosCampos()
    {
        using var db = Contexto();

        Articulo gabinete = db.Articulos.Include(a => a.Categoria).Single(a => a.Codigo == "gb1");
        Assert.Equal("Gabinete", gabinete.Categoria!.Descripcion);
        Assert.Equal(3500m, gabinete.Precio);
        Assert.Equal(10, gabinete.Stock);
        Assert.Equal("~/Imagenes_ar/Gabinete/gb1.png", gabinete.Imagen);
        Assert.True(gabinete.Activo);
        Assert.False(string.IsNullOrEmpty(gabinete.Marca));
        Assert.Contains(gabinete.ListaEspecificaciones(), e => e.Key.Length > 0 && e.Value.Length > 0);
        Assert.True(gabinete.GarantiaMeses > 0);

        Usuario admin = db.Usuarios.Single(u => u.Id == 1);
        Assert.Equal("admAmador", admin.Nombre);
        Assert.True(admin.EsAdmin);
        Assert.True(admin.Activo);
        Assert.NotNull(admin.FechaAlta);
        Assert.True(Contrasenas.Verificar("admpsw2", admin.HashContraseña));
    }

    [Fact]
    public void NombreEmailYCodigoNoDistinguenMayusculas()
    {
        using var db = Contexto();

        Assert.True(db.Usuarios.Any(u => u.Nombre == "ADMAMADOR"));
        Assert.True(db.Usuarios.Any(u => u.Email == "AMADOR@ADM.COM"));
        Assert.True(db.Articulos.Any(a => a.Codigo == "GB1"));
    }

    [Fact]
    public void LasContraseñasSeGuardanHasheadasYConSalt()
    {
        using var db = Contexto();
        var hashes = db.Usuarios.ToDictionary(u => u.Nombre, u => u.HashContraseña);

        Assert.All(hashes.Values, h => Assert.StartsWith("pbkdf2$", h));
        Assert.DoesNotContain("ctr1", hashes["Nom1"]);
        // admAmador y admNicolas tienen la misma contraseña, pero el salt hace distinto el hash.
        Assert.NotEqual(hashes["admAmador"], hashes["admNicolas"]);
    }

    [Fact]
    public void UnaVentaConDetallesDescuentaStockYSumaElTotal()
    {
        int idVenta;
        using (var db = Contexto())
        {
            var venta = new Venta { UsuarioId = 5, Fecha = DateTime.Now, PagoConCredito = true };
            venta.Detalles.Add(new DetalleVenta { UsuarioId = 5, ArticuloCodigo = "gb1", Cantidad = 2, PrecioUnitario = 3500, DireccionEntrega = "Calle 1", Contacto = "Yo" });
            venta.Detalles.Add(new DetalleVenta { UsuarioId = 5, ArticuloCodigo = "fnt1", Cantidad = 1, PrecioUnitario = 7300, DireccionEntrega = "Calle 1", Contacto = "Yo" });
            db.Ventas.Add(venta);
            db.SaveChanges();
            idVenta = venta.Id;
        }

        using var otra = Contexto();
        Venta guardada = otra.Ventas.Include(v => v.Detalles).Single(v => v.Id == idVenta);
        Assert.Equal(14300m, guardada.Total); // tr_SumaTotal
        Assert.Equal(2, guardada.Detalles.Count);
        Assert.Equal(8, otra.Articulos.Single(a => a.Codigo == "gb1").Stock); // tr_ControlStock
    }

    [Fact]
    public void ElStockNoPuedeQuedarNegativo()
    {
        using var db = Contexto();
        var venta = new Venta { UsuarioId = 5, Fecha = DateTime.Now };
        venta.Detalles.Add(new DetalleVenta { UsuarioId = 5, ArticuloCodigo = "gb1", Cantidad = 11, PrecioUnitario = 3500, DireccionEntrega = "Calle 1", Contacto = "Yo" });
        db.Ventas.Add(venta);

        Assert.Throws<DbUpdateException>(() => db.SaveChanges());
    }

    [Fact]
    public void TarjetasSeGuardanSinCodigoDeSeguridadYNoSeAceptanVencidas()
    {
        using (var db = Contexto())
        {
            db.Tarjetas.Add(new Tarjeta { UsuarioId = 5, Titular = "Nom Uno", Nombre = "Visa", EsCredito = true, Numero = 4567123412340012, Vencimiento = new DateOnly(2099, 12, 1) });
            db.SaveChanges();
        }

        using (var db = Contexto())
        {
            Tarjeta guardada = db.Tarjetas.Single();
            Assert.True(guardada.Id > 0);
            Assert.Equal(4567123412340012L, guardada.Numero); // 16 dígitos completos
            Assert.Equal("0012", guardada.UltimosDigitos);
            Assert.Equal(new DateOnly(2099, 12, 1), guardada.Vencimiento);
            Assert.Equal(0L, Escalar("select count(*) from pragma_table_info('Tarjetas') where name like 'Codigo_Seguridad%'"));

            db.Tarjetas.Add(new Tarjeta { UsuarioId = 5, Titular = "Nom Uno", Nombre = "Visa", Numero = 1234, Vencimiento = new DateOnly(2020, 1, 1) });
            var error = Assert.Throws<DbUpdateException>(() => db.SaveChanges());
            Assert.Contains("vencida", error.InnerException!.Message);
        }
    }

    [Fact]
    public void UnaBaseNuevaIncluyeElHistorialDeDemo()
    {
        CrearBase(conDemo: true);
        using var db = Contexto();

        Assert.Equal(29, db.Usuarios.Count());
        Assert.True(db.Ventas.Count() > 100);
        // Ninguna venta en el futuro ni fuera de los últimos 6 meses.
        Assert.Equal(0L, Escalar("select count(*) from Ventas where FechaVenta_ve > datetime('now','localtime') or FechaVenta_ve < datetime('now','localtime','-181 days')"));
        // El total de cada venta (calculado por tr_SumaTotal) coincide con sus detalles.
        Assert.Equal(0L, Escalar("select count(*) from Ventas v where TotalVenta_ve <> (select sum(Cantidad_dv * Precio_Unitario_dv) from Detalle_Ventas where IdDetVenta_dv = v.IdVentas_ve)"));
        // Ningún cliente compra antes de darse de alta.
        Assert.Equal(0L, Escalar("select count(*) from Ventas v join Usuarios u on u.IdUsuario_us = v.IdUsuario_ve where v.FechaVenta_ve < u.FechaAlta_us"));
        Assert.True(Contrasenas.Verificar("demo123", db.Usuarios.Single(u => u.Nombre == "lucia.g").HashContraseña));
    }

    [Fact]
    public void TodosLosArticulosDePruebaTienenDetalle()
    {
        using var db = Contexto();

        Assert.All(db.Articulos.ToList(), a =>
        {
            Assert.False(string.IsNullOrWhiteSpace(a.Marca), a.Codigo);
            Assert.NotEmpty(a.ListaEspecificaciones());
            Assert.True(a.GarantiaMeses > 0);
        });
    }
}
