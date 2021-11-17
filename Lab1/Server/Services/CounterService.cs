using Grpc.Core;
using System.Threading.Tasks;

namespace Server
{
    /// <summary>
    /// Implements GRPC service in service.proto file
    /// </summary>
    public class CounterService : Counter.CounterBase
    {
        /// <summary>
        /// Object implementing service operations
        /// </summary>
        private readonly CounterLogic _logic;

        public CounterService()
        {
            _logic = new CounterLogic();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="empty">Request with no arguments</param>
        /// <param name="context">Call context</param>
        /// <returns>State indicating if loaders are needed<</returns>
        public override Task<CounterResponse> NeedRefill(EmptyRequest empty, ServerCallContext context)
        {
            return Task.FromResult(new CounterResponse { State = _logic.NeedRefill() });
        }
        /// <summary>
        /// Checks if customer has finished buying items
        /// </summary>
        /// <param name="customer">>Request with customer data</param>
        /// <param name="context">Call context</param>
        /// <returns>State indicating if operation completed successfully<</returns>
        public override Task<CounterResponse> IsServed(CustomerRequest customer, ServerCallContext context)
        {
            return Task.FromResult(new CounterResponse { State = _logic.IsServed(customer) });
        }
        /// <summary>
        /// Adds user to queue
        /// </summary>
        /// <param name="customer">Request with customer data</param>
        /// <param name="context">Call context</param>
        /// <returns>Returns customer id</returns>
        public override Task<CounterIdResponse> EnterQueue(CustomerRequest customer, ServerCallContext context)
        {
            return Task.FromResult(new CounterIdResponse { Id = _logic.EnterQueue(customer) });
        }
        /// <summary>
        /// Removes user from queue
        /// </summary>
        /// <param name="customer">Request with customer data</param>
        /// <param name="context">Call context</param>
        /// <returns>State indicating if operation completed successfully<</returns>
        public override Task<CounterResponse> LeaveQueue(CustomerRequest customer, ServerCallContext context)
        {
            return Task.FromResult(new CounterResponse { State = _logic.LeaveQueue(customer) });
        }
        /// <summary>
        /// Adds offer to a list
        /// </summary>
        /// <param name="offer">Request with offer data</param>
        /// <param name="context">Call context</param>
        /// <returns>State indicating if operation completed successfully<</returns>
        public override Task<CounterResponse> TakeOffer(OfferRequest offer, ServerCallContext context)
        {
            return Task.FromResult(new CounterResponse { State = _logic.TakeOffer(offer) });
        }

    }
}
