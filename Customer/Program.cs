using System;
using Grpc.Core;
using System.Threading;
using Server;
namespace Customer
{
    /// <summary>
    /// Customer client
    /// </summary>
    static class Program
    {
        /// <summary>
        /// Generates a random number of items that customer wants to buy.
        /// </summary>
        /// <returns>Number of items customer is buying</returns>
        static int GetNumberOfItems()
        {
            return new Random().Next(1, 11);
        }
        /// <summary>
        /// Generates a random time to delay customer from entering the queue.
        /// </summary>
        /// <returns>Amount of time customer waits before entering queue</returns>
        static int GetDelay()
        {
            return new Random().Next(1, 10);
        }
        /// <summary>
        /// Property indicating if user is in queue or not
        /// </summary>
        static bool IsInQueue { get; set; } = false;
        /// <summary>
        /// Starts the customer client
        /// </summary>
        static void Main(string[] args)
        {
            var channel = new Channel("127.0.0.1:5000", ChannelCredentials.Insecure);
            var counter = new Counter.CounterClient(channel);
            int count, delay;
            EmptyRequest emptyRequest = new EmptyRequest();
            CustomerRequest customerRequest = null;

            while (true)
            {
                //Customer initial values are generated. Customer walks into market
                if(!IsInQueue && customerRequest == null)
                {
                    count = GetNumberOfItems();
                    delay = GetDelay(); 
                    customerRequest = new CustomerRequest { ItemCount = count, Delay = delay };
                    Console.WriteLine($"Customer has entered the market ");
                    Console.WriteLine($"Customer is buying {count} item(s) and waiting {delay}s before entering the queue");
                    Thread.Sleep(delay * 1000);
                }
                //Customer is removed from queue. Refill is needed
                if (counter.NeedRefill(emptyRequest).State && IsInQueue && customerRequest != null)
                {
                    IsInQueue = false;
                    var leave = counter.LeaveQueue(customerRequest);
                    Console.WriteLine(leave.State   ?   "Customer has left the queue sucessfully" : "Customer couldn't leave the queue");
                    Thread.Sleep(2000); // Wait 2 secs to prevent console spam
                }
                //Customer is added to queue.
                if (!IsInQueue  && customerRequest != null)
                {
                    IsInQueue = true;
                    var enter = counter.EnterQueue(customerRequest);
                    customerRequest.Id = enter.Id;

                    Console.WriteLine("Customer has entered the queue sucessfully");
                }
                // Customer is removed from queue. Has bought items
                if (counter.IsServed(customerRequest).State && IsInQueue && customerRequest != null)
                {
                    IsInQueue = false;
                    var leave = counter.LeaveQueue(customerRequest);
                    Console.WriteLine(leave.State ? "Customer has left the queue sucessfully" : "Customer couldn't leave the queue");
                    customerRequest = null;
                }
            }
        }
    }
}
