namespace Foo.Tests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using NUnit.Framework;
    using Simple.Testing.ClientFramework;

    public class Specifications
    {

        public Specification Spec()
        {
            return new QuerySpecification<SutQuery, int[]>
                       {
                           On = () => new SutQuery(),
                           When = x => x.Fibonacci(5),
                           Expect =
                               new List<Expression<Func<int[], bool>>>
                                   {
                                       x => x[0] == 0,
                                       x => x[1] == 1,
                                       x => x[2] == 1,
                                       x => x[3] == 2,
                                       x => x[4] == 3,
                                       x => x[5] == 5,
                                   }
                       };
        }

        public Specification negative_number_exception()
        {
            return new FailingSpecification<SutQuery, ArgumentOutOfRangeException>
                       {
                           On = () => new SutQuery(),
                           When = x => x.Fibonacci(-1),
                           Expect =
                               new List<Expression<Func<ArgumentOutOfRangeException, bool>>>
                                   {x => x != null, x => x.ParamName == "n",}
                       };
        }
    }
}