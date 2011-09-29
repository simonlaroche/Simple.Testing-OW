using Simple.Testing.Framework;

namespace Simple.Testing.Example
{
	using System.Linq;

	/*
     * A query specification is intended to be used on a method with a return value. The
     * general idea is that the On() will build the SUT. The when() will call the method
     * returning the methods return value. The expectations are then on the returned value.
     * 
     * You may wonder how you can assert on the sut after the call. You can't using this 
     * template. This is by design, see CQS by Bertrand Meyer, you should not mutate state
     * of the object when querying. If you want to break this rule use a more open template
     * and specialize it.
     */
    public class QuerySepcification
    {
        public Specification it_returns_something_interesting = new QuerySpecification<QueryExample, Product>
        {
            On = () => new QueryExample(),
            When = obj => obj.GetProduct(14),
            Expect =
                {
                    product => product.Id == 14,
                    product => product.Code == "TEST",
                    product => product.Description == "test description"
                },
        };
    }

    public class QueryExample
    {
        public Product GetProduct(int id)
        {
            return new Product(id, "TEST", "test description");
        }
    }

    public class Product
    {
        public readonly int Id;
        public readonly string Code;
        public readonly string Description;

        public Product(int id, string code, string description)
        {
            Id = id;
            Description = description;
            Code = code;
        }
    }

	public class Array
	{
		public static int[] FindMaxSubArray(int[] array)
		{
			//find max subarray starting at index 1
			var max = array.Max();
			if (max < 0) { return new[] { max }; }

			var maxSoFar = 0;
			var lowSoFar = 0;
			var highSoFar = 0;

			var maxEndingHere = 0;
			var lowEndingHere = 0;
			var highEndingHere = 0;

			for (var i = 0; i < array.Length; i++)
			{
				var newMaxEndingHere = maxEndingHere + array[i];
				if (newMaxEndingHere > 0)
				{
					maxEndingHere = newMaxEndingHere;
					highEndingHere = i;
				}
				else
				{
					maxEndingHere = 0;
					lowEndingHere = i + 1;
				}
				//Found a new overall max
				if (maxEndingHere > maxSoFar)
				{
					maxSoFar = maxEndingHere;
					lowSoFar = lowEndingHere;
					highSoFar = highEndingHere;
				}

			}

			return array.Skip(lowSoFar).Take(highSoFar - lowSoFar + 1).ToArray();

			//			max_so_far = max_ending_here = 0
			//			for x in A:
			//				max_ending_here = max(0, max_ending_here + x)
			//				max_so_far = max(max_so_far, max_ending_here)
			//			return max_so_far

			//find maximum subarray A[i..j+1] in constant time
		}
	}


	public class MaxSubArrayArraySpecifications
	{
		public Specification When_findon_a_max_sub_array = new QuerySpecification<int[], int[]>()
		{
			On =
				() =>
				new[]
		                                                   				{
		                                                   					13, -3, -25, 20, -3, -16, -23, 18, 20, -7, 12, -5, -22, 15, -4
		                                                   					, 7
		                                                   				},
			When = x => Array.FindMaxSubArray(x),
			Expect =
		                                                   			{
		                                                   				x => x.Length  == 4,
		                                                   				x => x.Sum() == 43
		                                                   			}
		};
	}
}
