using Microsoft.AspNetCore.Mvc;
using OPSPay.Client.Requests;
using OPSPay.Client.Results;
using OPSPay.Client.Types;
using OPSWebApplication.Models;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Xml.Linq;

namespace OPSWebApplication.Controllers
{
    public class ProcessController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IOpspay _opspay;

        public ProcessController(ILogger<HomeController> logger, IOpspay opspay)
        {
            _logger = logger;
            _opspay = opspay;
        }

        public IActionResult Index([FromServices] IHttpContextAccessor accessor)
        {
            HttpContext? context = accessor.HttpContext;
            var query = context.Request.QueryString;

            RequestAction action = _opspay.GetRequestAction(context.Request.QueryString.Value);

            // Если проверка возможности оплаты у продавца
            if (action is CheckPaymentRequest && action.Error == ErrorCodeType.NoErrors)
            {
                // TODO: проверить у продавца по врутренней системе статус счета и ответить им
                PaymentStateResult result = new PaymentStateResult
                {
                    State = PaymentState.Added,
                    Error = ErrorCodeType.NoErrors,
                    ActionDate = DateTime.Now
                };

                XDocument xDoc = _opspay.GetXmlPaymentStateResult((CheckPaymentRequest)action, result);
                string content = xDoc.ToString(SaveOptions.DisableFormatting);
                MediaTypeHeaderValue contType = new MediaTypeHeaderValue("application/xml");

                return Content(content, contType.MediaType);
            }

            // Если оплата
            if (action is PayPaymentRequest && action.State == PaymentState.Payable && action.Error == ErrorCodeType.NoErrors)
            {
                // TODO: проверить у продавца по врутренней системе статус счета и если он уже оплачен, то вернуть статус и ErrorCode = IllegalOperation
                PaymentStateResult result = new PaymentStateResult
                {
                    State = PaymentState.Paid,
                    Error = ErrorCodeType.NoErrors,
                    ActionDate = DateTime.Now
                };

                XDocument xDoc = _opspay.GetXmlPaymentStateResult((PayPaymentRequest)action, result);

                string content = xDoc.ToString(SaveOptions.DisableFormatting);
                MediaTypeHeaderValue contType = new MediaTypeHeaderValue("application/xml");

                return Content(content, contType.MediaType);
            }

            // Если вызов Success
            if (action is RedirectPaymentRequest && action.State == PaymentState.Paid && action.Error == ErrorCodeType.NoErrors)
            {
                return Content("Оплата прошла успешно!");
            }

            // Если вызов Cancel
            if (action is RedirectPaymentRequest && action.State == PaymentState.Cancel && action.Error == ErrorCodeType.NoErrors)
            {
                return Content("Оплата отменена!");
            }

            return Content(query.ToString());
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}