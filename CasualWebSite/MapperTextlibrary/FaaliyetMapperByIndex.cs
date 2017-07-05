using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.Data;
using System.Data;

namespace MapperTextlibrary
{
    public class FaaliyetMapperByIndex : IMapper<FaaliyetMSSQL>
    {
        public LinkedList<FaaliyetMSSQL> MapAll(IDataReader reader)
        {
            LinkedList<FaaliyetMSSQL> exampleList = new LinkedList<FaaliyetMSSQL>();

            while (reader.Read())
            {
                exampleList.AddLast(Map(reader));
            }

            return exampleList;
        }

        public FaaliyetMSSQL Map(IDataRecord record)
        {
            FaaliyetMSSQL example = new FaaliyetMSSQL();

            example.FaaliyetID = (decimal)record[0];
            example.AskerID = (decimal)record[1];
            example.BirlikID = (int)record[2];
            example.GelisTarih = (DateTime)record[3];
            example.CikisTarih = record[4] as DateTime?;
            example.BelgeCikisTarih = record[5] as DateTime?;
            example.DurumID = (int)record[6];
            example.Aciklama = (string)record[7];
            example.GeldigiMerkez = (int)record[8];
            example.GidecegiMerkez = (int)record[9];
            example.SorumluTel = (string)record[10];
            example.IsKonvoy = (int)record[11];
            example.KonvoyID = record[12] as int?;

            return example;
        }
    }
}
