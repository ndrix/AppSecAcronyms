using AppSecAcronyms.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AppSecAcronyms.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            string cookieName = "MsSecurityWebinar";
            if (string.IsNullOrEmpty(Request.Cookies[cookieName]))
            {
                CookieOptions options = new CookieOptions()
                {
                    Expires = DateTime.Now.AddMonths(4),
                    Secure = true,
                    HttpOnly = false
                };
                Response.Cookies.Append(cookieName, $"Secret value: 42", options);

                CookieOptions options2 = new CookieOptions()
                {
                    Expires = DateTime.Now.AddMonths(4),
                    Secure = true,
                    HttpOnly = true
                };
                Response.Cookies.Append($"Safe{cookieName}", $"Can't read me in JS", options);

            }

            return View();
        }

        [Route("showmetheanswers")]
        public IActionResult Answers()
        {
            return View();
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
