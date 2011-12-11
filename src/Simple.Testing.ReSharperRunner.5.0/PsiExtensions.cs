extern alias resharper;
using System;
using System.Collections.Generic;
using System.Linq;

using resharper::JetBrains.ReSharper.Psi;

#if RESHARPER_6
using CLRTypeName = resharper::JetBrains.ReSharper.Psi.ClrTypeName;
#endif

namespace Simple.Testing.ReSharperRunner
{
	using Framework;

	internal static partial class PsiExtensions
	{
		public static bool IsContext(this IDeclaredElement element)
		{
			var clazz = element as IClass;
			if (clazz == null)
			{
				return false;
			}

			var fields = clazz.Fields;
			if (fields == null || !fields.Any())
			{
				return false;
			}

			return clazz.IsValid() &&
				   !clazz.IsAbstract &&
				   fields.Any(x => IsSpecification(x));
		}

		public static bool IsSpecification(this IDeclaredElement element)
		{
			return element.IsValidFieldOfType(typeof(Specification));
		}

		public static bool IsField(this IDeclaredElement element)
		{
			return element is IField;
		}

		public static bool IsConstant(this IDeclaredElement element)
		{
			return (element.IsField() && ((IField)element).IsConstant) ||
				   (element.IsLocal() && ((ILocalVariable)element).IsConstant);
		}

		public static bool IsLocal(this IDeclaredElement element)
		{
			return element is ILocalVariable;
		}

		public static IClass GetFirstGenericArgument(this IDeclaredElement element)
		{
			IDeclaredType fieldType = element.GetValidatedFieldType();
			if (fieldType == null)
			{
				return null;
			}

			var firstArgument = fieldType.GetSubstitution().Domain.First();
			var referencedType = fieldType.GetSubstitution().Apply(firstArgument).GetScalarType();

			if (referencedType != null)
			{
				return referencedType.GetTypeElement() as IClass;
			}

			return null;
		}

		public static IEnumerable<IField> GetBehaviorSpecifications(this IClass clazz)
		{
			return clazz.Fields.Where(IsSpecification);
		}

		static bool IsValidFieldOfType(this IDeclaredElement element, Type type)
		{
			IDeclaredType fieldType = element.GetValidatedFieldType();
			if (fieldType == null)
			{
				return false;
			}

#if RESHARPER_6
      return fieldType.GetClrName().FullName == type.FullName;
#else
			return new CLRTypeName(fieldType.GetCLRName()) == new CLRTypeName(type.FullName);
#endif
		}

		static IDeclaredType GetValidatedFieldType(this IDeclaredElement element)
		{
			IField field = element as IField;
			if (field == null)
			{
				return null;
			}

			IDeclaredType fieldType = field.Type as IDeclaredType;
			if (fieldType == null)
			{
				return null;
			}

			if (!field.IsValid() ||
				!fieldType.IsResolved)
			{
				return null;
			}

			return fieldType;
		}

		static IEnumerable<AttributeValue> PositionParameters(this IAttributeInstance source)
		{
			for (int i = 0; i < source.PositionParameterCount; i++)
			{
				yield return source.PositionParameter(i);
			}
		}
	}
}
