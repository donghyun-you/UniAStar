using UnityEngine;
using System;
using System.Diagnostics;
using System.Collections;

namespace UniAStar 
{
	public class Logger
	{
		private static string GetTimeSummary() 
		{
			return DateTime.Now.ToString("hh:mm:ss");
		}

		[Conditional("VERBOSE_LOG")]
		public static void Verbose(object invoker,string text) 
		{
			UnityEngine.Debug.Log("[<color=silver>Verbose</color>/<color=gray>"+invoker.GetType().Name+"</color>/"+GetTimeSummary()+"] "+text);
		}

		public static void Info(object invoker,string text)
		{
			UnityEngine.Debug.Log("[<color=green>Info</color>/<color=gray>"+invoker.GetType().Name+"</color>/"+GetTimeSummary()+"] "+text);
		}

		public static void Exception(object invoker,Exception ex) 
		{
			UnityEngine.Debug.LogException(ex);
		}
	}
}
