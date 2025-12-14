using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MVCCore.Models;

public class PokemonModel
{
    public List<Stat>? StatList { get; set; }

    [Required]
    public string Query { get; set; } = "";

    public Exception? Error { get; set; }

    public bool ErrorExecutingSql => Error != null;
}

public class Stat
{
    public int gen { get; set; }

    public string id { get; set; } = "";

    public string name { get; set; } = "";

    public string nameStripped
    {
        get
        {
            var i = name.IndexOf('(');
            return i >= 0 ? name[..i] : name;
        }
    }

    public int hp { get; set; }

    public int attack { get; set; }

    public int defense { get; set; }

    public int spattack { get; set; }

    public int spdefense { get; set; }

    public int speed { get; set; }

    public int total { get; set; }

    public bool sinnoh { get; set; }

    public bool hoenn { get; set; }

    public string type1 { get; set; } = "";

    public string type2 { get; set; } = "";
}
