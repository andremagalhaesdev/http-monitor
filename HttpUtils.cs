using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.IO;
using System.Speech.Synthesis;


namespace HTTPMonitor
{
    public static class HttpUtils
    {
        public class URLStatus
        {
            public string Endereco { get; set; }
            public bool? Status { get; set; }
            public string StringStatus { get; set; }
        }
        public static bool IsURLValid(string url)
        {
            string pattern = @"^(https?://)?(www\.)?[\w-]+\.[a-zA-Z]{2,}(?:\.[a-zA-Z]{2,})?$";
            return Regex.IsMatch(url, pattern, RegexOptions.IgnoreCase);
        }

        public static bool EnderecoExiste(string endereco, ObservableCollection<MainWindow.URLStatus> urls)
        {
            return urls.Any(url => url.Endereco.Equals(endereco, StringComparison.OrdinalIgnoreCase));
        }

        public static async Task<bool> CheckURLStatus(string url, ObservableCollection<MainWindow.URLStatus> urls, List<string> offlineUrls, CheckBox checkboxAlarme, string telefone, string mensagem)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    bool status = response.IsSuccessStatusCode;
                    UpdateURLStatus(url, status, urls);
                    if (!status)
                    {
                        offlineUrls.Add(url);
                        if (checkboxAlarme.IsChecked == true)
                        {
                            GenerateVoiceAlarm(offlineUrls);
                        }
                    }
                    return status;
                }
                catch (HttpRequestException)
                {
                    UpdateURLStatus(url, false, urls);
                    offlineUrls.Add(url);
                    await MensagemUtils.EnviarMensagem(telefone, mensagem, offlineUrls);
                    if (checkboxAlarme.IsChecked == true)
                    {
                        GenerateVoiceAlarm(offlineUrls);
                    }
                    return false;
                }
            }
        }
        public static async Task GenerateVoiceAlarm(IEnumerable<string> offlineURLs)
        {
            if (offlineURLs.Any())
            {
                using (SpeechSynthesizer synth = new SpeechSynthesizer())
                {
                    string message = "Atenção! Sites Offline: ";
                    foreach (var url in offlineURLs)
                    {
                        string domainPattern = @"(?:www\.)?([\w-]+\.[a-zA-Z]{2,}(?:\.[a-zA-Z]{2,})?)";
                        Match match = Regex.Match(url, domainPattern);

                        if (match.Success)
                        {
                            string domainToSpeak = match.Groups[1].Value;
                            message += $"{domainToSpeak}, ";
                        }
                    }
                    message = message.TrimEnd(',', ' ');

                    synth.SpeakAsync(message);
                    await Task.Delay(10000);
                }
            }
        }
        private static void UpdateURLStatus(string url, bool status, ObservableCollection<MainWindow.URLStatus> urls)
        {
            var urlStatus = urls.FirstOrDefault(u => u.Endereco == url);
            if (urlStatus != null)
            {
                urlStatus.StringStatus = status ? "ONLINE" : "OFFLINE";
            }
        }

    }
}
