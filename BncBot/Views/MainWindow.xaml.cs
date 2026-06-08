using BncBot.Models;
using BncBot.Services;
using BncBot.Workers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Reflection;

namespace BncBot.Views
{
    public partial class MainWindow : Window
    {
        private readonly ChromeService _chromeService = new();
        private readonly BncService _bncService = new();
        private ObservableCollection<AbaModel> _listaAbas = new();

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private async Task CarregarAbasAsync()
        {
            try
            {
                _listaAbas.Clear();

                var abas = await _chromeService.ObterAbasAsync();

                foreach (var aba in abas)
                {
                    if (!aba.Url.Contains("bnccompras.com"))
                        continue;

                    _listaAbas.Add(new AbaModel
                    {
                        Titulo = await aba.EvaluateAsync<string>("() => document.title"),
                        Url = aba.Url,
                        Page = aba,
                        EstouEmPrimeiro =
                            await _bncService.EstouEmPrimeiroLugarAsync(aba)
                    });
                }

                dgAbas.ItemsSource = _listaAbas;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var launcher = new ChromeLauncherService();
            await VerificarAtualizacao();

            await CarregarAbasAsync();

            if (!launcher.ChromeDebugAtivo())
            {
                launcher.AbrirChromeDebug();

                for (int i = 0; i < 15; i++)
                {
                    if (launcher.ChromeDebugAtivo())
                        break;

                    await Task.Delay(1000);
                }
            }

            await CarregarAbasAsync();
        }

        private async Task VerificarAtualizacao()
        {
            try
            {
                var service = new UpdateService();

                var versaoOnline =
                    await service.VerificarAtualizacaoAsync();

                if (versaoOnline == null)
                    return;

                Version versaoAtual = Assembly.GetExecutingAssembly().GetName().Version!;

                Version versaoServidor =
                    new(versaoOnline.Version);

                if (versaoServidor > versaoAtual)
                {
                    var resultado = MessageBox.Show(
                        $"Nova versão disponível ({versaoOnline.Version})\n\nDeseja baixar?",
                        "Atualização",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information);

                    if (resultado == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(
                            new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = versaoOnline.Url,
                                UseShellExecute = true
                            });
                    }
                }
            }
            catch
            {
                // ignora erro de internet
            }
        }

        private async void BtnAtualizarAbas_Click(object sender, RoutedEventArgs e)
        {
            await CarregarAbasAsync();
        }

        private void BtnAutomacao_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            if (btn.Tag is not AbaModel aba) return;

            dgAbas.CommitEdit(DataGridEditingUnit.Row, exitEditingMode: true);

            if (aba.Monitorando)
            {
                aba.TokenSource?.Cancel();
                aba.Monitorando = false;
                aba.Status = "PARADO";
            }
            else
            {
                if (aba.ValorMinimo <= 0 || aba.ValorMaximo <= 0 || aba.Incremento <= 0)
                {
                    MessageBox.Show("Preencha Mín, Máx e Incremento antes de iniciar.",
                                    "Configuração incompleta",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                    return;
                }

                if (aba.ValorMaximo < aba.ValorMinimo)
                {
                    MessageBox.Show("Valor Máximo deve ser maior que Mínimo.",
                                    "Configuração inválida",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                    return;
                }

                aba.TokenSource = new CancellationTokenSource();
                aba.Monitorando = true;
                aba.Status = "MONITORANDO";

                var token = aba.TokenSource.Token;
                var worker = new MonitorWorker(_bncService, aba, () => { });

                Task.Run(() => worker.ExecutarAsync(token), token);
            }
        }

        // ---- Botões de teste (mantidos) ----

        private async void BtnTestarRanking_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selecionada = dgAbas.SelectedItem as AbaModel;
                if (selecionada == null) { MessageBox.Show("Selecione uma aba."); return; }
                var service = new BncService();
                var ranking = await service.ObterRankingAsync(selecionada.Page);
                MessageBox.Show(ranking);
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        private async void BtnTestarDados_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selecionada = dgAbas.SelectedItem as AbaModel;
                if (selecionada == null) { MessageBox.Show("Selecione uma aba."); return; }
                var service = new BncService();
                var info = await service.ObterInformacoesAsync(selecionada.Page);
                MessageBox.Show($"Participante: {info.Participante}\nMeu Lance: {info.MeuLance}");
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        private async void BtnUltimoLance_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selecionada = dgAbas.SelectedItem as AbaModel;
                if (selecionada == null) { MessageBox.Show("Selecione uma aba."); return; }
                var service = new BncService();
                var ultimo = await service.ObterUltimoLanceAsync(selecionada.Page);
                if (ultimo == null) { MessageBox.Show("Nenhum lance encontrado."); return; }
                MessageBox.Show($"Horário: {ultimo.Horario}\nParticipante: {ultimo.Participante}\nValor: {ultimo.Valor}");
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        private async void BtnTestarDecisao_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selecionada = dgAbas.SelectedItem as AbaModel;
                if (selecionada == null) { MessageBox.Show("Selecione uma aba."); return; }
                var service = new BncService();
                bool precisaEnviar = await service.PrecisaEnviarLanceAsync(selecionada.Page);
                MessageBox.Show(precisaEnviar ? "Outro participante fez o último lance" : "Seu participante fez o último lance");
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        private async void BtnCalcularLance_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var aba = dgAbas.SelectedItem as AbaModel;
                if (aba == null) { MessageBox.Show("Selecione uma aba."); return; }
                var service = new BncService();
                var ultimo = await service.ObterUltimoLanceAsync(aba.Page);
                if (ultimo == null) { MessageBox.Show("Nenhum lance encontrado."); return; }
                var proximo = service.CalcularProximoLance(aba, ultimo.Valor);
                MessageBox.Show($"Último: {ultimo.Valor}\nPróximo: {proximo}");
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        private async void BtnValidarFaixa_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var aba = dgAbas.SelectedItem as AbaModel;
                if (aba == null) { MessageBox.Show("Selecione uma aba."); return; }
                var service = new BncService();
                var ultimo = await service.ObterUltimoLanceAsync(aba.Page);
                if (ultimo == null) { MessageBox.Show("Nenhum lance encontrado."); return; }
                var proximo = service.CalcularProximoLance(aba, ultimo.Valor);
                bool valido = service.LanceDentroDaFaixa(aba, proximo);
                MessageBox.Show(valido ? $"Lance permitido: {proximo}" : $"Lance bloqueado: {proximo}");
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        private async void BtnPreencherLance_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var aba = dgAbas.SelectedItem as AbaModel;
                if (aba == null) { MessageBox.Show("Selecione uma aba."); return; }
                var service = new BncService();
                var ultimo = await service.ObterUltimoLanceAsync(aba.Page);
                if (ultimo == null) { MessageBox.Show("Nenhum lance encontrado."); return; }
                var proximo = service.CalcularProximoLance(aba, ultimo.Valor);
                if (!service.LanceDentroDaFaixa(aba, proximo)) { MessageBox.Show($"Lance fora da faixa: {proximo}"); return; }
                await service.PreencherLanceAsync(aba.Page, proximo);
                MessageBox.Show($"Campo preenchido com {proximo}");
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }
    }
}