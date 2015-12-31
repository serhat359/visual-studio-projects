using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.Data;

namespace ConsoleProgram
{
	public class MapperResult
	{
		public string MapperName { get; set; }

		public long ConsumedTime { get; set; }

		public ICollection<Example> ResultingList { get; set; }
	}
}
