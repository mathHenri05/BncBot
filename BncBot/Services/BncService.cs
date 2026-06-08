using BncBot.Models;
using Microsoft.Playwright;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BncBot.Services
{
    public class BncService
    {
        public async Task<string> ObterRankingAsync(IPage page)
        {
            var ranking = await page.Locator("#Ranking").InnerTextAsync();

            return ranking;
        }

        public async Task<InformacoesLoteModel> ObterInformacoesAsync(IPage page)
        {
            var html = await page.ContentAsync();

            var info = new InformacoesLoteModel();

            var participanteMatch =
                Regex.Match(html, @"LANCE\s*\(PARTICIPANTE\s*(\d+)\)");

            if (participanteMatch.Success)
            {
                info.Participante = participanteMatch.Groups[1].Value;
            }

            var lanceMatch =
                Regex.Match(html, @"Seu melhor lance:\s*<b>([\d\.,]+)");

            if (lanceMatch.Success)
            {
                var valorTexto = lanceMatch.Groups[1].Value
                    .Replace(".", "")
                    .Replace(",", ".");

                decimal.TryParse(
                    valorTexto,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out decimal valor);

                info.MeuLance = valor;
            }

            return info;
        }

        public async Task<UltimoLanceModel?> ObterUltimoLanceAsync(IPage page)
        {
            var linhas = await page.Locator("#bidsTableBody tr").AllAsync();

            if (linhas.Count == 0)
                return null;

            var ultimaLinha = linhas.Last();

            var colunas = await ultimaLinha.Locator("td").AllInnerTextsAsync();

            if (colunas.Count < 3)
                return null;

            var valorTexto = colunas[2]
                .Replace(".", "")
                .Replace(",", ".");

            decimal.TryParse(
                valorTexto,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out decimal valor);

            return new UltimoLanceModel
            {
                Horario = colunas[0],
                Participante = colunas[1],
                Valor = valor
            };
        }

        public async Task<bool> PrecisaEnviarLanceAsync(IPage page)
        {
            var info = await ObterInformacoesAsync(page);

            var ultimo = await ObterUltimoLanceAsync(page);

            if (ultimo == null)
                return false;

            return !ultimo.Participante.Contains(info.Participante);
        }

        public decimal CalcularProximoLance(AbaModel aba, decimal ultimoLance)
        {
            if (aba.TipoLance == "Menor")
                return ultimoLance - aba.Incremento;

            return ultimoLance + aba.Incremento;
        }

        public bool LanceDentroDaFaixa(AbaModel aba, decimal valor)
        {
            return valor >= aba.ValorMinimo
                && valor <= aba.ValorMaximo;
        }

        public async Task PreencherLanceAsync(IPage page, decimal valor)
        {
            string texto = valor
                .ToString("0.00")
                .Replace(".", ",");

            await page.Locator("#Value").FillAsync(texto);
        }

        public async Task<bool> EstouEmPrimeiroLugarAsync(IPage page)
        {
            var info = await ObterInformacoesAsync(page);

            var primeiraLinha =
                page.Locator("#Ranking table tr").Nth(1);

            var colunas =
                await primeiraLinha.Locator("td")
                                   .AllInnerTextsAsync();

            if (colunas.Count < 2)
                return false;

            return colunas[0]
                .Contains(info.Participante);
        }

        /// <summary>
        /// Preenche o campo de lance e clica no botão de confirmação.
        /// Ajuste o seletor do botão conforme o HTML real do BNC.
        /// </summary>
        public async Task<bool> ConfirmarLanceAsync(IPage page, decimal valor)
        {
            try
            {
                string texto = valor.ToString("0.00").Replace(".", ",");

                await page.Locator("#Value").FillAsync(texto);

                await page.WaitForSelectorAsync("#PerformBidBtn");

                await page.Locator("#PerformBidBtn").ClickAsync();

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"ConfirmarLance ERRO: {ex}");

                return false;
            }
        }
    }
}