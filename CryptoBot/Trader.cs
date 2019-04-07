using System;
using System.ComponentModel;
using System.Net.Http;
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

        [FunctionName("Trader")]
        public static void Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");
            // TODO: Call backgroundworker
            //_wait = new AutoResetEvent(true);
            //Console.WriteLine("derp2");
            //StartTrade();
            var url = Environment.GetEnvironmentVariable("ServiceUrl");
            var key = Environment.GetEnvironmentVariable("ServiceKey");

            client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Access-Token", key);
            var res = client.GetAsync(url).Result;

            log.Info($"result: {res}");
        }

        private static void BuyCurrency(string symbol, double amount)
        {

        }

        #region StartWatch

        private static void StartTrade()
        {
            StopTrade();
            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += new DoWorkEventHandler(DoTrade);
            worker.RunWorkerAsync();
        }

        #endregion

        #region DoTrade

        private static void DoTrade(object sender, DoWorkEventArgs e)
        {
            while (!(sender as BackgroundWorker).CancellationPending)
            {
                wait.WaitOne(500);
                // Call methods to do shit
                Console.WriteLine("derp");
            }
        }

        #endregion

        #region StopTrade

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
