using System.Diagnostics;

namespace AutoTf.TabletOS.Models;

public static class CommandExecuter
{
	public static string ExecuteCommand(string command)
	{
		Process process = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				FileName = "/bin/bash",
				Arguments = $"-c \"{command}\"",
				RedirectStandardOutput = true,
				UseShellExecute = false,
				CreateNoWindow = true
			}
		};

		process.Start();
		string result = process.StandardOutput.ReadToEnd();
		process.WaitForExit();

		return result.Trim();
	}

	public static void ExecuteSilent(string command, bool ignoreExceptions)
	{
		try
		{
			Process process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = "/bin/bash",
					Arguments = $"-c \"{command}\"",
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
					CreateNoWindow = true
				}
			};
			process.Start();
			process.WaitForExit();

			string output = process.StandardOutput.ReadToEnd();
			string error = process.StandardError.ReadToEnd();

			if (!string.IsNullOrEmpty(error) && !command.Contains("which"))
			{
				throw new Exception($"Error: {error}");
			}
            
			Statics.Logger.Log(output);
		}
		catch (Exception e)
		{
			Statics.Logger.Log((ignoreExceptions ? "IGNORED:" : "") + e.Message);
			if (!ignoreExceptions)
				throw;
		}
	}
}