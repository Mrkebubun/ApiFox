using ApiFox.DAL;
using ApiFox.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System.Threading.Tasks;
using Microsoft.Owin;
using System.Runtime.CompilerServices;
using System.Text;

namespace ApiFox.Controllers
{
   
    public class HomeController : Controller
    {
        ApifoxContext dbContext = new ApifoxContext();
        public ActionResult Index()
        {
            //var cookie = Request.Cookies["guid"];
            //if (cookie == null && !User.Identity.IsAuthenticated)
            //{
            //    cookie = new HttpCookie("guid", Guid.NewGuid().ToString());
            //    Response.Cookies.Add(cookie);
            //}

            return View();
        }
      
        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        public ActionResult Save(IEnumerable<HttpPostedFileBase> files)
        {
            var id = 0;
            string apiUrl = null;
            string username = User.Identity.IsAuthenticated ? User.Identity.Name : null;

            SaveUserInput(files, ref id, ref apiUrl, ref username);

            // Return an empty string to signify success
            return Json(new { id = id, apiUrl = apiUrl, newusername = username });
        }
        [ActionName("SaveGSheet")]
        public ActionResult Save(string gSheetUrl)
        {
            var id = 0;
            string apiUrl = null;
            string username = User.Identity.IsAuthenticated ? User.Identity.Name : null;

            SaveUserInput(gSheetUrl, ref id, ref apiUrl, ref username);

            // Return an empty string to signify success
            return Json(new { id = id, apiUrl = apiUrl, newusername = username });
        }

        private void SaveUserInput(string gSheetUrl, ref int id, ref string apiUrl, ref string username)
        {
            // The Name of the Upload component is "files"
            string defaultApiName = "gsheet.url";
            const string targetPrefix = "URL=";
            const string httpPrefix = "http://";
            
            var internetShortcut = new StringBuilder();
            internetShortcut.AppendLine("[InternetShortcut]");

            if (!string.IsNullOrEmpty(gSheetUrl))
            {
                if (gSheetUrl.StartsWith(httpPrefix) || gSheetUrl.StartsWith("https://"))
                    internetShortcut.AppendLine(targetPrefix + gSheetUrl);
                else
                    internetShortcut.AppendLine(targetPrefix + httpPrefix + gSheetUrl);
                string initialFileName;
                string fileName;
                string physicalPath;
                apiUrl = GenerateApiUrl(apiUrl, defaultApiName, out initialFileName, out fileName, out physicalPath);

                System.IO.File.WriteAllText(physicalPath, internetShortcut.ToString());

                //Store in DB
                StoreApi(ref id, apiUrl, ref username, initialFileName, fileName);
            }
        }

        private void SaveUserInput(IEnumerable<HttpPostedFileBase> files, ref int id, ref string apiUrl, ref string username)
        {
            // The Name of the Upload component is "files"
            if (files != null)
            {
                foreach (var file in files)
                {
                    string initialFileName;
                    string fileName;
                    string physicalPath;

                    // file.FileName - Some browsers send file names with full path.
                    // We are only interested in the file name.
                    apiUrl = GenerateApiUrl(apiUrl, file.FileName, out initialFileName, out fileName, out physicalPath);
                     
                    file.SaveAs(physicalPath);

                    //Store in DB
                    StoreApi(ref id, apiUrl, ref username, initialFileName, fileName);
                }
            }
        }
       
        private string GenerateApiUrl(string apiUrl, string defaultApiName, out string initialFileName, out string fileName, out string physicalPath)
        {
            var path = Server.MapPath(Path.Combine("~/App_Data", Path.GetFileName(defaultApiName)));
            var fileInfo = new FileInfo(path);
            initialFileName = Path.GetFileNameWithoutExtension(path);
            apiUrl = TryGetUniqueApiUrl(initialFileName);
            var fileInfo2 = new FileInfo(System.IO.Path.GetTempFileName());
            fileName = initialFileName + "--" + fileInfo2.Name + fileInfo.Extension;

            physicalPath = Server.MapPath(Path.Combine("~/App_Data", fileName));
            return apiUrl;
        }

        private void StoreApi(ref int id, string apiUrl, ref string username, string initialFileName, string fileName)
        {
            var record = new Models.ImportedFile { DateCreated = DateTime.UtcNow, Ip = Request.UserHostAddress, FileName = fileName };

            if (username == null)
            {
                username = Request.Cookies["guid"] != null ? Server.UrlDecode(Request.Cookies["guid"].Value) : null;
                if (username == null)
                    username = Guid.NewGuid().ToString() + "@anonymous" + DateTime.Now.Year + ".com";
            }

            var newApi = new Models.Apis
            {
                ApiName = initialFileName,
                ApiUrl = apiUrl,
                Owner = username,
                FileSource = record
            };
            dbContext.Entry(newApi).State = System.Data.Entity.EntityState.Added;
            dbContext.SaveChanges();
            id = newApi.Id;
        }
 
        private string TryGetUniqueApiUrl(string url)
        {
            //clean url
            var apiUrl = url.Trim().Trim(new char[] { '/' }).Trim(new char[] { '\\' });

            //check if it's an existing api url
            var existingApiUrl = dbContext.Apis.Any(a => a.ApiUrl.Equals(url, StringComparison.InvariantCultureIgnoreCase));
            if (existingApiUrl)
                apiUrl = apiUrl + GenerateRandomString();
            else
                return apiUrl;

            //try one more time
            existingApiUrl = dbContext.Apis.Any(a => a.ApiUrl.Equals(apiUrl, StringComparison.InvariantCultureIgnoreCase));
            return existingApiUrl ? apiUrl + GenerateRandomString() : apiUrl;
        }
        Random random = new Random((int)DateTime.Now.Ticks);
        private string GenerateRandomString(int length = 5)
        {
            string randString = "-";

            char[] codealphabet = "abcdefghijklmnopqrstuvwxyz12345".ToCharArray();
            while (randString.Length < length)
            {
                randString += codealphabet[random.Next(0, codealphabet.Length - 1)];
            }
            return randString;
        }

        public ActionResult Remove(string[] fileNames)
        {
            // The parameter of the Remove action must be called "fileNames"

            if (fileNames != null)
            {
                foreach (var fullName in fileNames)
                {
                    var fileName = Path.GetFileName(fullName);
                    var physicalPath = Path.Combine(Server.MapPath("~/App_Data"), fileName);

                    // TODO: Verify user permissions

                    if (System.IO.File.Exists(physicalPath))
                    {
                        // The files are not actually removed in this demo
                        System.IO.File.Delete(physicalPath);
                    }
                }
            }

            // Return an empty string to signify success
            return Content("");
        }
    }
}
