using System;
using System.Runtime.Serialization;
using System.ServiceModel;


namespace Loader
{
    public class CustomerRequest
    {
        public int itemCount { get; set; }
        public int id { get; set; }
        public int delay { get; set; }
    }

    public class OfferRequest
    {
        public int itemCount { get; set; }
        public int price { get; set; }
    }

    public interface ICounter
    {
        /// <summary>
        /// Checks if counter needs an item refill
        /// </summary>
        /// <returns>boolean indicating if refill is needed</returns>
        public bool NeedRefill();
        /// <summary>
        /// Takes offer and adds to offer list
        /// </summary>
        /// <param name="offer">Request containing offer data</param>
        /// <returns>boolean indicating if the operation completed sucessfuly</returns>
        public bool TakeOffer(OfferRequest offer);
        /// <summary>
        /// Adds customer to queue
        /// </summary>
        /// <param name="customer">Request containing customer data</param>
        /// <returns>boolean indicating if the operation completed sucessfuly</returns>
        public int EnterQueue(CustomerRequest customer);
        /// <summary>
        /// Removes customer from queue
        /// </summary>
        /// <param name="customer">Request containing customer data</param>
        /// <returns>boolean indicating if the operation completed sucessfuly</returns>
        public bool LeaveQueue(CustomerRequest customer);
        /// <summary>
        /// Checks if customer finished buying items
        /// </summary>
        /// <param name="customer">Request containing customer data</param>
        /// <returns>boolean indicating if the operation completed sucessfuly</returns>
        public bool IsServed(CustomerRequest customer);

    }
}