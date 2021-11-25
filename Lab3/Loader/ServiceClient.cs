using System;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using NLog;
using Common.MQ;


namespace Loader
{
	/// <summary>
	/// <para>RPC style wrapper for the service.</para>
	/// <para>Static members are thread safe, instance members are not.</para>
	/// </summary>
	class Counter : ICounter
	{
		/// <summary>
		/// Name of the request exchange.
		/// </summary>
		private static readonly String ExchangeName = "T120B180.DedicatedQueues.Exchange";

		/// <summary>
		/// Name of the request queue.
		/// </summary>
		private static readonly String ServerQueueName = "T120B180.DedicatedQueues.ServerQueue";

		/// <summary>
		/// Prefix for the name of the client queue.
		/// </summary>
		private static readonly String CustomerQueueNamePrefix = "T120B180.DedicatedQueues.ClientQueue_";


		/// <summary>
		/// Logger for this class.
		/// </summary>
		private Logger log = LogManager.GetCurrentClassLogger();


		/// <summary>
		/// Service client ID.
		/// </summary>
		public String ClientId {get;}

		/// <summary>
		/// Name of the client queue.
		/// </summary>
		private String ClientQueueName {get;}


		/// <summary>
		/// Connection to RabbitMQ message broker.
		/// </summary>
		private IConnection rmqConn;

		/// <summary>
		/// Communications channel to RabbitMQ message broker.
		/// </summary>
		private IModel rmqChann;


		/// <summary>
		/// Constructor.
		/// </summary>
		public Counter()
		{
			//initialize properties
			ClientId = Guid.NewGuid().ToString();
			ClientQueueName = CustomerQueueNamePrefix + ClientId;

			//connect to the RabbitMQ message broker
			var rmqConnFact = new ConnectionFactory();
			rmqConn = rmqConnFact.CreateConnection();

			//get channel, configure exchange and queue
			rmqChann = rmqConn.CreateModel();

			rmqChann.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Direct);
			rmqChann.QueueDeclare(queue: ClientQueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
			rmqChann.QueueBind(queue: ClientQueueName, exchange: ExchangeName, routingKey: ClientQueueName, arguments: null);

			//XXX: see https://www.rabbitmq.com/dotnet-api-guide.html#concurrency for threading issues
		}

		/// <summary>
		/// Generic method to call a remove operation on a server.
		/// </summary>
		/// <param name="requestAction">Name of the request action.</param>
		/// <param name="requestDataProvider">Request data provider.</param>
		/// <param name="responseAction">Name of the response action.</param>
		/// <param name="resultExtractor">Result extractor.</param>
		/// <typeparam name="RESULT">Type of the result.</typeparam>
		/// <returns>Result of the call.</returns>
		private RESULT Call<RESULT>(
			string requestAction,
			Func<String> requestDataProvider,
			string responseAction,
			Func<String, RESULT> resultExtractor
		) {
			//declare result storage
			RESULT result = default;

			//declare stuff used to avoid result owerwriting and to signal when result is ready
			var isResultReady = false;
			var resultReadySignal = new AutoResetEvent(false);

			//create request
			var request =
				new RPCMessage()
				{
					Action = requestAction,
					Data = requestDataProvider()
				};

			var requestProps = rmqChann.CreateBasicProperties();
			requestProps.CorrelationId = Guid.NewGuid().ToString();
			requestProps.ReplyTo = ClientQueueName;

			//ensure contents of variables set in main thread, are loadable by receiver thread
			Thread.MemoryBarrier();

			//attach message consumer to the response queue
			var consumer = new EventingBasicConsumer(rmqChann);
			consumer.Received +=
				(channel, delivery) => {
					//ensure contents of variables set in main thread are loaded into this thread
					Thread.MemoryBarrier();

					//prevent owerwriting of result, check if the expected message is received
					if( !isResultReady && (delivery.BasicProperties.CorrelationId == requestProps.CorrelationId) )
					{
						var response = JsonConvert.DeserializeObject<RPCMessage>(Encoding.UTF8.GetString(delivery.Body.ToArray()));
						if( response.Action == responseAction )
						{
							//extract the result
							result = resultExtractor(response.Data);

							//indicate result has been received, ensure it is loadable by main thread
							isResultReady = true;
							Thread.MemoryBarrier();

							//signal main thread that result has been received
							resultReadySignal.Set();
						}
						else
						{
							log.Info($"Unsupported type of RPC action '{request.Action}'. Ignoring the message.");
						}
					}
				};

			var consumerTag = rmqChann.BasicConsume(ClientQueueName, true, consumer);

			//send request
			rmqChann.BasicPublish(
				exchange : ExchangeName,
				routingKey : ServerQueueName,
				basicProperties : requestProps,
				body: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request))
			);

			//wait for the result to be ready
			resultReadySignal.WaitOne();

			//ensure contents of variables set by the receiver are loaded into this thread
			Thread.MemoryBarrier();

			//detach message consumer from the response queue
			rmqChann.BasicCancel(consumerTag);

			//
			return result;
		}
		/// <summary>
		/// Checks if counter needs an item refill
		/// </summary>
		/// <returns>boolean indicating if refill is needed</returns>
		public bool	NeedRefill() =>
			Call(
				"Call_NeedRefill",
				() => "",
				"Result_NeedRefill",
				bool.Parse
			);
		/// <summary>
		/// Takes offer and adds to offer list
		/// </summary>
		/// <param name="offer">Request containing offer data</param>
		/// <returns>boolean indicating if the operation completed sucessfuly</returns>
		public bool TakeOffer(OfferRequest offer) =>
			Call(
				"Call_TakeOffer",
				() => JsonConvert.SerializeObject(offer),
				"Result_TakeOffer",
				bool.Parse
			);
		/// <summary>
		/// Adds customer to queue
		/// </summary>
		/// <param name="customer">Request containing customer data</param>
		/// <returns>boolean indicating if the operation completed sucessfuly</returns>
		public int EnterQueue(CustomerRequest customer) =>
			Call(
				"Call_EnterQueue",
				() => JsonConvert.SerializeObject(customer),
				"Result_EnterQueue",
				int.Parse
			);
		/// <summary>
		/// Removes customer from queue
		/// </summary>
		/// <param name="customer">Request containing customer data</param>
		/// <returns>boolean indicating if the operation completed sucessfuly</returns>
		public bool LeaveQueue(CustomerRequest customer) =>
			Call(
				"Call_LeaveQueue",
				() => JsonConvert.SerializeObject(customer),
				"Result_LeaveQueue",
				bool.Parse
			);
		/// <summary>
		/// Checks if customer finished buying items
		/// </summary>
		/// <param name="customer">Request containing customer data</param>
		/// <returns>boolean indicating if the operation completed sucessfuly</returns>
		public bool IsServed(CustomerRequest customer) =>
			Call(
				"Call_IsServed",
				() => JsonConvert.SerializeObject(customer),
				"Result_IsServed",
				bool.Parse
			);
		}
}