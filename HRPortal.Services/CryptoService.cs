using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace HRPortal.Services
{
    public class CryptoService
    {

        public static AESKey GenerateAES()
        {
            byte[] Key;
            byte[] IV;
            using (Aes newAES = Aes.Create())
            {
                Key = newAES.Key;
                IV = newAES.IV;
            }
            return new AESKey() { Key = Key, IV = IV };
        }

        public static string ByteToHexBitFiddle(byte[] bytes)
        {
            char[] c = new char[bytes.Length * 2];
            int b;
            for (int i = 0; i < bytes.Length; i++)
            {
                b = bytes[i] >> 4;
                c[i * 2] = (char)(55 + b + (((b - 10) >> 31) & -7));
                b = bytes[i] & 0xF;
                c[i * 2 + 1] = (char)(55 + b + (((b - 10) >> 31) & -7));
            }
            return new string(c);
        }

        public static byte[] HexToByteArray(string input)
        {
            var outputLength = input.Length / 2;
            var output = new byte[outputLength];
            using (var sr = new StringReader(input))
            {
                for (var i = 0; i < outputLength; i++)
                    output[i] = Convert.ToByte(new string(new char[2] { (char)sr.Read(), (char)sr.Read() }), 16);
            }
            return output;
        }

        public static string GetRandomString(int length)
        {
            var str = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            var next = new Random();
            var builder = new StringBuilder();
            for (var i = 0; i < length - 1; i++)
            {
                builder.Append(str[next.Next(0, str.Length)]);
            }
            return builder.ToString();
        }

        public static string EncryptString(AESKey EncKey, string Target)
        {
            string result = "";
            using (Aes newAES = Aes.Create())
            {
                newAES.Key = EncKey.Key;
                newAES.IV = EncKey.IV;
                ICryptoTransform encryptor = newAES.CreateEncryptor(newAES.Key, newAES.IV);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(Target);
                        }
                        result = ByteToHexBitFiddle(ms.ToArray());
                    }
                }
            }
            return result;
        }

        public static string DecryptString(AESKey DecKey, string Target)
        {
            string result = "";
            using (Aes newAES = Aes.Create())
            {
                newAES.Key = DecKey.Key;
                newAES.IV = DecKey.IV;
                ICryptoTransform decryptor = newAES.CreateDecryptor(newAES.Key, newAES.IV);
                using (MemoryStream ms=new MemoryStream(HexToByteArray(Target)))
                {
                    using(CryptoStream cs=new CryptoStream(ms,decryptor,CryptoStreamMode.Read))
                    {
                        using (StreamReader sr =new StreamReader(cs))
                        {
                            result = sr.ReadToEnd();
                        }
                    }
                }
            }
            return result;
        }

    
    }

    public class AESKey
    {
        public byte[] Key { get; set; }
        public byte[] IV { get; set; }
    }

    

}
