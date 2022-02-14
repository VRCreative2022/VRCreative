using Microsoft.AspNetCore.Mvc;
using vrcreative.Extensions;
using VRCreative.Models;
using VRCreative.Models.ViewModels;
using VRCreative.Utility;
using VRCreativeDataAccess.Data.Repository;

namespace vrcreative.Areas.Customer.Controllers
{

    [Area("Customer")]
    public class CartController : Controller
    {

        private readonly IUnitOfWork? _unitOfWork;

        [BindProperty]
        public CartViewModel CartVM { get; set; }



        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            CartVM = new CartViewModel()
            {
                OrderHeader = new Models.OrderHeader(),
                ServiceList = new List<Service>()
            };

        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetObject<List<int>>(StaticDetails.SessionCart) != null)
            {
                List<int> sessionList = new();
                sessionList = HttpContext.Session.GetObject<List<int>>(StaticDetails.SessionCart);
                foreach (int serviceId in sessionList)
                {
                    CartVM.ServiceList.Add(_unitOfWork.Service.GetFirstOrDefault(u => u.Id == serviceId, includeProperties: "Category"));
                }
            }
            return View(CartVM);
        }


        public IActionResult Remove(int serviceId)
        {
            List<int> sessionList = new List<int>();
            sessionList = HttpContext.Session.GetObject<List<int>>(StaticDetails.SessionCart);
            sessionList.Remove(serviceId);
            HttpContext.Session.SetObject(StaticDetails.SessionCart, sessionList);

            return RedirectToAction(nameof(Index));
        }


        public IActionResult Summary()
        {
            if (HttpContext.Session.GetObject<List<int>>(StaticDetails.SessionCart) != null)
            {
                List<int> sessionList = new List<int>();
                sessionList = HttpContext.Session.GetObject<List<int>>(StaticDetails.SessionCart);
                foreach (int serviceId in sessionList)
                {
                    CartVM.ServiceList.Add(_unitOfWork.Service.GetFirstOrDefault(u => u.Id == serviceId));
                }
            }
            return View(CartVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public IActionResult SummaryPOST()
        {
            if (HttpContext.Session.GetObject<List<int>>(StaticDetails.SessionCart) != null)
            {
                List<int> sessionList = new List<int>();
                sessionList = HttpContext.Session.GetObject<List<int>>(StaticDetails.SessionCart);
                CartVM.ServiceList = new List<Service>();
                foreach (int serviceId in sessionList)
                {
                    CartVM.ServiceList.Add(_unitOfWork.Service.Get(serviceId));
                }
            }

            if (!ModelState.IsValid)
            {
                return View(CartVM);
            }
            else
            {
                CartVM.OrderHeader.OrderDate = DateTime.Now;
                CartVM.OrderHeader.Status = StaticDetails.StatusSubmitted;
                CartVM.OrderHeader.ServiceCount = CartVM.ServiceList.Count;
                _unitOfWork.OrderHeader.Add(CartVM.OrderHeader);

                // Save to Database 
                _unitOfWork.Save();

                foreach (var item in CartVM.ServiceList)
                {
                    OrderDetails orderDetails = new OrderDetails
                    {
                        ServiceId = item.Id,
                        OrderHeaderId = CartVM.OrderHeader.Id,
                        ServiceName = item.Name,
                        Price = item.Price
                    };

                    // Save to OrderDetails Database 
                    _unitOfWork.OrderDetails.Add(orderDetails);

                }
                _unitOfWork.Save();
                HttpContext.Session.SetObject(StaticDetails.SessionCart, new List<int>());
                return RedirectToAction("OrderConfirmation", "Cart", new { id = CartVM.OrderHeader.Id });
            }
        }



        public IActionResult OrderConfirmation(int id)
        {
            return View(id);
        }

    }
}
