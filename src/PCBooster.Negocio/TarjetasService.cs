using Microsoft.EntityFrameworkCore;
using PCBooster.Datos;
using PCBooster.Dominio;

namespace PCBooster.Negocio;

/* Tarjetas guardadas de cada cliente. El código de seguridad no se pide ni se guarda. */
public class TarjetasService(TiendaContext db)
{
    public Task<List<Tarjeta>> ListarAsync(int usuarioId) =>
        db.Tarjetas.AsNoTracking().Where(t => t.UsuarioId == usuarioId).OrderBy(t => t.Id).ToListAsync();

    public async Task<string?> AgregarAsync(Tarjeta tarjeta)
    {
        string digitos = tarjeta.Numero.ToString();
        if (tarjeta.Numero <= 0 || digitos.Length < 13 || digitos.Length > 19)
        {
            return "El número de tarjeta tiene que tener entre 13 y 19 dígitos.";
        }
        if (string.IsNullOrWhiteSpace(tarjeta.Titular) || string.IsNullOrWhiteSpace(tarjeta.Nombre))
        {
            return "Completá el titular y el nombre de la tarjeta.";
        }
        // Mismo criterio que el trigger tr_Tarjetas_Fecha_Vencimiento de la base.
        if (!Vigente(tarjeta.Vencimiento))
        {
            return "La tarjeta está vencida.";
        }
        if (await db.Tarjetas.AnyAsync(t => t.UsuarioId == tarjeta.UsuarioId && t.Numero == tarjeta.Numero))
        {
            return "Ya tenías guardada esa tarjeta.";
        }
        tarjeta.Id = 0;
        tarjeta.Titular = tarjeta.Titular.Trim();
        tarjeta.Nombre = tarjeta.Nombre.Trim();
        db.Tarjetas.Add(tarjeta);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Restricción de la base (por ejemplo, la misma tarjeta guardada en paralelo).
            db.Entry(tarjeta).State = EntityState.Detached;
            return "No se pudo guardar la tarjeta.";
        }
        return null;
    }

    /* La tarjeta se guarda con el primer día del mes de vencimiento y vale mientras esa fecha sea futura. */
    public static bool Vigente(DateOnly vencimiento) => vencimiento > DateOnly.FromDateTime(DateTime.Today);

    /* Solo dentro de las tarjetas del usuario: no se puede borrar una ajena. */
    public async Task<bool> EliminarAsync(int usuarioId, int tarjetaId) =>
        await db.Tarjetas.Where(t => t.Id == tarjetaId && t.UsuarioId == usuarioId).ExecuteDeleteAsync() == 1;
}
