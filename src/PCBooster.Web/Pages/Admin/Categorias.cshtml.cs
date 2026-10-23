using Microsoft.AspNetCore.Mvc;
using PCBooster.Dominio;
using PCBooster.Negocio;
using PCBooster.Web.Infraestructura;

namespace PCBooster.Web.Pages.Admin;

/* Alta y edición en línea: cada fila es su propio formulario. */
public class CategoriasModel(CatalogoService catalogo) : PaginaBase
{
    public List<Categoria> Categorias { get; private set; } = new();

    public async Task OnGetAsync() => Categorias = await catalogo.CategoriasAsync();

    public async Task<IActionResult> OnPostAgregarAsync(string? descripcion)
    {
        string? error = await catalogo.AgregarCategoriaAsync(descripcion ?? "");
        Avisar(error ?? $"Categoría \"{descripcion!.Trim()}\" agregada.", error == null ? TipoMensaje.Exito : TipoMensaje.Error);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostGuardarAsync(int id, string? descripcion, bool activa)
    {
        string? error = await catalogo.ModificarCategoriaAsync(id, descripcion ?? "", activa);
        Avisar(error ?? "Categoría modificada.", error == null ? TipoMensaje.Exito : TipoMensaje.Error);
        return RedirectToPage();
    }
}
