using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PCBooster.Dominio;

namespace PCBooster.Datos;

/* La base de la tienda. Las tablas y columnas toman el nombre de las entidades;
   el schema se crea y actualiza con las migraciones de EF (carpeta Migraciones). */
public class TiendaContext(DbContextOptions<TiendaContext> opciones) : DbContext(opciones)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Articulo> Articulos => Set<Articulo>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();
    public DbSet<Tarjeta> Tarjetas => Set<Tarjeta>();

    public static string CadenaConexion(string ruta) =>
        new SqliteConnectionStringBuilder { DataSource = ruta, ForeignKeys = true }.ToString();

    /* Opciones para una base en "ruta" (las usan la app y los tests). */
    public static DbContextOptions<TiendaContext> Opciones(string ruta) =>
        new DbContextOptionsBuilder<TiendaContext>().UseSqlite(CadenaConexion(ruta)).Options;

    protected override void ConfigureConventions(ModelConfigurationBuilder convenciones)
    {
        // SQLite no tiene decimal: los montos se guardan como REAL, lo que además
        // permite Sum en la base desde LINQ.
        convenciones.Properties<decimal>().HaveConversion<double>();
    }

    protected override void OnModelCreating(ModelBuilder modelo)
    {
        // NOCASE: nombres, e-mails, códigos y descripciones se comparan y son
        // únicos sin distinguir mayúsculas.
        modelo.Entity<Usuario>(e =>
        {
            e.Property(u => u.Nombre).HasMaxLength(10).UseCollation("NOCASE");
            e.Property(u => u.Email).HasMaxLength(30).UseCollation("NOCASE");
            e.Property(u => u.Telefono).HasMaxLength(15);
            e.HasIndex(u => u.Nombre).IsUnique();
            e.HasIndex(u => u.Email).IsUnique();
        });

        modelo.Entity<Categoria>(e =>
        {
            e.Property(c => c.Descripcion).HasMaxLength(30).UseCollation("NOCASE");
            e.HasIndex(c => c.Descripcion).IsUnique();
        });

        modelo.Entity<Articulo>(e =>
        {
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Articulos_Stock", "Stock >= 0"); // última barrera contra vender de más
                t.HasCheckConstraint("CK_Articulos_Precio", "Precio > 0");
            });
            e.HasKey(a => a.Codigo);
            e.Property(a => a.Codigo).HasMaxLength(4).UseCollation("NOCASE");
            e.Property(a => a.Descripcion).HasMaxLength(300).UseCollation("NOCASE");
            e.Property(a => a.Imagen).HasMaxLength(100);
            e.Property(a => a.Marca).HasMaxLength(50);
            e.HasIndex(a => a.Descripcion).IsUnique();
            e.HasOne(a => a.Categoria).WithMany(c => c.Articulos).HasForeignKey(a => a.CategoriaId).OnDelete(DeleteBehavior.Restrict);
        });

        modelo.Entity<Venta>(e =>
        {
            e.ToTable(t => t.HasCheckConstraint("CK_Ventas_Total", "Total >= 0"));
            e.Property(v => v.Contacto).HasMaxLength(20);
            e.HasIndex(v => v.Fecha);
            e.HasOne(v => v.Usuario).WithMany(u => u.Ventas).HasForeignKey(v => v.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        });

        modelo.Entity<DetalleVenta>(e =>
        {
            e.ToTable("DetallesVenta", t =>
            {
                t.HasCheckConstraint("CK_DetallesVenta_Cantidad", "Cantidad > 0");
                t.HasCheckConstraint("CK_DetallesVenta_PrecioUnitario", "PrecioUnitario >= 0");
            });
            e.HasKey(d => new { d.VentaId, d.ArticuloCodigo });
            e.Property(d => d.ArticuloCodigo).HasMaxLength(4).UseCollation("NOCASE");
            e.HasOne(d => d.Venta).WithMany(v => v.Detalles).HasForeignKey(d => d.VentaId);
            e.HasOne(d => d.Articulo).WithMany().HasForeignKey(d => d.ArticuloCodigo).OnDelete(DeleteBehavior.Restrict);
        });

        modelo.Entity<Tarjeta>(e =>
        {
            e.Property(t => t.Titular).HasMaxLength(50);
            e.Property(t => t.Nombre).HasMaxLength(20);
            e.HasIndex(t => new { t.UsuarioId, t.Numero }).IsUnique();
            e.HasOne(t => t.Usuario).WithMany(u => u.Tarjetas).HasForeignKey(t => t.UsuarioId);
        });
    }
}

/* Para "dotnet ef migrations add": arma el contexto sin levantar la app. */
public class TiendaContextDiseño : IDesignTimeDbContextFactory<TiendaContext>
{
    public TiendaContext CreateDbContext(string[] args) => new(TiendaContext.Opciones("diseño.db"));
}
