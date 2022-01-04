using Microsoft.AspNetCore.Mvc;
using OPSPay.Client;
using OPSPay.Client.Exceptions;
using OPSPay.Client.Types;
using OPSWebApplication.Models;
using System.Diagnostics;

namespace OPSWebApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IOpspay _opspay;

        public HomeController(ILogger<HomeController> logger, IOpspay opspay)
        {
            _logger = logger;
            _opspay = opspay;
        }

        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Создать платеж
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionName("Index")]
        public async Task<IActionResult> PayAsync()
        {
            int orderId = new Random().Next(1000, 10000);

            // счет в платеже
            Order order = new Order
            {
                // MerchantId = 6,
                OrderId = orderId,
                OrderNumber = $"A-{orderId}",
                OrderDate = DateTime.Now,
                Items = new List<OrderItem>
                        {
                            new OrderItem
                            {
                                ProductId = "356",
                                Description = "Первый товар",
                                Price = 500000,
                                // Quantity = 1,
                                // Unit = "шт."
                            },
                        new OrderItem
                        {
                            ProductId = "357",
                            Description = "Второй товар",
                            Price = 25500,
                            Quantity = 5.5,
                            Unit = "кг."
                        }
                },
            };

            // Общие параметры платежа
            Payment payment = new Payment()
            {
                IsTest = true,
                Orders = new List<Order> { order }
            };

            string mess = "All well done!!!";
            try
            {
                // создать платеж в OPS Pay   
                var result = await _opspay.CreatePaymentAsync(payment);

                // сделать редирект на оплату в процессинг OPS Pay
                if (result.State == PaymentState.Added)
                    return Redirect(_opspay.GetUrlRedirectToPay(result.PaymentNumber!));

            }
            catch (OpsException ex)
            {
                mess = ex.Message;
            }

            return Content(mess);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}