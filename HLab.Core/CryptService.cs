using HLab.Core.Annotations;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace HLab.Core;

public interface ICryptService : IService
{
   void Configure(string key, string iv);
   string Crypt(string password);
   string Decrypt(string encrypted);

   (string Key, string IV) GenerateKeys();
}

public class CryptService : ICryptService
{
   string _key = "";
   string _iv = "";
   public ServiceState ServiceState => string.IsNullOrWhiteSpace(_key) || string.IsNullOrWhiteSpace(_iv) ? ServiceState.NotConfigured : ServiceState.Available;
   public void Configure(string key, string iv)
   {
      _key = key;
      _iv = iv;
   }

   public string Crypt(string password)
   {
      try
      {
         var symmetricAlgorithm = TripleDES.Create();
         //var symmetricAlgorithm = Aes.Create();
         symmetricAlgorithm.Key = Convert.FromBase64String(_key);
         symmetricAlgorithm.IV = Convert.FromBase64String(_iv);

         var encryptor = symmetricAlgorithm.CreateEncryptor();

         var memoryStream = new MemoryStream();
         using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
         var valuePtr = nint.Zero;
         try
         {
            var bytes = Encoding.UTF8.GetBytes(password);
            cryptoStream.Write(bytes, 0, bytes.Length);
            cryptoStream.FlushFinalBlock();
            cryptoStream.Close();

            return Convert.ToBase64String(memoryStream.ToArray());
         }
         finally
         {
            Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
         }
      }
      catch
      {
         return "";
      }
   }

   public string Decrypt(string encrypted)
   {
      try
      {
         var symmetricAlgorithm = TripleDES.Create();
         //var symmetricAlgorithm = Aes.Create();
         symmetricAlgorithm.Key = Convert.FromBase64String(_key);
         symmetricAlgorithm.IV = Convert.FromBase64String(_iv);
         var decryptor = symmetricAlgorithm.CreateDecryptor();
         var memoryStream = new MemoryStream(Convert.FromBase64String(encrypted));
         using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
         using var reader = new StreamReader(cryptoStream, Encoding.UTF8);
         return reader.ReadToEnd();
      }
      catch
      {
         return "";
      }
   }

   public (string Key, string IV) GenerateKeys()
   {
      using var tripleDes = TripleDES.Create();
      tripleDes.GenerateKey();
      tripleDes.GenerateIV();
      string key = Convert.ToBase64String(tripleDes.Key);
      string iv = Convert.ToBase64String(tripleDes.IV);
      return (key, iv);
   }


}
