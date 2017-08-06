using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CasualConsole
{
    public class HeroesV
    {

        private static void HeroesFixMap()
        {
            XmlDocument document = new XmlDocument();

            //string filename = @"C:\Program Files\Heroes of Might and Magic V - Collectors Edition\HMM5\bina2\Maps\Generated Multiplayer Map 2017_2\Maps\RMG\CCA473B4-E1AA-438A-87EB-A0658C231A32\map.xdb";

            string filename = @"C:\Program Files\Heroes of Might and Magic V - Collectors Edition\HMM5\bina2\Maps\map.xdb";

            document.Load(filename);

            FixArtifacts(document); // seems allright!

            FixNumberCounts(document); // seems allright!

            RandomizeTreasureChest(document); // seems allright!

            FixMonsters(document); // seems allright!

            DeleteAdditionalStacks(document); // seems allright!

            document.Save(filename);
        }

        private static void DeleteAdditionalStacks(XmlDocument document)
        {
            var tags = Extensions.AsEnumerable<XmlNode>(document.GetElementsByTagName("AdditionalStacks"));

            List<XmlNode> removeList = new List<XmlNode>();

            foreach (var node in tags)
            {
                var child = node.ChildNodes[0];

                if (child != null && child.Name == "Item")
                {
                    removeList.Add(node);
                }
            }

            foreach (var node in removeList)
            {
                node.RemoveAll();
            }
        }

        private static void GenerateArtifactMapping()
        {
            string[] allFiles = Directory.GetFiles(@"C:\Program Files\Heroes of Might and Magic V - Collectors Edition\HMM5\bina2\Editor\IconCache\AdvMapObjectLink\MapObjects\_(AdvMapObjectLink)\Artifacts", "*", SearchOption.AllDirectories);

            List<Pair<string, string>> mappings = new List<Pair<string, string>>();

            allFiles.Each(filename =>
            {
                string alltext = File.ReadAllText(filename);

                string[] split = alltext.Split('\u0003');

                string trimmed = split.Last().Split('\u0005').First();

                string furtherTrimmed = trimmed.Split('\u0004').First().Substring(1);

                string onlyFileName = Path.GetFileName(filename);

                mappings.Add(new Pair<string, string>(onlyFileName, furtherTrimmed));
            });

            List<string[]> newList = mappings.Select(x => new string[] { x.value1, x.value2 }).ToList();

            File.WriteAllText(@"C:\Users\Xhertas\Desktop\artifect.json", JsonConvert.SerializeObject(newList));
        }

        private static void FixNumberCounts(XmlDocument document)
        {
            var monsters = document.GetElementsByTagName("AdvMapMonster");

            foreach (var monster in monsters.AsEnumerable<XmlNode>())
            {
                var custom = monster.ChildNodes.AsEnumerable<XmlNode>().First(x => x.Name == "Custom");

                if (custom.FirstChild.Value == "true")
                {

                    var amount = monster.ChildNodes.AsEnumerable<XmlNode>().First(x => x.Name == "Amount");

                    custom.FirstChild.Value = "false";

                    amount.FirstChild.Value = "0";
                }
            }
        }

        private static void FixMonsters(XmlDocument document)
        {
            var monsterTiers = JsonConvert.DeserializeObject<string[][]>(File.ReadAllText(@"C:\Users\Xhertas\Desktop\tier_list.txt"))
                .Select((x, i) => new { index = i + 1, obj = x.Select(y => y.ToLower()) });

            var monsters = document.GetElementsByTagName("AdvMapMonster");

            foreach (var monster in monsters.AsEnumerable<XmlNode>())
            {
                var shared = monster.ChildNodes.AsEnumerable<XmlNode>().First(x => x.Name == "Shared");

                var href = shared.Attributes["href"].Value;

                if (href.Contains("Random"))
                    continue;

                var dotSplit = href.Split('.');

                var monsterNameTemp = dotSplit[0];

                var monsterName = monsterNameTemp.Split('/').Last();

                monsterName = monsterName.Replace('_', ' ').ToLower();

                int monsterTier = monsterTiers.First(x => x.obj.Any(y => y == monsterName)).index;

                string edited = "/MapObjects/Random/Random-Monster-L" + monsterTier;

                dotSplit[0] = edited;

                string joined = string.Join(".", dotSplit);

                shared.Attributes["href"].Value = joined;
            }
        }

        private static void RandomizeTreasureChest(XmlDocument document)
        {
            var items = document.GetElementsByTagName("AdvMapTreasure");

            foreach (var item in items.AsEnumerable<XmlNode>())
            {
                var shared = item.ChildNodes.AsEnumerable<XmlNode>().First(x => x.Name == "Shared");

                var href = shared.Attributes["href"].Value;

                if (href.Contains("MapObjects/Chest"))
                {
                    var isCustom = item.ChildNodes.AsEnumerable<XmlNode>().First(x => x.Name == "IsCustom");

                    isCustom.FirstChild.Value = "false";
                }
            }
        }

        private static void FixArtifacts(XmlDocument document)
        {
            var artifactMapping = JsonConvert.DeserializeObject<List<string[]>>(File.ReadAllText(@"C:\Users\Xhertas\Desktop\artifect.json"));

            var artifactInfos = JsonConvert.DeserializeObject<List<string[]>>(File.ReadAllText(@"C:\Users\Xhertas\Desktop\artifact info.json"));

            var artifacts = document.GetElementsByTagName("AdvMapArtifact");

            foreach (var artifact in artifacts.AsEnumerable<XmlNode>())
            {
                var shared = artifact.ChildNodes.AsEnumerable<XmlNode>().First(x => x.Name == "Shared");

                var href = shared.Attributes["href"].Value;

                if (href.Contains("Random"))
                    continue;

                if (href.Contains("Graal"))
                    continue;

                var dotSplit = href.Split('.');

                var artifactNameTemp = dotSplit[0];

                var artifactID = artifactNameTemp.Split('/').Last();

                artifactID = artifactID.ToLower();

                string[] match = artifactMapping.First(x => x[0].ToLower() == artifactID);

                string actualName = match[1];

                if (!actualName.ToLower().Contains("tear of asha"))
                {
                    string[] artifactMatch = artifactInfos.First(x => x[0] == actualName);

                    string artifactValueType = artifactMatch[2];

                    string edited = "/MapObjects/Random/Random-" + artifactValueType;

                    dotSplit[0] = edited;

                    string joined = string.Join(".", dotSplit);

                    shared.Attributes["href"].Value = joined;
                }
            }
        }

    }
}
