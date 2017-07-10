using CasualConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using TorrentSeedLeechCounter;

namespace TorrentSeedLeechCorrecter
{
    class Program
    {
        static void Main(string[] args)
        {
            //SetGeneralCorrectedValues();

            //SetMistakenPlaces();

            Console.WriteLine("Okay");
            Console.ReadKey();
        }

        private static void SetMistakenPlaces()
        {
            {
                string query = @"select *
from TorrentPeerState
where ID between 17 and 48";

                List<TorrentPeerState> torrentStateList = RunQuery<TorrentPeerState>(query);

                var first = torrentStateList[0];
                var last = torrentStateList[torrentStateList.Count - 1];

                foreach (var item in torrentStateList)
                {
                    int id = item.ID;
                    int correctedSeed = (last.Seed - first.Seed) * (id - first.ID) / (last.ID - first.ID) + first.Seed;

                    item.SeedCorrected = correctedSeed;

                    Database.UpdateCorrected(item);
                }
            }

            {
                string query = @"select *
from TorrentPeerState
where ID between 442 and 475";

                List<TorrentPeerState> torrentStateList = RunQuery<TorrentPeerState>(query);

                var first = torrentStateList[0];
                var last = torrentStateList[torrentStateList.Count - 1];

                foreach (var item in torrentStateList)
                {
                    int id = item.ID;
                    int correctedLeech = (last.Leech - first.Leech) * (id - first.ID) / (last.ID - first.ID) + first.Leech;

                    item.LeechCorrected = correctedLeech;

                    Database.UpdateCorrected(item);
                }
            }
        }

        private static void SetGeneralCorrectedValues()
        {
            string query = @"select ID,Datetime,Seed,Leech
from TorrentPeerState
where torrenthash = '9ef5b515bdbaf20ce5da68da2c5fee0b213a57d4'
order by datetime";

            List<TorrentPeerState> torrentStateList = RunQuery<TorrentPeerState>(query);

            // 18 + 15k;

            var divided = DivideListToBlocks(torrentStateList).ToList();

            divided.Each((chunk, chunkIndex) =>
            {
                bool hasNext = chunkIndex + 1 < divided.Count;

                if (hasNext)
                {
                    List<TorrentPeerState> nextChunk = divided[chunkIndex + 1];

                    {
                        var chunkHavingMinSeed = chunk.Aggregate((e1, e2) => e1.Seed < e2.Seed ? e1 : e2);
                        var nextChunkHavingMinSeed = nextChunk.Aggregate((e1, e2) => e1.Seed < e2.Seed ? e1 : e2);

                        Action<TorrentPeerState> action = item =>
                        {
                            int id = item.ID;
                            int correctedSeed = (nextChunkHavingMinSeed.Seed - chunkHavingMinSeed.Seed) * (id - chunkHavingMinSeed.ID) / (nextChunkHavingMinSeed.ID - chunkHavingMinSeed.ID) + chunkHavingMinSeed.Seed;

                            item.SeedCorrected = correctedSeed;

                            Database.UpdateCorrected(item);
                        };

                        foreach (var item in chunk.SkipWhile(x => x.ID < chunkHavingMinSeed.ID))
                        {
                            action(item);
                        }

                        foreach (var item in chunk.TakeWhile(x => x.ID < nextChunkHavingMinSeed.ID))
                        {
                            action(item);
                        }
                    }

                    {
                        var chunkHavingMinLeech = chunk.Aggregate((e1, e2) => e1.Leech < e2.Leech ? e1 : e2);
                        var nextChunkHavingMinLeech = nextChunk.Aggregate((e1, e2) => e1.Leech < e2.Leech ? e1 : e2);

                        Action<TorrentPeerState> action = item =>
                        {
                            int id = item.ID;
                            int correctedLeech = (nextChunkHavingMinLeech.Leech - chunkHavingMinLeech.Leech) * (id - chunkHavingMinLeech.ID) / (nextChunkHavingMinLeech.ID - chunkHavingMinLeech.ID) + chunkHavingMinLeech.Leech;

                            item.LeechCorrected = correctedLeech;

                            Database.UpdateCorrected(item);
                        };

                        foreach (var item in chunk.SkipWhile(x => x.ID < chunkHavingMinLeech.ID))
                        {
                            action(item);
                        }

                        foreach (var item in chunk.TakeWhile(x => x.ID < nextChunkHavingMinLeech.ID))
                        {
                            action(item);
                        }
                    }
                }
            });
        }

        private static IEnumerable<List<TorrentPeerState>> DivideListToBlocks(List<TorrentPeerState> torrentStateList)
        {
            var list = torrentStateList.Skip(18);

            const int chunkSize = 16;

            // 18 + 15k;
            for (int k = 0; ; k += chunkSize)
            {
                var newList = list.Skip(k).Take(chunkSize);

                if (newList.Any())
                    yield return newList.ToList();
                else
                    break;
            }
        }

        private static List<T> RunQuery<T>(string query) where T : new()
        {
            return TorrentSeedLeechCounter.Database.RunSelectQuery<T>(query);
        }
    }
}
