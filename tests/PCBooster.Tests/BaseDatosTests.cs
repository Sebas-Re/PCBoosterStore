using Microsoft.EntityFrameworkCore;
using PCBooster.Datos;
using PCBooster.Dominio;

namespace PCBooster.Tests;

/* El schema que crean las migraciones y los datos de prueba. */
public class BaseDatosTests : BaseTemporal
{
    [Fact]
    public void CreaLaBaseConLosDatosDePrueba()
    {
        using var db = Contexto();

        Assert.Empty(db.Database.GetPendingMigrations());
        Assert.Equal(9, db.Usuarios.Count());
        Assert.Equal(8, db.Categorias.Count());
        Assert.Equal(68, db.Articulos.Count());
        Assert.Empty(db.Ventas);
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
        Assert.Equal("/img/productos/gb1.png", gabinete.Imagen);
        Assert.True(gabinete.Activo);
        Assert.False(string.IsNullOrEmpty(gabinete.Marca));
        Assert.Contains(gabinete.ListaEspecificaciones(), e => e.Key.Length > 0 && e.Value.Length > 0);
        Assert.True(gabinete.GarantiaMeses > 0);

        Usuario admin = db.Usuarios.Single(u => u.Id == 1);
        Assert.Equal("admAmador", admin.Nombre);
        Assert.True(admin.EsAdmin);
        Assert.True(admin.Activo);
        Assert.True(admin.FechaAlta < DateTime.Now);
        Assert.True(Contrasenas.Verificar("admpsw2", admin.HashContraseña));
    }

    [Fact]
    public void NombreEmailYCodigoNoDistinguenMayusculas()
    {
        using var db = Contexto();

        Assert.True(db.Usuarios.Any(u => u.Nombre == "ADMAMADOR"));
        Assert.True(db.Usuarios.Any(u => u.Email == "AMADOR@ADM.COM"));
        Assert.True(db.Articulos.Any(a => a.Codigo == "GB1"));

        db.Usuarios.Add(new Usuario { Nombre = "NOM1", Email = "otro@mail.com", HashContraseña = "x", FechaAlta = DateTime.Now });
        Assert.Throws<DbUpdateException>(() => db.SaveChanges()); // índice único sin distinguir mayúsculas
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
    public void UnaVentaSeGuardaConSusDetallesYSuEntrega()
    {
        int idVenta;
        using (var db = Contexto())
        {
            var venta = new Venta { UsuarioId = 5, Fecha = DateTime.Now, PagoConCredito = true, Total = 14300, DireccionEntrega = "Calle 1", Contacto = "Yo" };
            venta.Detalles.Add(new DetalleVenta { ArticuloCodigo = "gb1", Cantidad = 2, PrecioUnitario = 3500 });
            venta.Detalles.Add(new DetalleVenta { ArticuloCodigo = "fnt1", Cantidad = 1, PrecioUnitario = 7300 });
            db.Ventas.Add(venta);
            db.SaveChanges();
            idVenta = venta.Id;
        }

        using var otra = Contexto();
        Venta guardada = otra.Ventas.Include(v => v.Detalles).Single(v => v.Id == idVenta);
        Assert.Equal(14300m, guardada.Total);
        Assert.Equal("Calle 1", guardada.DireccionEntrega);
        Assert.Equal(new[] { 7300m, 7000m }, guardada.Detalles.OrderBy(d => d.ArticuloCodigo).Select(d => d.Subtotal));
    }

    [Fact]
    public void ElStockNoPuedeQuedarNegativo()
    {
        using var db = Contexto();
        db.Articulos.Single(a => a.Codigo == "gb1").Stock = -1;

        Assert.Throws<DbUpdateException>(() => db.SaveChanges());
    }

    [Fact]
    public void LasTarjetasGuardanElNumeroCompletoYElVencimiento()
    {
        using (var db = Contexto())
        {
            db.Tarjetas.Add(new Tarjeta { UsuarioId = 5, Titular = "Nom Uno", Nombre = "Visa", EsCredito = true, Numero = 4567123412340012, Vencimiento = new DateOnly(2099, 12, 1) });
            db.SaveChanges();
        }

        using var otra = Contexto();
        Tarjeta guardada = otra.Tarjetas.Single();
        Assert.True(guardada.Id > 0);
        Assert.Equal(4567123412340012L, guardada.Numero); // 16 dígitos completos
        Assert.Equal("0012", guardada.UltimosDigitos);
        Assert.Equal(new DateOnly(2099, 12, 1), guardada.Vencimiento);
    }

    [Fact]
    public void UnaBaseNuevaIncluyeElHistorialDeDemo()
    {
        CrearBase(conDemo: true);
        using var db = Contexto();

        Assert.Equal(29, db.Usuarios.Count());
        Assert.True(db.Ventas.Count() > 100);
        // Ninguna venta en el futuro ni fuera de los últimos 6 meses.
        Assert.False(db.Ventas.Any(v => v.Fecha > DateTime.Now || v.Fecha < DateTime.Now.AddDays(-181)));
        // El total de cada venta coincide con sus detalles.
        Assert.False(db.Ventas.Any(v => v.Total != v.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario)));
        // Ningún cliente compra antes de darse de alta.
        Assert.False(db.Ventas.Any(v => v.Fecha < v.Usuario!.FechaAlta));
        // El stock ya descuenta lo vendido.
        int vendidos = db.DetallesVenta.Where(d => d.ArticuloCodigo == "ps4").Sum(d => d.Cantidad);
        Assert.True(vendidos > 0);
        Assert.Equal(40 - vendidos, db.Articulos.Single(a => a.Codigo == "ps4").Stock);
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
