using System;
using System.ComponentModel;
using System.Net.Http;
using System.Text;
using System.Threading;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

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
            wait = new AutoResetEvent(true);
            //Console.WriteLine("derp2");
            //StartTrade();

            string apiUrl = Environment.GetEnvironmentVariable("ServiceUrl");
            string key = Environment.GetEnvironmentVariable("ServiceKey");

            client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Access-Token", key);

            // GetConnectionReponse(apiUrl);
            // BuyCurrency(apiUrl,"BTC", 0.1);
            //  GetHistory(apiUrl);






            //inicializált éertékek értékek szabályozáshoz
            int SafeMoney = 4550;
            bool btcWannaTrade = true;
            bool ethWannaTrade = true;
            bool xrpWannaTrade = true;
            double btcTradeAmount = 0.01;
            double ethTradeAmount = 0.1;
            double xrpTradeAmount = 10;

            //inicializállandó értékek mink van?
            double usdActual = 0;
            double btcActualAmount = 0;
            double xrpActualAmount = 0;
            double ethActualAmount = 0;

            GetBalance(apiUrl, ref usdActual, ref btcActualAmount, ref xrpActualAmount, ref ethActualAmount);



            if (btcWannaTrade)
            {
                // pihenés hogy elkerüjle a túl sok requestett
                wait.WaitOne(2000);
                //btc inicializáló értékek
                double btcAvaragePrice = 0;
                double btcActualPrice = 0;
                GetCurencyExchangeRate(apiUrl, "BTC", ref btcAvaragePrice, ref btcActualPrice);

                //btc vásárlás
                if (btcActualPrice < btcAvaragePrice && usdActual - (btcActualPrice * btcTradeAmount) > SafeMoney)
                {
                    BuyCurrency(apiUrl, "BTC", btcTradeAmount);
                }
                else if (btcActualPrice > btcAvaragePrice && btcActualAmount >= btcTradeAmount)
                {
                    SellCurrency(apiUrl, "BTC", btcTradeAmount);
                }
            }
            if (ethWannaTrade)
            {
                // pihenés hogy elkerüjle a túl sok requestett
                wait.WaitOne(2000);

                double ethAvaragePrice = 0;
                double ethActualPrice = 0;
                GetCurencyExchangeRate(apiUrl, "ETH", ref ethAvaragePrice, ref ethActualPrice);

                //eth vásárlás
                if (ethActualPrice < ethAvaragePrice && usdActual - (ethActualPrice * ethTradeAmount) > SafeMoney)
                {
                    BuyCurrency(apiUrl, "ETH", ethTradeAmount);
                }
                else if (ethActualPrice > ethAvaragePrice && ethActualAmount >= ethTradeAmount)
                {
                    SellCurrency(apiUrl, "ETH", ethTradeAmount);
                }
            }
            if (xrpWannaTrade)
            {
                // pihenés hogy elkerüjle a túl sok requestett
                wait.WaitOne(2000);

                double xrpAvaragePrice = 0;
                double xrpActualPrice = 0;
                GetCurencyExchangeRate(apiUrl, "XRP", ref xrpAvaragePrice, ref xrpActualPrice);

                //xrp vásárlás
                if (xrpActualPrice < xrpAvaragePrice && usdActual - (xrpActualPrice * xrpTradeAmount) > SafeMoney)
                {
                    BuyCurrency(apiUrl, "XRP", xrpTradeAmount);
                }
                else if (xrpActualPrice > xrpAvaragePrice && xrpActualAmount >= xrpTradeAmount)
                {
                    SellCurrency(apiUrl, "XRP", xrpTradeAmount);
                }
            }
        }

        private static void BuyCurrency(string url, string symbol, double amount)
        {
            wait.WaitOne(5000);
            string amountstring = amount.ToString().Replace(',', '.');
            string postJson = "{'symbol': '" + symbol + "','amount':'" + amountstring + "'}";

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

        private static void GetCurencyExchangeRate(string apiUrl, string symbol, ref double avarageprice, ref double actualprice)
        {
            wait.WaitOne(2000);
            try
            {
                var result = client.GetAsync(apiUrl + "/exchange/" + symbol).Result.Content.ReadAsStringAsync().Result;
                dynamic result2 = Newtonsoft.Json.Linq.JObject.Parse(result);
                _log.Info($"ExchangeRate result: {result2}");
                _log.Info($"symbol: {result2.symbol}");
                actualprice = result2.currentRate;
                _log.Info($"actual price: {actualprice}");
                int db = 0;
                double sum = 0;
                foreach (var item in result2.history)
                {
                    string[] a = item.ToString().Split(':');
                    string num = a[2];
                    num.Remove(0);
                    num.Remove(a[2].Length - 1);
                    sum += double.Parse(num);
                    db++;
                }
                avarageprice = sum / db;
                _log.Info($"{symbol} avarage price: {avarageprice}");
            }
            catch (Exception)
            {
                _log.Info($"Hiba a lekérdezésben. az alábbinál: {symbol}");
                avarageprice = 0;
                actualprice = 0;
            }
        }


        private static void GetBalance(string apiUrl, ref double usd, ref double btc, ref double xrp, ref double eth)
        {
            wait.WaitOne(5000);
            try
            {
                var result = client.GetAsync(apiUrl + "/account").Result.Content.ReadAsStringAsync().Result;
                dynamic result2 = Newtonsoft.Json.Linq.JObject.Parse(result);
                usd = result2.usd;
                _log.Info($"USD Amount: {usd}");
                btc = result2.btc;
                _log.Info($"BTC Amount: {btc}");
                xrp = result2.xrp;
                _log.Info($"XRP Amount: {xrp}");
                eth = result2.eth;
                _log.Info($"ETH Amount: {eth}");
            }
            catch (Exception)
            {
                usd = 0;
                btc = 0;
                xrp = 0;
                eth = 0;
            }
        }

        private static void SellCurrency(string url, string symbol, double amount)
        {
            wait.WaitOne(5000);
            string amountstring = amount.ToString().Replace(',', '.');
            string postJson = "{'symbol': '" + symbol + "','amount':'" + amountstring + "'}";
            string exchangeUrl = url + "/account/sell";
            var result = client.PostAsync(exchangeUrl, new StringContent(postJson, Encoding.UTF8, "application/json")).Result;
            _log.Info($"Sell result: {result}");
        }

        private static void GetExchangeRates(string apiUrl)
        {
            var result = client.GetAsync(apiUrl + "/exchange/btc").Result;
            _log.Info($"BTC result: {result}");
            result = client.GetAsync(apiUrl + "/exchange/eth").Result;
            _log.Info($"ETH result: {result}");
            result = client.GetAsync(apiUrl + "/exchange/xrp").Result;
            _log.Info($"XRP result: {result}");
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

        private static object obj = new object();

        private static void DoTrade(object sender, DoWorkEventArgs e)
        {
            while (!(sender as BackgroundWorker).CancellationPending)
            {
                wait.WaitOne(5000);
                string apiUrl = Environment.GetEnvironmentVariable("ServiceUrl");
                lock (obj)
                {
                    GetExchangeRates(apiUrl);
                }
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
