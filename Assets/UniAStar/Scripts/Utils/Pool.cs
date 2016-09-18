using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace UniAStar.Utils
{
	public class Pool<T> 
		where T : class
	{
		public Pool() 
		{
			this.ReleasedCount = 0;
		}

		private List<T> _resources = new List<T> ();

		public int ReleasedCount 
		{
			get;
			private set;
		}

		public void Free(T obj) 
		{
			--this.ReleasedCount;
			_resources.Add (obj);
		}

		public T Alloc() 
		{
			++this.ReleasedCount;
			if (_resources.Count == 0) 
			{
				return null;
			}
			else 
			{
				var ret = _resources[_resources.Count-1];
				_resources.RemoveAt(_resources.Count-1);
				return ret;
			}
		}
	}
}