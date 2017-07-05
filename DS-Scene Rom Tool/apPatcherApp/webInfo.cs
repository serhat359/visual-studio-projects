namespace apPatcherApp
{
    using CSEncryptDecrypt;
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    public class webInfo
    {
        private encryptRoutineType dbencryptor = new encryptRoutineType();
        public webInfoClass info = new webInfoClass();
        private runFunctionsType run = new runFunctionsType();

        public bool downloadRomInfo(string hash, ProgressBar progress, Label status)
        {
            bool flag;
            try
            {
                if (System.IO.File.Exists("data/web/info/" + hash + "_info.dsapdb"))
                {
                    System.IO.File.Delete("data/web/info/" + hash + "_info.dsapdb");
                }
                string url = "http://www.ds-scene.net/romtool.php?version=2&hash=" + hash;
                if (Program.form.downloadFile(url, "data/temp/", "Contacting DS-Scene.net", hash + "_info.raw", progress, status) && System.IO.File.Exists("data/temp/" + hash + "_info.raw"))
                {
                    string str2 = this.run.hexAndMathFunction.convertHexToEncryptionKey("790077003F0028003F0050003F003F00");
                    GCHandle gch = GCHandle.Alloc(str2, GCHandleType.Pinned);
                    this.encryptor.EncryptFile("data/temp/" + hash + "_info.raw", "data/web/info/" + hash + "_info.dsapdb", str2, gch);
                    System.IO.File.Delete("data/temp/" + hash + "_info.raw");
                    webInfoClass class2 = this.parseWebInfo(hash);
                    if (class2 != null)
                    {
                        if (class2.item[0].key == "error:bad hash")
                        {
                            MessageBox.Show("Bad Hash Detected! " + hash);
                            return false;
                        }
                        if (class2.item[0].key == "error:hash not found")
                        {
                            return false;
                        }
                        foreach (webInfoItemClass class3 in class2.item)
                        {
                            if (class3 != null)
                            {
                                if ((class3.key == "boxart") && (class3.val != ""))
                                {
                                    if (System.IO.File.Exists("data/web/images/" + hash + ".jpg"))
                                    {
                                        System.IO.File.Delete("data/web/images/" + hash + ".jpg");
                                    }
                                    Program.form.downloadFile(class3.val, "data/web/images/", "Downloading Boxart", hash + ".jpg", progress, status);
                                }
                                if ((class3.key == "icon") && (class3.val != ""))
                                {
                                    if (System.IO.File.Exists("data/web/images/" + hash + ".png"))
                                    {
                                        System.IO.File.Delete("data/web/images/" + hash + ".png");
                                    }
                                    Program.form.downloadFile(class3.val, "data/web/images/", "Downloading Icon", hash + ".png", progress, status);
                                }
                                if ((class3.key == "nfolink") && (class3.val != ""))
                                {
                                    if (System.IO.File.Exists("data/web/nfo/" + hash + ".nfo"))
                                    {
                                        System.IO.File.Delete("data/web/nfo/" + hash + ".nfo");
                                    }
                                    Program.form.downloadFile(class3.val, "data/web/nfo/", "Downloading NFO", hash + ".nfo", progress, status);
                                }
                                if (((class3.key == "romrgn") && (class3.val != "")) && !System.IO.File.Exists("data/web/images/flag_" + class3.val + ".gif"))
                                {
                                    Program.form.downloadFile("http://www.ds-scene.net/data/images/icons/flags/" + class3.val + ".gif", "data/web/images/", "Download Flag", "flag_" + class3.val + ".gif", progress, status);
                                }
                            }
                        }
                        goto Label_037C;
                    }
                    System.IO.File.Delete("data/web/info/" + hash + "_info.dsapdb");
                }
                return false;
            Label_037C:
                flag = true;
            }
            catch (Exception exception)
            {
                MessageBox.Show("An error occurred while trying to contact ds-scene.net\n\n" + exception.Message, "DS-Scene.net error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                flag = false;
            }
            return flag;
        }

        public webInfoClass parseWebInfo(string hash)
        {
            this.info = new webInfoClass();
            if (System.IO.File.Exists("data/web/info/" + hash + "_info.dsapdb"))
            {
                this.info.crcLoaded = hash;
                string str = "";
                FileStream fs = null;
                try
                {
                    string sKey = this.run.hexAndMathFunction.convertHexToEncryptionKey("790077003F0028003F0050003F003F00");
                    fs = new FileStream("data/web/info/" + hash + "_info.dsapdb", FileMode.Open, FileAccess.Read);
                    using (StreamReader reader = new StreamReader(this.encryptor.createDecryptionReadStream(sKey, fs)))
                    {
                        int index = 0;
                        while ((str = reader.ReadLine()) != null)
                        {
                            try
                            {
                                if (index < 20)
                                {
                                    string[] strArray = str.Split(new char[] { '>' });
                                    if (this.info.item[index] == null)
                                    {
                                        this.info.item[index] = new webInfoItemClass();
                                    }
                                    this.info.item[index].key = strArray[0].Replace("=", "");
                                    this.info.item[index].val = strArray[1];
                                    index++;
                                }
                                continue;
                            }
                            catch
                            {
                                continue;
                            }
                        }
                        reader.Close();
                        fs.Close();
                    }
                }
                catch (Exception exception)
                {
                    this.info.crcLoaded = hash;
                    if (fs != null)
                    {
                        fs.Close();
                    }
                Label_0153:
                    try
                    {
                        System.IO.File.Delete("data/web/info/" + hash + "_info.dsapdb");
                    }
                    catch
                    {
                        goto Label_0153;
                    }
                    if (Program.form.organiseForm.checkBoxDownload.Checked && Program.form.organiseForm.cancelBatchBtn.Visible)
                    {
                        MessageBox.Show("re-download because in batch and corrupt file");
                        this.downloadRomInfo(hash, Program.form.organiseForm.progressBarStage, Program.form.organiseForm.stageProgressGrpLabel);
                    }
                    else if (Program.form.options.getValue("auto_info_dl") == "1")
                    {
                        MessageBox.Show("re-download because auto download and corrupt file");
                        this.downloadRomInfo(hash, Program.form.toolStripProgressBar, Program.form.toolStripStatusLabel);
                    }
                    else
                    {
                        MessageBox.Show(exception.Message + "\n\nPlease re-open the rom and click refresh on the DS-Scene tab\n\nInvalid DS-Scene Web Data", "DS-Scene File Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                    return null;
                }
            }
            return this.info;
        }

        public string replaceIllegalFilenameCharacters(string fn)
        {
            fn = Program.form.run.hexAndMathFunction.string_replace(":", " -", fn);
            fn = Program.form.run.hexAndMathFunction.string_replace(";", " -", fn);
            fn = Program.form.run.hexAndMathFunction.string_replace(",", "", fn);
            fn = Program.form.run.hexAndMathFunction.string_replace("?", "", fn);
            fn = Program.form.run.hexAndMathFunction.string_replace("[", "(", fn);
            fn = Program.form.run.hexAndMathFunction.string_replace("]", ")", fn);
            fn = Program.form.run.hexAndMathFunction.string_replace("<", "(", fn);
            fn = Program.form.run.hexAndMathFunction.string_replace(">", ")", fn);
            fn = Program.form.run.hexAndMathFunction.string_replace("*", "", fn);
            fn = Program.form.run.hexAndMathFunction.string_replace("|", "", fn);
            fn = Program.form.run.hexAndMathFunction.string_replace("\"", "'", fn);
            fn = Program.form.run.hexAndMathFunction.string_replace(@"\", "_", fn);
            fn = Program.form.run.hexAndMathFunction.string_replace("/", "_", fn);
            fn = Program.form.run.hexAndMathFunction.string_replace("+", " ", fn);
            return fn;
        }

        public bool validateWebInfo(string hash)
        {
            crcDupes.possibleCrcType type = new crcDupes.possibleCrcType();
            type = Program.form.crcDb.crcToCleanCrc(hash.ToUpper());
            if ((type == null) || (type.hash == ""))
            {
                type = new crcDupes.possibleCrcType {
                    hash = hash.ToUpper()
                };
                Program.form.romHeader.romHeader.cleanCrc.type = crcDupes.romTypes.unknown;
            }
            webInfoClass class2 = Program.form.web.parseWebInfo(type.hash);
            if ((class2 == null) || (class2.item[0] == null))
            {
                return false;
            }
            foreach (webInfoItemClass class3 in class2.item)
            {
                DateTime utcNow;
                DateTime time2;
                if (class3 == null)
                {
                    continue;
                }
                switch (class3.key)
                {
                    case "error:hash not found":
                        System.IO.File.Delete("data/web/info/" + Program.form.romHeader.romHeader.cleanCrc.hash + "_info.dsapdb");
                        return false;

                    case "error:bad hash":
                        System.IO.File.Delete("data/web/info/" + Program.form.romHeader.romHeader.cleanCrc.hash + "_info.dsapdb");
                        return false;

                    case "romnum":
                    case "romnam":
                    case "romgrp":
                    case "romsav":
                    case "romzip":
                    case "romdir":
                    case "id":
                    case "wifi":
                    case "boxart":
                    case "dscompat":
                    case "newsdate":
                    case "romrgn":
                    case "nfolink":
                    case "icon":
                    case "n3dsopt":
                    case "romsize":
                    {
                        continue;
                    }
                    case "date":
                    {
                        utcNow = DateTime.UtcNow;
                        int year = int.Parse(class3.val.Substring(0, 4));
                        int month = int.Parse(class3.val.Substring(4, 2));
                        int day = int.Parse(class3.val.Substring(6, 2));
                        int hour = int.Parse(class3.val.Substring(8, 2));
                        int minute = int.Parse(class3.val.Substring(10, 2));
                        int second = int.Parse(class3.val.Substring(12, 2));
                        time2 = new DateTime(year, month, day, hour, minute, second);
                        long num7 = long.Parse(utcNow.Year.ToString("D2") + utcNow.Month.ToString("D2") + utcNow.Day.ToString("D2") + utcNow.Hour.ToString("D2") + utcNow.Minute.ToString("D2") + utcNow.Second.ToString("D2"));
                        long num8 = long.Parse(time2.Year.ToString("D2") + time2.Month.ToString("D2") + time2.Day.ToString("D2") + time2.Hour.ToString("D2") + time2.Minute.ToString("D2") + time2.Second.ToString("D2"));
                        if (num7 <= num8)
                        {
                            break;
                        }
                        if (utcNow.Subtract(time2).TotalMinutes <= 10080.0)
                        {
                            continue;
                        }
                        return false;
                    }
                    default:
                        MessageBox.Show("Invalid web info: " + class3.key);
                        return false;
                }
                if (time2.Subtract(utcNow).TotalMinutes > 0.0)
                {
                    return true;
                }
            }
            return true;
        }

        public encryptRoutineType encryptor
        {
            get { return this.dbencryptor; }
            set
            {
                this.dbencryptor = value;
            }
        }

        public class runFunctionsType
        {
            public hexAndMathFunctions hexAndMathFunction = new hexAndMathFunctions();
        }

        public class webInfoClass
        {
            public string crcLoaded = "";
            public webInfo.webInfoItemClass[] item = new webInfo.webInfoItemClass[20];
        }

        public class webInfoItemClass
        {
            public string key = "";
            public string val = "";
        }
    }
}

