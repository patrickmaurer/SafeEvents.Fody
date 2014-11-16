using System.Collections.Generic;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace SafeEvents.Fody
{
	public static class MethodReferenceExtensions
	{
		public static MethodReference MakeHostInstanceGeneric(this MethodReference @this, IEnumerable<TypeReference> genericArguments)
		{
			return @this.MakeHostInstanceGeneric(genericArguments.ToArray());
		}

		// https://stackoverflow.com/questions/16430947/emit-call-to-system-lazyt-constructor-with-mono-cecil
		public static MethodReference MakeHostInstanceGeneric(this MethodReference @this, params TypeReference[] genericArguments)
		{
			var reference = new MethodReference(@this.Name, @this.ReturnType, @this.DeclaringType.MakeGenericInstanceType(genericArguments))
			{
				HasThis = @this.HasThis,
				ExplicitThis = @this.ExplicitThis,
				CallingConvention = @this.CallingConvention
			};

			foreach (var parameter in @this.Parameters)
			{
				reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));
			}

			foreach (var genericParam in @this.GenericParameters)
			{
				reference.GenericParameters.Add(new GenericParameter(genericParam.Name, reference));
			}

			return reference;
		}
	}
}