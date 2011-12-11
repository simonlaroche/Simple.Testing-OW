using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Simple.Testing.Framework
{
    public  class SimpleRunner
    {
        public static IEnumerable<RunResult> RunAllInAssembly(string assemblyName)
        {
            var assembly = Assembly.LoadFrom(assemblyName);
            return RunAllInAssembly(assembly);
        }

		public static RunResult RunMember(MemberInfo memberInfo)
		{
			var generator = new MemberGenerator(memberInfo); ;
			var runner = new SpecificationRunner();
			var specificationToRun = generator.GetSpecifications().First();
			return runner.RunSpecification(specificationToRun);
		} 

        public static IEnumerable<RunResult> RunAllInAssembly(Assembly assembly)
        {
            var runner = new SpecificationRunner();
        	return runner.RunAssembly(assembly);
        }
    }
}
