using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VRCreativeDataAccess.Data.Repository;

namespace vrcreative.Areas.Admin.Controllers
{
    [Area("Admin")]
    // [Authorize(Roles = StaticDetails.Admin)]
    public class UserController : Controller
    {

        private readonly IUnitOfWork? _unitOfWork;

        public UserController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            return View(_unitOfWork.User.GetAll(u => u.Id != claims.Value));
        }

        public IActionResult Lock(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            _unitOfWork.User.LockUser(id);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult UnLock(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            _unitOfWork.User.UnLockUser(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
