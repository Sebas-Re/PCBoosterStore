using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using PCBooster.Dominio;
using PCBooster.Negocio;
using PCBooster.Web.Infraestructura;

namespace PCBooster.Web.Pages.Cuenta;

public class RegistroModel(UsuariosService usuarios) : PaginaBase
{
    public class Formulario
    {
        [Required(ErrorMessage = "Ingresá un nombre de usuario."), StringLength(10, ErrorMessage = "Hasta 10 caracteres.")]
        public string Nombre { get; set; } = "";

        [Required(ErrorMessage = "Ingresá un e-mail."), EmailAddress(ErrorMessage = "Ingresá un e-mail válido."), StringLength(30)]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Ingresá una contraseña.")]
        [StringLength(UsuariosService.LargoMaximoContraseña, MinimumLength = UsuariosService.LargoMinimoContraseña,
            ErrorMessage = "La contraseña tiene que tener entre 6 y 64 caracteres.")]
        [DataType(DataType.Password)]
        public string Contraseña { get; set; } = "";

        [Required(ErrorMessage = "Repetí la contraseña."), Compare(nameof(Contraseña), ErrorMessage = "Las contraseñas no coinciden.")]
        [DataType(DataType.Password)]
        public string RepetirContraseña { get; set; } = "";

        [RegularExpression(@"^\d*$", ErrorMessage = "Ingresá solo números."), StringLength(15)]
        public string? Telefono { get; set; }
    }

    [BindProperty]
    public Formulario Datos { get; set; } = new();

    public IActionResult OnGet() => User.Identity?.IsAuthenticated == true ? RedirectToPage("/Catalogo") : Page();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        string? error = await usuarios.RegistrarAsync(Datos.Nombre, Datos.Email, Datos.Telefono, Datos.Contraseña);
        if (error != null)
        {
            Avisar(error, TipoMensaje.Error);
            return Page();
        }
        Usuario usuario = (await usuarios.LoginAsync(Datos.Nombre, Datos.Contraseña))!;
        await Roles.IniciarSesionAsync(HttpContext, usuario);
        Avisar($"¡Bienvenido/a, {usuario.Nombre}! Tu cuenta ya está lista.", TipoMensaje.Exito);
        return RedirectToPage("/Catalogo");
    }
}
