using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Model.Data;

namespace Model.Web
{
    public class PokemonModel
    {
        public List<Stat> StatList { get; set; }

        [Required]
        public string Query { get; set; }

        public bool ErrorExecutingSql { get; set; }
    }
}
