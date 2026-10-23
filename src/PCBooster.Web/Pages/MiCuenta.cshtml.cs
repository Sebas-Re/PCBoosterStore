using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using PCBooster.Dominio;
using PCBooster.Negocio;
using PCBooster.Web.Infraestructura;

namespace PCBooster.Web.Pages;

public class MiCuentaModel(UsuariosService usuarios, TarjetasService tarjetas) : PaginaBase
{
    public class Formulario
    {
        [Required(ErrorMessage = "Ingresá un nombre."), StringLength(10, ErrorMessage = "Hasta 10 caracteres.")]
        public string Nombre { get; set; } = "";

        [Required(ErrorMessage = "Ingresá un e-mail."), EmailAddress(ErrorMessage = "Ingresá un e-mail válido."), StringLength(30)]
        public string Email { get; set; } = "";

        [RegularExpression(@"^\d*$", ErrorMessage = "Ingresá solo números."), StringLength(15)]
        public string? Telefono { get; set; }

        [DataType(DataType.Password), StringLength(UsuariosService.LargoMaximoContraseña)]
        public string? NuevaContraseña { get; set; }

        [DataType(DataType.Password), StringLength(UsuariosService.LargoMaximoContraseña)]
        public string? ContraseñaActual { get; set; }
    }

    [BindProperty]
    public Formulario Datos { get; set; } = new();

    public List<Tarjeta> Tarjetas { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Usuario usuario = (await usuarios.ObtenerAsync(UsuarioId))!;
        Datos = new Formulario { Nombre = usuario.Nombre, Email = usuario.Email, Telefono = usuario.Telefono };
        Tarjetas = await tarjetas.ListarAsync(UsuarioId);
    }

    public async Task<IActionResult> OnPostGuardarAsync()
    {
        if (ModelState.IsValid)
        {
            string? error = await usuarios.ActualizarDatosPropiosAsync(UsuarioId, Datos.Nombre, Datos.Email, Datos.Telefono,
                Datos.NuevaContraseña, Datos.ContraseñaActual);
            if (error == null)
            {
                // El nombre está en la cookie de sesión: se vuelve a emitir con los datos nuevos.
                await Roles.IniciarSesionAsync(HttpContext, (await usuarios.ObtenerAsync(UsuarioId))!, conservarCarrito: true);
                Avisar("Guardamos tus cambios.", TipoMensaje.Exito);
                return RedirectToPage();
            }
            Avisar(error, TipoMensaje.Error);
        }
        Tarjetas = await tarjetas.ListarAsync(UsuarioId);
        return Page();
    }

    public async Task<IActionResult> OnPostEliminarTarjetaAsync(int id)
    {
        if (await tarjetas.EliminarAsync(UsuarioId, id))
        {
            Avisar("Eliminaste la tarjeta.", TipoMensaje.Exito);
        }
        else
        {
            Avisar("No se pudo eliminar la tarjeta.", TipoMensaje.Error);
        }
        return RedirectToPage();
    }
}
