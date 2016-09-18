using UnityEngine;
using System;
using System.Collections;
using System.Linq;

namespace UniTest 
{
	public abstract class TestElement
		: Bases.DisposableBase
	{
		public TestNode Parent
		{
			get;
			protected set;
		}

		public int Order 
		{
			get; 
			protected set; 
		}

		public TestResultType TestState 
		{
			get;
			protected set;
		}

		public Exception FailedException 
		{
			get;
			protected set;
		}

		public object Instance 
		{
			get;
			protected set; 
		}

		public string SelfStory 
		{
			get;
			protected set;
		}

		public string Name 
		{
			get;
			protected set;
		}

		public virtual string Story 
		{
			get 
			{
				if(this.Parent == null) 
				{
					return this.SelfStory;
				}
				else 
				{
					return (this.Parent.Story ?? "")+" "+(this.SelfStory ?? "");
				}
			}
		}

		public string InstanceID 
		{
			get;
			private set;
		}

		public TestCaseAttribute Attr 
		{
			get; 
			protected set;
		}

		public static int s_instanceIdIncrement = 0;

		public TestElement() 
		{
			InstanceID = "test_element_"+(s_instanceIdIncrement++);
		}

		public virtual void Execute(Action<bool> onFinished,Action on_complete) 
		{
			TestLogger.Verbose(this,"executing: "+this.Summarize());

			if(this.Attr is TestSceneStoryAttribute) 
			{
				var attr = this.Attr as TestSceneStoryAttribute;
				TestLogger.Verbose(this,"scene story found. loading scene: "+(attr.SceneAt ?? ""));

				if(string.IsNullOrEmpty(attr.SceneAt) == false) 
				{
					UnityEngine.SceneManagement.SceneManager.LoadScene(attr.SceneAt);
				}
			}
		}
		public abstract string Summarize();

		public void MarkAsIgnored() 
		{
			this.TestState = TestResultType.kIgnored;
		}

		public virtual void Reset() 
		{
			this.TestState = TestResultType.kNotTested;
		}
	}
}