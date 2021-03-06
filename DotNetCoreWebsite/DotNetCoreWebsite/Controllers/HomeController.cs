﻿using DotNetCoreWebsite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
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
            var now = DateTime.Now;

            var rootPath = GetSafePath(q);

            var directoryInfos = Directory.EnumerateDirectories(rootPath).Select(x =>
            {
                var dInfo = new DirectoryInfo(x);
                return new FileInfoModel
                {
                    Name = dInfo.Name,
                    IsFolder = true,
                    FileSize = null,
                    ModifiedDate = dInfo.LastWriteTimeUtc,
                    Age = now - dInfo.LastWriteTimeUtc,
                };
            });

            var fileInfos = Directory.EnumerateFiles(rootPath).Select(x =>
            {
                var fInfo = new FileInfo(x);
                return new FileInfoModel
                {
                    Name = fInfo.Name,
                    IsFolder = false,
                    FileSize = fInfo.Length,
                    ModifiedDate = fInfo.LastWriteTimeUtc,
                    Age = now - fInfo.LastWriteTimeUtc,
                };
            });

            directoryInfos = directoryInfos.OrderByDescending(x => x.ModifiedDate);
            fileInfos = fileInfos.OrderByDescending(x => x.ModifiedDate);

            return View(new AllFilesModel
            {
                FileList = directoryInfos.Concat(fileInfos).ToList(),
                BackFolderPath = GetBackFolderPath(q),
                CurrentPath = q != null ? q + "/" : ""
            });
        }

        public IActionResult DownloadFile(string q)
        {
            var fullPath = GetSafePath(q);

            var rangeStart = GetByteOffset();

            long fileLength = new FileInfo(fullPath).Length;
            var fileStream = new EncryptStream(() => new FileStream(fullPath, FileMode.Open), this.coreEncryption, (rangeStart ?? 0) % 512);

            if (rangeStart == null)
            {
                Response.Headers.Add("Content-Length", fileLength.ToString());
            }
            else
            {
                long startbyte = rangeStart.Value;

                fileStream.Position = startbyte;

                Response.StatusCode = 206;
                Response.Headers.Add("Content-Length", (fileLength - startbyte).ToString());
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

            var fileStream = new EncryptStream(() => new FileStream(tempFullPath, FileMode.Open), this.coreEncryption, 0, () => System.IO.File.Delete(tempFullPath));

            return File(fileStream, "application/unknown", fileNameHelper.CreateAlternativeFileName(newGuidFileName) + extension);
        }

        public void AddFilesRecursively(ZipArchive zip, string folderHeader, string filePath)
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
                var entry = zip.CreateEntry(folderHeader + folderName + "/");
                var filesAndFolders = Directory.EnumerateDirectories(filePath).Concat(Directory.EnumerateFiles(filePath));
                foreach (var subPath in filesAndFolders)
                {
                    AddFilesRecursively(zip, folderHeader + folderName + "/", subPath);
                }
            }
        }

        [HttpPost]
        public IActionResult GetOriginalFileNames([FromBody] string[] names)
        {
            var dic = new Dictionary<string, string>();

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
            return configuration.GetSection("AllFilesRoot").Value;
        }

        private string GetTempPathConfig()
        {
            return SafeCombine(GetPathConfig(), "temp");
        }

        private string GetSafePath(string q)
        {
            var basePath = GetPathConfig();

            string pathFunc(string s) => SafeCombine(basePath, s);

            var path = pathFunc(q);
            if (!new DirectoryInfo(path).FullName.Contains(basePath))
            {
                path = pathFunc("");
            }

            return path;
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

        private long? GetByteOffset()
        {
            string rangeResult = Request.Query["HTTP_RANGE"].ToString().NullIfEmpty() ?? Request.Headers["Range"].ToString();

            if (string.IsNullOrEmpty(rangeResult))
                return null;
            else
            {
                var index = rangeResult.IndexOf('-');
                if (index >= 0)
                    rangeResult = rangeResult.Substring(0, index);

                long startbyte = long.Parse(Regex.Match(rangeResult, @"\d+").Value, NumberFormatInfo.InvariantInfo);

                return startbyte;
            }
        }

        private string GetShortFileName(string realFileNameFullPath)
        {
            var slashIndex = realFileNameFullPath.LastIndexOf('/');
            var backSlashIndex = realFileNameFullPath.LastIndexOf('\\');
            var index = Math.Max(slashIndex, backSlashIndex);

            var fileName = index >= 0 ? realFileNameFullPath.Substring(index + 1) : realFileNameFullPath;

            return fileName;
        }

        private string SafeCombine(string param1, string param2)
        {
            if (param1 == null) return param2;
            if (param2 == null) return param1;
            return Path.Combine(param1, param2);
        }
        #endregion
    }
}
