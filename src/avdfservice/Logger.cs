using System;
using System.IO;
using System.Configuration;

namespace ddlhelper
{
	public class Logger
	{
		string fileName;
		private StreamWriter _fileWriter;

		public Logger ()
		{
			fileName = ConfigurationManager.AppSettings ["logPath"];
		}

		public void OpenLogFile()
		{
			_fileWriter = new StreamWriter (fileName, true);
		}

		public void CloseLogFile()
		{
			_fileWriter.Close ();
		}

		static void ParamsReplace (ref string message, object[] args)
		{
			// Replace any Date tags with UTC time
			while (message.Contains(@"{Date}")) 
			{
				if (message.Contains (@"{Date}"))
					message.Replace (@"{Date}", "[" + DateTime.UtcNow.ToString () + "]");
			}

			// Replace the args into message
			for (int i = 0; i < args.Length; i++) {
				string s = args [i].ToString ();
				string strReplace = @"{" + i.ToString () + @"}";
				int tagIndex = message.IndexOf (strReplace);
				message = message.Substring (0, tagIndex) + s + message.Substring (tagIndex + 3, message.Length - (tagIndex + 3));
				//message.Replace (strReplace, s);
			}
		}

		private void WriteToLog(string msgType, string message, params object[]args)
		{
			ParamsReplace (ref message, args);

			message = msgType + message;

			// Write to log file
			OpenLogFile ();
			_fileWriter.WriteLine (message);
			CloseLogFile ();
		}

		public void Information (string message, params object[] args)
		{
			WriteToLog ("[INFORMATION]", message, args);
		}

		public void Error (string message, params object[] args)
		{
			WriteToLog ("[   ERROR   ]", message, args);
		}

		public void Warning (string message, params object[] args)
		{
			WriteToLog ("[  WARNING  ]", message, args);
		}
	}
}

