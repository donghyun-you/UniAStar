using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace UniTest 
{
	public class TestMainThreadDispatcher 
		: MonoBehaviour 
	{
		private object locker = new object();
		private Queue<Entry> _actions = new Queue<Entry>();
		private Queue<Entry> _passedEntries = new Queue<Entry>();

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

		public void Run(Action execution,Action<Exception> on_error)
		{
			lock(locker) 
			{
				_actions.Enqueue(new Entry { execution = execution, onError = on_error });
			}
		}

		static TestMainThreadDispatcher s_instance = null;
		public static TestMainThreadDispatcher Instance 
		{
			get 
			{
				if(s_instance != null) 
				{
					return s_instance;
				}
				else 
				{
					var go = new GameObject(typeof(TestMainThreadDispatcher).Name);
					GameObject.DontDestroyOnLoad(go);
					return s_instance = go.AddComponent<TestMainThreadDispatcher>();
				}
			}
		}

		void Update() 
		{
			lock(locker) 
			{
				while(_actions.Count > 0) 
				{
					_passedEntries.Enqueue(_actions.Dequeue());
				}
			}

			if(_passedEntries.Count > 0) 
			{
				using(var entry = _passedEntries.Dequeue()) 
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