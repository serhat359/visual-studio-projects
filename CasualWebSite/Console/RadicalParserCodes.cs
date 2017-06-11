using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.Data;
using MyConsole;
using Data;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace MyConsole
{
    class RadicalParserCodes
    {
        public static void Test()
        {
            string radicalquery = "select * from radical order by strokes";

            List<Radical> radicalList = DataFactory.RunSelectQuery<Radical>(radicalquery);

            List<RadicalJS> radicalsForJs = new List<RadicalJS>();

            foreach (var radical in radicalList)
            {
                RadicalJS r = new RadicalJS
                {
                    Kanji = radical.kanji,
                    Strokes = (int)radical.strokes,
                    Radicals = radical.radicals.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => Int32.Parse(x)).ToList()
                };
                radicalsForJs.Add(r);
            }

            var jsonStr = JsonConvert.SerializeObject(radicalsForJs);
        }


        private static void InsertRadicals()
        {
            string tempquery = "select * from radical_temp";

            List<RadicalTemp> radicalTempList = DataFactory.RunSelectQuery<RadicalTemp>(tempquery);

            foreach (var radicalTemp in radicalTempList)
            {
                foreach (var part in DivideParts(radicalTemp.json))
                {
                    System.Console.WriteLine(radicalTemp.id + "-" + part);

                    uint strokeCount = Convert.ToUInt32(Regex.Replace(part, "[^0-9]", ""));

                    var kanjis = part.Where(x => !IsDigit(x));

                    foreach (char kanji in kanjis)
                    {
                        string radicalquery = string.Format("select * from radical where kanji='{0}'", kanji);
                        List<Radical> radicals = DataFactory.RunSelectQuery<Radical>(radicalquery);

                        if (radicals.Any())
                        {
                            Radical radicalEx = radicals[0];
                            radicalEx.radicals += string.Format("{0},", radicalTemp.id);
                            string updateQuery = UpdateForRadical(radicalEx);
                            new Data.DBConnection(Data.DBConnection.Mode.Write).RunQueryUpdateOneRow(updateQuery);
                        }
                        else
                        {
                            Radical radicalNew = new Radical
                            {
                                kanji = kanji.ToString(),
                                strokes = strokeCount,
                                radicals = string.Format(",{0},", radicalTemp.id)
                            };

                            string insertQuery = InsertForRadical(radicalNew);
                            new Data.DBConnection(Data.DBConnection.Mode.Write).RunQueryUpdateOneRow(insertQuery);
                        }
                    }
                }
            }
        }

        private static string UpdateForRadical(Radical radicalEx)
        {
            string baseQuery = "update radical set radicals='{0}' where kanji='{1}'";
            return string.Format(baseQuery, radicalEx.radicals, radicalEx.kanji);
        }

        public static string InsertForRadical(Radical radicalNew)
        {
            string baseQuery = "INSERT INTO radical (kanji, strokes, radicals) VALUES ('{0}', {1}, '{2}')";
            return string.Format(baseQuery, radicalNew.kanji, radicalNew.strokes, radicalNew.radicals);
        }

        public static IEnumerable<string> DivideParts(string json)
        {
            RadicalJsonResult obj = JsonConvert.DeserializeObject<RadicalJsonResult>(json);

            string wholeStr = obj.results;

            int lastInt = 0;
            for (int i = 1; i < wholeStr.Length; i++)
            {
                if (IsDigit(wholeStr[i]) && !IsDigit(wholeStr[i - 1]))
                {
                    yield return wholeStr.Substring(lastInt, i - lastInt);
                    lastInt = i;
                }
            }

            yield return wholeStr.Substring(lastInt, wholeStr.Length - lastInt);
        }

        private static bool IsDigit(char x)
        {
            return x >= '0' && x <= '9';
        }

    }
}
