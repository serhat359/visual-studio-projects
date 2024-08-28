using Dapper;
using MVCCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVCCore.Helpers;

public class PokemonService
{
    private readonly DataContext dataContext;

    public PokemonService(DataContext dataContext)
    {
        this.dataContext = dataContext;
    }

    public async Task<PokemonModel> LoadCasual(string queryParam = "")
    {
        var query = "select * from stats " + queryParam;

        try
        {
            using var conn = dataContext.CreateConnection();
            List<Stat> statList = (await conn.QueryAsync<Stat>(new CommandDefinition(commandText: query))).ToList();

            return new PokemonModel
            {
                StatList = statList
            };
        }
        catch (Exception e)
        {
            return new PokemonModel
            {
                Error = e,
            };
        }
    }
}
