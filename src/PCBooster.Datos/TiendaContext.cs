using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using PCBooster.Dominio;

namespace PCBooster.Datos;

/* Mapea las entidades a las tablas que heredó el proyecto del sitio original
   (nombres como Nombre_us o Descripcion_ar). El schema lo crea y versiona
   InicializadorBase, no las migraciones de EF. */
public class TiendaContext : DbContext
{
    public TiendaContext(DbContextOptions<TiendaContext> opciones) : base(opciones)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Articulo> Articulos => Set<Articulo>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();
    public DbSet<Tarjeta> Tarjetas => Set<Tarjeta>();

    /* Opciones para una base en "ruta" (las usan la app y los tests). */
    public static DbContextOptions<TiendaContext> Opciones(string ruta) =>
        new DbContextOptionsBuilder<TiendaContext>().UseSqlite(InicializadorBase.CadenaConexion(ruta)).Options;

    protected override void ConfigureConventions(ModelConfigurationBuilder convenciones)
    {
        // SQLite no tiene decimal: los montos (columnas money) se leen y escriben
        // como double, lo que además permite Sum/Average en la base desde LINQ.
        convenciones.Properties<decimal>().HaveConversion<double>();
    }

    protected override void OnModelCreating(ModelBuilder modelo)
    {
        modelo.Entity<Usuario>(e =>
        {
            e.ToTable("Usuarios");
            e.HasKey(u => u.Id);
            e.Property(u => u.Id).HasColumnName("IdUsuario_us");
            e.Property(u => u.Nombre).HasColumnName("Nombre_us").HasMaxLength(10);
            e.Property(u => u.HashContraseña).HasColumnName("Contraseña_us");
            e.Property(u => u.Email).HasColumnName("Email_us").HasMaxLength(30);
            e.Property(u => u.Telefono).HasColumnName("Telefono_us");
            e.Property(u => u.EsAdmin).HasColumnName("Tipo_us");
            e.Property(u => u.Activo).HasColumnName("Estado_us");
            e.Property(u => u.FechaAlta).HasColumnName("FechaAlta_us");
        });

        modelo.Entity<Categoria>(e =>
        {
            e.ToTable("Categorias");
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).HasColumnName("IdCategoria_ca");
            e.Property(c => c.Descripcion).HasColumnName("Descripcion_ca").HasMaxLength(30);
            e.Property(c => c.Activa).HasColumnName("Estado_ca");
        });

        modelo.Entity<Articulo>(e =>
        {
            e.ToTable("Articulos");
            e.HasKey(a => a.Codigo);
            e.Property(a => a.Codigo).HasColumnName("CodArticulo_ar").HasMaxLength(4);
            e.Property(a => a.CategoriaId).HasColumnName("Categoria_ar");
            e.Property(a => a.Descripcion).HasColumnName("Descripcion_ar").HasMaxLength(300);
            e.Property(a => a.Stock).HasColumnName("Stock_ar");
            e.Property(a => a.Precio).HasColumnName("PrecioUnitario_ar");
            e.Property(a => a.Imagen).HasColumnName("Imagen").HasMaxLength(100);
            e.Property(a => a.Activo).HasColumnName("Estado_ar");
            e.Property(a => a.Marca).HasColumnName("Marca_ar");
            e.Property(a => a.DescripcionLarga).HasColumnName("DescripcionLarga_ar");
            e.Property(a => a.Especificaciones).HasColumnName("Especificaciones_ar");
            e.Property(a => a.GarantiaMeses).HasColumnName("Garantia_ar");
            e.HasOne(a => a.Categoria).WithMany(c => c.Articulos).HasForeignKey(a => a.CategoriaId);
        });

        modelo.Entity<Venta>(e =>
        {
            e.ToTable("Ventas");
            e.HasKey(v => v.Id);
            e.Property(v => v.Id).HasColumnName("IdVentas_ve");
            e.Property(v => v.UsuarioId).HasColumnName("IdUsuario_ve");
            // Lo calcula el trigger tr_SumaTotal: EF no lo escribe nunca.
            e.Property(v => v.Total).HasColumnName("TotalVenta_ve").ValueGeneratedOnAddOrUpdate();
            e.Property(v => v.Total).Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
            e.Property(v => v.Total).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            e.Property(v => v.Fecha).HasColumnName("FechaVenta_ve");
            e.Property(v => v.PagoConCredito).HasColumnName("Metodo_de_Pago_ve");
            e.HasOne(v => v.Usuario).WithMany(u => u.Ventas).HasForeignKey(v => v.UsuarioId);
        });

        modelo.Entity<DetalleVenta>(e =>
        {
            // Los triggers de esta tabla (total y stock) no conviven con el
            // RETURNING que EF usa por defecto en SQLite.
            e.ToTable("Detalle_Ventas", t => t.UseSqlReturningClause(false));
            e.HasKey(d => new { d.VentaId, d.ArticuloCodigo });
            e.Property(d => d.VentaId).HasColumnName("IdDetVenta_dv");
            e.Property(d => d.UsuarioId).HasColumnName("IdUsuario_dv");
            e.Property(d => d.ArticuloCodigo).HasColumnName("CodArticulo_dv");
            e.Property(d => d.Cantidad).HasColumnName("Cantidad_dv");
            e.Property(d => d.PrecioUnitario).HasColumnName("Precio_Unitario_dv");
            e.Property(d => d.DireccionEntrega).HasColumnName("Direccion_de_Entrega");
            e.Property(d => d.Contacto).HasColumnName("Contacto").HasMaxLength(20);
            e.HasOne(d => d.Venta).WithMany(v => v.Detalles).HasForeignKey(d => d.VentaId);
            e.HasOne(d => d.Articulo).WithMany().HasForeignKey(d => d.ArticuloCodigo);
        });

        modelo.Entity<Tarjeta>(e =>
        {
            e.ToTable("Tarjetas", t => t.UseSqlReturningClause(false)); // trigger de vencimiento
            e.HasKey(t => t.Id);
            e.Property(t => t.Id).HasColumnName("IdTarjeta_t");
            e.Property(t => t.UsuarioId).HasColumnName("Id_TarjetaUsuario_t");
            e.Property(t => t.Titular).HasColumnName("Titular_Tarjeta_t").HasMaxLength(50);
            e.Property(t => t.Nombre).HasColumnName("Nombre_Tarjeta_t").HasMaxLength(20);
            e.Property(t => t.EsCredito).HasColumnName("Tipo_Tarjeta_t");
            e.Property(t => t.Numero).HasColumnName("Num_Tarjeta_t");
            e.Property(t => t.Vencimiento).HasColumnName("Fecha_Vencimiento_t");
            e.Ignore(t => t.UltimosDigitos);
            e.HasOne(t => t.Usuario).WithMany(u => u.Tarjetas).HasForeignKey(t => t.UsuarioId);
        });

        modelo.Entity<DetalleVenta>().Ignore(d => d.Subtotal);
    }
}
