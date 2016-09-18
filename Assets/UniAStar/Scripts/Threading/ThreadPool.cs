using UnityEngine;
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

namespace UniAStar.Threading
{
	public static class ThreadPool
	{
		private static object locker = new object();
		private static Queue<Entry> _actions = new Queue<Entry>();

		public class Entry : IDisposable
		{
			public Action execution;
			public Action<Exception> onError;

			public void Dispose() 
			{
				execution = null;
				onError = null;
			}
		}

		public static void Run(Action execution,Action<Exception> on_error)
		{
			lock(locker) 
			{
				_actions.Enqueue(new Entry { execution = execution, onError = on_error });
			}
		}

		static ThreadPool() 
		{
			Logger.Verbose(locker,"starting thread pool");
			var thread = new Thread(new ThreadStart(main));
			thread.Start();
		}

		private static void main() 
		{
			Queue<Entry> passedEntries = new Queue<Entry>();

			Logger.Verbose(locker,"starting thread pool loop");
			for(;;) 
			{
				lock(locker) 
				{
					while(_actions.Count > 0) 
					{
						passedEntries.Enqueue(_actions.Dequeue());
						UniAStar.Logger.Verbose(locker,"dequeueing action...");
					}
				}

				if(passedEntries.Count > 0) 
				{
					using(var entry = passedEntries.Dequeue()) 
					{
						try 
						{
							entry.execution();
						}
						catch(Exception ex) 
						{
							entry.onError(ex);
						}
					}
				}
			}
		}
	}
}