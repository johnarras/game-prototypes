using Genrpg.ServerShared.Secrets.Services;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Serialization.Interfaces;
using System;
using System.Security.Cryptography;

namespace Genrpg.ServerShared.Crypto.Services
{

    public class CryptoService : ICryptoService
    {

        private ISecretsProvider _secretsProvider = null;
        private ITextSerializer _serializer = null;
        private ILogService _logService = null;

        public string GetPasswordHash(string salt, string passwordOrToken)
        {
            if (string.IsNullOrEmpty(passwordOrToken) || string.IsNullOrEmpty(salt))
            {
                return "";
            }

            string txt2 = salt + passwordOrToken;

            return SHA256Hash(txt2);
        }

        public string GetRandomBytes()
        {
            byte[] buff = RandomNumberGenerator.GetBytes(16);
            return Convert.ToBase64String(buff);
        }


        public string SlowHash(string txt)
        {
            return SHA256Hash(txt);
        }

        private string SHA256Hash(string txt)
        {
            // For now to avoid adding keygen lib...stronger hashes don't work always too.
            SHA256 algo = SHA256.Create();
            byte[] arr = System.Text.Encoding.UTF8.GetBytes(txt);
            byte[] arr2 = algo.ComputeHash(arr);
            return Convert.ToBase64String(arr2);
        }
    }
}


