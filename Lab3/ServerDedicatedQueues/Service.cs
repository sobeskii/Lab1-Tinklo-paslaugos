using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using NLog;
using Newtonsoft.Json;

using Common.MQ;


namespace Server 
{
	/// <summary>
	/// Service
	/// </summary>
	class Service
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
		/// Logger for this class.
		/// </summary>
		private Logger log = LogManager.GetCurrentClassLogger();


		/// <summary>
		/// Connection to RabbitMQ message broker.
		/// </summary>
		private IConnection rmqConn;

		/// <summary>
		/// Communications channel to RabbitMQ message broker.
		/// </summary>
		private IModel rmqChann;

		/// <summary>
		/// Service logic.
		/// </summary>
		private readonly ICounter _logic = new CounterLogic();


		/// <summary>
		/// Constructor.
		/// </summary>
		public Service() 
		{
			//connect to the RabbitMQ message broker
			var rmqConnFact = new ConnectionFactory();
			rmqConn = rmqConnFact.CreateConnection();

			//get channel, configure exchanges and request queue
			rmqChann = rmqConn.CreateModel();
			
			rmqChann.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Direct);
			rmqChann.QueueDeclare(queue: ServerQueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
			rmqChann.QueueBind(queue: ServerQueueName, exchange: ExchangeName, routingKey: ServerQueueName, arguments: null);

			//connect to the queue as consumer
			//XXX: see https://www.rabbitmq.com/dotnet-api-guide.html#concurrency for threading issues
			var rmqConsumer = new EventingBasicConsumer(rmqChann);
			rmqConsumer.Received += (consumer, delivery) => OnMessageReceived(((EventingBasicConsumer)consumer).Model, delivery);
			rmqChann.BasicConsume(queue: ServerQueueName, autoAck: true, consumer : rmqConsumer);
		}

		/// <summary>
		/// Is invoked to process messages received.
		/// </summary>
		/// <param name="channel">Related communications channel.</param>
		/// <param name="delivery">Message deliver data.</param>
		private void OnMessageReceived(IModel channel, BasicDeliverEventArgs delivery)
		{
			try 
			{
				//get RPC call request
				var request = JsonConvert.DeserializeObject<RPCMessage>(Encoding.UTF8.GetString(delivery.Body.ToArray()));

				//prepare common reply properties
				var replyProps = channel.CreateBasicProperties();
				replyProps.CorrelationId = delivery.BasicProperties.CorrelationId;
				
				//make the call and send response
				switch( request.Action )
				{
					case "Call_NeedRefill":
					{
						//make the call
						bool result;
						lock( _logic )
						{
							result = _logic.NeedRefill();
						}
						//send response
						var response = 
							new RPCMessage() {
								Action = "Result_NeedRefill",
								Data = JsonConvert.SerializeObject(result)
							};

						channel.BasicPublish(
							exchange : ExchangeName, 
							routingKey : delivery.BasicProperties.ReplyTo, 
							basicProperties : replyProps,
							body: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)) 
						);

						//
						break;
					}

					case "Call_TakeOffer":
					{
						//get call arguments
						var args = JsonConvert.DeserializeObject<OfferRequest>(request.Data);

						//make the call
						bool result;
						lock( _logic )
						{
							result = _logic.TakeOffer(args);
						}

						//send response
						var response = 
							new RPCMessage() {
								Action = "Result_TakeOffer",
								Data = JsonConvert.SerializeObject(result)
							};
							
						channel.BasicPublish(
							exchange : ExchangeName, 
							routingKey : delivery.BasicProperties.ReplyTo, 
							basicProperties : replyProps,
							body: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)) 
						);

						//
						break;
					}

					case "Call_EnterQueue":
						{
							//get call arguments
							var args = JsonConvert.DeserializeObject<CustomerRequest>(request.Data);

							//make the call
							int result = 0;
							lock (_logic)
							{
								result = _logic.EnterQueue(args);
							}

							//send response
							var response =
								new RPCMessage()
								{
									Action = "Result_EnterQueue",
									Data = JsonConvert.SerializeObject(result)
								};

							channel.BasicPublish(
								exchange: ExchangeName,
								routingKey: delivery.BasicProperties.ReplyTo,
								basicProperties: replyProps,
								body: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response))
							);

							//
							break;
						}
					case "Call_LeaveQueue":
						{
							//get call arguments
							var args = JsonConvert.DeserializeObject<CustomerRequest>(request.Data);

							//make the call
							bool result;
							lock (_logic)
							{
								result = _logic.LeaveQueue(args);
							}

							//send response
							var response =
								new RPCMessage()
								{
									Action = "Result_LeaveQueue",
									Data = JsonConvert.SerializeObject(result)
								};

							channel.BasicPublish(
								exchange: ExchangeName,
								routingKey: delivery.BasicProperties.ReplyTo,
								basicProperties: replyProps,
								body: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response))
							);

							//
							break;
						}
					case "Call_IsServed":
						{
							//get call arguments
							var args = JsonConvert.DeserializeObject<CustomerRequest>(request.Data);

							//make the call
							bool result;
							lock (_logic)
							{
								result = _logic.IsServed(args);
							}

							//send response
							var response =
								new RPCMessage()
								{
									Action = "Result_IsServed",
									Data = JsonConvert.SerializeObject(result)
								};

							channel.BasicPublish(
								exchange: ExchangeName,
								routingKey: delivery.BasicProperties.ReplyTo,
								basicProperties: replyProps,
								body: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response))
							);

							//
							break;
						}
					default: 
					{
						log.Info($"Unsupported type of RPC action '{request.Action}'. Ignoring the message.");
						break;
					}
				}
			}
			catch( Exception e )
			{
				log.Error(e, "Unhandled exception caught when processing a message. The message is now lost.");				
			}			
		}
	}
}