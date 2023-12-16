using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace HTTPMonitor
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<URLStatus> URLs = new ObservableCollection<URLStatus>();
        private DispatcherTimer timer;
        public class URLStatus
        {
            public string Endereco { get; set; }
            public bool? Status { get; set; }
            public string StringStatus { get; set; }

        }

        public MainWindow()
        {
            InitializeComponent();
            ListaURLs.ItemsSource = URLs;

            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromMinutes(10); // Intervalo de 10 minutos
            timer.Start();
        }

        private async void Timer_Tick(object sender, EventArgs e)
        {
            await AtualizarUrls();
        }

        private async Task AtualizarUrls()
        {
            if (URLs.Count == 0)
            {
                MessageBox.Show("A lista de URLs está vazia.");
                return;
            }

            try
            {
                LoadingIndicator.Visibility = Visibility.Visible;

                foreach (var urlStatus in URLs)
                {
                    List<string> offlineUrls = new List<string>();
                    bool status = await HttpUtils.CheckURLStatus(urlStatus.Endereco, URLs, offlineUrls, CheckboxAlarme, numeroTelefone.Text, mensagem.Text);
                    urlStatus.Status = status;
                }

                ListaURLs.Items.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocorreu um erro: {ex.Message}");
            }
            finally
            {
                LoadingIndicator.Visibility = Visibility.Collapsed;

                DateTime ultimaAtualizacao = DateTime.Now;
                UltimaAtualizacaoTextBlock.Text = $"Atualizado em:\n{ultimaAtualizacao.ToString("HH:mm dd/MM/yyyy")}";
            }
        }

        private async void AdicionarURL_Click(object sender, RoutedEventArgs e)
        {
            if (URLs.Count < 5)
            {
                string url = TextBoxURL.Text.Trim();
                if (!string.IsNullOrEmpty(url) && HttpUtils.IsURLValid(url))
                {
                    if (!HttpUtils.EnderecoExiste(url, URLs))
                    {
                        URLs.Add(new URLStatus { Endereco = url, StringStatus = "" });
                        TextBoxURL.Text = "";
                    }
                    else
                    {
                        MessageBox.Show("O endereço já está na lista de monitoramento");
                    }
                }
                else
                {
                    MessageBox.Show("Endereço inválido. Verifique sua URL.");
                }
            }
            else
            {
                MessageBox.Show("Limite máximo de monitoramento atingido.");
            }
        }

        private void RemoverURL_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                string enderecoParaRemover = btn.Tag as string;

                URLStatus urlParaRemover = URLs.FirstOrDefault(url => url.Endereco == enderecoParaRemover);
                if (urlParaRemover != null)
                {
                    URLs.Remove(urlParaRemover);
                }
            }
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                DragMove();
        }

        private void ClearTextOnFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.Text == "Digite o Telefone" || textBox.Text == "Digite a Mensagem")
                {
                    textBox.Text = string.Empty;
                }
            }
        }

        private async void Atualizar_Click(object sender, EventArgs e)
        {
            await AtualizarUrls();
        }
    }
}
