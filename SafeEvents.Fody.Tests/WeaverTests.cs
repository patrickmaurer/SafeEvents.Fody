﻿using System;
using System.IO;
using System.Reflection;

using Mono.Cecil;

using NUnit.Framework;

namespace SafeEvents.Fody.Tests
{
	[TestFixture]
	public class WeaverTests
	{
		private string _assemblyPath;
		private string _newAssemblyPath;
		private Assembly _assembly;

		[TestFixtureSetUp]
		public void Setup()
		{
			var projectPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\..\AssemblyToProcess\AssemblyToProcess.csproj"));
			_assemblyPath = Path.Combine(Path.GetDirectoryName(projectPath), @"bin\Debug\AssemblyToProcess.dll");
#if (!DEBUG)
			assemblyPath = assemblyPath.Replace("Debug", "Release");
#endif

			_newAssemblyPath = _assemblyPath.Replace(".dll", "2.dll");
			File.Copy(_assemblyPath, _newAssemblyPath, true);

			var moduleDefinition = ModuleDefinition.ReadModule(_newAssemblyPath);
			var weavingTask = new ModuleWeaver
			{
				ModuleDefinition = moduleDefinition
			};

			weavingTask.Execute();
			moduleDefinition.Write(_newAssemblyPath);

			_assembly = Assembly.LoadFile(_newAssemblyPath);
		}

		[Test]
		public void ValidateEventHandlerProducesNoNullReferenceException()
		{
			var type = _assembly.GetType("AssemblyToProcess.SimpleEventHandler");
			var instance = (dynamic)Activator.CreateInstance(type);

			TestDelegate call = () => instance.RaiseMyEvent();

			Assert.That(call, Throws.Nothing);
		}

#if(DEBUG)
		[Test]
		public void PeVerify()
		{
			Verifier.Verify(_assemblyPath, _newAssemblyPath);
		}
#endif
	}
}