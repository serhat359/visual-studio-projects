namespace apPatcherApp
{
    using CSEncryptDecrypt;
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    public class crcDupes
    {
        public crcDbType db = new crcDbType();
        private encryptRoutineType dbencryptor = new encryptRoutineType();
        private runFunctionsType run = new runFunctionsType();

        public bool addCRCToDbItem(int i, string thecrc, romTypes type)
        {
            if (this.db.item[i] == null)
            {
                this.db.item[i] = new crcDbItemType();
            }
            if (this.db.item[i].filled >= 50)
            {
                return false;
            }
            for (int j = 0; j < this.db.item[i].filled; j++)
            {
                if (this.db.item[i].possible_crc[j].hash == thecrc)
                {
                    return false;
                }
            }
            if (this.db.item[i].possible_crc[this.db.item[i].filled] == null)
            {
                this.db.item[i].possible_crc[this.db.item[i].filled] = new possibleCrcType();
            }
            this.db.item[i].possible_crc[this.db.item[i].filled].hash = thecrc.ToUpper();
            this.db.item[i].possible_crc[this.db.item[i].filled].type = type;
            crcDbItemType type1 = this.db.item[i];
            type1.filled++;
            return true;
        }

        public int addNewFileCRCToDb(string cleancrc, string newfn, romTypes type, ProgressBar progress, Label status)
        {
            if (!this.db.active)
            {
                return -4;
            }
            if (this.db.filled >= 0x61a8)
            {
                return -2;
            }
            string thecrc = "";
            if (!Program.form.checkCrc32(newfn, progress, status))
            {
                return -5;
            }
            thecrc = Program.form.crchash;
            if ((thecrc == "") || (thecrc.Length != 8))
            {
                return -1;
            }
            int index = 0;
            while (index < this.db.filled)
            {
                if (this.db.item[index].clean_crc == cleancrc)
                {
                    if (!this.addCRCToDbItem(index, thecrc, type))
                    {
                        return -3;
                    }
                    break;
                }
                for (int i = 0; i < this.db.item[index].filled; i++)
                {
                    if (this.db.item[index].possible_crc[i].hash != cleancrc)
                    {
                        continue;
                    }
                    switch (this.db.item[index].possible_crc[i].type)
                    {
                        case romTypes.apPatched:
                            switch (type)
                            {
                                case romTypes.apPatched:
                                    goto Label_0112;

                                case romTypes.trimmed:
                                    goto Label_0117;

                                case romTypes.apAndTrim:
                                    goto Label_011C;
                            }
                            goto Label_0154;

                        case romTypes.trimmed:
                            switch (type)
                            {
                                case romTypes.clean:
                                    goto Label_013D;

                                case romTypes.apPatched:
                                    goto Label_0142;

                                case romTypes.trimmed:
                                    goto Label_0147;

                                case romTypes.apAndTrim:
                                    goto Label_014C;
                            }
                            goto Label_0154;

                        case romTypes.apAndTrim:
                            type = romTypes.apAndTrim;
                            goto Label_0154;

                        default:
                            goto Label_0154;
                    }
                    type = romTypes.apPatched;
                    goto Label_0154;
                Label_0112:
                    type = romTypes.apPatched;
                    goto Label_0154;
                Label_0117:
                    type = romTypes.apAndTrim;
                    goto Label_0154;
                Label_011C:
                    type = romTypes.apAndTrim;
                    goto Label_0154;
                Label_013D:
                    type = romTypes.trimmed;
                    goto Label_0154;
                Label_0142:
                    type = romTypes.apAndTrim;
                    goto Label_0154;
                Label_0147:
                    type = romTypes.trimmed;
                    goto Label_0154;
                Label_014C:
                    type = romTypes.apAndTrim;
                Label_0154:
                    if (!this.addCRCToDbItem(index, thecrc, type))
                    {
                        return -3;
                    }
                    return 1;
                }
                index++;
            }
            if (!this.addCRCToDbItem(index, thecrc, type))
            {
                return -3;
            }
            this.db.item[this.db.filled].clean_crc = cleancrc;
            this.db.filled++;
            return 1;
        }

        public possibleCrcType crcToCleanCrc(string crc)
        {
            possibleCrcType type = new possibleCrcType();
            for (int i = 0; i < this.db.filled; i++)
            {
                if (this.db.item[i].clean_crc.ToUpper() == crc.ToUpper())
                {
                    type.hash = crc.ToUpper();
                    type.type = romTypes.clean;
                    return type;
                }
                for (int j = 0; j < this.db.item[i].filled; j++)
                {
                    if (this.db.item[i].possible_crc[j].hash.ToUpper() == crc.ToUpper())
                    {
                        type.hash = this.db.item[i].clean_crc.ToUpper();
                        type.type = this.db.item[i].possible_crc[j].type;
                        return type;
                    }
                }
            }
            return null;
        }

        public bool loadDb()
        {
            string path = "data/databases/crc_dupes.dscrcdb";
            if (System.IO.File.Exists(path))
            {
                this.db.filled = 0;
                string str2 = "";
                try
                {
                    string sKey = this.run.hexAndMathFunction.convertHexToEncryptionKey("790077003F0028003F0050003F003F00");
                    FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                    using (StreamReader reader = new StreamReader(this.encryptor.createDecryptionReadStream(sKey, fs)))
                    {
                        while ((str2 = reader.ReadLine()) != null)
                        {
                            try
                            {
                                string[] strArray = str2.Split(new char[] { '|' });
                                int num = 0;
                                foreach (string str4 in strArray)
                                {
                                    if ((num == 0) && (str4.Length == 8))
                                    {
                                        if (this.db.item[this.db.filled] == null)
                                        {
                                            this.db.item[this.db.filled] = new crcDbItemType();
                                        }
                                        this.db.item[this.db.filled].clean_crc = str4.ToUpper();
                                        this.db.item[this.db.filled].filled = 0;
                                        this.db.filled++;
                                    }
                                    else
                                    {
                                        if ((num <= 0) || (str4.Length != 10))
                                        {
                                            break;
                                        }
                                        string[] strArray2 = str4.Split(new char[] { ',' });
                                        if (this.db.item[this.db.filled - 1].possible_crc[this.db.item[this.db.filled - 1].filled] == null)
                                        {
                                            this.db.item[this.db.filled - 1].possible_crc[this.db.item[this.db.filled - 1].filled] = new possibleCrcType();
                                        }
                                        this.db.item[this.db.filled - 1].possible_crc[this.db.item[this.db.filled - 1].filled].hash = strArray2[0].ToUpper();
                                        this.db.item[this.db.filled - 1].possible_crc[this.db.item[this.db.filled - 1].filled].type = (romTypes) int.Parse(strArray2[1]);
                                        crcDbItemType type1 = this.db.item[this.db.filled - 1];
                                        type1.filled++;
                                    }
                                    num++;
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
                    MessageBox.Show(exception.Message + "\r\n" + str2 + "\r\nCRC Database is corrupt", "CRC Database Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }
            }
            return true;
        }

        public void writeDb()
        {
            string path = "data/temp/crc_dupes.txt";
            string sOutputFilename = "data/databases/crc_dupes.dscrcdb";
            try
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    string str3 = "";
                    for (int i = 0; i < this.db.filled; i++)
                    {
                        str3 = this.db.item[i].clean_crc + "|";
                        for (int j = 0; j < this.db.item[i].filled; j++)
                        {
                            object obj2 = str3;
                            str3 = string.Concat(new object[] { obj2, this.db.item[i].possible_crc[j].hash, ",", (int) this.db.item[i].possible_crc[j].type, "|" });
                        }
                        writer.WriteLine(str3);
                    }
                    writer.Close();
                    string str4 = this.run.hexAndMathFunction.convertHexToEncryptionKey("790077003F0028003F0050003F003F00");
                    GCHandle gch = GCHandle.Alloc(str4, GCHandleType.Pinned);
                    this.encryptor.EncryptFile(path, sOutputFilename, str4, gch);
                    System.IO.File.Delete(path);
                }
            }
            catch
            {
                MessageBox.Show("Failed to write the crc dupes database");
            }
        }

        public encryptRoutineType encryptor
        {
            get { return this.dbencryptor; }
            set
            {
                this.dbencryptor = value;
            }
        }

        public class crcDbItemType
        {
            public string clean_crc = "";
            public int filled;
            public crcDupes.possibleCrcType[] possible_crc = new crcDupes.possibleCrcType[50];
        }

        public class crcDbType
        {
            public bool active;
            public int filled;
            public crcDupes.crcDbItemType[] item = new crcDupes.crcDbItemType[0x61a8];
        }

        public class possibleCrcType
        {
            public string hash = "";
            public crcDupes.romTypes type;
        }

        public enum romTypes
        {
            clean,
            apPatched,
            trimmed,
            apAndTrim,
            unknown
        }

        public class runFunctionsType
        {
            public hexAndMathFunctions hexAndMathFunction = new hexAndMathFunctions();
        }
    }
}

