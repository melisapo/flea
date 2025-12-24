using System.Security.Cryptography;
using System.Text;

namespace flea_WebProj.Services
{
    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string password, string passwordHash);
    }

    public class PasswordHasher : IPasswordHasher
    {
        // Hashea una contraseña usando SHA256 con salt
        public string Hash(string password)
        {
            // Generar un salt aleatorio de 16 bytes
            var salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Combinar password + salt y hashear con SHA256
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var saltedPassword = new byte[passwordBytes.Length + salt.Length];

            // Copiar password y salt al array combinado
            Buffer.BlockCopy(passwordBytes, 0, saltedPassword, 0, passwordBytes.Length);
            Buffer.BlockCopy(salt, 0, saltedPassword, passwordBytes.Length, salt.Length);

            // Hashear
            var hash = SHA256.HashData(saltedPassword);

            // Combinar salt + hash para guardar en BD
            var hashWithSalt = new byte[salt.Length + hash.Length];
            Buffer.BlockCopy(salt, 0, hashWithSalt, 0, salt.Length);
            Buffer.BlockCopy(hash, 0, hashWithSalt, salt.Length, hash.Length);

            // Convertir a Base64 para guardar como string
            return Convert.ToBase64String(hashWithSalt);
        }

        // Verifica si una contraseña coincide con el hash guardado
        public bool Verify(string password, string passwordHash)
        {
            try
            {
                // Convertir el hash guardado de Base64 a bytes
                var hashWithSalt = Convert.FromBase64String(passwordHash);

                // Extraer el salt (primeros 16 bytes)
                var salt = new byte[16];
                Buffer.BlockCopy(hashWithSalt, 0, salt, 0, 16);

                // Extraer el hash guardado (resto de bytes)
                var storedHash = new byte[hashWithSalt.Length - 16];
                Buffer.BlockCopy(hashWithSalt, 16, storedHash, 0, storedHash.Length);

                // Hashear la contraseña ingresada con el mismo salt
                var passwordBytes = Encoding.UTF8.GetBytes(password);
                var saltedPassword = new byte[passwordBytes.Length + salt.Length];

                Buffer.BlockCopy(passwordBytes, 0, saltedPassword, 0, passwordBytes.Length);
                Buffer.BlockCopy(salt, 0, saltedPassword, passwordBytes.Length, salt.Length);

                var computedHash = SHA256.HashData(saltedPassword);

                // Comparar el hash computado con el hash guardado
                return CompareHashes(computedHash, storedHash);
            }
            catch
            {
                return false;
            }
        }

        // Compara dos arrays de bytes de forma segura (previene timing attacks)
        private static bool CompareHashes(byte[] hash1, byte[] hash2)
        {
            if (hash1.Length != hash2.Length)
                return false;

            var result = 0;
            for (var i = 0; i < hash1.Length; i++)
            {
                result |= hash1[i] ^ hash2[i];
            }

            return result == 0;
        }
    }
}