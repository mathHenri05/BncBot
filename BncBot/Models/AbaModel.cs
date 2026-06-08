using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace BncBot.Models
{
    public class AbaModel : INotifyPropertyChanged
    {
        private bool _estouEmPrimeiro;
        private string _status = string.Empty;
        private bool _monitorando;
        private decimal _valorMinimo;
        private decimal _valorMaximo;
        private decimal _incremento;
        private string _tipoLance = "Menor"; // "Menor" ou "Maior"

        public string Titulo { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public Microsoft.Playwright.IPage? Page { get; set; }

        public string Participante { get; set; } = string.Empty;
        public decimal MenorLance { get; set; }

        public decimal ValorMinimo
        {
            get => _valorMinimo;
            set { _valorMinimo = value; OnPropertyChanged(); }
        }

        public decimal ValorMaximo
        {
            get => _valorMaximo;
            set { _valorMaximo = value; OnPropertyChanged(); }
        }

        public decimal Incremento
        {
            get => _incremento;
            set { _incremento = value; OnPropertyChanged(); }
        }

        // "Menor" = disputa pelo menor lance | "Maior" = disputa pelo maior lance
        public string TipoLance
        {
            get => _tipoLance;
            set { _tipoLance = value; OnPropertyChanged(); }
        }

        public bool EstouEmPrimeiro
        {
            get => _estouEmPrimeiro;
            set { _estouEmPrimeiro = value; OnPropertyChanged(); }
        }

        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        public bool Monitorando
        {
            get => _monitorando;
            set { _monitorando = value; OnPropertyChanged(); }
        }

        public CancellationTokenSource? TokenSource { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}