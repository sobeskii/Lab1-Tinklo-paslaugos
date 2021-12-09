using System.ServiceModel;
using System.Runtime.Serialization;


namespace GRPCtoREST

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
        public bool NeedRefill();
        public bool TakeOffer(OfferRequest offer);
        public int EnterQueue(CustomerRequest customer);
        public bool LeaveQueue(CustomerRequest customer);
        public bool IsServed(CustomerRequest customer);

    }
}