using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Model.Data
{
	public class Example : IEquatable<Example>
	{
		public UInt32 id { get; set; }

		public string name { get; set; }

		public string largetext { get; set; }

		public DateTime date { get; set; }

		public double money { get; set; }

		public string code { get; set; }

		public DateTime? insertdate { get; set; }

		public bool? isgood { get; set; }

		public long? somecount { get; set; }

		public bool Equals(Example ex){
			return ex.id == this.id
				&& ex.name == this.name
				&& ex.largetext == this.largetext
				&& ex.date == this.date
				&& ex.money == this.money
				&& ex.code == this.code
				&& ex.insertdate == this.insertdate
				&& ex.isgood == this.isgood
				&& ex.somecount == this.somecount
				;
		}
	}
}
