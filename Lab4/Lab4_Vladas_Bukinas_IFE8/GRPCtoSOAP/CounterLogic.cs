using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Grpc.Core;
using Server;
namespace GRPCtoSOAP
{
    public class CounterLogic : ICounter
    {
        /// <summary>
        /// Logger for this class.
        /// </summary>
        Logger log = LogManager.GetCurrentClassLogger();

        readonly ReaderWriterLock _lock = new();


        private static Channel channel = new Channel("127.0.0.1:5002", ChannelCredentials.Insecure);
        private Counter.CounterClient counter = new Counter.CounterClient(channel);

        /// <summary>
        /// Checks if counter needs an item refill
        /// </summary>
        /// <returns>boolean indicating if refill is needed</returns>
        public bool NeedRefill()
        {
            try
            {
                _lock.AcquireReaderLock(-1);
                return counter.NeedRefill(new EmptyRequest()).State;
            }
            catch (Exception)
            {
                log.Info("Failed to acquire reader lock");
                throw;
            }
            finally
            {
                _lock.ReleaseReaderLock();
            }
        }
        /// <summary>
        /// Takes offer and adds to offer list
        /// </summary>
        /// <param name="offerData">Request containing offer data</param>
        /// <returns>boolean indicating if the operation completed sucessfuly</returns>
        public bool TakeOffer(OfferRequest offerData)
        {
            try
            {

                _lock.AcquireWriterLock(-1);

                Server.OfferRequest offerRequest = new Server.OfferRequest { ItemCount = offerData.itemCount, Price = offerData.price };

                return counter.TakeOffer(offerRequest).State;
            }
            catch (Exception)
            {
                log.Info("Failed to acquire writer lock");
                throw;
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }
        }
        /// <summary>
        /// Adds customer to queue
        /// </summary>
        /// <param name="customerData">Request containing customer data</param>
        /// <returns>boolean indicating if the operation completed sucessfuly</returns>
        public int EnterQueue(CustomerRequest customerData)
        {
            try
            {
                _lock.AcquireWriterLock(-1);

                Server.CustomerRequest customerRequest = new Server.CustomerRequest { ItemCount = customerData.itemCount, Delay = customerData.delay };

                var enter = counter.EnterQueue(customerRequest);

                return enter.Id;
            }
            catch (Exception)
            {
                log.Info("Failed to acquire writer lock");
                throw;
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }
        }
        /// <summary>
        /// Removes customer from queue
        /// </summary>
        /// <param name="customerData">Request containing customer data</param>
        /// <returns>boolean indicating if the operation completed sucessfuly</returns>
        public bool LeaveQueue(CustomerRequest customerData)
        {
            try
            {
                _lock.AcquireWriterLock(-1);

                Server.CustomerRequest customerRequest = new Server.CustomerRequest { ItemCount = customerData.itemCount, Id = customerData.id, Delay = customerData.delay };
                var leave = counter.LeaveQueue(customerRequest);

                return leave.State;
            }
            catch (Exception)
            {
                log.Info("Failed to acquire writer lock");
                throw;
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }
        }
        /// <summary>
        /// Checks if customer finished buying items
        /// </summary>
        /// <param name="customerData">Request containing customer data</param>
        /// <returns>boolean indicating if the operation completed sucessfuly</returns>
        public bool IsServed(CustomerRequest customerData)
        {
            try
            {
                _lock.AcquireReaderLock(-1);
                Server.CustomerRequest customerRequest = new Server.CustomerRequest { ItemCount = customerData.itemCount, Id = customerData.id, Delay = customerData.delay };
                var IsServed = counter.IsServed(customerRequest).State;

                return IsServed;
            }
            catch (Exception)
            {
                log.Info("Failed to acquire reader lock");
                throw;
            }
            finally
            {
                _lock.ReleaseReaderLock();
            }
        }
    }
}