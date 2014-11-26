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

			var eventSignatures = from e in eventsToWeaveForType
								  let invokeMethod = GetEventHandlerInvokeMethod(e)
								  let parameterTypes = GetParameterTypes(e, invokeMethod)
								  select new
								  {
									  Event = e,
									  ParameterTypes = parameterTypes
								  };

			(from eventSignature in eventSignatures
			 group eventSignature by eventSignature.ParameterTypes.Select(pt => pt.FullName).Aggregate((current, next) => current + "#" + next)
				 into g
				 orderby g.Key ascending
				 select new
				 {
					 GroupingKey = g.Key,
					 g.First().ParameterTypes,
					 EventsToWeave = g.Select(e => e.Event)
				 })
				.ForEach(e => WeaveEventGroup(eventsToWeaveForType.Key, e.EventsToWeave, e.ParameterTypes));
		}

		private IEnumerable<TypeReference> GetParameterTypes(EventDefinition eventToWeave, MethodDefinition invokeMethod)
		{
			return invokeMethod.Parameters.Select(p => GetParameterType(eventToWeave, p));
		}

		private TypeReference GetParameterType(EventDefinition eventToWeave, ParameterDefinition parameter)
		{
			TypeDefinition typeDefinition = parameter.ParameterType.Resolve();
			if (typeDefinition != null && typeDefinition.IsValueType)
			{
				return typeDefinition;
			}

			if (parameter.ParameterType.IsGenericParameter)
			{
				var genericParameter = (GenericParameter)parameter.ParameterType;

				TypeReference genericArgumentType = ((GenericInstanceType)eventToWeave.EventType).GenericArguments[genericParameter.Position];
				if (genericArgumentType != null && genericArgumentType.IsValueType)
				{
					return ModuleDefinition.Import(genericArgumentType);
				}
			}
			return _typeSystem.Object;
		}

		private void WeaveEventGroup(TypeDefinition type, IEnumerable<EventDefinition> eventsToWeave, IEnumerable<TypeReference> parameterTypes)
		{
			var method = AddStaticEventHandlerMethod(type, parameterTypes);
			foreach (var eventDefinition in eventsToWeave)
			{
				WeaveEvent(eventDefinition, method);
			}
		}

		private MethodReference AddStaticEventHandlerMethod(TypeDefinition typeToWeave, IEnumerable<TypeReference> parameterTypes)
		{
			var method = new MethodDefinition("<.ctor>f__0", MethodAttributes.Private | MethodAttributes.Static, _typeSystem.Void);

			int number = 0;
			foreach (TypeReference parameterType in parameterTypes)
			{
				method.Parameters.Add(new ParameterDefinition("a" + (++number), ParameterAttributes.In, parameterType));
			}

			var processor = method.Body.GetILProcessor();
			processor.Emit(OpCodes.Nop);
			processor.Emit(OpCodes.Ret);
			typeToWeave.Methods.Add(method);

			LogInfo("Adding static method with " + method.Parameters.Count + " parameters");

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

		private MethodDefinition GetEventHandlerInvokeMethod(EventDefinition eventToWeave)
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