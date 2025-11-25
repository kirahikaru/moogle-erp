using System.Security.Cryptography;

namespace DataLayer.AuxComponents;
/// <summary>
/// Source: Encrypt & Decrypt a String in C#
/// Url: https://www.selamigungor.com/post/7/encrypt-decrypt-a-string-in-csharp (OBSOLETE FOR .NET 7)
/// 
/// https://code-maze.com/csharp-string-encryption-decryption/
/// </summary>
public static class CustomCipher
{
	public static string EncryptString(string key, string plainText)
	{
		byte[] iv = new byte[16];
		byte[] array;

		using (Aes aes = Aes.Create())
		{
			aes.Key = Encoding.UTF8.GetBytes(key);
			aes.IV = iv;

			ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

			using MemoryStream memoryStream = new();
			using CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write);
			using (StreamWriter streamWriter = new(cryptoStream))
			{
				streamWriter.Write(plainText);
			}

			array = memoryStream.ToArray();
		}

		return Convert.ToBase64String(array);
	}

	public static string DecryptString(string key, string cipherText)
	{
		byte[] iv = new byte[16];
		byte[] buffer = Convert.FromBase64String(cipherText);

		using Aes aes = Aes.Create();
		aes.Key = Encoding.UTF8.GetBytes(key);
		aes.IV = iv;
		ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

		using MemoryStream memoryStream = new(buffer);
		using CryptoStream cryptoStream = new(memoryStream, decryptor, CryptoStreamMode.Read);
		using StreamReader streamReader = new(cryptoStream);
		return streamReader.ReadToEnd();
	}

	//private readonly string _password;

	/*
    public CustomCipher(string password)
    {
        _password = password;
    }

    /// <summary>
    /// Encrypt when password already initialized
    /// </summary>
    /// <param name="plainText"></param>
    /// <returns></returns>
    public string Encrypt(string plainText)
    {
        return Encrypt(plainText, _password);
    }

    /// <summary>
    /// Decrypt when password already initialized
    /// </summary>
    /// <param name="plainText"></param>
    /// <returns></returns>
    public string Decrypt(string plainText)
    {
        return Decrypt(plainText, _password);
    }

    /// <summary>
    /// Encrypt a string.
    /// </summary>
    /// <param name="plainText">String to be encrypted</param>
    /// <param name="password">Password</param>
    public static string Encrypt(string plainText, string password)
    {
        ArgumentNullException.ThrowIfNull(plainText, nameof(plainText));

        if (password == null)
        {
            password = String.Empty;
        }

        // Get the bytes of the string
        var bytesToBeEncrypted = Encoding.UTF8.GetBytes(plainText);
        var passwordBytes = Encoding.UTF8.GetBytes(password);

        // Hash the password with SHA256
        using (SHA256 hasher = SHA256.Create())
        {
            passwordBytes = hasher.ComputeHash(passwordBytes);
        }
        var bytesEncrypted = Encrypt(bytesToBeEncrypted, passwordBytes);

        if (bytesEncrypted != null)
            return Convert.ToBase64String(bytesEncrypted);
        else
            return String.Empty;
    }

    /// <summary>
    /// Decrypt a string.
    /// </summary>
    /// <param name="encryptedText">String to be decrypted</param>
    /// <param name="password">Password used during encryption</param>
    /// <exception cref="FormatException"></exception>
    public static string Decrypt(string encryptedText, string password)
    {
        ArgumentNullException.ThrowIfNull(encryptedText, nameof(encryptedText));

        if (password == null)
        {
            password = String.Empty;
        }

        // Get the bytes of the string
        var bytesToBeDecrypted = Convert.FromBase64String(encryptedText);
        var passwordBytes = Encoding.UTF8.GetBytes(password);

        using (var hasher = SHA256.Create())
        {
            passwordBytes = hasher.ComputeHash(passwordBytes);
        }

        var bytesDecrypted = Decrypt(bytesToBeDecrypted, passwordBytes);

        if (bytesDecrypted != null)
            return Encoding.UTF8.GetString(bytesDecrypted);
        else
            return String.Empty;
    }

    private static byte[]? Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
    {
        byte[]? encryptedBytes = null;

        // Set your salt here, change it to meet your flavor:
        // The salt bytes must be at least 8 bytes.
        var saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

        using (MemoryStream ms = new())
        {
            //using RijndaelManaged AES = new();
            using var AES = Aes.Create("AesManaged");

            if (AES != null)
            {
                //CA5379: Ensure key derivation function algorithm is sufficiently strong
                using (var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000))
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;
                }

                using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                    cs.Close();
                }

                encryptedBytes = ms.ToArray();
            }
        }

        return encryptedBytes;
    }

    private static byte[]? Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
    {
        byte[]? decryptedBytes = null;

        // Set your salt here, change it to meet your flavor:
        // The salt bytes must be at least 8 bytes.
        var saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

        using (MemoryStream ms = new())
        {
            //using RijndaelManaged AES = new();
            using var AES = Aes.Create("AesManaged");

            if (AES != null)
            {
                using (var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000))
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);
                    AES.Mode = CipherMode.CBC;
                }

                using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                    cs.Close();
                }

                decryptedBytes = ms.ToArray();
            }
        }

        return decryptedBytes;
    }
    */
}