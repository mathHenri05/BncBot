using System.Net.Http;
using System.Text.Json;
using System.Windows;

namespace BncBot.Services
{
    public class UpdateService
    {
        private const string UrlVersao =
            "https://raw.githubusercontent.com/mathHenri05/BncBot/main/version.json";

        public async Task<VersionInfo?> VerificarAtualizacaoAsync()
        {
            using HttpClient client = new();

            string json = await client.GetStringAsync(UrlVersao);

            return JsonSerializer.Deserialize<VersionInfo>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
    }

    public class VersionInfo
    {
        public string Version { get; set; } = "";
        public string Url { get; set; } = "";
    }
}