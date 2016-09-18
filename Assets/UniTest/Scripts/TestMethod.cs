using System;
using System.Reflection;
using System.Collections;
using System.Linq;

namespace UniTest 
{
	public class TestMethod 
		: TestElement
	{
		public MethodInfo Invoker 
		{
			get;
			private set;
		}

		protected override void onDisposed ()
		{
			// nothing to dispose
		}

		public override string Summarize() 
		{
			return "["+this.Instance.GetType().Name+"::"+this.Invoker.Name+"] "+this.Story+" ("+TestState+")";
		}

		public static class Factory 
		{
			public static TestMethod Create(TestNode parent,string method_name) 
			{
				if(parent == null) 
				{
					throw new ArgumentNullException("parent");
				}

				var instance = parent.Instance;

				if(instance == null) 
				{
					throw new ArgumentNullException("parent.Instance");
				}

				var node_type 		= instance.GetType();
				var method_names 	= node_type.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(method=>method.Name == method_name);
				var invoker 		= method_names.First();

				var testCaseAttributes = invoker.GetCustomAttributes(typeof(TestCaseAttribute),false).Select(entry => entry as TestCaseAttribute);

				if(!testCaseAttributes.Any()) 
				{
					throw new InvalidProgramException("TestCaseAttribute required for "+node_type);	
				}

				var testCaseAttribute = testCaseAttributes.First();

				if(method_names.Any()) 
				{
					return new TestMethod() 
					{
						Parent		= parent,
						Invoker 	= invoker,
						Instance 	= instance,
						Attr		= testCaseAttribute,
						Order 		= testCaseAttribute.Order,
						SelfStory	= testCaseAttribute.Summary,
						Name		= method_name,
					};
				}
				else 
				{
					return null;
				}

			}
		}

		public override void Execute(Action<bool> on_determined = null,Action on_complete=null) 
		{
			base.Execute(on_determined,on_complete);

			// NOTE(ruel): if it was coroutine.
			if(Invoker.ReturnType == typeof(IEnumerator)) 
			{
				IEnumerator enumerator = (IEnumerator)Invoker.Invoke(this.Instance,null);

				enumerator.Run(()=>
				{
					this.TestState			= TestResultType.kPassed;
					on_determined(true);
					on_complete();
				},
				ex=>
				{
					this.TestState 			= TestResultType.kFailed;
					this.FailedException 	= ex;
					on_determined(false);
					on_complete();	
				});
			}
			else
			{
				try 
				{
					Invoker.Invoke(this.Instance,null);
					this.TestState = TestResultType.kPassed;
					on_determined(true);
					on_complete();
				}
				catch(Exception ex) 
				{
					this.TestState = TestResultType.kFailed;
					this.FailedException = ex;
					on_determined(false);
					on_complete();
				} 
			}
		}
	}	
}