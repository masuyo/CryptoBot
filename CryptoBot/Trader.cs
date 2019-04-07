using System;
using System.ComponentModel;
using System.Threading;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace CryptoBot
{
    public static class Trader
    {
        private static BackgroundWorker _worker;
        private static AutoResetEvent _wait;

        [FunctionName("Trader")]
        public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");
            // TODO: Call backgroundworker
            _wait = new AutoResetEvent(true);
            StartTrade();
        }

        #region StartWatch

        private static void StartTrade()
        {
            StopTrade();
            _worker = new BackgroundWorker();
            _worker.WorkerSupportsCancellation = true;
            _worker.DoWork += new DoWorkEventHandler(DoTrade);
            _worker.RunWorkerAsync();
        }

        #endregion

        #region DoTrade

        private static void DoTrade(object sender, DoWorkEventArgs e)
        {
            while (!(sender as BackgroundWorker).CancellationPending)
            {
                _wait.WaitOne(500);
                // Call methods to do shit
                Console.WriteLine("derp");
            }
        }

        #endregion

        #region StopTrade

        private static void StopTrade()
        {
            if (_worker != null)
            {
                _worker.CancelAsync();
            }
        }

        #endregion
    }
}
