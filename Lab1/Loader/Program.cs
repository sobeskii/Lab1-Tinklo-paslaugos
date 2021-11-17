using System;
using Grpc.Core;
using System.Threading;
using Server;

namespace Loader
{
    class Program
    {

        /// <summary>
        /// Generates a random number of items that customer wants to buy.
        /// </summary>
        /// <returns>Number of items loader is offering</returns>
        static int GetNumberOfItems()
        {
            return new Random().Next(10, 200);
        }
        /// <summary>
        /// Generates a random time to delay customer from entering the queue.
        /// </summary>
        /// <returns>The price of all the items that are being sold</returns>
        static int GetPriceOfItems()
        {
            return new Random().Next(99, 1000);
        }
        /// <summary>
        /// Starts the loader client
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var channel = new Channel("127.0.0.1:5000", ChannelCredentials.Insecure);
            var counter = new Counter.CounterClient(channel);

            EmptyRequest emptyRequest = new EmptyRequest();

            while (true)
            {
                if (counter.NeedRefill(emptyRequest).State) {

                    int count = GetNumberOfItems();
                    int itemprice = GetPriceOfItems();
                    OfferRequest offerRequest = new OfferRequest { ItemCount = count, Price = itemprice };
                    counter.TakeOffer(offerRequest);
                    Console.WriteLine($"Offered {count} items for the price of {itemprice}");
                    Thread.Sleep(500);
                }
            }
        }
    }
}
