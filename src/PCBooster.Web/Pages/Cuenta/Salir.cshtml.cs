using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PCBooster.Web.Pages.Cuenta;

/* Solo por POST (con token antiforgery): un GET no cierra la sesión. */
public class SalirModel : PageModel
{
    public IActionResult OnGet() => RedirectToPage("/Catalogo");

    public async Task<IActionResult> OnPostAsync()
    {
        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToPage("/Index");
    }
}
