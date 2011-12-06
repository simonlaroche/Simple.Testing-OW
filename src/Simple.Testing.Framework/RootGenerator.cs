using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Simple.Testing.Framework
{
	public class MemberGenerator: ISpecificationGenerator
	{
		private readonly MemberInfo info;

		public MemberGenerator(MemberInfo info)
		{
			this.info = info;
		}

		public IEnumerable<SpecificationToRun> GetSpecifications()
		{
			return TypeReader.GetSpecificationsIn(info);
			
		}
	}

	public class RootGenerator : ISpecificationGenerator
    {
        private readonly Assembly _assembly;

        public RootGenerator(Assembly assembly)
        {
            _assembly = assembly;
        }

        public IEnumerable<SpecificationToRun> GetSpecifications()
        {
            return _assembly.GetTypes().SelectMany(TypeReader.GetSpecificationsIn);
        }

    }

    public static class TypeReader
    {
        public static IEnumerable<SpecificationToRun> GetSpecificationsIn(Type t)
        {
            foreach (var methodSpec in AllMethodSpecifications(t)) yield return methodSpec;
            foreach (var fieldSpec in AllFieldSpecifications(t)) yield return fieldSpec;
        }

		public static IEnumerable<SpecificationToRun> GetSpecificationsIn(MemberInfo info)
		{
			if(info is FieldInfo)
			{
				return SpecificationToRuns((FieldInfo) info);
			}
			if(info is MethodInfo)
			{
				return SpecificationToRuns((MethodInfo)info);
			}
			return Enumerable.Empty<SpecificationToRun>();
		}


    	private static IEnumerable<SpecificationToRun> AllMethodSpecifications(Type t)
        {
            foreach (var s in t.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
            	foreach (var specificationToRun in SpecificationToRuns(s)) yield return specificationToRun;
            }
        }

    	private static IEnumerable<SpecificationToRun> SpecificationToRuns(MethodInfo s)
    	{
    		if (typeof (Specification).IsAssignableFrom(s.ReturnType))
    		{
    			var result = CallMethod(s);
    			if (result != null) yield return new SpecificationToRun((Specification) result, s);
    		}
    		if (typeof (IEnumerable<Specification>).IsAssignableFrom(s.ReturnType))
    		{
    			var obj = (IEnumerable<Specification>) CallMethod(s);
    			foreach (var item in obj)
    				yield return new SpecificationToRun(item, s);
    		}
    	}

    	private static IEnumerable<SpecificationToRun> AllFieldSpecifications(Type t)
        {
            foreach (var m in t.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
            	foreach (var specificationToRun in SpecificationToRuns(m)) yield return specificationToRun;
            }
        }

    	private static IEnumerable<SpecificationToRun> SpecificationToRuns(FieldInfo m)
    	{
    		if (typeof (Specification).IsAssignableFrom(m.FieldType))
    		{
    			yield return new SpecificationToRun((Specification) m.GetValue(Activator.CreateInstance(m.DeclaringType)), m);
    		}
    		if (typeof (IEnumerable<Specification>).IsAssignableFrom(m.FieldType))
    		{
    			var obj = (IEnumerable<Specification>) m.GetValue(Activator.CreateInstance(m.DeclaringType));
    			foreach (var item in obj)
    				yield return new SpecificationToRun(item, m);
    		}
    	}

    	private static object CallMethod(MethodInfo methodInfo)
        {
            if (methodInfo.GetParameters().Length > 0) return null;
            var obj = Activator.CreateInstance(methodInfo.DeclaringType);
            var ret = methodInfo.Invoke(obj, null);
            return ret;
        }
    }

    public class SpecificationToRun
    {
        public readonly Specification Specification;
        public readonly MemberInfo FoundOn;
        public readonly bool IsRunnable;
        public readonly string Reason;
        public readonly Exception Exception;

        public SpecificationToRun(Specification specification, MemberInfo foundOn)
        {
            IsRunnable = true;
            Reason = "";
            Exception = null;
            Specification = specification;
            FoundOn = foundOn;
        }

        public SpecificationToRun(Specification specification, string reason, Exception exception, MemberInfo foundOn)
        {
            FoundOn = foundOn;
            Specification = specification;
            Exception = exception;
            Reason = reason;
            IsRunnable = false;
        }
    }
}
