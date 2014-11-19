using System;
using System.Collections.Generic;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace SafeEvents.Fody
{
	public class ModuleWeaver
	{
		// Will log an informational message to MSBuild
		public Action<string> LogInfo { get; set; }

		// An instance of Mono.Cecil.ModuleDefinition for processing
		public ModuleDefinition ModuleDefinition { get; set; }

		private TypeSystem _typeSystem;

		// Init logging delegates to make testing easier
		public ModuleWeaver()
		{
			LogInfo = m => { };
		}

		public void Execute()
		{
			_typeSystem = ModuleDefinition.TypeSystem;

			GetEventsToWeave().GroupBy(e => e.DeclaringType).ForEach(WeaveType);

			LogInfo("Done.");
		}

		private IEnumerable<EventDefinition> GetEventsToWeave()
		{
			return ModuleDefinition.Types.SelectMany(t => t.Events);
		}

		private void WeaveType(IGrouping<TypeDefinition, EventDefinition> eventsToWeaveForType)
		{
			LogInfo("Weaving type '" + eventsToWeaveForType.Key.FullName + "'");

			(from e in eventsToWeaveForType
			 let invokeMethod = GetEventHandlerInvokeMethod(e)
			 group e by invokeMethod.Parameters.Count
				 into g
				 orderby g.Key ascending
				 select new
				 {
					 ParameterCount = g.Key,
					 EventsToWeave = g.AsEnumerable()
				 })
				.ForEach(e => WeaveEventGroup(eventsToWeaveForType.Key, e.EventsToWeave, e.ParameterCount));
		}

		private void WeaveEventGroup(TypeDefinition type, IEnumerable<EventDefinition> eventsToWeave, int parameterCount)
		{
			LogInfo("Weaving events with " + parameterCount + " parameters");

			var method = AddStaticEventHandlerMethod(type, parameterCount);
			foreach (var eventDefinition in eventsToWeave)
			{
				WeaveEvent(eventDefinition, method);
			}
		}

		private MethodReference AddStaticEventHandlerMethod(TypeDefinition typeToWeave, int parameterCount)
		{
			LogInfo("Adding static method with " + parameterCount + " parameters");
			var method = new MethodDefinition("<.ctor>f__0", MethodAttributes.Private | MethodAttributes.Static, _typeSystem.Void);

			foreach (int number in Enumerable.Range(1, parameterCount))
			{
				method.Parameters.Add(new ParameterDefinition("a" + number, ParameterAttributes.In, _typeSystem.Object));
			}

			var processor = method.Body.GetILProcessor();
			processor.Emit(OpCodes.Nop);
			processor.Emit(OpCodes.Ret);
			typeToWeave.Methods.Add(method);

			return method;
		}

		private void WeaveEvent(EventDefinition eventToWeave, MethodReference handlerMethod)
		{
			LogInfo("Initialize event '" + eventToWeave.Name + "'");

			var method = eventToWeave.DeclaringType.GetConstructors().First();
			var processor = method.Body.GetILProcessor();
			var firstInstruction = method.Body.Instructions.First();

			//push this reference onto stack (first parameter of Delegate ctor)
			processor.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Ldarg_0));
			//push null onto stack (second parameter of Delegate ctor)
			processor.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Ldnull));
			//push function pointer of handler to stack
			processor.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Ldftn, handlerMethod));
			//create instance of event handler delegate
			processor.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Newobj, GetEventHandlerConstructor(eventToWeave)));
			//set event to instance of delegate
			processor.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Stfld, GetEventField(eventToWeave)));
		}

		private MethodReference GetEventHandlerInvokeMethod(EventDefinition eventToWeave)
		{
			return eventToWeave.EventType.Resolve().Methods.Single(m => m.Name == "Invoke");
		}

		private MethodReference GetEventHandlerConstructor(EventDefinition eventToWeave)
		{
			TypeReference eventType = eventToWeave.EventType;
			MethodReference ctor = eventType.Resolve().GetConstructors().Single();

			if (eventType.IsGenericInstance)
			{
				var genericType = (GenericInstanceType)eventType;
				ctor = ctor.MakeHostInstanceGeneric(genericType.GenericArguments);
			}

			return ModuleDefinition.Import(ctor);
		}

		private FieldReference GetEventField(EventDefinition eventToWeave)
		{
			return eventToWeave.DeclaringType.Fields.Single(f => f.Name == eventToWeave.Name);
		}
	}
}