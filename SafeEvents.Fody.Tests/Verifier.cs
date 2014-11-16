using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

using NUnit.Framework;

namespace SafeEvents.Fody.Tests
{
	public static class Verifier
	{
		private static readonly string ExePath;

		static Verifier()
		{
			ExePath = Environment.ExpandEnvironmentVariables(@"%ProgramFiles(x86)%\Microsoft SDKs\Windows\v7.0A\Bin\NETFX 4.0 Tools\PEVerify.exe");

			if (!File.Exists(ExePath))
			{
				ExePath = Environment.ExpandEnvironmentVariables(@"%ProgramFiles(x86)%\Microsoft SDKs\Windows\v8.0A\Bin\NETFX 4.0 Tools\PEVerify.exe");
			}

			if (!File.Exists(ExePath))
			{
				ExePath = null;
			}
		}

		public static void Verify(string beforeAssemblyPath, string afterAssemblyPath)
		{
			var before = Validate(beforeAssemblyPath);
			var after = Validate(afterAssemblyPath);
			var message = string.Format("Failed processing {0}\r\n{1}", Path.GetFileName(afterAssemblyPath), after);
			Assert.AreEqual(TrimLineNumbers(before), TrimLineNumbers(after), message);
		}

		private static string Validate(string assemblyPath2)
		{
			if (string.IsNullOrEmpty(ExePath)) { Assert.Inconclusive("PEVerify.exe not found"); }

			var process = Process.Start(new ProcessStartInfo(ExePath, '"' + assemblyPath2 + '"')
			{
				RedirectStandardOutput = true,
				UseShellExecute = false,
				CreateNoWindow = true
			});

			process.WaitForExit(10000);
			return process.StandardOutput.ReadToEnd().Trim().Replace(assemblyPath2, string.Empty);
		}

		private static string TrimLineNumbers(string foo)
		{
			return Regex.Replace(foo, @"0x.*]", "");
		}
	}
}