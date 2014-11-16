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

			GetEventsToWeave().ToList().ForEach(WeaveEvent);
		}

		private void WeaveEvent(EventDefinition eventDefinition)
		{
			LogInfo("Weaving event '" + eventDefinition.DeclaringType.FullName + "." + eventDefinition.Name + "'");

			var method = AddStaticEventHandlerMethod(eventDefinition.DeclaringType, GetEventHandlerInvokeMethod(eventDefinition).Parameters);
			InitializeEventWithStaticHandler(eventDefinition, method);

			LogInfo("Weaving event done");
		}

		private IEnumerable<EventDefinition> GetEventsToWeave()
		{
			return ModuleDefinition.Types.SelectMany(t => t.Events);
		}

		private MethodReference AddStaticEventHandlerMethod(TypeDefinition typeToWeave, IEnumerable<ParameterDefinition> parameters)
		{
			var method = new MethodDefinition("<.ctor>f__0", MethodAttributes.Private | MethodAttributes.Static, _typeSystem.Void);

			int number = 0;
			foreach (ParameterDefinition parameter in parameters)
			{
				method.Parameters.Add(new ParameterDefinition("a" + (++number), ParameterAttributes.In, _typeSystem.Object));
			}

			var processor = method.Body.GetILProcessor();
			processor.Emit(OpCodes.Nop);
			processor.Emit(OpCodes.Ret);
			typeToWeave.Methods.Add(method);

			return method;
		}

		private void InitializeEventWithStaticHandler(EventDefinition eventToWeave, MethodReference handlerMethod)
		{
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
				var genericType = (GenericInstanceType) eventType;
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