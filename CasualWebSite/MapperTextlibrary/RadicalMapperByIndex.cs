using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.Data;
using System.Data;

namespace MapperTextlibrary
{
    public class RadicalMapperByIndex : IMapper<Radical>
    {
        public LinkedList<Radical> MapAll(IDataReader reader)
        {
            LinkedList<Radical> exampleList = new LinkedList<Radical>();

            while (reader.Read())
            {
                exampleList.AddLast(Map(reader));
            }

            return exampleList;
        }

        public Radical Map(IDataRecord record)
        {
            Radical example = new Radical();

            example.kanji = (string)record[0]; // kanji
            example.strokes = (UInt32)record[1]; // strokes
            example.radicals = (string)record[2]; // radicals

            return example;
        }
    }
}
