using System;
using System.Text;
using System.Security.Cryptography;

namespace Ecoinv.Common
{
  public class MD5Func
  {
    public string MD5Encode(string sourcestr)
    {
      string ret_hash;
      using (var md5Hash = MD5.Create())
      {
        // Byte array representation of source string
        var sourceBytes = Encoding.UTF8.GetBytes(sourcestr);

        // Generate hash value(Byte Array) for input data
        var hashBytes = md5Hash.ComputeHash(sourceBytes);

        // Convert hash byte array to string
        ret_hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);
      }

      return ret_hash;
    }


    public string CreateMD5(string input)
    {
      // Use input string to calculate MD5 hash
      using (MD5 md5 = MD5.Create())
      {
        byte[] inputBytes = Encoding.ASCII.GetBytes(input);
        byte[] hashBytes = md5.ComputeHash(inputBytes);

        return BitConverter.ToString(hashBytes).Replace("-", string.Empty); // .NET 5 +
              //Convert.ToHexString(hashBytes);


        // Convert the byte array to hexadecimal string prior to .NET 5
        // StringBuilder sb = new System.Text.StringBuilder();
        // for (int i = 0; i < hashBytes.Length; i++)
        // {
        //     sb.Append(hashBytes[i].ToString("X2"));
        // }
        // return sb.ToString();
      }
    }



    static string key { get; set; } = "A!9HHhi%XjjYY4YP2@Nob009X";

    public string Encrypt(string cleartext)
    {
      using (var md5 = new MD5CryptoServiceProvider())
      {
        using (var tdes = new TripleDESCryptoServiceProvider())
        {
          tdes.Key = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
          tdes.Mode = CipherMode.ECB;
          tdes.Padding = PaddingMode.PKCS7;

          using (var transform = tdes.CreateEncryptor())
          {
            byte[] textBytes = UTF8Encoding.UTF8.GetBytes(cleartext);
            byte[] bytes = transform.TransformFinalBlock(textBytes, 0, textBytes.Length);
            return Convert.ToBase64String(bytes, 0, bytes.Length);
          }
        }
      }
    }

    public string Decrypt(string cipher)
    {
      using (var md5 = new MD5CryptoServiceProvider())
      {
        using (var tdes = new TripleDESCryptoServiceProvider())
        {
          tdes.Key = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
          tdes.Mode = CipherMode.ECB;
          tdes.Padding = PaddingMode.PKCS7;

          using (var transform = tdes.CreateDecryptor())
          {
            byte[] cipherBytes = Convert.FromBase64String(cipher);
            byte[] bytes = transform.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return UTF8Encoding.UTF8.GetString(bytes);
          }
        }
      }
    }

  }
}
