//using AMS.Interfaces;
//using AMS.Models.ViewModel;
//using CRM.Services;
//using Microsoft.AspNetCore.Mvc;

//namespace AMS.Controllers
//{
//    public class AccountController : Controller
//    {

//        private readonly IUserRepository _userRepository;
//        private readonly AuthService _authService;



//        public AccountController(AuthService authService, IUserRepository userRepository)




//        {
//            _authService = authService;
//            _userRepository = userRepository;
//        }


//        public IActionResult Login()
//        {
//            TempData.Clear();
//            return View();
//        }


//[HttpPost]
//public async Task<IActionResult> Login(LoginVM model)

//{

//    var user = await _userRepository.GetByUsername(model.UserCode);
//    if (user == null)
//    {
//        ModelState.AddModelError("UserCode", "Invalid Credentials.");
//        return View(model);
//    }









//}






//    }
//}
