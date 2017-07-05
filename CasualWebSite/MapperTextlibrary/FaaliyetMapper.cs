using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.Data;
using System.Data;

namespace MapperTextlibrary
{
    public class FaaliyetMapper : IMapper<FaaliyetMSSQL>
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

            example.FaaliyetID = (decimal)record["FaaliyetID"];
            example.AskerID = (decimal)record["AskerID"];
            example.BirlikID = (int)record["BirlikID"];
            example.GelisTarih = (DateTime)record["GelisTarih"];
            example.CikisTarih = record["CikisTarih"] as DateTime?;
            example.BelgeCikisTarih = record["BelgeCikisTarih"] as DateTime?;
            example.DurumID = (int)record["DurumID"];
            example.Aciklama = (string)record["Aciklama"];
            example.GeldigiMerkez = (int)record["GeldigiMerkez"];
            example.GidecegiMerkez = (int)record["GidecegiMerkez"];
            example.SorumluTel = (string)record["SorumluTel"];
            example.IsKonvoy = (int)record["IsKonvoy"];
            example.KonvoyID = record["KonvoyID"] as int?;

            return example;
        }
    }
}
