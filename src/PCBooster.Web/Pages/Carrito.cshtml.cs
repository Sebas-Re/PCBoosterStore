using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using PCBooster.Dominio;
using PCBooster.Negocio;
using PCBooster.Web.Infraestructura;

namespace PCBooster.Web.Pages;

public class CarritoModel(CarritoSesion carritoSesion, TarjetasService tarjetas, ComprasService compras) : PaginaBase
{
    /* Cada formulario de la página valida solo sus campos: se reciben como
       parámetros del handler (con prefijo), no como propiedades enlazadas. */
    public class FormularioTarjeta
    {
        [Required(ErrorMessage = "Ingresá el número."), RegularExpression(@"^\d{13,19}$", ErrorMessage = "Entre 13 y 19 dígitos, sin espacios.")]
        public string Numero { get; set; } = "";

        [Required(ErrorMessage = "Ingresá el titular."), StringLength(50)]
        public string Titular { get; set; } = "";

        [Required(ErrorMessage = "Ingresá la marca."), StringLength(20)]
        public string Nombre { get; set; } = "";

        public bool EsCredito { get; set; }

        [Required(ErrorMessage = "Requerido."), Range(1, 12, ErrorMessage = "Mes inválido.")]
        public int? Mes { get; set; }

        [Required(ErrorMessage = "Requerido."), Range(2000, 2099, ErrorMessage = "Año inválido.")]
        public int? Año { get; set; }
    }

    public class FormularioCompra
    {
        [Required(ErrorMessage = "Elegí una tarjeta.")]
        public int? TarjetaId { get; set; }

        [Required(ErrorMessage = "Ingresá la dirección de entrega."), StringLength(50)]
        public string Direccion { get; set; } = "";

        [Required(ErrorMessage = "Ingresá quién recibe el pedido."), StringLength(20)]
        public string Contacto { get; set; } = "";
    }

    public Carrito Carrito { get; private set; } = new();
    public List<Tarjeta> Tarjetas { get; private set; } = new();
    public FormularioTarjeta NuevaTarjeta { get; private set; } = new();
    public FormularioCompra Compra { get; private set; } = new();

    public async Task OnGetAsync() => await CargarAsync();

    async Task CargarAsync()
    {
        Carrito = carritoSesion.Obtener();
        Tarjetas = await tarjetas.ListarAsync(UsuarioId);
        if (Compra.TarjetaId == null && Tarjetas.Count == 1)
        {
            Compra.TarjetaId = Tarjetas[0].Id;
        }
    }

    public IActionResult OnPostQuitar(string codigo)
    {
        Carrito carrito = carritoSesion.Obtener();
        carrito.Quitar(codigo);
        carritoSesion.Guardar(carrito);
        return RedirectToPage();
    }

    public IActionResult OnPostVaciar()
    {
        carritoSesion.Vaciar();
        Avisar("Vaciaste el carrito.", TipoMensaje.Info);
        return RedirectToPage("/Catalogo");
    }

    public async Task<IActionResult> OnPostGuardarTarjetaAsync([Bind(Prefix = "Tarjeta")] FormularioTarjeta formulario)
    {
        NuevaTarjeta = formulario;
        if (!ModelState.IsValid)
        {
            await CargarAsync();
            return Page();
        }
        string? error = await tarjetas.AgregarAsync(new Tarjeta
        {
            UsuarioId = UsuarioId,
            Numero = long.Parse(formulario.Numero),
            Titular = formulario.Titular,
            Nombre = formulario.Nombre,
            EsCredito = formulario.EsCredito,
            Vencimiento = new DateOnly(formulario.Año!.Value, formulario.Mes!.Value, 1)
        });
        if (error != null)
        {
            Avisar("No se pudo guardar la tarjeta: " + error, TipoMensaje.Error);
            await CargarAsync();
            return Page();
        }
        Avisar("Tarjeta guardada. Ya podés elegirla como medio de pago.", TipoMensaje.Exito);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostConfirmarAsync([Bind(Prefix = "Compra")] FormularioCompra formulario)
    {
        Compra = formulario;
        if (!ModelState.IsValid)
        {
            await CargarAsync();
            return Page();
        }
        ResultadoCompra resultado = await compras.ConfirmarAsync(UsuarioId, carritoSesion.Obtener(), formulario.Direccion, formulario.Contacto, formulario.TarjetaId!.Value);
        if (!resultado.Ok)
        {
            Avisar(resultado.Error!, TipoMensaje.Error);
            await CargarAsync();
            return Page();
        }
        carritoSesion.Vaciar();
        Avisar($"¡Listo! Registramos tu compra N° {resultado.IdVenta}.", TipoMensaje.Exito);
        return RedirectToPage("/MisCompras", new { compra = resultado.IdVenta });
    }
}
