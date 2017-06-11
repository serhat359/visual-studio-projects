using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.Data;
using System.Data;

namespace MapperTextlibrary
{
    public class StatMapper : IMapper<Stat>
    {
        public LinkedList<Stat> MapAll(IDataReader reader)
        {
            LinkedList<Stat> statList = new LinkedList<Stat>();

            while (reader.Read())
            {
                statList.AddLast(Map(reader));
            }

            return statList;
        }

        public Stat Map(IDataRecord record)
        {
            Stat stat = new Stat();

            stat.gen = (Int32)record["gen"];
            stat.id = (string)record["id"];
            stat.name = (string)record["name"];
            stat.hp = (Int32)record["hp"];
            stat.attack = (Int32)record["attack"];
            stat.defense = (Int32)record["defense"];
            stat.spattack = (Int32)record["spattack"];
            stat.spdefense = (Int32)record["spdefense"];
            stat.speed = (Int32)record["speed"];
            stat.total = (Int32)record["total"];
            stat.hoenn = (bool)record["hoenn"];
            stat.sinnoh = (bool)record["sinnoh"];
            stat.type1 = (string)record["type1"];
            stat.type2 = (string)record["type2"];

            return stat;
        }
    }
}
