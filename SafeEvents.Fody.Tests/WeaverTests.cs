﻿using System;
using System.Diagnostics;
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
			_assemblyPath = Path.Combine(Path.GetDirectoryName(projectPath), @"bin\Release\AssemblyToProcess.dll");
			SetDebugPath();

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

		[Conditional("DEBUG")]
		private void SetDebugPath()
		{
			_assemblyPath = _assemblyPath.Replace("Release", "Debug");
		}

		[Test]
		public void ValidateEventHandlerProducesNoNullReferenceException()
		{
			var type = _assembly.GetType("AssemblyToProcess.SimpleEventHandler");
			var instance = (dynamic)Activator.CreateInstance(type);

			AssertRaiseEventThrowsNothing(instance);
		}

		[Test]
		public void ValidateEventHandlerOfTProducesNoNullReferenceException()
		{
			var type = _assembly.GetType("AssemblyToProcess.GenericEventHandler");
			var instance = (dynamic)Activator.CreateInstance(type);

			AssertRaiseEventThrowsNothing(instance);
		}

		[Test]
		public void ValidateEventHandlerOfEventArgsOfTProducesNoNullReferenceException()
		{
			var type = _assembly.GetType("AssemblyToProcess.GenericEventHandlerGenericArgument");
			var instance = (dynamic)Activator.CreateInstance(type);

			AssertRaiseEventThrowsNothing(instance);
		}

		[Test]
		public void ValidateTwoEventHandlersNoNullReferenceException()
		{
			var type = _assembly.GetType("AssemblyToProcess.TwoEvents");
			var instance = (dynamic)Activator.CreateInstance(type);

			AssertRaiseEventThrowsNothing(instance);
		}

		[Test]
		public void ValidateTwoEventHandlersDifferentSignatureNoNullReferenceException()
		{
			var type = _assembly.GetType("AssemblyToProcess.TwoDifferentEvents");
			var instance = (dynamic)Activator.CreateInstance(type);

			AssertRaiseEventThrowsNothing(instance);
		}

		[Test]
		public void ValidateValueTypeEventHandlersNoNullReferenceException()
		{
			var type = _assembly.GetType("AssemblyToProcess.ValueTypeEventHandler");
			var instance = (dynamic)Activator.CreateInstance(type);

			AssertRaiseEventThrowsNothing(instance);
		}

		[Test]
		public void ValidateValueTypeGenericEventHandlersNoNullReferenceException()
		{
			var type = _assembly.GetType("AssemblyToProcess.ValueTypeGenericEventHandler");
			var instance = (dynamic)Activator.CreateInstance(type);

			AssertRaiseEventThrowsNothing(instance);
		}

		private void AssertRaiseEventThrowsNothing(dynamic instance)
		{
			TestDelegate call = () => instance.RaiseEvent();
			Assert.That(call, Throws.Nothing);
		}

		[Test]
		public void PeVerify()
		{
			Verifier.Verify(_assemblyPath, _newAssemblyPath);
		}
	}
}