using System.Security.Cryptography;

namespace PCBooster.Datos;

/* Hash de contraseñas con PBKDF2-SHA256 y salt aleatorio. Se guarda como
   "pbkdf2$iteraciones$salt$hash" (salt y hash en Base64): cada hash recuerda sus
   iteraciones, así se pueden subir sin invalidar las contraseñas existentes
   (las bases del sitio WebForms usaban 100.000). */
public static class Contrasenas
{
    const string Prefijo = "pbkdf2$";
    const int BytesSalt = 16;
    const int BytesHash = 32;

    /* Lo que recomienda OWASP para PBKDF2-SHA256. Los tests la bajan. */
    public static int Iteraciones { get; set; } = 600_000;

    public static string Hashear(string contraseña)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(BytesSalt);
        byte[] hash = Derivar(contraseña, salt, Iteraciones);
        return $"{Prefijo}{Iteraciones}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public static bool Verificar(string? contraseña, string? guardada)
    {
        if (contraseña == null || !EstaHasheada(guardada))
        {
            return false;
        }
        string[] partes = guardada!.Split('$');
        if (partes.Length != 4 || !int.TryParse(partes[1], out int iteraciones))
        {
            return false;
        }
        byte[] salt = Convert.FromBase64String(partes[2]);
        byte[] esperado = Convert.FromBase64String(partes[3]);
        return CryptographicOperations.FixedTimeEquals(Derivar(contraseña, salt, iteraciones), esperado);
    }

    /* Para que un usuario inexistente tarde lo mismo que uno con contraseña
       incorrecta (si no, el tiempo de respuesta revela qué usuarios existen). */
    public static void SimularVerificacion(string? contraseña)
    {
        Derivar(contraseña ?? "", new byte[BytesSalt], Iteraciones);
    }

    public static bool EstaHasheada(string? valor) => valor != null && valor.StartsWith(Prefijo, StringComparison.Ordinal);

    static byte[] Derivar(string contraseña, byte[] salt, int iteraciones) =>
        Rfc2898DeriveBytes.Pbkdf2(contraseña, salt, iteraciones, HashAlgorithmName.SHA256, BytesHash);
}
