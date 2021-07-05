using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Decryptor
{
    public partial class Form1 : Form
    {
        private string[] selectedFiles;
        private int bufferSizeBytes = 2 * 1024 * 1024; // 2 Megabytes
        private const int threadCount = 4;

        public Form1()
        {
            InitializeComponent();

            folderPathTextBox.Click += FolderPathTextBox_Click;

            new CoreEncryption(System.Configuration.ConfigurationManager.AppSettings["EncryptionKey"]);
        }

        private void FolderPathTextBox_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Custom Files (*.serhatCustom)|*.serhatCustom|All files (*.*)|*.*";

            var result = openFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                this.selectedFiles = openFileDialog.FileNames;

                folderPathTextBox.Text = string.Join(",", this.selectedFiles);

                decryptButton.Enabled = true;
            }
        }

        private void decryptButton_Click(object sender, EventArgs e)
        {
            decryptButton.Enabled = false;

            try
            {
                decryptButton.Enabled = false;

                var newThread = Task.Run(() =>
                {
                    var fileInfos = this.selectedFiles.Select(x => new FileInfo(x)).ToList();

                    var apiBaseLink = System.Configuration.ConfigurationManager.AppSettings["ApiBaseLink"];
                    var apiEndPoint = System.Configuration.ConfigurationManager.AppSettings["ApiEndPoint"];

                    var fullUrl = apiBaseLink + apiEndPoint;

                    var result = PostJson<Dictionary<string, string>>(fullUrl, fileInfos.Select(x => x.Name).ToArray());

                    long totalByteLength = fileInfos.Sum(x => x.Length);

                    var folderPath = Path.Combine(fileInfos[0].DirectoryName, "decrypted_files");

                    Directory.CreateDirectory(folderPath);

                    long totalBytesDone = 0L;
                    foreach (var fileInfo in fileInfos)
                    {
                        var realFileNameFullPath = result[fileInfo.Name];

                        var slashIndex = realFileNameFullPath.LastIndexOf('/');
                        var backSlashIndex = realFileNameFullPath.LastIndexOf('\\');
                        var index = Math.Max(slashIndex, backSlashIndex);

                        var fileName = index >= 0 ? realFileNameFullPath.Substring(index + 1) : realFileNameFullPath;

                        var newPath = Path.Combine(folderPath, fileName);
                        var buffers = Enumerable.Range(0, threadCount).Select(x => new byte[bufferSizeBytes]).ToArray();
                        var tasks = new Task[threadCount];
                        var readCounts = new int[threadCount];

                        using (var destination = File.Create(newPath))
                        using (var source = File.Open(fileInfo.FullName, FileMode.Open, FileAccess.Read))
                        {
                            while (true)
                            {
                                for (int i = 0; i < threadCount; i++)
                                {
                                    var buffer = buffers[i];
                                    var readCount = source.Read(buffer, 0, buffer.Length);

                                    var task = Task.Run(() => CoreEncryption.Instance.DecryptInPlace(buffer));

                                    tasks[i] = task;
                                    readCounts[i] = readCount;
                                }

                                var nestedBreak = false;
                                for (int i = 0; i < threadCount; i++)
                                {
                                    tasks[i].Wait();
                                    destination.Write(buffers[i], 0, readCounts[i]);

                                    // Update UI
                                    totalBytesDone += readCounts[i];
                                    var percentage = (int)(totalBytesDone * 100.0 / totalByteLength);
                                    state.ThreadSafe(x => { x.Text = $"{percentage}% completed"; });

                                    if (readCounts[i] == 0)
                                        nestedBreak = true;
                                }

                                if (nestedBreak)
                                    break;
                            }
                        }
                    }

                    state.ThreadSafe(x => { x.Text = $"100% completed"; });
                    decryptButton.Enabled = true;
                });
            }
            catch (Exception ex)
            {
                decryptButton.Enabled = true;
                throw;
            }
        }

        private T PostJson<T>(string url, object data)
        {
            // These 2 lines are necessary for allowing self signed SSL certificates
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = JsonConvert.SerializeObject(data);

                streamWriter.Write(json);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();

                return JsonConvert.DeserializeObject<T>(result);
            }
        }
    }
}
