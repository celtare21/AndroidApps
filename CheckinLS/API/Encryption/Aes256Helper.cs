using System.Threading.Tasks;
using Xamarin.Essentials;

namespace CheckinLS.API.Encryption
{
    public static class AesKeyHelper
    {
        public static async Task SetAesKeyAsync()
        {
            if (!GetAesKeySet())
            {
                await SecureStorage.SetAsync("aesKey", Aes256Encrypter.GenerateKey());
                Preferences.Set("aesKeySet", "1");
            }
        }

        public static Task<string> GetAesKeyAsync() =>
                SecureStorage.GetAsync("aesKey");

        private static bool GetAesKeySet() =>
                string.Equals(Preferences.Get("aesKeySet", "0"), "1");
    }
}
