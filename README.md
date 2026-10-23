# PC Booster Store

Tienda online de componentes de PC hecha con **ASP.NET Core 10 (Razor Pages)**, **Entity Framework Core 10** y **SQLite**.

Los clientes recorren el catálogo, arman el carrito, guardan tarjetas y compran. Los administradores tienen un panel con
métricas de e-commerce (ingresos, pedidos, ticket promedio, clientes nuevos, recompra, productos más vendidos) y el
ABM de artículos, categorías y usuarios.

Es la reescritura del trabajo práctico integrador de Programación III (UTN), que estaba en ASP.NET WebForms sobre
.NET Framework: [Sebas-Re/TPFINAL_Prog3](https://github.com/Sebas-Re/TPFINAL_Prog3). De ahí vienen los datos de
prueba y el diseño con Bootstrap.

## Cómo correrlo

Requisito: el [SDK de .NET 10](https://dotnet.microsoft.com/download). No hace falta instalar una base de datos.

```
dotnet run --project src/PCBooster.Web
```

La primera vez, la app crea `src/PCBooster.Web/App_Data/PCbooster.db` con los datos de prueba y un historial de
demo (20 clientes y unas 180 ventas de los últimos 6 meses, con fechas relativas al día en que se crea), para que el
panel tenga qué mostrar. Para volver a empezar de cero, borrá ese archivo. Para usar otra ruta, definí la variable
de entorno `PCBOOSTER_DB`.

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
| `PCBooster.Tests`     | El schema y los datos de prueba, y las reglas de negocio, contra un SQLite temporal      |
| `PCBooster.Web.Tests` | El sitio completo en memoria (`WebApplicationFactory`): login, compra, administración y seguridad |

## Estructura

```
src/
  PCBooster.Dominio/   Entidades (Usuario, Articulo, Venta, Tarjeta...) y el carrito
  PCBooster.Datos/     TiendaContext (EF Core), migraciones, datos de prueba (Sql/) y hash de contraseñas
  PCBooster.Negocio/   Servicios con las reglas y las consultas LINQ: usuarios, catálogo, tarjetas, compras, métricas
  PCBooster.Web/       Razor Pages, layouts, seguridad y recursos estáticos
tests/
  PCBooster.Tests/
  PCBooster.Web.Tests/
```

Las páginas solo hablan con los servicios de Negocio, que reciben el `TiendaContext` por inyección de dependencias.

## Base de datos

- El schema lo definen las entidades y `TiendaContext` (índices únicos, `COLLATE NOCASE`, checks), y se crea con
  las migraciones de EF Core de [`src/PCBooster.Datos/Migraciones`](src/PCBooster.Datos/Migraciones). Al arrancar,
  la app aplica las pendientes y, si la base está vacía, carga los datos de prueba de
  [`src/PCBooster.Datos/Sql`](src/PCBooster.Datos/Sql) (embebidos en el ensamblado).
- Para cambiar el schema, modificá las entidades o `TiendaContext` y generá una migración:

  ```
  dotnet tool restore
  dotnet ef migrations add NombreDelCambio --project src/PCBooster.Datos --output-dir Migraciones
  ```

- La compra completa (venta, detalles y descuento de stock) va en una sola transacción `BEGIN IMMEDIATE`: dos
  compras simultáneas no pueden vender el mismo stock, y un check de la base impide que quede negativo.
- Los listados y las métricas son consultas LINQ (`GroupBy` por mes, día, categoría, producto y medio de pago,
  paginado con `Skip`/`Take`) que EF traduce a SQL. Los montos se guardan como `REAL` en SQLite para poder sumarlos
  en la base.

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
