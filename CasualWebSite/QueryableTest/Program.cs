using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QueryableTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            MyDatas datas = new MyDatas();

            IQueryable<Data> filtered = datas.Where(x => x.value != 0);

            filtered = datas.Where(x => true);

            List<Data> dataGot = filtered.ToList();

            foreach (var item in dataGot)
            {
                Console.WriteLine(item);
            }
        }
    }
}
