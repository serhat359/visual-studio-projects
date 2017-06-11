using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.Data;
using System.Data;

namespace MapperTextlibrary
{
    public class StatMapperByIndex : IMapper<Stat>
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

            stat.gen = (Int32)record[0]; // gen
            stat.id = (string)record[1]; // id
            stat.name = (string)record[2]; // name
            stat.hp = (Int32)record[3]; // hp
            stat.attack = (Int32)record[4]; // attack
            stat.defense = (Int32)record[5]; // defense
            stat.spattack = (Int32)record[6]; // spattack
            stat.spdefense = (Int32)record[7]; // spdefense
            stat.speed = (Int32)record[8]; // speed
            stat.total = (Int32)record[9]; // total
            stat.sinnoh = (bool)record[10]; // sinnoh
            stat.hoenn = (bool)record[11]; // hoenn
            stat.type1 = (string)record[12]; // type1
            stat.type2 = (string)record[13]; // type2

            return stat;
        }
    }
}
