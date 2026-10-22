namespace PCBooster.Dominio;

public class Articulo
{
    /* Código de 1 a 4 letras o números (por ejemplo "gb1"); sin distinguir mayúsculas. */
    public string Codigo { get; set; } = "";

    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    public string Descripcion { get; set; } = "";
    public int Stock { get; set; }
    public decimal Precio { get; set; }

    /* Ruta del sitio ("~/Imagenes_ar/...") o dirección http(s). */
    public string? Imagen { get; set; }

    /* Baja lógica: un artículo inactivo no se muestra ni se vende. */
    public bool Activo { get; set; } = true;

    // Detalle del producto (modal del catálogo).
    public string? Marca { get; set; }
    public string? DescripcionLarga { get; set; }

    /* Una por línea, con el formato "Clave: Valor". */
    public string? Especificaciones { get; set; }

    public int? GarantiaMeses { get; set; }

    /* Pares (clave, valor) de Especificaciones, en orden. */
    public IReadOnlyList<KeyValuePair<string, string>> ListaEspecificaciones()
    {
        var pares = new List<KeyValuePair<string, string>>();
        foreach (string linea in (Especificaciones ?? "").Split('\n'))
        {
            int separador = linea.IndexOf(':');
            if (separador > 0)
            {
                pares.Add(new(linea[..separador].Trim(), linea[(separador + 1)..].Trim()));
            }
        }
        return pares;
    }
}
