using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace YoungCloud.Security.Cryptography
{
    /// <summary>
    /// Format convert tool for RSA
    /// </summary>
    public class RSAKeyFormatConverter
    {
        /// <summary>
        /// Convert RSA private key from XML format to PEM format
        /// </summary>
        /// <param name="xmlKey">RSA private key in XML string</param>
        /// <returns>RSA private key in PEM</returns>
        public static string ConvertXMLPrivateKeyToPEM(string xmlKey)
        {
            XmlDocument _XMLDoc = new XmlDocument();
            _XMLDoc.LoadXml(xmlKey);

            List<byte> _Modulus = Convert.FromBase64String(_XMLDoc.SelectSingleNode("RSAKeyValue/Modulus").InnerText).ToList();
            List<byte> _Exponent = Convert.FromBase64String(_XMLDoc.SelectSingleNode("RSAKeyValue/Exponent").InnerText).ToList();
            List<byte> _D = Convert.FromBase64String(_XMLDoc.SelectSingleNode("RSAKeyValue/D").InnerText).ToList();
            List<byte> _P = Convert.FromBase64String(_XMLDoc.SelectSingleNode("RSAKeyValue/P").InnerText).ToList();
            List<byte> _Q = Convert.FromBase64String(_XMLDoc.SelectSingleNode("RSAKeyValue/Q").InnerText).ToList();
            List<byte> _DP = Convert.FromBase64String(_XMLDoc.SelectSingleNode("RSAKeyValue/DP").InnerText).ToList();
            List<byte> _DQ = Convert.FromBase64String(_XMLDoc.SelectSingleNode("RSAKeyValue/DQ").InnerText).ToList();
            List<byte> _InverseQ = Convert.FromBase64String(_XMLDoc.SelectSingleNode("RSAKeyValue/InverseQ").InnerText).ToList();

            byte[] _ObjectID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 }; // Object ID for RSA

            CalculateAndAppendLength(ref _Modulus);
            _Modulus.Insert(0, 0x02);

            CalculateAndAppendLength(ref _Exponent);
            _Exponent.Insert(0, 0x02);

            CalculateAndAppendLength(ref _D);
            _D.Insert(0, 0x02);

            CalculateAndAppendLength(ref _P);
            _P.Insert(0, 0x02);

            CalculateAndAppendLength(ref _Q);
            _Q.Insert(0, 0x02);

            CalculateAndAppendLength(ref _DP);
            _DP.Insert(0, 0x02);

            CalculateAndAppendLength(ref _DQ);
            _DQ.Insert(0, 0x02);

            CalculateAndAppendLength(ref _InverseQ);
            _InverseQ.Insert(0, 0x02);

            List<byte> _BinaryPublicKey = new List<byte>() { 0x02, 0x01, 0x00 };
            _BinaryPublicKey.AddRange(_Modulus);
            _BinaryPublicKey.AddRange(_Exponent);
            _BinaryPublicKey.AddRange(_D);
            _BinaryPublicKey.AddRange(_P);
            _BinaryPublicKey.AddRange(_Q);
            _BinaryPublicKey.AddRange(_DP);
            _BinaryPublicKey.AddRange(_DQ);
            _BinaryPublicKey.AddRange(_InverseQ);

            CalculateAndAppendLength(ref _BinaryPublicKey);
            _BinaryPublicKey.Insert(0, 0x30);

            string _Result = "-----BEGIN RSA PRIVATE KEY-----";
            _Result += System.Convert.ToBase64String(_BinaryPublicKey.ToArray());
            _Result += "-----END RSA PRIVATE KEY-----";
            return _Result;
        }

        /// <summary>
        /// Convert RSA public key from XML format to PEM format
        /// </summary>
        /// <param name="xmlKey">RSA public key in XML string</param>
        /// <returns>RSA public key in PEM</returns>
        public static string ConvertXMLPublicKeyToPEM(string xmlKey)
        {
            XmlDocument _XMLDoc = new XmlDocument();
            _XMLDoc.LoadXml(xmlKey);

            List<byte> _Modulus = Convert.FromBase64String(_XMLDoc.SelectSingleNode("RSAKeyValue/Modulus").InnerText).ToList();
            List<byte> _Exponent = Convert.FromBase64String(_XMLDoc.SelectSingleNode("RSAKeyValue/Exponent").InnerText).ToList();

            // Object ID for RSA : 1.2.840.113549.1.1.1
            byte[] _ObjectID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };

            CalculateAndAppendLength(ref _Modulus);
            _Modulus.Insert(0, 0x02);

            CalculateAndAppendLength(ref _Exponent);
            _Exponent.Insert(0, 0x02);

            List<byte> _BinaryPublicKey = new List<byte>();
            _BinaryPublicKey.AddRange(_Modulus);
            _BinaryPublicKey.AddRange(_Exponent);
            CalculateAndAppendLength(ref _BinaryPublicKey);
            _BinaryPublicKey.Insert(0, 0x30);

            _BinaryPublicKey.Insert(0, 0x00);
            CalculateAndAppendLength(ref _BinaryPublicKey);
            _BinaryPublicKey.Insert(0, 0x03);

            _BinaryPublicKey.InsertRange(0, _ObjectID);
            CalculateAndAppendLength(ref _BinaryPublicKey);
            _BinaryPublicKey.Insert(0, 0x30);

            string _Result = "-----BEGIN PUBLIC KEY-----";
            _Result += System.Convert.ToBase64String(_BinaryPublicKey.ToArray());
            _Result += "-----END PUBLIC KEY-----";
            return _Result;
        }

        /// <summary>
        /// Prepend the der byte(s) of length on a byte array
        /// </summary>
        /// <param name="arrBinaryData">Data byte array</param>
        private static void CalculateAndAppendLength(ref List<byte> arrBinaryData)
        {
            int _Length;
            _Length = arrBinaryData.Count;
            if (_Length <= 127)
            {
                arrBinaryData.Insert(0, Convert.ToByte(_Length));
            }
            else if (_Length <= 255)
            {
                arrBinaryData.Insert(0, Convert.ToByte(_Length));
                arrBinaryData.Insert(0, 0x81); //This byte means that the length between 128-255
            }
            else
            {
                arrBinaryData.Insert(0, Convert.ToByte(_Length % 256));
                arrBinaryData.Insert(0, Convert.ToByte(_Length / 256));
                arrBinaryData.Insert(0, 0x82); //This byte means that the length more than 256
            }
        }
    }
}
