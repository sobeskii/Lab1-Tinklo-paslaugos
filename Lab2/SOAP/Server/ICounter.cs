using System.ServiceModel;
using System.Runtime.Serialization;


namespace Server
{
	[DataContract]
    public class CustomerRequest
    {

        [DataMember]
        public int itemCount { get; set; }
        [DataMember]
        public int id { get; set; }
        [DataMember]
        public int delay { get; set; }
    }
    [DataContract]
    public class OfferRequest
    {
        [DataMember]
        public int itemCount { get; set; }
        [DataMember]
        public int price { get; set; }
    }

    [ServiceContract]
    public interface ICounter
    {

        [OperationContract]
        public bool NeedRefill();

        [OperationContract]
        public bool TakeOffer(OfferRequest offer);

        [OperationContract]
        public int EnterQueue(CustomerRequest customer);

        [OperationContract]
        public bool LeaveQueue(CustomerRequest customer);

        [OperationContract]
        public bool IsServed(CustomerRequest customer);

    }
}