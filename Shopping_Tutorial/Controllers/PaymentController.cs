using Microsoft.AspNetCore.Mvc;
using Shopping_Tutorial.Models.Order;
using Shopping_Tutorial.Models.Vnpay;
using Shopping_Tutorial.Services.Momo;
using Shopping_Tutorial.Services.Vnpay;

namespace Shopping_Tutorial.Controllers
{
    public class PaymentController : Controller
    {
        private IMomoService _momoService;
        private readonly IVnpayService _vnPayService;

        public PaymentController(IMomoService momoService, IVnpayService vnPayService)
        {
            _momoService = momoService;
            _vnPayService = vnPayService;
        }

        [HttpPost]
        [Route("CreatePaymentMomo")]
        public async Task<IActionResult> CreatePaymentMomo(OrderInfoModel model)
        {
            var response = await _momoService.CreatePaymentAsync(model);
            return Redirect(response.PayUrl);
        }

        
        [Route("CreatePaymentVnpay")]
        public IActionResult CreatePaymentVnpay(PaymentInformationModel model)
        {
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);

            return Redirect(url);
        }

        

    }
}
