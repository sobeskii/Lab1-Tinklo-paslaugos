namespace Server
{
    class Customer
    {
        public int Id { get; set; }
        public int NumberOfItems { get; set; }
        public int DelayBeforeEnteringQueue { get; set; }
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
