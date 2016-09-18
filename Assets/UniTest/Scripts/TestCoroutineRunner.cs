using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace UniTest 
{
	public static class TestCoroutineRunner 
	{
		public class CancellationToken 
			: Bases.DisposableBase
		{
			public bool IsCancelled 
			{
				get; 
				private set; 
			}

			protected override void onDisposed ()
			{
				this.IsCancelled = true;
			}
		}

		#region implement editor psuedo coroutine
		#if UNITY_EDITOR

		private static Queue<Action> _editorUpdateQueue = new Queue<Action>();

		public static void OnEditorUpdate() 
		{
			while(_editorUpdateQueue.Count > 0) 
			{
				try 
				{
					var act = _editorUpdateQueue.Dequeue();
					act();
				}
				catch(Exception ex)
				{
					Debug.LogException(ex);
				}
			}
		}

		static void consumeRoutineOnEditor(IEnumerator routine,CancellationToken canceller) 
		{
			if(routine.MoveNext()) 
			{
				var current = routine.Current;
				Action enqueueAction = null;

				if(current == null) 
				{
					enqueueAction = () => consumeRoutineOnEditor(routine,canceller);
				}
				else 
				{
					Type routineType = routine.GetType();

					if(routineType == typeof(WWW)) 
					{
						enqueueAction = () => consumeRoutineOnEditor(unwrapWWW((WWW)current, routine, canceller),canceller);
					}
					else if(routineType == typeof(AsyncOperation)) 
					{
						enqueueAction = () => consumeRoutineOnEditor(unwrapAsyncOperation((AsyncOperation)current, routine, canceller),canceller);
					}
					else if (routineType == typeof(WaitForSeconds))
					{
						enqueueAction = () => consumeRoutineOnEditor(unwrapWaitForSeconds((WaitForSeconds)current, routine, canceller),canceller);
					}
					else 
					{
						enqueueAction = () => consumeRoutineOnEditor(routine,canceller);
					}
				}

				if(canceller.IsCancelled == false) 
				{ 
					_editorUpdateQueue.Enqueue(enqueueAction);
				}
			}
		}

		static IEnumerator unwrapWWW(WWW target,IEnumerator routine,CancellationToken canceller) 
		{
			while(target.isDone == false) 
			{
				yield return null;
				if(canceller.IsCancelled) break;
			}

			if(canceller.IsCancelled == false) 
			{
				consumeRoutineOnEditor(routine,canceller);
			}
		}

		static IEnumerator unwrapAsyncOperation(AsyncOperation target,IEnumerator routine,CancellationToken canceller) 
		{
			while(target.isDone == false) 
			{
				yield return null;
				if(canceller.IsCancelled) break;
			}

			if(canceller.IsCancelled == false) 
			{
				consumeRoutineOnEditor(routine,canceller);
			}
		}

		static IEnumerator unwrapWaitForSeconds(WaitForSeconds target,IEnumerator routine,CancellationToken canceller) 
		{
			var field 		= typeof(WaitForSeconds).GetField("m_Seconds", BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic);
			var duration 	= (float)field.GetValue(target);
			var begin 		= DateTimeOffset.UtcNow;

			while ((DateTimeOffset.UtcNow - begin).TotalSeconds >= duration)
			{
				yield return null;
				if(canceller.IsCancelled) break;
			}

			if(canceller.IsCancelled == false) 
			{
				consumeRoutineOnEditor(routine,canceller);
			}
		}
		#endif
		#endregion

		public static IDisposable Run(this IEnumerator self,Action on_complete=null,Action<Exception> on_error=null) 
		{
			var canceller = new CancellationToken();
			IEnumerator routine = _wrapErrorableEnumerator(self,on_complete,on_error,canceller);

			if(Application.isPlaying) 
			{
				TestMainThreadDispatcher.Instance.StartCoroutine(routine);
			}
			#if UNITY_EDITOR
			else 
			{
				consumeRoutineOnEditor(routine,canceller);
			}
			#endif

			return canceller;
		}

		/// <summary>
		/// wrap an IEnumerator to get error from specific coroutine and reveive the event that end of the coroutine.
		/// </summary>
		/// <returns>The errorable enumerator.</returns>
		/// <param name="self">wrapping IEnumerator(routine of coroutine)</param>
		/// <param name="on_complete">On complete action</param>
		/// <param name="on_error">On error action with thrown exception</param>
		/// <param name="cancellation_token">Cancellation token, if it has disposed. coroutine will stop like as Task</param>
		static IEnumerator _wrapErrorableEnumerator(IEnumerator self,Action on_complete,Action<Exception> on_error,CancellationToken cancellation_token) 
		{
			bool hasNext = false;

			on_complete = on_complete ?? delegate() {};
			on_error = on_error ?? delegate(Exception ex) {};

			do
			{
				try 
				{
					hasNext = self.MoveNext();
				}
				catch(Exception ex) 
				{
					try 
					{
						on_error(ex);
					}
					finally
					{
						var disposable = self as IDisposable;
						if(disposable != null) 
						{
							disposable.Dispose();
						}
					}

					yield break;
				}

				yield return self.Current;

			} while(hasNext && cancellation_token.IsCancelled == false);

			try 
			{
				on_complete();
			}
			finally
			{
				var disposable = self as IDisposable;
				if(disposable != null) 
				{
					disposable.Dispose();
				}
			}

		}
	}
}