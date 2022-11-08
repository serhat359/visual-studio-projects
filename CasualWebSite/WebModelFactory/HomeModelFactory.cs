using System.Collections.Generic;
using Data;
using Model.Data;
using Model.Web;
using Extensions;
using System;

namespace WebModelFactory
{
    public class HomeModelFactory : ModelFactoryBase
    {
        public PokemonModel LoadCasual(PokemonModel request = null)
        {
            PokemonModel model = new PokemonModel();

            string query = "select * from stats " + (request.GetPropertyOrDefault(x => x.Query) ?? "");

            try
            {
                List<Stat> statList = DataFactory.RunSelectQuery<Stat>(query);

                model.StatList = statList;
            }
            catch(Exception)
            {
                model.StatList = new List<Stat>();
                model.ErrorExecutingSql = true;
            }
            
            return model;
        }
    }
}
