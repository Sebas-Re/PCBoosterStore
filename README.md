# PC Booster Store

Tienda online de componentes de PC hecha con **ASP.NET Core 10 (Razor Pages)**, **Entity Framework Core 10** y **SQLite**.

Los clientes recorren el catálogo, arman el carrito, guardan tarjetas y compran. Los administradores tienen un panel con
métricas de e-commerce (ingresos, pedidos, ticket promedio, clientes nuevos, recompra, productos más vendidos) y el
ABM de artículos, categorías y usuarios.

Es la reescritura del trabajo práctico integrador de Programación III (UTN), que estaba en ASP.NET WebForms sobre
.NET Framework: [Sebas-Re/TPFINAL_Prog3](https://github.com/Sebas-Re/TPFINAL_Prog3). De ahí vienen el schema, los
datos de prueba y el diseño con Bootstrap.

## Cómo correrlo

Requisito: el [SDK de .NET 10](https://dotnet.microsoft.com/download). No hace falta instalar una base de datos.

```
dotnet run --project src/PCBooster.Web
```

La primera vez, la app crea `src/PCBooster.Web/App_Data/PCbooster.db` con los datos de prueba y un historial de
demo (20 clientes y unas 180 ventas de los últimos 6 meses, con fechas relativas al día en que se crea), para que el
panel tenga qué mostrar. Para volver a empezar de cero, borrá ese archivo.

| Usuario     | Contraseña | Rol           |
|-------------|------------|---------------|
| `admAmador` | `admpsw2`  | Administrador |
| `Nom1`      | `ctr1`     | Cliente       |
| `lucia.g`   | `demo123`  | Cliente (historial de demo, igual que el resto de los `*.x`) |

## Tests

```
dotnet test
```

| Proyecto              | Qué prueba                                                                             |
|-----------------------|----------------------------------------------------------------------------------------|
| `PCBooster.Tests`     | Mapeo de EF Core, triggers y migraciones de la base, y las reglas de negocio, contra un SQLite temporal |
| `PCBooster.Web.Tests` | El sitio completo en memoria (`WebApplicationFactory`): login, compra, administración y seguridad |

## Estructura

```
src/
  PCBooster.Dominio/   Entidades (Usuario, Articulo, Venta, Tarjeta...) y el carrito
  PCBooster.Datos/     TiendaContext (EF Core), creación y migración de la base, hash de contraseñas, scripts SQL
  PCBooster.Negocio/   Servicios con las reglas y las consultas LINQ: usuarios, catálogo, tarjetas, compras, métricas
  PCBooster.Web/       Razor Pages, layouts, seguridad y recursos estáticos
tests/
  PCBooster.Tests/
  PCBooster.Web.Tests/
```

Las páginas solo hablan con los servicios de Negocio, que reciben el `TiendaContext` por inyección de dependencias.

## Base de datos

- El schema viene del proyecto original (SQL Server → MySQL → SQLite), con sus nombres de tabla y columna
  (`Usuarios.Nombre_us`, `Articulos.Descripcion_ar`...). `TiendaContext` los mapea a entidades con nombres limpios
  mediante Fluent API.
- No se usan migraciones de EF: el schema y los datos están en [`src/PCBooster.Datos/Sql`](src/PCBooster.Datos/Sql),
  embebidos en el ensamblado, y la versión se guarda en `PRAGMA user_version`. Al arrancar, la app crea la base o le
  aplica los `migracion_N.sql` pendientes, así que también abre una base creada por el sitio WebForms.
- El total de cada venta y el descuento de stock los hacen triggers de la base al insertar los detalles. La compra
  completa (venta y detalles) va en una sola transacción.
- Las métricas del panel son consultas LINQ (`GroupBy` por mes, día, categoría, producto y medio de pago) que EF
  traduce a SQL. Los montos se mapean a `double` en SQLite para poder sumarlos en la base.

## Seguridad

- **SQL injection:** todo pasa por EF Core o comandos parametrizados.
- **XSS:** Razor encodea toda la salida. La `Content-Security-Policy` no permite scripts inline (todo el JavaScript
  está en `wwwroot/js` y el markup usa atributos `data-*`). Las URLs de imagen que carga el admin solo pueden ser
  rutas del sitio o `http(s)`.
- **Contraseñas:** PBKDF2-SHA256 con salt y 600.000 iteraciones. El login se bloquea 5 minutos después de 5 intentos
  fallidos, y cambiar nombre, e-mail o contraseña pide la contraseña actual.
- **Sesión:** cookie de autenticación `HttpOnly`, `SameSite=Lax` y `Secure` con https, nueva en cada login. Se
  revalida en cada request, así que si el admin desactiva a un usuario o le cambia el rol, la sesión abierta cae.
- **CSRF:** token antiforgery en todos los formularios, incluido "Salir", que es un POST.
- **Tarjetas:** no se pide ni se guarda el código de seguridad. Las páginas solo muestran los últimos 4 dígitos, y al
  pagar se verifica que la tarjeta sea del usuario y esté vigente.
- **Headers:** CSP, `X-Frame-Options`, `X-Content-Type-Options`, `Referrer-Policy`, HSTS en producción, sin header
  `Server`.

Límites conocidos (es un proyecto de demostración):

- El número completo de la tarjeta se guarda en la base. Una tienda real delegaría el pago en una pasarela que
  devuelve un token.
- El bloqueo por intentos fallidos y la sesión (el carrito) viven en memoria: se reinician con la app y no se
  comparten entre servidores.
