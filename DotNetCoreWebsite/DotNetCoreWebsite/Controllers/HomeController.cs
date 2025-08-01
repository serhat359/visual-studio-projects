using DotNetCoreWebsite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DotNetCoreWebsite.Controllers
{
    public class HomeController : Controller
    {
        private readonly string extension = ".serhatCustom";

        private IConfiguration configuration;
        private FileNameHelper fileNameHelper;
        private CoreEncryption coreEncryption;
        private IHttpClientFactory httpClientFactory;

        public HomeController(IConfiguration configuration, FileNameHelper fileNameHelper, CoreEncryption coreEncryption, IHttpClientFactory httpClientFactory)
        {
            this.configuration = configuration;
            this.fileNameHelper = fileNameHelper;
            this.coreEncryption = coreEncryption;
            this.httpClientFactory = httpClientFactory;
        }

        public IActionResult Index()
        {
            return View();
        }

        [ActionName("1337")]
        [HttpGet]
        public async Task<IActionResult> Simple1337(string? query)
        {
            string noResultText = "No results were returned. Please refine your search.";

            string? tableData = null;
            if (query != null)
            {
                var link = $"https://1337x.to/search/{Uri.EscapeDataString(query)}/1/";

                var httpClient = httpClientFactory.CreateClient();
                using var response = await httpClient.GetAsync(link);
                string responseContent = await response.Content.ReadAsStringAsync();

                var indexBegin = responseContent.IndexOf("<table class=\"table-list table table-responsive table-striped\">");
                if (indexBegin >= 0)
                {
                    var endPattern = "</table>";
                    var indexEnd = responseContent.IndexOf(endPattern, indexBegin);
                    if (indexEnd < 0)
                        throw new Exception();
                    tableData = responseContent.Substring(indexBegin, length: indexEnd - indexBegin + endPattern.Length);
                    tableData = tableData.Replace("<i class=\"flaticon-message\"></i>", "💬");
                }
                else if (responseContent.Contains(noResultText))
                {
                    tableData = "<p>" + noResultText + "</p>";
                }
                else
                    throw new Exception();
            }

            var model = new Simple1337Model
            {
                Query = query,
                TableData = tableData
            };
            return View(model);
        }

        [ActionName("1337")]
        [HttpPost]
        public async Task<IActionResult> Simple1337(string? customLink, object? _ = null)
        {
            if (string.IsNullOrEmpty(customLink))
                return View(new Simple1337Model { ErrorMessage = "Link is empty" });

            var browserPath = configuration.GetSection("BrowserPath").Value;
            if (string.IsNullOrEmpty(browserPath))
                return View(new Simple1337Model { ErrorMessage = "Browser path not set" });

            await AddLink(browserPath, link: customLink);

            return View(new Simple1337Model{ SuccessMessage = "Success" });
        }

        public async Task<IActionResult> Yts(string query)
        {
            var model = new YtsModel { Query = query };

            var baseUrl = "https://yts.mx/ajax/search?query=";

            if (!string.IsNullOrEmpty(query))
            {
                var link = baseUrl + Uri.EscapeDataString(query);

                var httpClient = httpClientFactory.CreateClient();
                using var response = await httpClient.GetAsync(link);
                if (response.IsSuccessStatusCode)
                {
                    var responseParsed = JsonSerializer.Deserialize<YtsResponseModel>(await response.Content.ReadAsStringAsync());
                    if (responseParsed!.status == "ok")
                    {
                        model.ResponseData = responseParsed.data;
                    }
                }
            }

            return View(model);
        }

        public async Task<IActionResult> YtsDetails(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                var link = "https://yts.mx" + path;

                var httpClient = httpClientFactory.CreateClient();
                using var response = await httpClient.GetAsync(link);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var beginPart = "<div class=\"modal-content\">";
                    var beginIndex = responseContent.IndexOf(beginPart, responseContent.IndexOf("<div class=\"modal-title\"> Select movie quality </div>"));

                    var beginParticle = "<div ";
                    var endPart = "/div>";
                    var endIndex = FindMatchingEnd(responseContent, beginIndex + beginPart.Length, beginParticle, endPart);
                    return Content(responseContent.Substring(beginIndex, length: endIndex - beginIndex + endPart.Length));
                }
            }

            return Content("");
        }

        public async Task<IActionResult?> Torrent(string id1, string id2)
        {
            var path = Request.Path.ToString();
            var link = "https://1337x.to" + path;

            var httpClient = httpClientFactory.CreateClient();
            using var response = await httpClient.GetAsync(link);
            string responseContent = await response.Content.ReadAsStringAsync();

            var ulPart = GetULPart(responseContent);

            var regex = new Regex("href=\"([^\"]+)\"", RegexOptions.Compiled);

            var matches = regex.Matches(ulPart);

            foreach (Match match in matches)
            {
                var torrentLink = match.Groups[1].Value;
                using var torrentResponse = await httpClient.GetAsync(torrentLink);
                var torrentBytes = await torrentResponse.Content.ReadAsByteArrayAsync();
                return File(torrentBytes, "application/x-bittorrent", "torrent.torrent");
            }

            return null;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult AllFiles(string? q = null)
        {
            var now = DateTime.Now;

            var rootPath = GetSafePath(q);

            var rootInfo = new DirectoryInfo(rootPath);
            var rootSystemInfos = rootInfo.GetFileSystemInfos();

            var fileList = rootSystemInfos.Select(info =>
            {
                var attr = info.Attributes;
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    // IsDirectory
                    var dInfo = info;
                    return new FileInfoModel
                    {
                        Name = dInfo.Name,
                        IsFolder = true,
                        FileSize = null,
                        ModifiedDate = dInfo.LastWriteTimeUtc,
                        Age = now - dInfo.LastWriteTimeUtc,
                    };
                }
                else
                {
                    // IsFile
                    var fInfo = (FileInfo)info;
                    return new FileInfoModel
                    {
                        Name = fInfo.Name,
                        IsFolder = false,
                        FileSize = fInfo.Length,
                        ModifiedDate = fInfo.LastWriteTimeUtc,
                        Age = now - fInfo.LastWriteTimeUtc,
                    };
                }
            }).OrderByDescending(x => x.IsFolder).ThenByDescending(x => x.ModifiedDate).ToList();

            return View(new AllFilesModel
            {
                FileList = fileList,
                BackFolderPath = GetBackFolderPath(q),
                CurrentPath = q != null ? q + "/" : ""
            });
        }

        public IActionResult DownloadFile(string q)
        {
            var fullPath = GetSafePath(q);

            var rangeStart = GetByteOffset();

            long fileLength = new FileInfo(fullPath).Length;
            var fileStream = new EncryptStream(System.IO.File.OpenRead(fullPath), this.coreEncryption, rangeStart ?? 0);

            if (rangeStart == null)
            {
                Response.Headers.Add("Content-Length", fileLength.ToString());
            }
            else
            {
                long startbyte = rangeStart.Value;

                fileStream.Position = startbyte;

                Response.StatusCode = 206;
                string contentRange = string.Format("bytes {0}-{1}/{2}", startbyte, fileLength - 1, fileLength);
                Response.Headers.Add("Content-Range", contentRange);
            }

            Response.Headers.Add("Accept-Ranges", "bytes");

            return File(fileStream, "application/unknown", fileNameHelper.CreateAlternativeFileName(q) + extension);
        }

        public IActionResult DownloadMultiFile(string[] q, string path)
        {
            var filePaths = q.Select(x => GetSafePath(SafeCombine(path, x))).ToList();

            var tempLocation = GetTempPathConfig();
            var newGuidFileName = Guid.NewGuid() + ".zip";
            var tempFullPath = SafeCombine(tempLocation, newGuidFileName);

            using (var archiveStream = System.IO.File.OpenWrite(tempFullPath))
            using (var zip = new ZipArchive(archiveStream, ZipArchiveMode.Create))
            {
                foreach (var filePath in filePaths)
                {
                    AddFilesRecursively(zip, "", filePath);
                }
            }

            var fileStream = new EncryptStream(System.IO.File.OpenRead(tempFullPath), this.coreEncryption, 0, onClose: () => System.IO.File.Delete(tempFullPath));

            return File(fileStream, "application/unknown", fileNameHelper.CreateAlternativeFileName(newGuidFileName) + extension);
        }

        public IActionResult GetFolderSize(string? q = null)
        {
            var rootPath = GetSafePath(q);

            var rootSystemInfos = new DirectoryInfo(rootPath).GetDirectories();

            var results = rootSystemInfos.ToDictionary(folder => folder.Name, folder =>
            {
                var fileSize = folder.GetFiles("*", SearchOption.AllDirectories).Sum(x => x.Length);
                var fileSizeString = ((long?)fileSize).ToFileSizeString();
                return new { fileSize, fileSizeString };
            });

            return Json(results);
        }

        [HttpPost]
        public IActionResult GetOriginalFileNames([FromBody] string[] names)
        {
            var dic = new Dictionary<string, string?>();

            foreach (var name in names)
            {
                var correctName = name.Replace(extension, "");
                dic[name] = fileNameHelper.GetOriginalFileName(correctName);
            }

            return Json(dic);
        }

        #region Private Methods
        private string GetPathConfig()
        {
            return configuration.GetSection("AllFilesRoot").Value!;
        }

        private string GetTempPathConfig()
        {
            return SafeCombine(GetPathConfig(), "temp");
        }

        private string GetSafePath(string? q)
        {
            var basePath = GetPathConfig();

            string pathFunc(string? s) => SafeCombine(basePath, s);

            var path = pathFunc(q);
            if (!new DirectoryInfo(path).FullName.Contains(basePath))
            {
                path = pathFunc("");
            }

            return path;
        }

        private static string? GetBackFolderPath(string? q)
        {
            string? backFolderPath;
            if (string.IsNullOrEmpty(q))
            {
                backFolderPath = null;
            }
            else
            {
                int index = q.LastIndexOf('/');
                if (index >= 0)
                    backFolderPath = q[..index];
                else
                    backFolderPath = "";
            }

            return backFolderPath;
        }

        private long? GetByteOffset()
        {
            string rangeResult = Request.Query["HTTP_RANGE"].ToString().NullIfEmpty() ?? Request.Headers["Range"].ToString();

            if (string.IsNullOrEmpty(rangeResult))
                return null;
            else
            {
                var index = rangeResult.IndexOf('-');
                if (index >= 0)
                    rangeResult = rangeResult[..index];

                long startbyte = long.Parse(Regex.Match(rangeResult, @"\d+").Value, NumberFormatInfo.InvariantInfo);

                return startbyte;
            }
        }

        private static string GetShortFileName(string realFileNameFullPath)
        {
            var index = realFileNameFullPath.LastIndexOfAny(new char[] { '/', '\\' });

            var fileName = index >= 0 ? realFileNameFullPath.Substring(index + 1) : realFileNameFullPath;

            return fileName;
        }

        private static string GetULPart(string responseContent)
        {
            var indexBegin = responseContent.IndexOf("<ul class=\"dropdown-menu\" aria-labelledby=\"dropdownMenu1\">");
            if (indexBegin < 0)
                throw new Exception();
            var endPattern = "</ul>";
            var indexEnd = responseContent.IndexOf(endPattern, indexBegin);
            if (indexEnd < 0)
                throw new Exception();
            var data = responseContent.Substring(indexBegin, length: indexEnd - indexBegin + endPattern.Length);
            return data;
        }

        private static void AddFilesRecursively(ZipArchive zip, string folderHeader, string filePath)
        {
            var attr = System.IO.File.GetAttributes(filePath);
            var isfile = !attr.HasFlag(FileAttributes.Directory);

            if (isfile)
            {
                var entryName = folderHeader + GetShortFileName(filePath);
                zip.CreateEntryFromFile(sourceFileName: filePath, entryName: entryName);
            }
            else
            {
                var folderName = GetShortFileName(filePath);
                var filesAndFolders = Directory.EnumerateDirectories(filePath).Concat(Directory.EnumerateFiles(filePath));
                foreach (var subPath in filesAndFolders)
                {
                    AddFilesRecursively(zip, folderHeader + folderName + "/", subPath);
                }
            }
        }

        private static string SafeCombine(string? param1, string? param2)
        {
            if (param1 == null) return param2;
            if (param2 == null) return param1;
            return Path.Combine(param1, param2);
        }

        private static int FindMatchingEnd(string text, int startingPoint, string beginTag, string endTag)
        {
            int SafeIndex(int index) => index < 0 ? text.Length : index;

            int counter = 0;

            while (true)
            {
                int beginFound = SafeIndex(text.IndexOf(beginTag, startingPoint));
                int endFound = SafeIndex(text.IndexOf(endTag, startingPoint));

                if (endFound < beginFound)
                {
                    if (counter == 0)
                        return endFound;
                    else
                        counter--;
                }
                else
                    counter++;

                startingPoint = Math.Min(beginFound, endFound) + beginTag.Length;
            }
        }

        private async Task AddLink(string browserFilePath, string link)
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo(browserFilePath, new string[]{ "-new-tab", link }),
            };
            process.Start();
            await process.WaitForExitAsync();
        }
        #endregion
    }
}
