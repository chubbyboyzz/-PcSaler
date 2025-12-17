using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PcSaler.Helpers
{
    public static class EncryptionHelper
    {
        // KHÓA BÍ MẬT: Ông có thể đổi chuỗi này, nhưng phải đủ 32 ký tự.
        // Tuyệt đối không được làm mất khóa này, mất là mất sạch dữ liệu.
        private static readonly string Key = "PcSaler_Secret_Key_2024_@Admin#1";

        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;

            // Nếu chuỗi đã được mã hóa rồi (tránh mã hóa kép) thì trả về luôn (cơ bản)
            // Nhưng tốt nhất cứ mã hóa mọi thứ đi vào.

            byte[] iv = new byte[16]; // IV rỗng (để đơn giản cho việc tìm kiếm chính xác nếu cần sau này)
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(Key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }
                        array = memoryStream.ToArray();
                    }
                }
            }
            return Convert.ToBase64String(array);
        }

        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;

            try
            {
                // Thử giải mã
                byte[] iv = new byte[16];
                byte[] buffer = Convert.FromBase64String(cipherText);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(Key);
                    aes.IV = iv;
                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (MemoryStream memoryStream = new MemoryStream(buffer))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                            {
                                return streamReader.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch
            {
                // MẸO QUAN TRỌNG:
                // Nếu giải mã lỗi (do dữ liệu cũ chưa mã hóa), trả về nguyên gốc.
                // Giúp web không bị chết khi chạy dữ liệu cũ.
                return cipherText;
            }
        }
    }
}