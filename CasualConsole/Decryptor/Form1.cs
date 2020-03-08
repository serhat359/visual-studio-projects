using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace Decryptor
{
    public partial class Form1 : Form
    {
        private string[] selectedFiles;
        private int bufferSizeBytes = 8 * 1024 * 1024;

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
            try
            {
                decryptButton.Enabled = false;

                var newThread = MyThread.DoInThread(false, () =>
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
                        var buffer = new byte[bufferSizeBytes];

                        using (var destination = File.Create(newPath))
                        using (var source = File.Open(fileInfo.FullName, FileMode.Open, FileAccess.Read))
                        {
                            while (true)
                            {
                                var readCount = source.Read(buffer, 0, buffer.Length);
                                CoreEncryption.Instance.DecryptInPlace(buffer);
                                destination.Write(buffer, 0, readCount);

                                // Update UI
                                totalBytesDone += readCount;
                                var percentage = (int)(totalBytesDone * 100.0 / totalByteLength);
                                state.ThreadSafe(x => { x.Text = $"{percentage}% completed"; });

                                if (readCount == 0)
                                    break;
                            }
                        }
                    }

                    state.ThreadSafe(x => { x.Text = $"100% completed"; });

                    return 0;
                });
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                decryptButton.Enabled = true;
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
