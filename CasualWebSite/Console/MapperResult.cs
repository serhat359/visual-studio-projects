using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.Data;

namespace ConsoleProgram
{
	public class MapperResult<T>
	{
		public string MapperName { get; set; }

		public long ConsumedTime { get; set; }

		public ICollection<T> ResultingList { get; set; }
	}
}
