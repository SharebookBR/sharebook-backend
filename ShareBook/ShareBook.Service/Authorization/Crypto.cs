using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ShareBook.Service.Authorization;

public class Crypto : ICrypto
{

    private readonly IConfiguration _configuration;
    public string _secret { get; set; }

    public Crypto(IConfiguration configuration)
    {
        _configuration = configuration;
        _secret = _configuration["TokenConfigurations:SecretJwtKey"];
    }

    public string Encrypt(string input)
    {
        // Converte a chave secreta para bytes
        byte[] key = GetKey(_secret);

        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.GenerateIV(); // Gera um IV aleatório

            using (MemoryStream ms = new MemoryStream())
            {
                // Escreve o IV no início do stream
                ms.Write(aes.IV, 0, aes.IV.Length);

                using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                    cs.Write(inputBytes, 0, inputBytes.Length);
                    cs.FlushFinalBlock();
                }

                // Retorna a string criptografada (Base64)
                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    public string Decrypt(string input)
    {
        byte[] key = GetKey(_secret);
        byte[] inputBytes = Convert.FromBase64String(input);

        using (Aes aes = Aes.Create())
        {
            aes.Key = key;

            using (MemoryStream ms = new MemoryStream(inputBytes))
            {
                // Lê o IV do início do stream
                byte[] iv = new byte[16]; // O tamanho do IV para AES é 16 bytes
                ms.Read(iv, 0, iv.Length);
                aes.IV = iv;

                using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    using (StreamReader sr = new StreamReader(cs))
                    {
                        // Retorna o texto descriptografado
                        return sr.ReadToEnd();
                    }
                }
            }
        }
    }

    private byte[] GetKey(string secret)
    {
        // Garante que a chave terá 32 bytes (256 bits) para AES
        using (SHA256 sha256 = SHA256.Create())
        {
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(secret));
        }
    }
}
