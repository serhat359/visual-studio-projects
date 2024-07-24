using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MVCCore.Models
{
    public class PokemonModel
    {
        public List<Stat> StatList { get; set; }

        [Required]
        public string Query { get; set; }

        public Exception? Error { get; set; }

        public bool ErrorExecutingSql => Error != null;
    }

    public class Stat
    {
        public Int32 gen { get; set; }

        public string id { get; set; }

        public string name { get; set; }

        public string nameStripped
        {
            get
            {
                var i = name.IndexOf('(');
                return i >= 0 ? name[..i] : name;
            }
        }

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
    }
}
