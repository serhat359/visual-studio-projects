using System.Collections.Generic;
using Data;
using Model.Data;
using Model.Web;
using Extensions;

namespace WebModelFactory
{
    public class HomeModelFactory : ModelFactoryBase
    {
        public PokemonModel LoadCasual(PokemonModel request = null)
        {
            PokemonModel model = new PokemonModel();

            string query = "select * from stat " + (request.GetPropertyOrDefault(x => x.Query) ?? "");

            try
            {
                List<Stat> statList = DataFactory.RunSelectQuery<Stat>(query);

                model.StatList = statList;
            }
            catch
            {
                model.StatList = new List<Stat>();
                model.ErrorExecutingSql = true;
            }

            return model;
        }
    }
}
