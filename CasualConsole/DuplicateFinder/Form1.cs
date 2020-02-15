﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DuplicateFinder
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            var ss = new List<string>();

            var sourcePath = sourcePathTxt.Text;
            var destinationPath = destinationPathTxt.Text;

            var sourcePathThread = MyThread.DoInThread(false, () => GetHashes(sourcePath));
            var destinationPathThread = MyThread.DoInThread(false, () => GetHashes(destinationPath));

            var sourceHashes = sourcePathThread.Await();
            var destinationHashes = destinationPathThread.Await();

            foreach (var key in destinationHashes.Keys)
            {
                if (sourceHashes.TryGetValue(key, out var sourceFoundFiles))
                {
                    var destinationFoundFiles = destinationHashes[key];

                    ss.Add("FoundSource:\n");
                    foreach (var item in sourceFoundFiles)
                        ss.Add(item + "\n");
                    foreach (var item in destinationFoundFiles)
                        ss.Add(item + "\n");

                    ss.Add("\n");

                    foreach (var filePath in destinationFoundFiles)
                    {
                        File.Delete(filePath);
                    }
                }
            }

            foreach (var pair in sourceHashes)
            {
                var list = pair.Value;
                if (list.Count > 1)
                {
                    for (int i = 1; i < list.Count; i++)
                    {
                        File.Delete(list[i]);
                    }
                }
            }

            File.WriteAllLines("output.txt", ss.ToArray());
        }

        private Dictionary<string, List<string>> GetHashes(string path)
        {
            var dic = new Dictionary<string, List<string>>();
            var fileList = DirSearch(path, false);

            dic = fileList.Select(file => new { Path = file, Hash = CalculateHash(file) })
                .GroupBy(x => x.Hash)
                .Select(x => new { Hash = x.Key, Files = x.Select(c => c.Path).ToList() })
                .ToDictionary(x => x.Hash, x => x.Files);

            return dic;
        }

        private string CalculateHash(string file)
        {
            var crc32 = new Crc32();
            var ss = new StringBuilder();

            using (FileStream fs = File.Open(file, FileMode.Open))
            {
                foreach (byte b in crc32.ComputeHash(fs))
                {
                    ss.Append(b.ToString("x2").ToLower());
                }
            }

            return ss.ToString();
        }

        private List<string> DirSearch(string sDir, bool suppressExceptions)
        {
            List<string> files = new List<string>();

            try
            {
                foreach (string f in Directory.GetFiles(sDir))
                {
                    files.Add(f);
                }
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    files.AddRange(DirSearch(d, suppressExceptions));
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (Exception excpt)
            {
                if (!suppressExceptions)
                    MessageBox.Show(excpt.Message);
            }

            return files;
        }
    }
}
