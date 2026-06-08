using BncBot.Models;
using BncBot.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BncBot.Workers
{
    public class MonitorWorker
    {
        private readonly BncService _bncService;
        private readonly AbaModel _aba;
        private readonly Action _onUpdate; // callback para atualizar a UI

        private const int IntervaloNormal = 3000;   // ms entre cada verificação
        private const int IntervaloAposPerda = 2000; // ms de espera antes de enviar lance

        public MonitorWorker(BncService bncService, AbaModel aba, Action onUpdate)
        {
            _bncService = bncService;
            _aba = aba;
            _onUpdate = onUpdate;
        }

        public async Task ExecutarAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (_aba.Page == null) break;

                    // 1. Atualizar quem está em 1º e quem sou eu
                    var info = await _bncService.ObterInformacoesAsync(_aba.Page);
                    _aba.Participante = info.Participante;
                    _aba.MenorLance = info.MeuLance;

                    bool emPrimeiro = await _bncService.EstouEmPrimeiroLugarAsync(_aba.Page);
                    _aba.EstouEmPrimeiro = emPrimeiro;
                    _aba.Status = emPrimeiro ? "EM 1º" : "FORA DO 1º";

                    _onUpdate();

                    // 2. Se perdi o 1º lugar, aguarda e envia lance
                    if (!emPrimeiro)
                    {
                        await Task.Delay(IntervaloAposPerda, token);

                        if (token.IsCancellationRequested) break;

                        decimal proximoLance = CalcularProximoLance();

                        // Verifica se o próximo lance está dentro da faixa configurada
                        if (!LanceDentroDaFaixa(proximoLance))
                        {
                            _aba.Status = "LIMITE ATINGIDO";
                            _aba.Monitorando = false;
                            _onUpdate();
                            break; // Para automação desta aba
                        }

                        bool enviado = await _bncService.ConfirmarLanceAsync(_aba.Page, proximoLance);
                        if (enviado)
                        {
                            _aba.MenorLance = proximoLance;
                        }
                    }

                    await Task.Delay(IntervaloNormal, token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _aba.Status = $"ERRO: {ex.Message}";
                    _onUpdate();
                    await Task.Delay(IntervaloNormal, token);
                }
            }

            _aba.Monitorando = false;
            _onUpdate();
        }

        private decimal CalcularProximoLance()
        {
            // Menor: queremos dar o menor lance possível (subtrair incremento)
            // Maior: queremos dar o maior lance possível (somar incremento)
            if (_aba.TipoLance == "Menor")
                return _aba.MenorLance - _aba.Incremento;
            else
                return _aba.MenorLance + _aba.Incremento;
        }

        private bool LanceDentroDaFaixa(decimal valor)
        {
            return valor >= _aba.ValorMinimo && valor <= _aba.ValorMaximo;
        }
    }
}