extern alias resharper;
using System;
using System.Collections.Generic;
using System.Linq;

using resharper::JetBrains.Metadata.Reader.API;
#if RESHARPER_6
using CLRTypeName = resharper::JetBrains.ReSharper.Psi.ClrTypeName;
#else
using resharper::JetBrains.ReSharper.Psi;
#endif

namespace Simple.Testing.ReSharperRunner
{
	using Framework;

	internal static partial class MetadataExtensions
	{
		public static bool IsContext(this IMetadataTypeInfo type)
		{
			return !type.IsAbstract &&
				   !type.IsStruct() &&
				   type.GenericParameters.Length == 0 &&
				   (type.GetSpecifications().Any());
		}

		static bool IsStruct(this IMetadataTypeInfo type)
		{
			return type.Base.Type.FullyQualifiedName == typeof(ValueType).FullName;
		}

		public static IEnumerable<IMetadataField> GetSpecifications(this IMetadataTypeInfo type)
		{
			return type.GetInstanceFieldsOfType<Specification>();
		}

		public static IMetadataTypeInfo GetFirstGenericArgument(this IMetadataField genericField)
		{
			var genericArgument = ((IMetadataClassType)genericField.Type).Arguments.First();
			return ((IMetadataClassType)genericArgument).Type;
		}

		public static IMetadataClassType FirstGenericArgumentClass(this IMetadataField genericField)
		{
			var genericArgument = ((IMetadataClassType)genericField.Type).Arguments.First();
			return genericArgument as IMetadataClassType;
		}

		public static bool IsIgnored(this IMetadataEntity type)
		{
			return false;
		}

		static IEnumerable<IMetadataField> GetInstanceFields(this IMetadataTypeInfo type)
		{
			return type.GetFields().Where(field => !field.IsStatic);
		}

		static IEnumerable<IMetadataField> GetInstanceFieldsOfType<T>(this IMetadataTypeInfo type)
		{
			return type.GetInstanceFieldsWith(typeof(T));
		}

		static IEnumerable<IMetadataField> GetInstanceFieldsWith(this IMetadataTypeInfo type, Type fieldType)
		{
			return type.GetInstanceFields()
			  .Where(x => x.Type is IMetadataClassType)
			  .Where(x => new CLRTypeName(((IMetadataClassType)x.Type).Type.FullyQualifiedName) ==
						  new CLRTypeName(fieldType.FullName));
		}
	}
}
