using System.Web.Mvc;
using WebModelFactory;
using Model.Web;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CasualWebSite.Controllers
{
    [HandleError]
    public class HomeController : ControllerBase<HomeModelFactory>
    {
        #region Get Methods
        public ActionResult Index()
        {
            ViewData["Message"] = "Welcome to ASP.NET MVC!";

            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Pokemon()
        {
            PokemonModel model = base.ModelFactory.LoadCasual();

            return View(model);
        }

        [HttpGet]
        public ActionResult Download(string path)
        {
            var rangeResult = Request.Params["HTTP_RANGE"];

            string root = @"C:\Users\Xhertas\";

            string fullPath = root + path;

            string extension = Path.GetExtension(fullPath);

            if (rangeResult == null)
            {
                string requestStr = ModelFactoryBase.Stringify(Request);

                var fileStream = new FileStream(fullPath, FileMode.Open);

                return File(fileStream, "application/unknown", "new file" + extension);
            }
            else
            {
                long bytesToSkip = long.Parse(Regex.Match(rangeResult, @"\d+").Value, NumberFormatInfo.InvariantInfo);
                
                long fSize = (new System.IO.FileInfo(fullPath)).Length;
                
                var fileStream = new FileStream(fullPath, FileMode.Open);
                fileStream.Position = bytesToSkip;

                var result = File(fileStream, "application/unknown", "new file" + extension);

                long startbyte = 0;
                long endbyte = fSize - 1;
                long desSize = endbyte - startbyte + 1;
                Response.StatusCode = 206;
                Response.AddHeader("Content-Length", desSize.ToString());
                Response.AddHeader("Content-Range", string.Format("bytes {0}-{1}/{2}", startbyte, endbyte, fSize));
                //Data

                return result;
            }
        }
        #endregion

        #region Post Methods
        [HttpPost]
        public ActionResult Pokemon(PokemonModel request)
        {
            PokemonModel model = ModelState.IsValid ? base.ModelFactory.LoadCasual(request) : base.ModelFactory.LoadCasual();

            return View(model);
        }
        #endregion
    }
}
