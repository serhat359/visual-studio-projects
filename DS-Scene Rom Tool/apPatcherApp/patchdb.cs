namespace apPatcherApp
{
    using CSEncryptDecrypt;
    using System;
    using System.Collections;
    using System.IO;
    using System.Windows.Forms;

    public class patchdb
    {
        public int db_filled;
        private encryptRoutineType dbencryptor = new encryptRoutineType();
        public patchdbClass patch_db = new patchdbClass();
        private runFunctionsType run = new runFunctionsType();
        private bool shownCorruptDb;

        public void addPatchToDb(StreamReader sr, string name, string hash, string creator)
        {
            if (this.db_filled >= 0x1388)
            {
                MessageBox.Show("Fatal Error! Database is too large for the allocated memory!");
            }
            patchdb_PatchClass class2 = this.readPatchFromStream(sr, name, hash, creator);
            if (class2 != null)
            {
                this.patch_db.patch[this.db_filled] = class2;
                this.db_filled++;
            }
            else if (!this.shownCorruptDb)
            {
                MessageBox.Show("The database appears to need updating\r\nPlease update from the database menu\r\n\r\nInvalid format for one of the integers at row " + this.db_filled, "Corrupt Database", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                this.shownCorruptDb = true;
            }
        }

        public patchdb_PatchClass findPatchInDb(string hash)
        {
            for (int i = 0; i < this.db_filled; i++)
            {
                if (hash.ToUpper() == this.patch_db.patch[i].hash.ToUpper())
                {
                    return this.patch_db.patch[i];
                }
            }
            return null;
        }

        private string formatPatchCreator(string creator)
        {
            string str = creator;
            if (str == "")
            {
                str = "anonymous";
            }
            if (str.Substring(0, 1) == " ")
            {
                str = str.Substring(1, str.Length - 1);
            }
            return str;
        }

        public bool loadDatabase()
        {
            this.db_filled = 0;
            try
            {
                string sKey = this.run.hexAndMathFunction.convertHexToEncryptionKey("790077003F0028003F0050003F003F00");
                FileStream fs = new FileStream("data/databases/offsets.dsapdb", FileMode.Open, FileAccess.Read);
                using (StreamReader reader = new StreamReader(this.encryptor.createDecryptionReadStream(sKey, fs)))
                {
                    string str2;
                    while ((str2 = reader.ReadLine()) != null)
                    {
                        try
                        {
                            string[] strArray = str2.Split(new char[] { '[' });
                            string name = strArray[0];
                            strArray = strArray[1].Split(new char[] { ']' });
                            if (strArray[0].Length == 8)
                            {
                                this.addPatchToDb(reader, name, strArray[0], strArray[1]);
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
                    this.sortPatchDbByName();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message + "\r\n\r\nPlease run a database update from the menu", "AP Patch Database Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
            return true;
        }

        private patchLineInfo patchStringToPatch(string line)
        {
            patchLineInfo info = new patchLineInfo();
            try
            {
                info.orig_str = line;
                info.offset = line.Substring(0, 8);
                string[] strArray = line.Substring(8).Replace(" ", "").Replace(":", "").Replace("\r", "").Replace("\n", "").Split(new char[] { '→' });
                info.find = strArray[0];
                info.replace = strArray[1];
            }
            catch (Exception exception)
            {
                MessageBox.Show("'" + line + "' " + exception.Message);
            }
            return info;
        }

        public patchdb_PatchClass readPatchFromStream(StreamReader sr, string name, string hash, string creator)
        {
            patchdb_PatchClass newPatch = new patchdb_PatchClass();
            try
            {
                string str;
                newPatch.name = name;
                newPatch.creator = this.formatPatchCreator(creator);
                newPatch.hash = hash;
                newPatch.patchlines = 0;
                while ((str = sr.ReadLine()) != null)
                {
                    if (str.Length <= 8)
                    {
                        break;
                    }
                    newPatch.patchline[newPatch.patchlines] = this.patchStringToPatch(str);
                    newPatch.patchlines++;
                }
                newPatch = this.sortPatchByOffset(newPatch);
            }
            catch
            {
                return null;
            }
            return newPatch;
        }

        public patchdb_PatchClass sortPatchByOffset(patchdb_PatchClass newPatch)
        {
            ArrayList list = new ArrayList();
            int index = 0;
            for (index = 0; index < newPatch.patchlines; index++)
            {
                if ((newPatch.patchline[index] != null) && (newPatch.patchline[index].offset != ""))
                {
                    list.Add(newPatch.patchline[index].offset);
                }
            }
            list.Sort();
            patchdb_PatchClass class2 = new patchdb_PatchClass {
                creator = newPatch.creator,
                hash = newPatch.hash,
                name = newPatch.name,
                patchlines = newPatch.patchlines,
                patchline = new patchLineInfo[100]
            };
            index = 0;
            bool flag = false;
            foreach (object obj2 in list)
            {
                if (obj2 == null)
                {
                    continue;
                }
                flag = false;
                for (int i = 0; i < 100; i++)
                {
                    try
                    {
                        if ((newPatch.patchline[i] != null) && (newPatch.patchline[i].offset == obj2.ToString()))
                        {
                            class2.patchline[index] = new patchLineInfo();
                            class2.patchline[index] = newPatch.patchline[i];
                            flag = true;
                            index++;
                            break;
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Trying to sort something that is incorrect");
                    }
                }
                if (!flag)
                {
                    MessageBox.Show("Failed sorting the patch");
                }
            }
            return class2;
        }

        public patchdbClass sortPatchDbByName()
        {
            ArrayList list = new ArrayList();
            int index = 0;
            index = 0;
            while (index < this.db_filled)
            {
                if ((this.patch_db.patch[index] != null) && (this.patch_db.patch[index].name != ""))
                {
                    list.Add(this.patch_db.patch[index].name);
                }
                index++;
            }
            list.Sort();
            bool flag = false;
            patchdbClass class2 = new patchdbClass();
            foreach (object obj2 in list)
            {
                if (obj2 == null)
                {
                    continue;
                }
                flag = false;
                for (int i = 0; i < this.db_filled; i++)
                {
                    try
                    {
                        if ((this.patch_db.patch[i] != null) && (this.patch_db.patch[i].name == obj2.ToString()))
                        {
                            class2.patch[index] = this.patch_db.patch[i];
                            flag = true;
                            index++;
                            break;
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Trying to sort something in the db that is incorrect");
                    }
                }
                if (!flag)
                {
                    MessageBox.Show("Failed sorting the patch");
                }
            }
            return class2;
        }

        public encryptRoutineType encryptor
        {
            get { return this.dbencryptor; }
            set
            {
                this.dbencryptor = value;
            }
        }

        public class patchdb_PatchClass
        {
            public string creator;
            public string hash;
            public string name;
            public patchdb.patchLineInfo[] patchline = new patchdb.patchLineInfo[100];
            public int patchlines;
        }

        public class patchdbClass
        {
            public patchdb.patchdb_PatchClass[] patch = new patchdb.patchdb_PatchClass[0x1388];
        }

        public class patchLineInfo
        {
            public string find;
            public string offset;
            public string orig_str;
            public string replace;
        }

        public class runFunctionsType
        {
            public hexAndMathFunctions hexAndMathFunction = new hexAndMathFunctions();
        }
    }
}

