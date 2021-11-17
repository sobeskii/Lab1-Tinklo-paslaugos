namespace Server
{
    class Offer
    {
        public int NumberOfItems { get; set; }
        public int Price { get; set; }
        public float Ratio { get { return _Ratio; } }

        public Offer(int numberOfItems, int price)
        {
            NumberOfItems = numberOfItems;
            Price = price;
        }
        private float _Ratio => (float)Price / (float)NumberOfItems;
    }
}
