using System;

namespace Model.Data
{
    public class Stat : IEquatable<Stat>
    {
        public Int32 gen { get; set; }

        public string id { get; set; }

        public string name { get; set; }

        public Int32 hp { get; set; }

        public Int32 attack { get; set; }

        public Int32 defense { get; set; }

        public Int32 spattack { get; set; }

        public Int32 spdefense { get; set; }

        public Int32 speed { get; set; }

        public Int32 total { get; set; }

        public bool sinnoh { get; set; }

        public bool hoenn { get; set; }

        public string type1 { get; set; }

        public string type2 { get; set; }

        public bool Equals(Stat ex)
        {
            bool result = ex.gen == this.gen
                && ex.id == this.id
                && ex.name == this.name
                && ex.hp == this.hp
                && ex.attack == this.attack
                && ex.defense == this.defense
                && ex.spattack == this.spattack
                && ex.spdefense == this.spdefense
                && ex.speed == this.speed
                && ex.total == this.total
                && ex.sinnoh == this.sinnoh
                && ex.hoenn == this.hoenn
                && ex.type1 == this.type1
                && ex.type2 == this.type2;

            return result;
        }
    }
}
