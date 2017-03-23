using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace WebModelFactory
{
    public class ModelFactoryBase
    {
        public static string Stringify(object obj)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();

            settings.Formatting = Formatting.Indented;
            settings.Error = HandleError;

            return JsonConvert.SerializeObject(obj, settings);
        }

        public static void HandleError(object sender, ErrorEventArgs args)
        {
            JsonSerializer senderX = (JsonSerializer)sender;

            args.ErrorContext.Handled = true;
        }
    }
}
