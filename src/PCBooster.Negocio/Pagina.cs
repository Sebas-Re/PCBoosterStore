using Microsoft.EntityFrameworkCore;

namespace PCBooster.Negocio;

/* Una página de resultados. */
public record Pagina<T>(IReadOnlyList<T> Items, int Total, int Numero, int TamañoPagina)
{
    public int TotalPaginas => Consultas.CantidadPaginas(Total, TamañoPagina);
}

static class Consultas
{
    public static int CantidadPaginas(int total, int tamañoPagina) => Math.Max(1, (int)Math.Ceiling(total / (double)tamañoPagina));

    /* Cuenta los resultados y trae solo la página pedida, todo en la base. La
       consulta tiene que venir ordenada. Fuera de rango, devuelve la página más cercana. */
    public static async Task<Pagina<T>> PaginarAsync<T>(this IQueryable<T> consulta, int pagina, int tamañoPagina)
    {
        int total = await consulta.CountAsync();
        int numero = Math.Clamp(pagina, 1, CantidadPaginas(total, tamañoPagina));
        List<T> items = await consulta.Skip((numero - 1) * tamañoPagina).Take(tamañoPagina).ToListAsync();
        return new Pagina<T>(items, total, numero, tamañoPagina);
    }

    public const string Escape = "\\";

    /* Patrón LIKE (con Escape) para buscar "texto" en cualquier parte. LIKE de
       SQLite no distingue mayúsculas; % y _ del texto se buscan literales. */
    public static string Contiene(string texto) =>
        "%" + texto.Trim().Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_") + "%";
}
