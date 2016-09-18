using System;

namespace UniTest.Bases
{
	public abstract class DisposableBase : IDisposable
	{
		public bool IsDisposed { get; private set; }
		
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		
		protected void Dispose(bool disposing)
		{
			if(this.IsDisposed == false) 
			{
				this.IsDisposed = true;
				this.onDisposed();
			}
		}

		protected abstract void onDisposed();
	}
}