using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PCBooster.Datos;
using PCBooster.Negocio;
using PCBooster.Web.Infraestructura;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(o => o.AddServerHeader = false); // no anunciar el servidor

// Base de datos: App_Data/PCbooster.db, o la ruta de PCBOOSTER_DB (variable de entorno o
// configuración; la usan los tests).
string rutaBase = builder.Configuration[InicializadorBase.ClaveRuta] is { Length: > 0 } configurada
    ? configurada
    : Path.Combine(builder.Environment.ContentRootPath, "App_Data", "PCbooster.db");
builder.Services.AddDbContext<TiendaContext>(o => o.UseSqlite(TiendaContext.CadenaConexion(rutaBase)));
builder.Services.AddScoped<UsuariosService>();
builder.Services.AddScoped<CatalogoService>();
builder.Services.AddScoped<TarjetasService>();
builder.Services.AddScoped<ComprasService>();
builder.Services.AddScoped<MetricasService>();

builder.Services.AddRazorPages(o =>
{
    // Todo exige sesión salvo el login y el registro; /Admin, además, el rol Admin.
    o.Conventions.AuthorizeFolder("/");
    o.Conventions.AllowAnonymousToPage("/Index");
    o.Conventions.AllowAnonymousToPage("/Cuenta/Registro");
    o.Conventions.AllowAnonymousToPage("/Error");
    o.Conventions.AuthorizeFolder("/Admin", Roles.Politica);
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.Cookie.Name = "PCBooster.Auth";
        o.Cookie.HttpOnly = true;
        o.Cookie.SameSite = SameSiteMode.Lax;
        o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        o.ExpireTimeSpan = TimeSpan.FromHours(2);
        o.SlidingExpiration = true;
        o.LoginPath = "/";
        o.AccessDeniedPath = "/Catalogo";
        // Si el admin desactiva al usuario o le cambia el rol, la sesión abierta deja de valer.
        o.Events.OnValidatePrincipal = async contexto =>
        {
            var usuarios = contexto.HttpContext.RequestServices.GetRequiredService<UsuariosService>();
            ClaimsPrincipal principal = contexto.Principal!;
            int id = int.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var usuario = await usuarios.ObtenerAsync(id);
            if (usuario == null || !usuario.Activo || usuario.EsAdmin != principal.IsInRole(Roles.Admin))
            {
                contexto.RejectPrincipal();
                await contexto.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        };
    });
builder.Services.AddAuthorizationBuilder().AddPolicy(Roles.Politica, p => p.RequireRole(Roles.Admin));

// El carrito vive en la sesión (en memoria del servidor).
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o =>
{
    o.Cookie.Name = "PCBooster.Sesion";
    o.Cookie.HttpOnly = true;
    o.Cookie.SameSite = SameSiteMode.Lax;
    o.Cookie.IsEssential = true;
    o.IdleTimeout = TimeSpan.FromHours(2);
});
builder.Services.AddAntiforgery(o => o.Cookie.Name = "PCBooster.Antiforgery");
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CarritoSesion>();

var app = builder.Build();

// Aplica las migraciones pendientes y, la primera vez, carga los datos de prueba.
InicializadorBase.Inicializar(rutaBase);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}
// Precios como "$ 7.300" y fechas dd/MM/yyyy sin depender de la configuración del servidor.
app.UseRequestLocalization("es-AR");
app.UseStatusCodePagesWithReExecute("/Error", "?codigo={0}");
app.UseMiddleware<HeadersSeguridad>();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.Run();

/* Para los tests (WebApplicationFactory<Program>). */
public partial class Program;
