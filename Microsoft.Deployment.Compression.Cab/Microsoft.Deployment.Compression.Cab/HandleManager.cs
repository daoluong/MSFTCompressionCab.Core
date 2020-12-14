using System.Collections.Generic;

namespace Microsoft.Deployment.Compression.Cab
{
	internal sealed class HandleManager<T> where T : class
	{
		private List<T> handles;

		public T this[int handle]
		{
			get
			{
				if (handle > 0 && handle <= handles.Count)
				{
					return handles[checked(handle - 1)];
				}
				return null;
			}
		}

		public HandleManager()
		{
			handles = new List<T>();
		}

		public int AllocHandle(T obj)
		{
			handles.Add(obj);
			return handles.Count;
		}

		public void FreeHandle(int handle)
		{
			if (handle > 0 && handle <= handles.Count)
			{
				handles[checked(handle - 1)] = null;
			}
		}
	}
}
