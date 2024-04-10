using System.Text;
using System;
using System.Security.Cryptography;

namespace Netmarble.Core
{
    public class Encrypter
    {
        // private static string _saltForKey;

        private static byte[] _keys;
        private static byte[] _iv;
        private static int keySize = 256;
        private static int blockSize = 128;
        //private static int _hashLen = 32;

        static Encrypter()
        {
            // 8 바이트로 하고, 변경해서 쓸것
            byte[] saltBytes = new byte[] { 43, 55, 22, 98, 38, 10, 52, 85 };

            // 길이 상관 없고, 키를 만들기 위한 용도로 씀
            // string randomSeedForKey = "5b6ecb4adc0a42acae649eb845ac06ec";

            // 길이 상관 없고, aes에 쓸 key 와 iv 를 만들 용도
            string randomSeedForValue = "4e327725789841b5bfadcb2ada97";

            /*{
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(randomSeedForKey, saltBytes, 1000);
                _saltForKey = System.Convert.ToBase64String(key.GetBytes(blockSize / 8));
            }*/

            {
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(randomSeedForValue, saltBytes, 1000);
                _keys = key.GetBytes(keySize / 8);
                _iv = key.GetBytes(blockSize / 8);
            }
        }

        public static string MakeHash(string original)
        {
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(original);
                byte[] hashBytes = md5.ComputeHash(bytes);

                string hashToString = "";
                for (int i = 0; i < hashBytes.Length; ++i)
                    hashToString += hashBytes[i].ToString("x2");

                return hashToString;
            }
        }

        public static string GenerateSHA256(string original)
        {
            using (SHA256Managed sha256 = new SHA256Managed())
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(original);
                byte[] hashBytes = sha256.ComputeHash(bytes);

                string hashToString = "";
                for (int i = 0; i < hashBytes.Length; ++i)
                    hashToString += hashBytes[i].ToString("x2");

                return hashToString;
            }
        }

        public static string PBKDF2Sha256GetBytes(string original, string salt)
        {
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(original + salt);
            byte[] saltBytes = System.Text.Encoding.UTF8.GetBytes(salt);
            int iterationCount = 1000;
            int dklen = 32;
            using (var hmac = new System.Security.Cryptography.HMACSHA256(passwordBytes))
            {
                int hashLength = hmac.HashSize / 8;
                if ((hmac.HashSize & 7) != 0)
                    hashLength++;
                int keyLength = dklen / hashLength;
                if ((long)dklen > (0xFFFFFFFFL * hashLength) || dklen < 0)
                    throw new ArgumentOutOfRangeException("dklen");
                if (dklen % hashLength != 0)
                    keyLength++;
                byte[] extendedkey = new byte[saltBytes.Length + 4];
                Buffer.BlockCopy(saltBytes, 0, extendedkey, 0, saltBytes.Length);
                using (var ms = new System.IO.MemoryStream())
                {
                    for (int i = 0; i < keyLength; i++)
                    {
                        extendedkey[saltBytes.Length] = (byte)(((i + 1) >> 24) & 0xFF);
                        extendedkey[saltBytes.Length + 1] = (byte)(((i + 1) >> 16) & 0xFF);
                        extendedkey[saltBytes.Length + 2] = (byte)(((i + 1) >> 8) & 0xFF);
                        extendedkey[saltBytes.Length + 3] = (byte)(((i + 1)) & 0xFF);
                        byte[] u = hmac.ComputeHash(extendedkey);
                        Array.Clear(extendedkey, saltBytes.Length, 4);
                        byte[] f = u;
                        for (int j = 1; j < iterationCount; j++)
                        {
                            u = hmac.ComputeHash(u);
                            for (int k = 0; k < f.Length; k++)
                            {
                                f[k] ^= u[k];
                            }
                        }

                        ms.Write(f, 0, f.Length);
                        Array.Clear(u, 0, u.Length);
                        Array.Clear(f, 0, f.Length);
                    }

                    byte[] dk = new byte[dklen];
                    ms.Position = 0;
                    ms.Read(dk, 0, dklen);
                    ms.Position = 0;
                    for (long i = 0; i < ms.Length; i++)
                    {
                        ms.WriteByte(0);
                    }

                    Array.Clear(extendedkey, 0, extendedkey.Length);
                    return Convert.ToBase64String(dk);
                }
            }
        }

        public static byte[] Encrypt(byte[] bytesToBeEncrypted)
        {
            using (RijndaelManaged aes = new RijndaelManaged())
            {
                aes.KeySize = keySize;
                aes.BlockSize = blockSize;

                aes.Key = _keys;
                aes.IV = _iv;

                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform ct = aes.CreateEncryptor())
                {
                    return ct.TransformFinalBlock(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                }
            }
        }

        public static byte[] Decrypt(byte[] bytesToBeDecrypted)
        {
            using (RijndaelManaged aes = new RijndaelManaged())
            {
                aes.KeySize = keySize;
                aes.BlockSize = blockSize;

                aes.Key = _keys;
                aes.IV = _iv;

                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform ct = aes.CreateDecryptor())
                {
                    return ct.TransformFinalBlock(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                }
            }
        }

        public static string Encrypt(string input)
        {
            byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(input);
            byte[] bytesEncrypted = Encrypt(bytesToBeEncrypted);

            return System.Convert.ToBase64String(bytesEncrypted);
        }

        public static string Decrypt(string input)
        {
            byte[] bytesToBeDecrypted = System.Convert.FromBase64String(input);
            byte[] bytesDecrypted = Decrypt(bytesToBeDecrypted);

            return Encoding.UTF8.GetString(bytesDecrypted);
        }

        /*
        private static void SetSecurityValue(string key, string value)
        {
            string hideKey = MakeHash(key + _saltForKey);
            string encryptValue = Encrypt(value + MakeHash(value));

            PlayerPrefs.SetString(hideKey, encryptValue);
        }

        private static string GetSecurityValue(string key)
        {
            string hideKey = MakeHash(key + _saltForKey);

            string encryptValue = PlayerPrefs.GetString(hideKey);
            if (true == string.IsNullOrEmpty(encryptValue))
                return string.Empty;

            string valueAndHash = Decrypt(encryptValue);
            if (_hashLen > valueAndHash.Length)
                return string.Empty;

            string savedValue = valueAndHash.Substring(0, valueAndHash.Length - _hashLen);
            string savedHash = valueAndHash.Substring(valueAndHash.Length - _hashLen);

            if (MakeHash(savedValue) != savedHash)
                return string.Empty;

            return savedValue;
        }*/
    }
}