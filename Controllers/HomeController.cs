using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SPOJ.Models;
using SPOJ.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Renci.SshNet;
using System.IO;

namespace SPOJ.Controllers
{
    
    public class HomeController : Controller
    {
        SpojContext db;
        UserManager<User> _userManager;
        SignInManager<User> _signInManager;
        IWebHostEnvironment _appEnvironment;
        

        public HomeController(SpojContext context, IWebHostEnvironment appEnvironment, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            db = context;
            _appEnvironment = appEnvironment;
            _userManager = userManager;
            _signInManager = signInManager;
            
        }

         
        public IActionResult Menu()
        {
            if (User.Identity.IsAuthenticated)
            {
                return View();
            }
            return RedirectToAction("AuthErr","Account");
        }


        public IActionResult Privacy()
        {

            //if (User.Identity.IsAuthenticated)
            if (User.IsInRole("admin"))
            {
                return View();
            }
            return RedirectToAction("AuthErr", "Account");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
