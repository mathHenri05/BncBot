using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Playwright;


namespace BncBot.Services
{
    public class ChromeService
    {
        public async Task<List<IPage>> ObterAbasAsync()
        {
            var playwright = await Playwright.CreateAsync();

            var browser = await playwright.Chromium.ConnectOverCDPAsync("http://127.0.0.1:9222");

            return browser.Contexts
                .SelectMany(x => x.Pages)
                .ToList();
        }
    }
}
