using System;
using System.ComponentModel;
using System.Net.Http;
using System.Text;
using System.Threading;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;

namespace CryptoBot
{
    public static class Trader
    {
        private static BackgroundWorker worker;
        private static AutoResetEvent wait;
        private static HttpClient client;
        private static TraceWriter _log;

        [FunctionName("Trader")]
        public static void Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            _log = log;
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");
            // TODO: Call backgroundworker
            //_wait = new AutoResetEvent(true);
            //Console.WriteLine("derp2");
            //StartTrade();

            string apiUrl = Environment.GetEnvironmentVariable("ServiceUrl");
            string key = Environment.GetEnvironmentVariable("ServiceKey");

            client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Access-Token", key);

            GetConnectionReponse(apiUrl);
            BuyCurrency(apiUrl,"BTC", 0.1);
            GetHistory(apiUrl);
        }

        private static void BuyCurrency(string url, string symbol, double amount)
        {
            string postJson = "{'symbol': '" + symbol + "','amount':'0.01'}";

            string exchangeUrl = url + "/account/purchase";
            var result = client.PostAsync(exchangeUrl, new StringContent(postJson, Encoding.UTF8, "application/json")).Result;
            _log.Info($"BUY result: {result}");
        }

        private static void GetConnectionReponse(string apiUrl)
        {
            var result = client.GetAsync(apiUrl).Result;
            _log.Info($"CONNECTION result: {result}");
        }

        private static void GetHistory(string apiUrl)
        {
            var result = client.GetAsync(apiUrl + "/account/history").Result;
            _log.Info($"HISTORY result: {result}");
        }

        #region BackgroundWorker

        private static void StartTrade()
        {
            StopTrade();
            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += new DoWorkEventHandler(DoTrade);
            worker.RunWorkerAsync();
        }


        private static void DoTrade(object sender, DoWorkEventArgs e)
        {
            while (!(sender as BackgroundWorker).CancellationPending)
            {
                wait.WaitOne(500);
                // Call methods
            }
        }

        private static void StopTrade()
        {
            if (worker != null)
            {
                worker.CancelAsync();
            }
        }

        #endregion

    }
}
