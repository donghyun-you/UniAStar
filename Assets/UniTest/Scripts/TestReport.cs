using System;

namespace UniTest 
{
	public class TestReport 
	{
		public TestReportType 	type;
		public string 			message;
		public object[] 		attachments;

		public static class Factory 
		{
			public static TestReport Create(TestReportType type,string message, params object[] attachments) 
			{
				return new TestReport { type = type, message = message, attachments = attachments };
			}
		}
	}
}