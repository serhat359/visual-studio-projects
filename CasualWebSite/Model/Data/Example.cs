using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.Types;

namespace Model.Data
{
	public class Example
	{
		public UInt32 id { get; set; }

		public string name { get; set; }

		public string largetext { get; set; }

		public DateTime date { get; set; }

		public double money { get; set; }

		public string code { get; set; }

		public DateTime insertdate { get; set; }

		public bool isgood { get; set; }

		public long? somecount { get; set; }
	}
}
