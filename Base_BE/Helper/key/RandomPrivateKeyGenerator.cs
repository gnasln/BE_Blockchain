using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Base_BE.Helper.key
{
    public static class RandomPrivateKeyGenerator
    {
        private const int LeftLimit = 48; // ASCII '0'
        private const int RightLimit = 102; // ASCII 'f'
        private const int TargetStringLength = 64; // Private key length in hex

        // Generate a random private key
        public static string GetRandomPrivateKey()
        {
            var random = new Random();
            var privateKey = new StringBuilder();

            while (privateKey.Length < TargetStringLength)
            {
                // Generate random character within range
                var randomChar = (char)random.Next(LeftLimit, RightLimit + 1);

                // Filter to ensure character is valid for hexadecimal (0-9, a-f)
                if ((randomChar >= '0' && randomChar <= '9') || (randomChar >= 'a' && randomChar <= 'f'))
                {
                    privateKey.Append(randomChar);
                }
            }

            string generatedKey = privateKey.ToString();

            // Validate the generated key
            if (generatedKey.Length != TargetStringLength || !IsHex(generatedKey))
            {
                throw new InvalidOperationException("Generated private key is not valid.");
            }

            return generatedKey;
        }

        private static bool IsHex(string value)
        {
            foreach (char c in value)
            {
                if (!Uri.IsHexDigit(c))
                {
                    return false;
                }
            }
            return true;
        }

        // Generate key pair from a private key
        public static Dictionary<string, string> GenerateKeyPair(string privateKey)
        {
            if (string.IsNullOrEmpty(privateKey))
            {
                throw new ArgumentException("Private key cannot be null or empty.");
            }

            // Strip prefix if present
            privateKey = privateKey.StartsWith("0x") ? privateKey.Substring(2) : privateKey;

            if (privateKey.Length != 64 || !IsHex(privateKey))
            {
                throw new FormatException($"Provided private key is not a valid 64-character hexadecimal string. Provided key: {privateKey}");
            }

            var keyPair = new Dictionary<string, string>();

            try
            {
                // Generate Ethereum EC Key Pair
                var ecKey = new EthECKey(privateKey);

                // Add private and public keys to dictionary
                keyPair["privateKey"] = ecKey.GetPrivateKeyAsBytes().ToHex();
                keyPair["publicKey"] = ecKey.GetPubKeyNoPrefix().ToHex();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to generate key pair.", ex);
            }

            return keyPair;
        }
    }
}
