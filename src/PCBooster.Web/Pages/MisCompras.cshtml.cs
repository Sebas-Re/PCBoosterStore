using Microsoft.AspNetCore.Mvc;
using PCBooster.Dominio;
using PCBooster.Negocio;
using PCBooster.Web.Infraestructura;

namespace PCBooster.Web.Pages;

public class MisComprasModel(ComprasService compras) : PaginaBase
{
    /* Compra elegida (?compra=N), por ejemplo la que se acaba de confirmar. */
    [BindProperty(SupportsGet = true)]
    public int? Compra { get; set; }

    public List<Venta> Ventas { get; private set; } = new();

    /* Detalle de la compra elegida; solo si es del usuario logueado. */
    public Venta? Detalle { get; private set; }

    public async Task OnGetAsync()
    {
        Ventas = await compras.ComprasDeAsync(UsuarioId);
        if (Compra != null)
        {
            Detalle = await compras.DetalleAsync(Compra.Value, UsuarioId);
        }
    }
}
