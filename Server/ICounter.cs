namespace Server
{
    public interface ICounter
    {
        /// <summary>
        /// Takes offer and adds to offer list
        /// </summary>
        /// <param name="offerData">Request containing offer data</param>
        /// <returns>boolean indicating if the operation completed sucessfuly</returns>
        public bool NeedRefill();
        /// <summary>
        /// Takes offer and adds to offer list
        /// </summary>
        /// <param name="offerData">Request containing offer data</param>
        /// <returns>boolean indicating if the operation completed sucessfuly</returns>
        public bool TakeOffer(OfferRequest offer);
        /// <summary>
        /// Adds customer to queue
        /// </summary>
        /// <param name="customerData">Request containing customer data</param>
        /// <returns>boolean indicating if the operation completed sucessfuly</returns>
        public int EnterQueue(CustomerRequest customer);
        /// <summary>
        /// Removes customer from queue
        /// </summary>
        /// <param name="customerData">Request containing customer data</param>
        /// <returns>boolean indicating if the operation completed sucessfuly</returns>
        public bool LeaveQueue(CustomerRequest customer);
        /// <summary>
        /// Checks if customer finished buying items
        /// </summary>
        /// <param name="customerData">Request containing customer data</param>
        /// <returns>boolean indicating if the operation completed sucessfuly</returns>
        public bool IsServed(CustomerRequest customer);

    }
}