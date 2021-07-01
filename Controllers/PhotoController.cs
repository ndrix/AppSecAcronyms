namespace AppSecAcronyms.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using System.IO;
    using System.Data.SqlClient;
    using AppSecAcronyms.Helpers;
    using Microsoft.AspNetCore.Http;

    public class PhotoController : Controller
    {

        [HttpGet]
        [Route("images")]
        public string GetImages()
        {
            // get all the images form the DB 
            List<string> images = new List<string>();
            using (SqlConnection conn = new SqlConnection(Utils.GetConnectionString()))
            {
                string[] admins = new string[] { };
                int counterAdd = 0;
                conn.Open();
                try
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT TOP (20) filename, added_by FROM images ORDER BY id DESC", conn))
                    {
                        using (SqlDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                string filename = rdr["filename"].ToString();
                                string addedBy = rdr["added_by"].ToString();
                                if (filename.Contains("<") && filename.Contains(">"))
                                {
                                    /// Only allow "poisoned" images given by self or a 
                                    /// user defined in the admins array.                                    
                                    if (admins.Contains(addedBy))
                                    {
                                        images.Add(string.Format("https://webinarmsftsec.blob.core.windows.net/photos/{0}", filename));
                                        counterAdd++;
                                    }
                                }
                                else
                                {
                                    images.Add(string.Format("https://webinarmsftsec.blob.core.windows.net/photos/{0}", filename));
                                    counterAdd++;
                                }

                                /// Only show 10 images
                                if (counterAdd > 10)
                                    break;
                            }
                        }
                    }
                }
                catch (Exception)
                {

                }
                finally
                {
                    conn.Close();
                }
            }

            string retval = string.Empty;
            int counter = 0;

            var unique_images = new HashSet<string>(images);

            foreach (string img in unique_images)
            {
                counter++;
                retval += "<div class=\"item item" + counter.ToString() + (counter == 1 ? " active" : "") + "\" " +
                            "style=\"background-size: cover; -moz-background-size: cover; background: linear-gradient( rgba(0, 0, 0, 0.9), rgba(0, 0, 0, 0.5) ), url(" + img + ")\" />";
            }

            return retval;
        }




        [HttpPost]
        [Route("upload")]
        public IActionResult UploadFile(IFormFile f, string container)
        {
            if (f != null && f.FileName.Length > 0)
            {
                // Warning! could overwrite previous file

                // raw WebAPI 
                try
                {
                    #region Write to storage
                    // Make byteArray
                    byte[] imageData;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        f.OpenReadStream().CopyTo(ms);
                        imageData = ms.ToArray();
                    }

                    /// Protecting the environment
                    string hostname = "webinarmsftsec.blob.core.windows.net";

                    string url = String.Format("https://{0}{1}{2}{3}", hostname, container, f.FileName, Utils.GetSasToken());
                    if (Uri.TryCreate(url, UriKind.Absolute, out Uri tmp))
                    {
                        /// This is an SSRF call then
                        if (!tmp.Host.Equals(hostname))
                        {
                            string fakeToken = "?sv=2021-07-01&this_is_a_fake_token_but_your_SSRF_was_successful&well=done!";
                            url = String.Format("https://{0}{1}{2}{3}", hostname, container, f.FileName, fakeToken);
                        }
                    }


                    using (var client = new System.Net.WebClient())
                    {
                        client.Headers.Add("x-ms-date", DateTime.UtcNow.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
                        client.Headers.Add("x-ms-version", "2020-02-10");
                        client.Headers.Add("x-ms-blob-type", "BlockBlob");
                        client.Headers.Add("Content-Type", f.ContentType);
                        client.UploadData(url, "PUT", imageData);
                    }
                    #endregion


                    #region SQL write
                    /// Stuff in the DB now 
                    using (SqlConnection conn = new SqlConnection(Utils.GetConnectionString()))
                    {
                        conn.Open();
                        try
                        {
                            using (SqlCommand cmd = new SqlCommand("INSERT INTO images (filename, added_by) VALUES (@Filename, @User)", conn))
                            {
                                cmd.Parameters.AddWithValue("@Filename", f.FileName);
                                cmd.Parameters.AddWithValue("@User", User.Identity.Name);
                                int? i = cmd.ExecuteNonQuery();
                                if (!i.HasValue || i.Value != 1)
                                {
                                    // Something went wrong
                                    return Ok(new { status = "error", msg = "Not added to DB" });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            return Ok(new { status = "error", msg = ex.Message });
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                    #endregion

                }
                catch (Exception ex)
                {
                    return Ok(new { status = "error", msg = ex.Message });
                }
            }
            return Ok(new { status = "success", add = f.FileName });
        }



    }
}
