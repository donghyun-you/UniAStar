using UnityEngine;
using System.Collections;
using System;

namespace UniTest 
{
	public class DisposableCreator
	{
		public class AnonymousDisposable 
			: Bases.DisposableBase 
		{
			private Action onDisposeReservedAction = null;

			public AnonymousDisposable(Action on_dispose) 
			{
				onDisposeReservedAction = on_dispose ?? delegate() {};
			}

			protected override void onDisposed ()
			{
				if(this.IsDisposed == false) 
				{
					onDisposeReservedAction();	
				}
			}
		}

		public static IDisposable Create(Action on_dispose) 
		{
			return new AnonymousDisposable(on_dispose);
		}
	}
}
