using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Well.Models;

namespace Well.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Uploadfiles(HttpPostedFileBase file)
        {
            try
            {
                // Deal with a series of possible input file error conditions

                // The file is zero length therefore not a LAS file by definition
                var lasFileName = Path.GetFileName(file.FileName);
                if (file.ContentLength == 0) return View(new Model (lasFileName + " Upload error: Empty file received by the server."));

                var fileName = Path.GetFileName(file.FileName);

                // Empty file name
                if (string.IsNullOrWhiteSpace(fileName)) return View(new Model (lasFileName + " Upload error: The file name received by the server was null or white space."));
                
                // No .las file extension.  Could check the file by parsing it but that is beyond the scope of this story. 
                var extension = Path.GetExtension(lasFileName).ToLowerInvariant();
                var tester = ".LAS".ToLowerInvariant();
                if (! string.Equals( extension, tester) ) 
                    return View
                        (new Model (lasFileName + " Upload error: The file received by the server did not have a LAS file extension:" + extension + " and " + tester ));

                // As a safety check, refuse files bigger than 100,000,000 characters
                if (file.ContentLength > 100000000) return View(new Model (lasFileName + " Upload error: More than 100000000 Bytes of data was received by the server."));

                // Error conditions are finished with, so load the LAS file, thin the data to JSON form suitable for D3JS and C3JS
                var path = Path.Combine(Server.MapPath("~/"), fileName);
                file.SaveAs(path);
                var inputWell = (new LAS(path)).GetWell();
                
                // We believe everything is OK, so return the view for display
                return View(new Model(inputWell.WellToJson(40, 12)));
            }

            catch (Exception ex)
            // Catch any unforseen blowups
            {
                return View(ex.Message);
            }
        }
    }
}
