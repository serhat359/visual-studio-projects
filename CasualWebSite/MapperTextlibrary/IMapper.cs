using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace MapperTextlibrary
{
	public interface IMapper<T> where T : new()
	{
		LinkedList<T> MapAll(IDataReader reader);
	}
}
