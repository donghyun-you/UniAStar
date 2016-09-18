using System;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UniTest 
{
	public partial class TestFlow
	{
		public delegate void TestedScenarioEvent(TestFlow flow, string flow_method, TestReport report);
		public event TestedScenarioEvent OnTestSucceed = delegate(TestFlow flow, string flow_method, TestReport report) {};

		private string message = "";

		public TestReportType ReportType 
		{
			get;
			private set;
		}

		public TestFlow Parent 
		{
			get; 
			private set;
		}

		public string ParentMethod 
		{
			get; 
			private set;
		}

		public object Subject
		{
			get; 
			private set;
		}

		private bool _negation = true;

		#region subject

		public TestFlow CommentIf(string message,object subject) 
		{
			return new TestFlow(this,getMethodName(),message,TestReportType.kComment,subject);
		}

		public TestFlow CommentAbout(object subject) 
		{
			return new TestFlow(this,getMethodName(),toStringOrNull(subject),TestReportType.kComment,subject);
		}

		public TestFlow WarnIf(string message,object subject) 
		{
			return new TestFlow(this,getMethodName(),message,TestReportType.kWarning,subject);
		}

		public TestFlow WarnAbout(object subject) 
		{
			return new TestFlow(this,getMethodName(),toStringOrNull(subject),TestReportType.kWarning,subject);
		}

		public TestFlow AssertIf(string message,object subject) 
		{
			return new TestFlow(this,getMethodName(),message,TestReportType.kPass,subject);
		}

		public TestFlow AssertAbout(object subject) 
		{
			return new TestFlow(this,getMethodName(),toStringOrNull(subject),TestReportType.kPass,subject);
		}

		protected TestFlow() 
		{
		}

		protected TestFlow(TestFlow parent,string parent_method,string message,TestReportType report_type,object subject) 
		{
			this.Parent 		= parent;
			this.ParentMethod 	= parent_method;
			this.message 		= message;
			this.ReportType		= report_type;
			this.Subject 		= subject;
		}

		#endregion

		#region conclusion assertion

		public TestFlow TypeOf(Type type,string conclusion=null) 
		{
			message += " type of "+type.Name;

			try 
			{
				if((Subject.GetType() == type) != _negation) 
				{
					throw new ScenarioFailureException(message.Trim());
				} 

				return conclude(conclusion,null);

			} catch(Exception ex) {

				if(this.ReportType == TestReportType.kPass) 
				{
					throw ex;
				}

				return conclude(conclusion,ex);

			}
		}

		public TestFlow TypeOf<T>(string conclusion=null) 
		{
			message += " type of "+typeof(T).Name;

			try 
			{
				if((Subject is T) != _negation) 
				{
					throw new ScenarioFailureException(message.Trim());
				} 

				return conclude(conclusion,null);

			} catch(Exception ex) {

				if(this.ReportType == TestReportType.kPass) 
				{
					throw ex;
				}

				return conclude(conclusion,ex);

			}
		}

		public TestFlow Thrown(string conclusion=null) 
		{
			message += " thrown";

			try 
			{
				if((Subject != null && Subject is Exception) != _negation) 
				{
					if(Subject is Exception) 
					{
						throw new ScenarioFailureException(message.Trim(),Subject as Exception);
					} 
					else 
					{
						throw new ScenarioFailureException(message.Trim());
					}
				}

				return conclude(conclusion,null);

			} catch(Exception ex) {

				if(this.ReportType == TestReportType.kPass) 
				{
					throw ex;
				}

				return conclude(conclusion,ex);

			}
		}

		public TestFlow EqualTo(IComparable target,string conclusion=null) 
		{
			message += " equal to "+toStringOrNull(target);

			try 
			{
				if(Subject is IComparable == false) 
				{
					throw new InvalidOperationException("[EqualTo] must chained with IComparable");	
				}

				if(((Subject as IComparable).CompareTo(target) == 0) != _negation) 
				{
					throw new ScenarioFailureException(message.Trim());
				} 

				return conclude(conclusion,null);

			} catch(Exception ex) {

				if(this.ReportType == TestReportType.kPass) 
				{
					throw ex;
				}

				return conclude(conclusion,ex);

			}
		}

		public TestFlow GreaterThan(IComparable target,string conclusion=null) 
		{
			message += " greater than "+toStringOrNull(target);

			try 
			{
				if(Subject is IComparable == false) 
				{
					throw new InvalidOperationException("[GreaterThan] must chained with IComparable");	
				}

				if(((Subject as IComparable).CompareTo(target) > 0) != _negation) 
				{
					throw new ScenarioFailureException(message.Trim());
				} 

				return conclude(conclusion,null);

			} catch(Exception ex) {

				if(this.ReportType == TestReportType.kPass) 
				{
					throw ex;
				}

				return conclude(conclusion,ex);

			}
		}

		public TestFlow GreaterThanOrEqualTo(IComparable target,string conclusion=null) 
		{
			message += " greater than or equal to "+toStringOrNull(target);

			try 
			{
				if(Subject is IComparable == false) 
				{
					throw new InvalidOperationException("[GreaterThanOrEqualTo] must chained with IComparable");	
				}

				if(((Subject as IComparable).CompareTo(target) >= 0) != _negation) 
				{
					throw new ScenarioFailureException(message.Trim());
				} 

				return conclude(conclusion,null);

			} catch(Exception ex) {

				if(this.ReportType == TestReportType.kPass) 
				{
					throw ex;
				}

				return conclude(conclusion,ex);

			}
		}

		public TestFlow LesserThan(IComparable target,string conclusion=null) 
		{
			message += " lesser than "+toStringOrNull(target);

			try 
			{
				if(Subject is IComparable == false) 
				{
					throw new InvalidOperationException("[LesserThan] must chained with IComparable");	
				}

				if(((Subject as IComparable).CompareTo(target) < 0) != _negation) 
				{
					throw new ScenarioFailureException(message.Trim());
				} 

				return conclude(conclusion,null);

			} catch(Exception ex) {

				if(this.ReportType == TestReportType.kPass) 
				{
					throw ex;
				}

				return conclude(conclusion,ex);

			}
		}

		public TestFlow LesserThanOrEqualTo(IComparable target,string conclusion=null) 
		{
			message += " lesser than or equal to "+toStringOrNull(target);

			try 
			{
				if(Subject is IComparable == false) 
				{
					throw new InvalidOperationException("[LesserThanOrEqualTo] must chained with IComparable");	
				}

				if(((Subject as IComparable).CompareTo(target) <= 0) != _negation) 
				{
					throw new ScenarioFailureException(message.Trim());
				} 

				return conclude(conclusion,null);

			} catch(Exception ex) {

				if(this.ReportType == TestReportType.kPass) 
				{
					throw ex;
				}

				return conclude(conclusion,ex);
			}
		}

		public TestFlow OK(string conclusion=null) 
		{
			message += " OK";

			try 
			{
				if(Subject is bool == false) 
				{
					throw new InvalidOperationException("[OK] must chained with boolean");	
				}

				if((Subject is bool && Convert.ToBoolean(Subject)) != _negation) 
				{
					throw new ScenarioFailureException(message.Trim());
				}

				return conclude(conclusion,null);

			} catch(Exception ex) {

				if(this.ReportType == TestReportType.kPass) 
				{
					throw ex;
				}

				return conclude(conclusion,ex);
			}
		}

		public TestFlow Null(string conclusion=null) 
		{
			message += " null";

			try 
			{
				if((Subject == null) != _negation)
				{
					throw new ScenarioFailureException(message.Trim());
				}

				return conclude(conclusion,null);

			} catch(Exception ex) {

				if(this.ReportType == TestReportType.kPass) 
				{
					throw ex;
				}

				return conclude(conclusion,ex);
			}
		}

		#region conclude with string
		public TestFlow String(string conclusion=null) 
		{
			message += " string";

			try 
			{
				if((Subject is string) != _negation)
				{
					throw new ScenarioFailureException(message.Trim());
				}

				return conclude(conclusion,null);

			} catch(Exception ex) {

				if(this.ReportType == TestReportType.kPass) 
				{
					throw ex;
				}

				return conclude(conclusion,ex);
			}
		}

		public TestFlow StartsWith(string value,string conclusion=null) 
		{
			if(value == null) 
			{
				throw new ArgumentNullException("value");
			}

			message += " starts with "+value;

			try 
			{
				if(Subject == null) 
				{
					throw new ArgumentNullException("Subject");	
				}

				if(Subject is string == false)
				{
					throw new InvalidOperationException("[StartsWith] must chained with string");	
				}

				if((((string)Subject).StartsWith(value)) != _negation)
				{
					throw new ScenarioFailureException(message.Trim());
				}

				return conclude(conclusion,null);

			} catch(Exception ex) {

				if(this.ReportType == TestReportType.kPass) 
				{
					throw ex;
				}

				return conclude(conclusion,ex);
			}
		}

		public TestFlow EndsWith(string value,string conclusion=null) 
		{
			if(value == null) 
			{
				throw new ArgumentNullException("value");
			}

			message += " ends with "+value;

			try 
			{
				if(Subject == null) 
				{
					throw new ArgumentNullException("Subject");	
				}

				if(Subject is string == false)
				{
					throw new InvalidOperationException("[EndsWith] must chained with string");	
				}

				if((((string)Subject).EndsWith(value)) != _negation)
				{
					throw new ScenarioFailureException(message.Trim());
				}

				return conclude(conclusion,null);

			} catch(Exception ex) {

				if(this.ReportType == TestReportType.kPass) 
				{
					throw ex;
				}
				else 
				{
					return conclude(conclusion,ex);
				}
			}
		}

		public TestFlow MatchesWith(string pattern,string conclusion=null) 
		{
			if(pattern == null) 
			{
				throw new ArgumentNullException("pattern");
			}

			message += " matches with "+pattern;

			try 
			{
				if(Subject == null) 
				{
					throw new ArgumentNullException("Subject");	
				}

				if(Subject is string == false)
				{
					throw new InvalidOperationException("[MatchesWith] must chained with string");	
				}

				if((Regex.IsMatch(((string)Subject),pattern)) != _negation)
				{
					throw new ScenarioFailureException(message.Trim());
				}

				return conclude(conclusion,null);

			} catch(Exception ex) {

				if(this.ReportType == TestReportType.kPass) 
				{
					throw ex;
				}
				else 
				{
					return conclude(conclusion,ex);
				}
			}
		}

		#endregion

		public TestFlow Numeric(string conclusion=null) 
		{
			message += " numeric";
			try 
			{
				if((Subject is int || Subject is uint || Subject is long || Subject is ulong || Subject is float || Subject is double || Subject is decimal || Subject is short || Subject is ushort) != _negation)
				{
					throw new ScenarioFailureException(message.Trim());
				}

				return conclude(conclusion,null);

			} catch(Exception ex) {

				if(this.ReportType == TestReportType.kPass) 
				{
					throw ex;
				}

				return conclude(conclusion,ex);
			}
		}

		public TestFlow ValueType(string conclusion=null) 
		{
			message += " value-type";
			try 
			{
				if((Subject is ValueType) != _negation)
				{
					throw new ScenarioFailureException(message.Trim());
				}

				return conclude(conclusion,null);

			} catch(Exception ex) {

				if(this.ReportType == TestReportType.kPass) 
				{
					throw ex;
				}

				return conclude(conclusion,ex);
			}
		}

		public TestFlow Contains(object target,string conclusion=null) 
		{
			message += " contains "+ toStringOrNull(target);

			try 
			{
				if(Subject is IList == false) 
				{
					throw new InvalidOperationException("[Contains] must chained with IList");	
				}

				if(((Subject as IList).Contains(target)) != _negation)
				{
					throw new ScenarioFailureException(message.Trim());
				}

				return conclude(conclusion,null);

			} catch(Exception ex) {

				if(this.ReportType == TestReportType.kPass) 
				{
					throw ex;
				}
				else 
				{
					return conclude(conclusion,ex);
				}
			}
		}

		public TestFlow True(string conclusion=null) 
		{
			message += " true";
				
			try 
			{	
				if(Subject is bool == false) 
				{
					throw new InvalidOperationException("[True] must chained with bool");	
				}

				if((Convert.ToBoolean(Subject) == true) != _negation)
				{
					throw new ScenarioFailureException(message.Trim());
				}

				return conclude(conclusion,null);

			} catch(Exception ex) {

				if(this.ReportType == TestReportType.kPass) 
				{
					throw ex;
				}
				else 
				{
					return conclude(conclusion,ex);
				}
			}
		}

		public TestFlow False(string conclusion=null) 
		{
			message += " false";

			try 
			{
				if(Subject is bool == false) 
				{
					throw new InvalidOperationException("[False] must chained with bool");	
				}

				if((Subject is bool && Convert.ToBoolean(Subject) == false) != _negation)
				{
					throw new ScenarioFailureException(message.Trim());
				}

				return conclude(conclusion,null);

			} catch(Exception ex) {

				if(this.ReportType == TestReportType.kPass) 
				{
					throw ex;
				}
				else 
				{
					return conclude(conclusion,ex);
				}
			}
		}

		private TestFlow conclude(string add_message,Exception ex)
		{
			var reportResult = TestReport.Factory.Create(this.ReportType,(this.message+" "+(add_message ?? "")).Trim(),ex);

			OnTestSucceed.Invoke(this,null,reportResult);

			if(Parent != null) 
			{
				Parent.OnTestSucceed(this.Parent,this.ParentMethod,reportResult);
			}

			// NOTE(ruel): unset after report. consider concluded. and continued something.
			this.message = "...";

			return this;
		}

		#endregion

		#region negation chaining

		public TestFlow Not		{ get { this.message += " not"; this._negation = !this._negation; return this; } }

		#endregion

		#region chaining

		public TestFlow Be 		{ get { this.message += " be"; 		return this; } }
		public TestFlow An 		{ get { this.message += " an"; 		return this; } }
		public TestFlow Of 		{ get { this.message += " of"; 		return this; } }
		public TestFlow A 		{ get { this.message += " a"; 		return this; } }
		public TestFlow And 	{ get { this.message += " and"; 	return this; } }
		public TestFlow Have 	{ get { this.message += " have"; 	return this; } }
		public TestFlow Has 	{ get { this.message += " has"; 	return this; } }
		public TestFlow With 	{ get { this.message += " with"; 	return this; } }
		public TestFlow Is 		{ get { this.message += " is"; 		return this; } }
		public TestFlow Which 	{ get { this.message += " which"; 	return this; } }
		public TestFlow The 	{ get { this.message += " the"; 	return this; } }
		public TestFlow It 		{ get { this.message += " it"; 		return this; } }
		public TestFlow Should	{ get { this.message += " should";	return this; } }
		public TestFlow Must	{ get { this.message += " must";	return this; } }

		#endregion

		#region assert

		public void Assert(Exception ex) 
		{
			if(ex != null) 
			{
				throw ex;
			}
		}

		public void Assert(bool condition) 
		{
			if(condition == false) 
			{
				throw new Exception(getAssertedFilename());
			}
			else 
			{
				OnTestSucceed.Invoke(this,getMethodName(),TestReport.Factory.Create(TestReportType.kPass,message));
			}
		}

		public void Assert(string message,bool condition) 
		{
			if(message == null) 
			{
				message = getAssertedFilename();	
			}

			if(condition == false) 
			{
				throw new Exception(message);
			} 
			else 
			{
				OnTestSucceed.Invoke(this,getMethodName(),TestReport.Factory.Create(TestReportType.kPass,message));
			}
		}

		#endregion

		#region utils

		private string getAssertedFilename() 
		{
			return (new StackTrace(2)).GetFrames()[0].GetFileName();
		}

		private string getMethodName() 
		{
			StackTrace calledInfo = new StackTrace(2);
			string methodName = calledInfo.GetFrame(0).GetMethod().Name;

			// NOTE(ruel): estimate this coroutine.
			if(methodName == "MoveNext") 
			{
				string reflectedTypeName = calledInfo.GetFrame(0).GetMethod().ReflectedType.Name;
				methodName = reflectedTypeName.Substring(reflectedTypeName.IndexOf('<') + 1, reflectedTypeName.IndexOf('>') - 1);
			}

			return methodName;
		}

		private string toStringOrNull(object target) 
		{
			if(target is string) 
			{
				return string.Format("\"{0}\"",target);
			}
			else 
			{
				return target == null ? "(null)" : target.ToString();
			}
		}
		#endregion
	}
}