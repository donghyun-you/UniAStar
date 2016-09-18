using System;

public class ScenarioFailureException 
	: Exception 
{

	public ScenarioFailureException(string message) : base(message) 
	{
		
	}

	public ScenarioFailureException(string message,Exception innerException) : base(message,innerException) 
	{

	}
}
