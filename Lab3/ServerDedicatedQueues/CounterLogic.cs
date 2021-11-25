using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace Server
{
	/// <summary>
	/// Service logic.
	/// </summary>
	class CounterLogic : ICounter 
	{
        /// <summary>
        /// Logger for this class.
        /// </summary>
        Logger log = LogManager.GetCurrentClassLogger();

        readonly ReaderWriterLock _lock = new();
        public CounterLogic() => new Thread(Run).Start();

        /// <summary>
        /// Unique generated customer ID
        /// </summary>
        private int _customerid = 0;
        /// <summary>
        /// Property indicating if the counter needs an item refill
        /// </summary>
        bool Refill { get; set; }
        /// <summary>
        /// Generates a random customer ID
        /// </summary>
        /// <returns>Customer ID</returns>
        int GenerateCustomerId()
        {
            return _customerid++;
        }
        /// <summary>
        /// Count of items on which a refill is triggered
        /// </summary>
        const int LowItemCount = 0;
        /// <summary>
        /// Initial item count
        /// </summary>
        private int ItemCount { get; set; } = 30;
        /// <summary>
        /// List that represents the customer queue
        /// </summary>
        private List<Customer> queue { get; set; } = new List<Customer>();
        /// <summary>
        /// List of offers that loaders give
        /// </summary>
        private List<Offer> offers { get; set; } = new List<Offer>();
        /// <summary>
        /// Amount of items that customer is buying 
        /// </summary>
        private int CustomerItems { get; set; } = 0;

        /// <summary>
        /// Runs counter that serves customers every 2 seconds
        /// </summary>
        private void Run()
        {
            log.Info($"Counter started");
            while (true)
            {
                int WaitTime = (Refill) ? 5000 : 2000;
                try
                {
                    _lock.AcquireWriterLock(-1);
                    if (!Refill)
                    {
                        if (queue.Count > 0)
                        {
                            Customer customer = queue.First();
                            if (!customer.IsServed)
                            {
                                CustomerItems = customer.NumberOfItems;
                                RefreshProperties(CustomerItems);
                                if (!Refill)
                                {
                                    customer.IsServed = true;
                                    ItemCount -= customer.NumberOfItems;
                                    log.Info($"Serving customer: {customer.Id}");
                                    log.Info($"Customer {customer.Id} has bought {customer.NumberOfItems} item(s). There are {ItemCount} item(s) remaining.");
                                    CustomerItems = 0;
                                }
                                else
                                {
                                    WaitTime = 5000;
                                    log.Info($"Not enough items to serve customer. Clearing queue...");
                                }
                            }
                            RefreshProperties(CustomerItems);
                        }
                    }
                    else
                    {
                        WaitTime = 5000;
                        Offer offer = offers.OrderBy(o => o.Ratio).First();
                        log.Info($"Offer with {offer.Ratio} ratio has won. Adding {offer.NumberOfItems} items to the counter...");
                        ItemCount += offer.NumberOfItems;
                        offers.Clear();
                        RefreshProperties();
                    }
                }
                catch (ApplicationException)
                {
                    log.Info("Failed to acquire writer lock");
                    throw;
                }
                finally
                {
                    _lock.ReleaseWriterLock();
                }
                Thread.Sleep(WaitTime);
            }
        }
        /// <summary>
        /// Checks if counter needs an item refill
        /// </summary>
        /// <returns>boolean indicating if refill is needed</returns>
        public bool NeedRefill()
        {
            try
            {
                _lock.AcquireReaderLock(-1);
                return Refill;
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
                Offer offer = new Offer(offerData.itemCount, offerData.price);
                offers.Add(offer);
                log.Info($"Loader offered {offerData.itemCount} items for the price of {offerData.price}");
                return true;
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

                int id = GenerateCustomerId();

                Customer customer = new Customer(id, customerData.itemCount, customerData.delay, false);
                queue.Add(customer);
                log.Info($"Customer {customer.Id} is waiting in queue. Queue size is {queue.Count}");

                return id;
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
                log.Info($"Customer {customerData.id} is leaving the queue");
                queue.Remove(queue.Find(x => x.Id == customerData.id));

                return true;
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
                Customer customer = queue.Find(x => x.Id == customerData.id);
                bool IsServed = (customer != null) ? customer.IsServed : false;

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
        /// <summary>
        /// Refreshes the refill property
        /// </summary>
        void RefreshProperties(int itemsOnCounterCount = 0)
        {
            Refill = (ItemCount - itemsOnCounterCount) < LowItemCount;
        }
    }
}