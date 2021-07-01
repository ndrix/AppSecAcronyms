namespace AppSecAcronyms.Controllers
{
    using AppSecAcronyms.Helpers;
    using AppSecAcronyms.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

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
                Response.Cookies.Append(cookieName, $"Secretvalue", options);

                CookieOptions options2 = new CookieOptions()
                {
                    Expires = DateTime.Now.AddMonths(4),
                    Secure = true,
                    HttpOnly = true
                };
                Response.Cookies.Append($"Safe{cookieName}", $"HahaHttpOnlyCookie", options2);

            }

            return View();
        }

        [Route("showmetheanswers")]
        public IActionResult Answers()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        /// <summary>
        /// Shows settings for admin only
        /// </summary>
        /// <returns></returns>
        [Route("settings"), HttpGet]
        public IActionResult Settings()
        {
            if (isAdmin())
            {
                using (SqlConnection conn = new SqlConnection(Utils.GetConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT value FROM settings WHERE name = 'csp'", conn))
                    {
                        try
                        {
                            using (SqlDataReader rdr = cmd.ExecuteReader())
                            {
                                while (rdr.Read())
                                {
                                    ViewBag.cspvalue = rdr["value"];
                                }
                            }
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                }

                return View();
            }
            else
            {
                return View("401");
            }
        }

        [Route("settings"), HttpPost]
        public string ChangeSettings(Boolean csp)
        {
            if (isAdmin())
            {
                using (SqlConnection conn = new SqlConnection(Utils.GetConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("UPDATE settings SET value = @NewValue WHERE name = 'csp'", conn))
                    {
                        try
                        {
                            cmd.Parameters.AddWithValue("@NewValue", csp.ToString());
                            int? i = cmd.ExecuteNonQuery();
                        }
                        catch (Exception)
                        {

                            throw;
                        }
                    }
                }
                return csp.ToString();
            }
            return "error";
        }

        [Route("doucblecheck"), HttpGet]
        public IActionResult CheckPictures()
        {
            if (isAdmin())
            {
                using (SqlConnection conn = new SqlConnection(Utils.GetConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT id, filename FROM images WHERE IsApproved = 0", conn))
                    {
                        try
                        {
                            using (SqlDataReader rdr = cmd.ExecuteReader())
                            {
                                while (rdr.Read())
                                {
                                    ViewBag.cspvalue = rdr["value"];
                                }
                            }
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                }

                return View();
            }
            else
            {
                return View("401");
            }
        }



        private Boolean isAdmin()
        {
            try
            {
                if (!string.IsNullOrEmpty(Request.Cookies["admin"]))
                {
                    string givenValue = Request.Cookies["admin"];
                    string secret = Utils.GetMagicCookie();
                    if (!string.IsNullOrEmpty(secret))
                    {
                        return secret.Equals(givenValue);
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }

    }
}
