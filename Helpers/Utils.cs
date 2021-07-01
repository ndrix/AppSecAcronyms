

namespace AppSecAcronyms.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Azure.Identity;
    using Azure.Security.KeyVault;
    using Azure.Security.KeyVault.Secrets;


    public class Utils
    {
        static string _connString = "";
        static string _sasToken = "";
        static string _magicCookie = "";
        static SecretClient _client = null;


        private static SecretClient GetKeyvaultClient()
        {
            string vaultName = "mnsft-sec-webinar";
            return new SecretClient(
                        new Uri($"https://{vaultName}.vault.azure.net"), 
                        new DefaultAzureCredential());
        }

        public static string GetConnectionString()
        {
            if (String.IsNullOrEmpty(_connString))
            {
                if (_client == null)
                    _client = GetKeyvaultClient();
                _connString = _client.GetSecret("connstr").Value.Value;
            }
            return _connString;
        }

        public static string GetSasToken()
        {
            if (String.IsNullOrEmpty(_sasToken))
            {
                if (_client == null)
                    _client = GetKeyvaultClient();
                _sasToken = _client.GetSecret("sastoken").Value.Value;
            }
            return _sasToken;
        }

        public static string GetMagicCookie()
        {
            if (String.IsNullOrEmpty(_magicCookie))
            {
                if (_client == null)
                    _client = GetKeyvaultClient();
                _magicCookie = _client.GetSecret("cookie").Value.Value;
            }
            return _magicCookie;
        }

    }
}
