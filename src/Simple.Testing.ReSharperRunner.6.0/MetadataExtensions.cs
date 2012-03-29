extern alias resharper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using resharper::JetBrains.Metadata.Reader.API;
using resharper::JetBrains.Util;

namespace Simple.Testing.ReSharperRunner
{
	using ClientFramework;
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
      if (type.Base != null)
      {
        return type.Base.Type.FullyQualifiedName == typeof(ValueType).FullName;
      }
      return false;
    }

    public static IEnumerable<IMetadataField> GetSpecifications(this IMetadataTypeInfo type)
    {
        var privateFieldsOfType = type.GetInstanceFieldsOfType<Specification>();
        return privateFieldsOfType;
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

    static IEnumerable<TResult> Flatten<TSource, TResult>(this IEnumerable<TSource> source,
                                                          Func<TSource, TResult> singleResultSelector,
                                                          Func<TSource, IEnumerable<TResult>> collectionResultSelector)
    {
      foreach (var s in source)
      {
        yield return singleResultSelector(s);
          var resultSelector = collectionResultSelector(s);
          if (resultSelector == null)
          {
              yield break;
          }
           foreach (var coll in collectionResultSelector(s))
            {
                yield return coll;
            }
      }
    }

    static IEnumerable<IMetadataField> GetInstanceFields(this IMetadataTypeInfo type)
    {
      return type.GetFields().Where(field => !field.IsStatic);
    }

    static IEnumerable<IMetadataField> GetInstanceFieldsOfType<T>(this IMetadataTypeInfo type)
    {
      return type.GetInstanceFieldsOfType(typeof(T));
    }

    static IEnumerable<IMetadataField> GetInstanceFieldsOfType(this IMetadataTypeInfo type, Type fieldType)
    {
        var metadataFields = type.GetInstanceFields();
        var fields = metadataFields.Where(x => x.Type is IMetadataClassType);
        return fields.Where(x => (((IMetadataClassType)x.Type).Type.FullyQualifiedName == fieldType.FullName));
    }
  }
}
