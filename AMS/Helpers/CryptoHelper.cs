//using System.Numerics;
//using System.Security.Cryptography;
//using System.Text;

//namespace CRM.Helpers
//{
//    public class CryptoHelper
//    {
//        private static readonly string EncryptionKey = "47b6769de903188ab324534295449d19";

//        public static string EncryptId(int id)
//        {
//            return Encrypt(id.ToString());
//        }

//        public static int DecryptId(string encryptedId)
//        {
//            var decryptedText = Decrypt(encryptedId);
//            return int.Parse(decryptedText);
//        }

//        public static string Encrypt(string plainText)
//        {
//            using (var aes = Aes.Create())
//            {
//                var encryptor = aes.CreateEncryptor(Encoding.UTF8.GetBytes(EncryptionKey), aes.IV);
//                using (var ms = new MemoryStream())
//                {
//                    ms.Write(aes.IV, 0, aes.IV.Length);
//                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
//                    {
//                        using (var sw = new StreamWriter(cs))
//                        {
//                            sw.Write(plainText);
//                        }
//                    }
//                    var encryptedBytes = ms.ToArray();
//                    // Convert to Base64 and make URL safe
//                    return Convert.ToBase64String(encryptedBytes)
//                                  .Replace("+", "-")  // URL-safe replacement
//                                  .Replace("/", "_")
//                                  .Replace("=", "");
//                }
//            }
//        }

//        public static string Decrypt(string cipherText)
//        {
//            if (string.IsNullOrEmpty(cipherText))
//            {
//                throw new ArgumentNullException(nameof(cipherText), "The cipherText cannot be null or empty.");
//            }
//            string incoming = cipherText.Replace("-", "+").Replace("_", "/");
//            switch (incoming.Length % 4)
//            {
//                case 2: incoming += "=="; break;
//                case 3: incoming += "="; break;
//            }

//            var fullCipher = Convert.FromBase64String(incoming);
//            using (var aes = Aes.Create())
//            {
//                var iv = new byte[aes.IV.Length];
//                var cipher = new byte[fullCipher.Length - iv.Length];
//                Array.Copy(fullCipher, iv, iv.Length);
//                Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);
//                var decryptor = aes.CreateDecryptor(Encoding.UTF8.GetBytes(EncryptionKey), iv);
//                using (var ms = new MemoryStream(cipher))
//                {
//                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
//                    {
//                        using (var sr = new StreamReader(cs))
//                        {
//                            return sr.ReadToEnd();
//                        }
//                    }
//                }
//            }
//        }
//    }
//}
