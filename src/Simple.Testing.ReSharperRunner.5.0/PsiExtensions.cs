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

			var methods = clazz.Methods;
			if (methods == null || !methods.Any())
			{
				return false;
			}

			return clazz.IsValid() &&
				   !clazz.IsAbstract &&
				   methods.Any(x => IsSpecification(x));
		}

		public static bool IsSpecification(this IDeclaredElement element)
		{
			//use type name because the Specification assemnly might not be loaded
			//return element.IsValidFieldOfType(typeof(Specification));

		    var method = element as IMethod;
            if (method == null) return false;
            if (!method.ReturnType.IsValid()) return false;

		    var type = method.ReturnType;

            if (type.ToString() == "Simple.Testing.ClientFramework.Specification") return true;
            return type.ToString() ==
                   "System.Collections.Generic.IEnumerable`1[T -> Simple.Testing.ClientFramework.Specification]";
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

        static bool IsType(this IType type, string typeName)
        {
            if (!type.IsValid()) return false;
            
#if RESHARPER_6

            if (type.ToString() == "Simple.Testing.ClientFramework.Specification") return true;
            return type.ToString() ==
                   "System.Collections.Generic.IEnumerable`1[T -> Simple.Testing.ClientFramework.Specification]";

            
#else
            return new CLRTypeName(type.GetScalarType().GetCLRName()) == new CLRTypeName(typeName);
#endif
        }

        

//        static bool IsType(this IDeclaredType element, string typeName)
//        {
//            if (!element.IsValid()) return false;
//            
//
//#if RESHARPER_6
//      return element.GetClrName() == ;
//#else
//            return new CLRTypeName(element.GetCLRName()) == new CLRTypeName(typeName);
//#endif
//        }


		static bool IsValidFieldOfType(this IDeclaredElement element, string typeName)
		{
			IDeclaredType fieldType = element.GetValidatedFieldType();
			if (fieldType == null)
			{
				return false;
			}

#if RESHARPER_6
      return fieldType.GetClrName().FullName == typeName;
#else
			return new CLRTypeName(fieldType.GetCLRName()) == new CLRTypeName(typeName);
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
