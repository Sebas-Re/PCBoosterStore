using PCBooster.Datos;

// Hay estado estático (iteraciones del hash, variable de entorno, intentos de login):
// los tests corren de a uno.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace PCBooster.Tests;

/* Migración de bases creadas por versiones anteriores (incluido el sitio WebForms). */
public class VersionadoTests : BaseTemporal
{
    /* Devuelve Tarjetas y Ventas a la forma que tenían hasta la versión 4
       (con el código de seguridad) y marca la base con esa versión. */
    void VolverAVersion(int version)
    {
        CrearBase();
        Ejecutar("""
            drop table Tarjetas;
            create table Tarjetas(
                Id_TarjetaUsuario_t bigint not null, Titular_Tarjeta_t varchar(50) not null, Nombre_Tarjeta_t varchar(20) not null,
                Tipo_Tarjeta_t bit not null, Num_Tarjeta_t bigint not null, Fecha_Vencimiento_t date not null, Codigo_Seguridad_t int not null,
                constraint PK_Tarjetas primary key (Id_TarjetaUsuario_t, Codigo_Seguridad_t));
            alter table Ventas add column Codigo_Seguridad_ve int not null default 0;
            """);
        Ejecutar("pragma user_version = " + version);
    }

    /* Una base en la versión 5, con los datos de prueba. Sirve para "reiniciar la app". */
    void Migrar() => InicializadorBase.Inicializar(Ruta);

    [Fact]
    public void LaMigracion5DejaDeGuardarElCodigoDeSeguridadSinPerderTarjetasNiVentas()
    {
        VolverAVersion(4);
        Ejecutar("""
            insert into Tarjetas values (5, 'Nom Uno', 'Visa', 1, 4567123412340012, '2099-12-01', 321);
            insert into Tarjetas values (5, 'Nom Uno', 'Visa', 1, 4567123412340012, '2099-12-01', 999);
            insert into Tarjetas values (6, 'Nom Dos', 'Master', 0, 5555444433332222, '2098-01-01', 123);
            insert into Ventas (IdUsuario_ve, Metodo_de_Pago_ve, Codigo_Seguridad_ve) values (5, 1, 321);
            """);
        long rowidDeNom2 = (long)Escalar("select rowid from Tarjetas where Id_TarjetaUsuario_t = 6")!;

        Migrar();

        Assert.Equal(5L, Escalar("pragma user_version"));
        Assert.Equal(0L, Escalar("select count(*) from pragma_table_info('Tarjetas') where name = 'Codigo_Seguridad_t'"));
        Assert.Equal(0L, Escalar("select count(*) from pragma_table_info('Ventas') where name = 'Codigo_Seguridad_ve'"));
        Assert.Equal(1L, Escalar("select count(*) from Ventas"));
        // El número repetido del mismo usuario queda una sola vez; el id es el rowid que ya usaba el sitio.
        using var db = Contexto();
        Assert.Equal(2, db.Tarjetas.Count());
        Assert.Equal(rowidDeNom2, db.Tarjetas.Single(t => t.UsuarioId == 6).Id);
        // El trigger de vencimiento sigue vigente.
        Assert.Throws<Microsoft.Data.Sqlite.SqliteException>(() =>
            Ejecutar("insert into Tarjetas (Id_TarjetaUsuario_t, Titular_Tarjeta_t, Nombre_Tarjeta_t, Tipo_Tarjeta_t, Num_Tarjeta_t, Fecha_Vencimiento_t) values (5, 'X', 'Visa', 1, 1, '2020-01-01')"));
    }

    [Fact]
    public void LaMigracion4HasheaLasContraseñasEnTextoPlano()
    {
        VolverAVersion(3);
        Ejecutar("update Usuarios set Contraseña_us = 'ctr1' where Nombre_us = 'Nom1'");

        Migrar();

        Assert.Equal(0L, Escalar("select count(*) from Usuarios where Contraseña_us not like 'pbkdf2$%'"));
        using var db = Contexto();
        Assert.True(Contrasenas.Verificar("ctr1", db.Usuarios.Single(u => u.Nombre == "Nom1").HashContraseña));
    }

    [Fact]
    public void LaMigracion3CorrigeLosTotalesInfladosPorElTriggerViejo()
    {
        VolverAVersion(2);
        // tr_SumaTotal original: sumaba cada detalle a todas las ventas del usuario.
        Ejecutar("""
            drop trigger tr_SumaTotal;
            create trigger tr_SumaTotal after insert on Detalle_Ventas begin
                update Ventas set TotalVenta_ve = TotalVenta_ve + new.Cantidad_dv * new.Precio_Unitario_dv where IdUsuario_ve = new.IdUsuario_dv;
            end;
            insert into Ventas (IdVentas_ve, IdUsuario_ve, Metodo_de_Pago_ve) values (1, 5, 1), (2, 5, 1);
            insert into Detalle_Ventas values (1, 5, 'gb1', 1, 3500, 'Calle 1', 'Yo');
            insert into Detalle_Ventas values (2, 5, 'fnt1', 1, 7300, 'Calle 1', 'Yo');
            """);
        Assert.Equal(10800.0, Convert.ToDouble(Escalar("select TotalVenta_ve from Ventas where IdVentas_ve = 1"))); // inflado

        Migrar();

        Assert.Equal(3500.0, Convert.ToDouble(Escalar("select TotalVenta_ve from Ventas where IdVentas_ve = 1")));
        Assert.Equal(7300.0, Convert.ToDouble(Escalar("select TotalVenta_ve from Ventas where IdVentas_ve = 2")));
    }

    [Fact]
    public void UnaBaseSinVersionarSeMigraSinPerderDatos()
    {
        // Como una base creada antes del versionado: sin fecha de alta, sin detalle de
        // productos, con la imagen rota y las contraseñas en texto plano.
        VolverAVersion(0);
        Ejecutar("""
            insert into Categorias (Descripcion_ca) values ('Monitores');
            alter table Usuarios drop column FechaAlta_us;
            alter table Articulos drop column Marca_ar; alter table Articulos drop column DescripcionLarga_ar;
            alter table Articulos drop column Especificaciones_ar; alter table Articulos drop column Garantia_ar;
            update Articulos set Imagen = '~/Imagenes_ar/Fuente/fnt8png' where CodArticulo_ar = 'fnt8';
            update Usuarios set Contraseña_us = 'admpsw2' where Nombre_us = 'admAmador';
            """);

        Migrar();

        Assert.Equal(5L, Escalar("pragma user_version"));
        Assert.Equal(0L, Escalar("select count(*) from Usuarios where FechaAlta_us is null"));
        Assert.Equal("~/Imagenes_ar/Fuente/fnt8.png", Escalar("select Imagen from Articulos where CodArticulo_ar = 'fnt8'"));
        Assert.Equal(1L, Escalar("select count(*) from Categorias where Descripcion_ca = 'Monitores'"));
        Assert.Equal(68L, Escalar("select count(*) from Articulos where DescripcionLarga_ar is not null"));
        using var db = Contexto();
        Assert.True(Contrasenas.Verificar("admpsw2", db.Usuarios.Single(u => u.Id == 1).HashContraseña));
    }

    [Fact]
    public void UnaBaseAlDiaNoSeVuelveAMigrar()
    {
        CrearBase();
        string hash = (string)Escalar("select Contraseña_us from Usuarios where IdUsuario_us = 1")!;

        Migrar();

        Assert.Equal(hash, Escalar("select Contraseña_us from Usuarios where IdUsuario_us = 1"));
        Assert.Equal(9L, Escalar("select count(*) from Usuarios"));
    }
}
