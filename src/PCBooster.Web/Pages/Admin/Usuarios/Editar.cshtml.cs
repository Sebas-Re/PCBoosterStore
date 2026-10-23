using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using PCBooster.Dominio;
using PCBooster.Negocio;
using PCBooster.Web.Infraestructura;

namespace PCBooster.Web.Pages.Admin.Usuarios;

/* Alta (sin id en la ruta) o modificación de un usuario. El admin puede crear
   otros admins; no necesita la contraseña actual del usuario para cambiarla. */
public class EditarModel(UsuariosService usuarios) : PaginaBase
{
    public class Formulario
    {
        [Required(ErrorMessage = "Ingresá un nombre."), StringLength(10, ErrorMessage = "Hasta 10 caracteres.")]
        public string Nombre { get; set; } = "";

        [Required(ErrorMessage = "Ingresá un e-mail."), EmailAddress(ErrorMessage = "Ingresá un e-mail válido."), StringLength(30)]
        public string Email { get; set; } = "";

        [RegularExpression(@"^\d*$", ErrorMessage = "Ingresá solo números."), StringLength(15)]
        public string? Telefono { get; set; }

        public bool EsAdmin { get; set; }
        public bool Activo { get; set; } = true;

        [DataType(DataType.Password), StringLength(UsuariosService.LargoMaximoContraseña)]
        public string? Contraseña { get; set; }
    }

    /* Vacío = usuario nuevo. */
    [BindProperty(SupportsGet = true)]
    public int? Id { get; set; }

    [BindProperty]
    public Formulario Datos { get; set; } = new();

    public bool EsNuevo => Id == null;
    public bool EsElMismo => Id == UsuarioId;

    public async Task<IActionResult> OnGetAsync()
    {
        if (EsNuevo)
        {
            return Page();
        }
        Usuario? usuario = await usuarios.ObtenerAsync(Id!.Value);
        if (usuario == null)
        {
            return NotFound();
        }
        Datos = new Formulario
        {
            Nombre = usuario.Nombre,
            Email = usuario.Email,
            Telefono = usuario.Telefono == "Sin telefono" ? "" : usuario.Telefono,
            EsAdmin = usuario.EsAdmin,
            Activo = usuario.Activo
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (EsNuevo && string.IsNullOrEmpty(Datos.Contraseña))
        {
            ModelState.AddModelError("Datos.Contraseña", "Ingresá una contraseña.");
        }
        if (!ModelState.IsValid)
        {
            return Page();
        }
        // Un admin no puede quitarse a sí mismo el rol ni desactivarse (se quedaría afuera).
        if (EsElMismo)
        {
            Datos.EsAdmin = true;
            Datos.Activo = true;
        }
        string? error = EsNuevo
            ? await usuarios.RegistrarAsync(Datos.Nombre, Datos.Email, Datos.Telefono, Datos.Contraseña!, Datos.EsAdmin)
            : await usuarios.ModificarAsync(Id!.Value, Datos.Nombre, Datos.Email, Datos.Telefono, Datos.EsAdmin, Datos.Activo, Datos.Contraseña);
        if (error != null)
        {
            Avisar(error, TipoMensaje.Error);
            return Page();
        }
        if (EsElMismo)
        {
            // Su nombre va en la cookie de sesión: se vuelve a emitir.
            await Roles.IniciarSesionAsync(HttpContext, (await usuarios.ObtenerAsync(UsuarioId))!, conservarCarrito: true);
        }
        Avisar($"{(Datos.EsAdmin ? "Administrador" : "Cliente")} {Datos.Nombre.Trim()} {(EsNuevo ? "agregado" : "modificado")}.", TipoMensaje.Exito);
        return RedirectToPage("Index");
    }
}
