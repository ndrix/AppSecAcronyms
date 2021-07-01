namespace AppSecAcronyms
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Builder;
    using AppSecAcronyms.Helpers;

    /// <summary>
    /// Largely implemented with tips from https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?tabs=aspnetcore2x#writing-middleware
    /// </summary>


    public class CSPMiddleWare
    {
        private readonly RequestDelegate _next;

        public CSPMiddleWare(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext context)
        {
            if (IsCspEnabled())
            {
                context.Response.Headers.Add(
                    "Content-Security-Policy",
                    "script-src 'self' https://az416426.vo.msecnd.net;" +
                    // "style-src 'self'; " +
                    "img-src 'self' https://striketla.blob.core.windows.net;"
                );

            }

            return this._next(context);
        }


        private bool IsCspEnabled()
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
                                string s = rdr["value"].ToString();
                                if (s != null && s.Length > 0)
                                {
                                    return s.ToLower().Equals("true");
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // fail safe 
                        return true;
                    }
                }
            }

            // fail safe
            return true;
        }

    }

    /// Exposing the class statically 
    public static class RequestCspMiddlewareExtensions
    {
        public static IApplicationBuilder UseCspMiddleWare(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CSPMiddleWare>();
        }
    }
}
