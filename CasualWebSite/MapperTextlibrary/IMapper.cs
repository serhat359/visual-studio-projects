using System.Collections.Generic;
using System.Data;

namespace MapperTextlibrary
{
	public interface IMapper<T> where T : new()
	{
		LinkedList<T> MapAll(IDataReader reader);
	}
}
