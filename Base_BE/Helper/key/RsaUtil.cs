namespace Base_BE.Helper.key
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Text;

    public static class RsaUtil
    {
        // Encrypt content using a public key
        public static string Encrypt(string content, string publicKey)
        {
            using (var rsa = RSA.Create())
            {
                rsa.ImportFromPem(publicKey);
                byte[] contentBytes = Encoding.UTF8.GetBytes(content);
                byte[] encryptedBytes = rsa.Encrypt(contentBytes, RSAEncryptionPadding.Pkcs1);
                return Convert.ToBase64String(encryptedBytes);
            }
        }

        // Decrypt content using a private key
        public static string Decrypt(string cipherContent, string privateKey)
        {
            using (var rsa = RSA.Create())
            {
                rsa.ImportFromPem(privateKey);
                byte[] cipherBytes = Convert.FromBase64String(cipherContent);
                byte[] decryptedBytes = rsa.Decrypt(cipherBytes, RSAEncryptionPadding.Pkcs1);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }


        // Encode a key to Base64 string
        public static string EncodeKey(RSA rsaKey, bool isPrivateKey)
        {
            if (isPrivateKey)
            {
                return Convert.ToBase64String(rsaKey.ExportRSAPrivateKey());
            }
            else
            {
                return Convert.ToBase64String(rsaKey.ExportRSAPublicKey());
            }
        }

        // Decode a public key from Base64 string
        public static RSA DecodePublicKey(string publicKey)
        {
            var rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKey), out _);
            return rsa;
        }

        // Decode a private key from Base64 string
        public static RSA DecodePrivateKey(string privateKey)
        {
            var rsa = RSA.Create();
            rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(privateKey), out _);
            return rsa;
        }

        // Generate a public-private key pair
        public static Dictionary<string, string> GenerateKeyPair()
        {
            using (var rsa = RSA.Create())
            {
                rsa.KeySize = 4096;
                var keyPair = new Dictionary<string, string>
                {
                    ["publicKey"] = Convert.ToBase64String(rsa.ExportRSAPublicKey()),
                    ["privateKey"] = Convert.ToBase64String(rsa.ExportRSAPrivateKey())
                };
                return keyPair;
            }
        }
    }

}
