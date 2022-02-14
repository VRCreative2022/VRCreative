using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using vrcreative.Extensions;
using vrcreative.Models;
using VRCreative.Models.ViewModels;
using VRCreative.Utility;
using VRCreativeDataAccess.Data.Repository;

namespace vrcreative.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        //private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private HomeViewModel HomeVM;

        public HomeController(IUnitOfWork unitOfWork)
        {
            //   _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {

            HomeVM = new HomeViewModel()
            {
                CategoryList = _unitOfWork.Category.GetAll(),
                ServiceList = _unitOfWork.Service.GetAll()
            };
            return View(HomeVM);
        }


        public IActionResult Details(int id)
        {
            var serviceFromDb = _unitOfWork.Service.GetFirstOrDefault(includeProperties: "Category", filter: c => c.Id == id);
            return View(serviceFromDb);
        }

        public IActionResult AddToCart(int serviceId)
        {
            List<int> sessionList = new List<int>();
            if (string.IsNullOrEmpty(HttpContext.Session.GetString(StaticDetails.SessionCart)))
            {
                sessionList.Add(serviceId);
                HttpContext.Session.SetObject(StaticDetails.SessionCart, sessionList);
            }
            else
            {
                sessionList = HttpContext.Session.GetObject<List<int>>(StaticDetails.SessionCart);
                if (!sessionList.Contains(serviceId))
                {
                    sessionList.Add(serviceId);
                    HttpContext.Session.SetObject(StaticDetails.SessionCart, sessionList);
                }
            }

            return RedirectToAction(nameof(Index));
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}