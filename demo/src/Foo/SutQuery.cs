using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Foo
{
	public class SutQuery
	{
		public int[] Fibonacci(int n)
		{
			if(n < 0 )
			{
				throw new ArgumentOutOfRangeException("n", n, "Argument must be greater or equal to 0.");
			}
			var result = new List<int>();
			for (int i = 0; i <= n; i++)
			{
				if(i ==0){result.Add(0); continue;}
				if(i == 1){result.Add(1); continue;}
				result.Add(result[i-1]+ result[i-2]);
			}
			return result.ToArray();
		}
	}
}
