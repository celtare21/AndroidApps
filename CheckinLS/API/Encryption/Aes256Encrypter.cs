using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace CheckinLS.API.Encryption
{
    public static class Aes256Encrypter
    {
        private static readonly Encoding EncodeType = Encoding.UTF8;

        public static string GenerateKey()
        {
            using (var aes = new RijndaelManaged())
            {
                aes.KeySize = 128;
                aes.BlockSize = 128;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                aes.GenerateKey();

                var keyStr = Convert.ToBase64String(aes.Key);

                return Convert.ToBase64String(EncodeType.GetBytes(keyStr));
            }
        }

        public static string Encrypt(string plainText, string key)
        {
            using (var aes = new RijndaelManaged())
            {
                aes.KeySize = 128;
                aes.BlockSize = 128;
                aes.Padding = PaddingMode.PKCS7;
                aes.Mode = CipherMode.CBC;
                aes.Key = EncodeType.GetBytes(key);

                aes.GenerateIV();

                var aesEncrypt = aes.CreateEncryptor(aes.Key, aes.IV);
                var buffer = EncodeType.GetBytes(plainText);

                var encryptedText = Convert.ToBase64String(aesEncrypt.TransformFinalBlock(buffer, 0, buffer.Length));

                var mac = BitConverter.ToString(HmacSha256($"{Convert.ToBase64String(aes.IV)}{encryptedText}", key))
                    .Replace("-", string.Empty).ToLower();

                var keyValues = new Dictionary<string, string>
                {
                    {"iv", Convert.ToBase64String(aes.IV)},
                    {"value", encryptedText},
                    {"mac", mac}
                };

                return Convert.ToBase64String(EncodeType.GetBytes(JsonConvert.SerializeObject(keyValues)));
            }
        }

        public static string Decrypt(string plainText, string key)
        {
            using (var aes = new RijndaelManaged())
            {
                aes.KeySize = 128;
                aes.BlockSize = 128;
                aes.Padding = PaddingMode.PKCS7;
                aes.Mode = CipherMode.CBC;
                aes.Key = EncodeType.GetBytes(key);

                var base64Decoded = Convert.FromBase64String(plainText);
                var base64DecodedStr = EncodeType.GetString(base64Decoded);

                var payload = JsonConvert.DeserializeObject<Dictionary<string, string>>(base64DecodedStr);

                aes.IV = Convert.FromBase64String(payload["iv"]);

                var aesDecrypt = aes.CreateDecryptor(aes.Key, aes.IV);
                var buffer = Convert.FromBase64String(payload["value"]);

                return EncodeType.GetString(aesDecrypt.TransformFinalBlock(buffer, 0, buffer.Length));
            }
        }

        private static byte[] HmacSha256(string data, string key)
        {
            using (var hmac = new HMACSHA256(EncodeType.GetBytes(key)))
            {
                return hmac.ComputeHash(EncodeType.GetBytes(data));
            }
        }
    }
}