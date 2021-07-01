namespace AppSecAcronyms.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;
    using System;
    using AppSecAcronyms.Helpers;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;

    public class AcronymController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }


        [HttpGet]
        [Route("list")]
        public string Get(string tla)
        {
            Response.ContentType = "application/json";
            return GetValue(tla);
        }


        [ValidateAntiForgeryToken]
        [HttpPost]
        [Route("add")]
        public string Post(string tla, string title, string desc)
        {
            Response.ContentType = "application/json";
            return PutValue(tla, title, desc);
        }


        [HttpGet]
        [Route("like/{id}")]
        public void Like(int id)
        {
            if (id < 1) return;

            // Allow for a better view
            // if (LikedAlready(id)) return;

            // Get data from DB
            using (SqlConnection conn = new SqlConnection(Utils.GetConnectionString()))
            {
                string ip = Request.HttpContext.Connection.RemoteIpAddress.ToString();


                conn.Open();
                try
                {
                    // Add a link so we can't add it twice
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO likes (tla_id, ip_address) VALUES (@id, @ip)", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.Parameters.AddWithValue("@User", User.Identity.Name);
                        cmd.ExecuteNonQuery();
                    }


                    using (SqlCommand cmd = new SqlCommand("UPDATE tlas SET likes = likes + 1 FROM tlas WHERE id = @Id", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception)
                {
                    return;
                }
                finally
                {
                    conn.Close();
                }
            }
            return;
        }





        #region TLA values
        private string GetValue(string tla)
        {
            List<string[]> retval = new List<string[]>();

            if (string.IsNullOrEmpty(tla))
                return string.Empty;

            // Get data from DB
            using (SqlConnection conn = new SqlConnection(Utils.GetConnectionString()))
            {
                conn.Open();
                try
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM tlas WHERE LOWER(tla) LIKE @Tla ORDER BY tla DESC", conn))
                    {
                        cmd.Parameters.AddWithValue("@Tla", String.Format("{0}%", tla.ToLower()));
                        using (SqlDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                string tmpTla = rdr["tla"].ToString();
                                string tmpTitle = rdr["title"].ToString();
                                string tmpDec = rdr["description"].ToString();
                                string tmpAuthor = rdr["added_by"].ToString();
                                string tmpLikes = rdr["likes"].ToString();
                                string tmpId = rdr["id"].ToString();

                                retval.Add(new string[] { tmpTla, tmpTitle, tmpDec, tmpAuthor, tmpLikes, tmpId });
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }

            if (retval.Count > 0)
            {
                var jobj = new Dictionary<string, object>();
                jobj.Add("success", true);
                jobj.Add("results", retval);
                JObject j = JObject.FromObject(jobj);
                return j.ToString();
            }

            return String.Empty;
        }


        /// <summary>
        /// Method to put TLA's in the DB
        /// </summary>
        /// <param name="tla"></param>
        /// <param name="desc"></param>
        private string PutValue(string tla, string title, string desc)
        {

            // Sanitize, you know, just in case
            tla = Sanitize(tla);
            title = Sanitize(title);
            desc = Sanitize(desc);

            // Title case the title
            title = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(title.ToLower());

            Dictionary<string, string> retval = new Dictionary<string, string>();
            // Do some checking
            if (tla.Length < 3)
            {
                retval.Add("status", "error");
                retval.Add("msg", "The acronym should be at least 3 characters");
                return JObject.FromObject(retval).ToString();
            }

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(desc))
            {
                retval.Add("status", "error");
                retval.Add("msg", "title and description cannot be empty.");
                return JObject.FromObject(retval).ToString(); ;
            }

            if (TlaAndTitleExists(tla, title))
            {
                retval.Add("status", "error");
                retval.Add("msg", "An acronym with this title exists already.");
                return JObject.FromObject(retval).ToString(); ;
            }

            /// get the IP
            string ip = Request.HttpContext.Connection.RemoteIpAddress.ToString();


            /// all is good, let's write it to the DB
            using (SqlConnection conn = new SqlConnection(Utils.GetConnectionString()))
            {
                conn.Open();
                try
                {
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO tlas (tla, title, description, added_by, likes)  VALUES (@Tla, @Title, @Desc, @AddedBy, 0)", conn))
                    {
                        cmd.Parameters.AddWithValue("@Tla", tla.Trim());
                        cmd.Parameters.AddWithValue("@Title", title.Trim());
                        cmd.Parameters.AddWithValue("@Desc", desc.Trim());
                        cmd.Parameters.AddWithValue("@AddedBy", ip);

                        int? success = cmd.ExecuteNonQuery();
                        if (success.HasValue)
                        {
                            retval.Add("status", "success");
                            return JObject.FromObject(retval).ToString(); ;
                        }
                        else
                        {
                            retval.Add("status", "error");
                            retval.Add("msg", "Database error");
                            return JObject.FromObject(retval).ToString(); ;
                        }
                    }
                }
                catch (Exception ex)
                {
                    retval.Add("error", String.Format("Something went wrong: {0}", ex.Message));
                    return JObject.FromObject(retval).ToString(); ;
                }
                finally
                {
                    conn.Close();
                }
            }
        }



        #endregion


        #region Helper functions
        private string Sanitize(string input)
        {
            input = System.Web.HttpUtility.HtmlEncode(input);
            return input.Trim();
        }



        private bool TlaAndTitleExists(string tla, string title)
        {
            using (SqlConnection conn = new SqlConnection(Utils.GetConnectionString()))
            {
                conn.Open();
                try
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM tlas WHERE tla = @Tla AND title LIKE @Title", conn))
                    {
                        cmd.Parameters.AddWithValue("@Tla", tla);
                        cmd.Parameters.AddWithValue("@Title", title);
                        using (SqlDataReader rdr = cmd.ExecuteReader())
                        {
                            if (rdr.Read())
                            {
                                conn.Close();
                                return true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }
            return false;
        }




        #endregion

    }
}
