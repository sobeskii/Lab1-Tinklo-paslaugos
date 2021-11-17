using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers
{
    /// <summary>
    /// Service. Class must be marked public, otherwise ASP.NET core runtime will not find it.
    /// </summary>
    [Route("/service")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        /// <summary>
        /// Service logic
        /// </summary>
        private static readonly ICounter _logic = new CounterLogic();

        [HttpGet]
        [Route("NeedRefill")]
        public ActionResult<bool> NeedRefill()
        {
            return _logic.NeedRefill();
        }

        [HttpGet]
        [Route("IsServed")]
        public ActionResult<bool> IsServed([FromQuery] CustomerRequest customer)
        {
            return _logic.IsServed(customer);
        }

        [HttpPost]
        [Route("EnterQueue")]
        public ActionResult<int> EnterQueue([FromBody] CustomerRequest customer)
        {
            return _logic.EnterQueue(customer);
        }
        [HttpPost]
        [Route("LeaveQueue")]
        public ActionResult<bool> LeaveQueue([FromBody] CustomerRequest customer)
        {
            return _logic.LeaveQueue(customer);
        }
        [HttpPost]
        [Route("TakeOffer")]
        public ActionResult<bool> TakeOffer([FromBody] OfferRequest offer)
        {
            return _logic.TakeOffer(offer);
        }
    }
}



