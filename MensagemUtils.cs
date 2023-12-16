using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dotenv.net;
using RestSharp;

namespace HTTPMonitor
{
    public static class MensagemUtils
    {
        public static async Task EnviarMensagem(string numeroTelefone, string mensagem, List<string> offlineUrls)
        {
            try
            {
                var url = "https://api.ultramsg.com/INSTANCE_DA_API/messages/chat";
                var client = new RestClient(url);

                var request = new RestRequest(url, Method.Post);
                request.AddHeader("content-type", "application/x-www-form-urlencoded");
                request.AddParameter("token", "TOKEN_DA_API");
                request.AddParameter("to", $"{numeroTelefone}");
                request.AddParameter("body", $"{mensagem} {string.Join(", ", offlineUrls)}");


                RestResponse response = await client.ExecuteAsync(request);
                var output = response.Content;
                Console.WriteLine(output);

            }
            catch (Exception ex)
            {
            }
        }
    }
}