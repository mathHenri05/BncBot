using System.Diagnostics;
using System.IO;
using System.Net.Sockets;

namespace BncBot.Services
{
    public class ChromeLauncherService
    {
        public bool ChromeDebugAtivo()
        {
            try
            {
                using var client = new TcpClient();

                client.Connect("localhost", 9222);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public void AbrirChromeDebug()
        {
            string? chromePath = ObterCaminhoChrome();

            if (string.IsNullOrEmpty(chromePath))
                throw new Exception("Chrome não encontrado.");

            string profilePath =
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.LocalApplicationData),
                    "BncBotChrome");

            Process.Start(
                chromePath,
                $"--remote-debugging-port=9222 --user-data-dir=\"{profilePath}\"");
        }

        private string? ObterCaminhoChrome()
        {
            string[] caminhos =
            {
        @"C:\Program Files\Google\Chrome\Application\chrome.exe",
        @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            @"Google\Chrome\Application\chrome.exe")
    };

            return caminhos.FirstOrDefault(File.Exists);
        }
    }
}