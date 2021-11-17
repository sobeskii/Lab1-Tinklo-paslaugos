namespace Server
{
    /// <summary>
    /// Implements SOAP service
    /// </summary>
    class CounterService : ICounter
    {
	    /// <summary>
		/// Object implementing service operations
		/// </summary>
		private readonly ICounter _counter;

        public CounterService()
        {
            _counter = new CounterLogic();
        }
        /// <summary>
        /// Checks if counter needs an item refill
        /// </summary>
        /// <returns>Indicator if loaders are needed<</returns>
        public bool NeedRefill()
        {
            return _counter.NeedRefill();
        }
        /// <summary>
        /// Checks if customer has finished buying items
        /// </summary>
        /// <param name="customer">>Request with customer data</param>
        /// <returns>Indicator if operation completed successfully<</returns>
        public bool IsServed(CustomerRequest customer)
        {
            return _counter.IsServed(customer);
        }
        /// <summary>
        /// Adds user to queue
        /// </summary>
        /// <param name="customer">Request with customer data</param>
        /// <returns>Returns customer id</returns>
        public int EnterQueue(CustomerRequest customer)
        {
            return _counter.EnterQueue(customer);
        }
        /// <summary>
        /// Removes user from queue
        /// </summary>
        /// <param name="customer">Request with customer data</param>
        /// <returns>Indicator if operation completed successfully<</returns>
        public bool LeaveQueue(CustomerRequest customer)
        {
            return _counter.LeaveQueue(customer);
        }
        /// <summary>
        /// Adds offer to a list
        /// </summary>
        /// <param name="offer">Request with offer data</param>
        /// <returns>Indicator if operation completed successfully<</returns>
        public bool TakeOffer(OfferRequest offer)
        {
            return _counter.TakeOffer(offer);
        }

    }
}
