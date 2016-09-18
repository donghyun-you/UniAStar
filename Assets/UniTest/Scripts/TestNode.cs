using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UniTest 
{
	public class TestNode
		: TestElement
	{

		public TestElement[] Children
		{
			get;
			private set;
		}

		public Type NodeType 
		{
			get; 
			private set; 
		}

		public bool IsIgnoreNextOnFailure 
		{
			get;
			private set;
		}

		/// <summary>
		/// Reports from children. dic key=method name, list value = reports various
		/// </summary>
		/// <value>The tested method reports.</value>
		public Dictionary<string,List<TestReport>> TestedMethodReports 
		{
			get;
			private set;
		}

		public void AddReport(string method, TestReport result) 
		{
			List<TestReport> reports;
			if(this.TestedMethodReports.TryGetValue(method,out reports) == false) 
			{
				reports = TestedMethodReports[method] = new List<TestReport>();
			} 

			reports.Add(result);
		}

		private IDisposable _disposable = null;

		protected override void onDisposed ()
		{
			if(_disposable != null) 
			{
				_disposable.Dispose();
			}
		}

		public override string Summarize() 
		{
			StringBuilder builder = new StringBuilder();

			var instanceType = this.Instance == null ? "<unknown type>" : this.Instance.GetType().ToString();
			builder.AppendFormat("{0}: {1} ({2})",this.TestState,instanceType,this.Story);

			return builder.ToString();
		}

		public static class Factory 
		{
			public static TestNode Create(Type node_type, TestNode parent) 
			{
				var testCaseAttributes = node_type.GetCustomAttributes(typeof(TestCaseAttribute),false).Select(entry => entry as TestCaseAttribute);

				if(!testCaseAttributes.Any()) 
				{
					throw new InvalidProgramException("TestCaseAttribute required for "+node_type);	
				}

				var testCaseAttribute = testCaseAttributes.First();

				var node = new TestNode() 
				{
					Parent					= parent,
					NodeType 				= node_type,
					TestState 				= TestResultType.kNotTested,
					Instance 				= Activator.CreateInstance(node_type),
					Attr					= testCaseAttribute,
					Order 					= testCaseAttribute.Order,
					SelfStory				= testCaseAttribute.Summary,
					IsIgnoreNextOnFailure 	= true,
					TestedMethodReports		= new Dictionary<string, List<TestReport>>(),
				};

				if(node.Instance is TestFlow) 
				{
					var testFlow			= node.Instance as TestFlow;

					TestFlow.TestedScenarioEvent onTestSucceed = delegate(TestFlow flow, string flow_method, TestReport report) 
					{
						if(object.ReferenceEquals(flow,node.Instance)) node.AddReport(flow_method,report);
					};

					testFlow.OnTestSucceed  += onTestSucceed;

					node._disposable		= DisposableCreator.Create(()=>{

						testFlow.OnTestSucceed  -= onTestSucceed;
					
					});
				}

				if(parent == null) node.buildHierarchy();

				return node;
			}

			public static TestNode CreateCompositionNode(string self_story) 
			{
				return new TestNode() 
				{
					TestState 				= TestResultType.kNotTested,
					SelfStory 				= self_story,
					IsIgnoreNextOnFailure 	= false,
				};
			}
		}

		private void buildHierarchy() 
		{
			// NOTE(ruel): nestedClasses which is contains TestStoryNodeAttribute
			var nestedClasses 	= NodeType	// Reterieve all nested classes of this class
											.GetNestedTypes()
											// filter all nested classes that contains TestCaseAttribute 
											.Where(type=>type.GetCustomAttributes(typeof(TestCaseAttribute),false).Any())
											// create BddNode for normalize interface
											.Select(type=>Factory.Create(type,this) as TestElement);
											
			var testMethods		= NodeType	// Reterieve all methods of this class
											.GetMethods(BindingFlags.Instance | BindingFlags.Public)
											// filter all methods that contains TestCaseAttribute
											.Where(method=>method.GetCustomAttributes(typeof(TestCaseAttribute),false).Any())
											// create TestElement for create interface to control
											.Select(method=>TestMethod.Factory.Create(this,method.Name) as TestElement);
			
			this.Children 		= (nestedClasses.Concat(testMethods)).OrderBy(element=>element.Order).ToArray();

			TestLogger.Verbose(this,"TestCases: count: "+this.Children.Length+"/ \nList/ \n"+string.Join("\n",this.Children.Select(cases=>"["+cases.Order+"] "+cases.Summarize()).ToArray()));

			if(this.Children.Any()) 
			{
				// NOTE(ruel): build children recursively.
				foreach(var child in this.Children.Where(cases=>cases is TestNode).Select(cases=>cases as TestNode)) 
				{
					child.buildHierarchy();
				}
			}
		}

		public override void Execute(Action<bool> on_determined = null,Action on_complete=null) 
		{
			base.Execute(on_determined,on_complete);
			execute(on_determined,on_complete).Run();
		}

		private IEnumerator execute(Action<bool> on_determined, Action on_complete) 
		{
			on_determined = on_determined ?? delegate(bool is_succeeded) 
			{
				TestLogger.Verbose(this,"succeeded: "+is_succeeded);
			};

			on_complete = on_complete ?? delegate() {};

			foreach(TestElement testCase in Children) 
			{
				TestLogger.Verbose(this,"running "+this.SelfStory+"->"+testCase.SelfStory);

				if(this.TestState == TestResultType.kFailed && this.IsIgnoreNextOnFailure) 
				{
					testCase.MarkAsIgnored();
					TestLogger.Info(this,"<color=gray>[ignored] "+testCase.SelfStory+"</color>");
				} 
				else 
				{
					bool isComplete = false;
					testCase.Execute(result=> 
					{
						if(result) 
						{
							TestLogger.Info(this,"<color=green>\u2714 "+testCase.SelfStory+"</color>");	
						}
						else 
						{
							TestLogger.Info(this,"<color=red>\u2716 "+testCase.SelfStory+"</color>");
						}

						if(result == false) 
						{
							this.TestState = TestResultType.kFailed;
							this.FailedException = testCase.FailedException;
							on_determined(result);
						}
					},()=>
					{
						isComplete = true;
					});

					// NOTE(ruel): wait for testCase runner complete
					while(isComplete == false) 
					{	
						yield return null;
					}
				}
			}

			TestLogger.Verbose(this,"Node("+this.SelfStory+"), complete with : "+this.TestState+"/"+this.SelfStory);

			if(this.TestState == TestResultType.kNotTested) 
			{
				this.TestState = TestResultType.kPassed;
				TestLogger.Info(this,"finishing(pass) "+this.SelfStory);
				on_determined(true);
			}

			on_complete();
		}

		public void Add(TestElement element) 
		{
			if(element != null) 
			{
				if(this.Children == null) 
				{
					this.Children = new TestElement[] { element };
				}
				else 
				{
					this.Children = this.Children.Concat(Enumerable.Repeat(element,1)).ToArray();	
				}
			}
		}

		public void AddRange(IEnumerable<TestElement> elements) 
		{
			if(elements != null && elements.Any()) 
			{
				if(this.Children == null) 
				{
					this.Children = elements.ToArray();
				}
				else 
				{
					this.Children = this.Children.Concat(elements).ToArray();
				}
			}
		}

		public override void Reset ()
		{
			base.Reset ();

			if(this.TestedMethodReports != null) 
			{
				this.TestedMethodReports.Clear();
			}
		}
	}
}