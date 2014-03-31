using System;
#if UNITY_4_3
using UnityEngine;
#endif

public class Helper
{
	public static TEnum StringToEnum<TEnum>(string strEnumValue, TEnum defaultValue)
	{
		var str = strEnumValue.Trim();
		if (Enum.IsDefined(typeof(TEnum), str))
			return (TEnum)Enum.Parse(typeof(TEnum), str);
		else
			return defaultValue;
	}
}

public class Logger
{
	public static void Log(string message)
	{
		#if UNITY_4_3
		Debug.Log(message);
		#else
		Console.WriteLine(message);
		#endif
	}

	public static void LogWarning(string message)
	{
		#if UNITY_4_3
		Debug.LogWarning(message);
		#else
		Console.WriteLine("<WARNING> " + message);
		#endif
	}
	public static void LogError(string message)
	{
		#if UNITY_4_3
		Debug.LogError(message);
		#else
		Console.WriteLine("<ERROR> " + message);
		#endif
	}
}