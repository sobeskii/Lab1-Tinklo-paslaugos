namespace GRPCtoSOAP
{
    class Customer
    {
        /// <summary>
        /// Unique user ID
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Number of items that the customer is buying
        /// </summary>
        public int NumberOfItems { get; set; }
        /// <summary>
        /// Delay before the customers enters the queue
        /// </summary>
        public int DelayBeforeEnteringQueue { get; set; }
        /// <summary>
        /// Property that indicates if the customer was served by counter
        /// </summary>
        public bool IsServed { get; set; }
        public Customer(int id, int numberOfItems, int delay,bool isServed = false)
        {
            Id = id;
            NumberOfItems = numberOfItems;
            DelayBeforeEnteringQueue = delay;
            IsServed = isServed;
        }
    }
}
