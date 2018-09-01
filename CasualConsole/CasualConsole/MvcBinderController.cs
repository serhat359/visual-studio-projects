using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CasualConsole
{
    public class MvcBinderController
    {

        public static int Number(int num)
        {
            return num;
        }

        public static string ReturnString(string str)
        {
            return str;
        }

        [Get]
        public static string ToString(int num)
        {
            return num.ToString();
        }

        [Post]
        public static int ToString(int num, string t)
        {
            return num;
        }

        public static DateTime LoneFunction()
        {
            return new DateTime(2018, 1, 1);
        }

        public static string GetMethod(GetMethodModel model)
        {
            return model.Number.ToString();
        }
    }

    public class GetMethodModel
    {
        public int Number { get; set; }
        public string Text { get; set; }
    }
}
