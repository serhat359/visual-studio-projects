using DotNetCoreWebsite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace DotNetCoreWebsite.Controllers
{
    public class HomeController : BaseController
    {
        private readonly string extension = ".serhatCustom";

        public IConfiguration configuration;
        public FileNameHelper fileNameHelper;
        public CoreEncryption coreEncryption;

        public HomeController(IConfiguration configuration, FileNameHelper fileNameHelper, CoreEncryption coreEncryption)
        {
            this.configuration = configuration;
            this.fileNameHelper = fileNameHelper;
            this.coreEncryption = coreEncryption;
        }

        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult AllFiles(string q = null)
        {
            var rootPath = GetPathConfig();

            if (q != null)
                rootPath = System.IO.Path.Combine(rootPath, q);

            var directoryInfos = System.IO.Directory.EnumerateDirectories(rootPath).Select(x => new FileInfo
            {
                Name = new System.IO.DirectoryInfo(x).Name,
                IsFolder = true,
                FileSize = null
            });

            var fileInfos = System.IO.Directory.EnumerateFiles(rootPath).Select(x =>
            {
                var fInfo = new System.IO.FileInfo(x);
                return new FileInfo
                {
                    Name = fInfo.Name,
                    IsFolder = false,
                    FileSize = fInfo.Length
                };
            });

            return View(new AllFilesModel
            {
                FileList = directoryInfos.Concat(fileInfos).ToList(),
                BackFolderPath = GetBackFolderPath(q),
                CurrentPath = q != null ? q + "/" : ""
            });
        }

        public IActionResult DownloadFile(string q)
        {
            var rootPath = GetPathConfig();

            if (q != null)
                rootPath = System.IO.Path.Combine(rootPath, q);

            string fullPath = rootPath;
            long fileLength = (new System.IO.FileInfo(fullPath)).Length;
            var fileStream = new EncryptStream(() => new System.IO.FileStream(fullPath, System.IO.FileMode.Open), this.coreEncryption);

            long contentLength;
            string rangeResult = Request.Query["HTTP_RANGE"].ToString().NullIfEmpty() ?? Request.Headers["Range"].ToString();
            if (string.IsNullOrEmpty(rangeResult))
            {
                contentLength = fileLength;
                Response.Headers.Add("Content-Length", contentLength.ToString());
            }
            else
            {
                long startbyte = long.Parse(Regex.Match(rangeResult, @"\d+").Value, NumberFormatInfo.InvariantInfo);
                long endByte = fileLength - 1;

                fileStream.Position = startbyte;

                Response.StatusCode = 206;
                Response.Headers.Add("Content-Length", (fileLength - startbyte).ToString());
                Response.Headers.Add("Content-Range", string.Format("bytes {0}-{1}/{2}", startbyte, fileLength - 1, fileLength));
            }

            Response.Headers.Add("Accept-Ranges", "bytes");

            //Response.BufferOutput = false;

            return File(fileStream, "application/unknown", fileNameHelper.CreateAlternativeFileName(q) + extension);

            //return File(fileStream, "application/unknown", q);
        }

        [HttpPost]
        public IActionResult GetOriginalFileNames([FromBody]string[] names)
        {
            var dic = new Dictionary<string, string>();

            foreach (var name in names)
            {
                var correctName = name.Replace(extension, "");
                dic[name] = fileNameHelper.GetOriginalFileName(correctName);
            }

            return Json(dic);
        }

        private string GetPathConfig()
        {
            return configuration.GetSection("AllFilesRoot").Value;
        }

        private string GetBackFolderPath(string q)
        {
            string backFolderPath;
            if (string.IsNullOrEmpty(q))
            {
                backFolderPath = null;
            }
            else
            {
                int index = q.LastIndexOf('/');
                if (index >= 0)
                    backFolderPath = q.Substring(0, index);
                else
                    backFolderPath = "";
            }

            return backFolderPath;
        }
    }
}
