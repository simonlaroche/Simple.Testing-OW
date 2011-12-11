using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using PowerAssert;
using PowerAssert.Infrastructure;
using PowerAssert.Infrastructure.Nodes;

namespace PowerAssertTests
{
	using System.Collections;
	using NUnit.Framework;

	/// <summary>
	/// Summary description for UnitTest1
	/// </summary>
	[TestFixture]
	public class EndToEndTest
	{


		[Test]
		public void PrintResults()
		{
			int x = 11;
			int y = 6;
			DateTime d = new DateTime(2010, 3, 1);
			Expression<Func<bool>> expression = () => x + 5 == d.Month*y;
			Node constantNode = ExpressionParser.Parse(expression.Body);
			string[] strings = NodeFormatter.Format(constantNode);
			string s = string.Join(Environment.NewLine, strings);
			Console.Out.WriteLine(s);
		}

		[Test]
		[Ignore("This test will fail for demo purposes")]
		public void RunComplexExpression()
		{
			int x = 11;
			int y = 6;
			DateTime d = new DateTime(2010, 3, 1);
			PAssert.IsTrue(() => x + 5 == d.Month*y);
		}

		private static int field = 11;

		[Test]
		[Ignore("This test will fail for demo purposes")]
		public void RunComplexExpressionWithStaticField()
		{
			int y = 6;
			DateTime d = new DateTime(2010, 3, 1);
			PAssert.IsTrue(() => field + 5 == d.Month*y);
		}

		[Test]
		[Ignore("This test will fail for demo purposes")]
		public void RunComplexExpression2()
		{
			string x = " lalalaa ";
			int i = 10;
			PAssert.IsTrue(() => x.Trim().Length == Math.Max(4, new int[] {5, 4, i/3, 2}[0]));
		}

		[Test]
		[Ignore("This test will fail for demo purposes")]
		public void RunComplexExpression3()
		{
			List<int> l = new List<int> {1, 2, 3};
			bool b = false;
			PAssert.IsTrue(() => l[2].ToString() == (b ? "three" : "four"));
		}

		[Test]
		[Ignore("This test will fail for demo purposes")]
		public void RunStringCompare()
		{
			string s = "hello, bobby";
			Tuple<string> t = new Tuple<string> ("hello, Bobby");
			PAssert.IsTrue(() => s == t.Item1);
		}

		[Test]
		[Ignore("This test will fail for demo purposes")]
		public void RunRoundingEdgeCase()
		{
			double d = 3;
			int i = 2;


			PAssert.IsTrue(() => 4.5 == d + 3/i);
		}

		[Test]
		[Ignore("This test will fail for demo purposes")]
		public void EqualsButNotOperatorEquals()
		{
			var t1 = new List<string>() {"foo"};
			var t2 = new List<string>() {"foo"};

			PAssert.IsTrue(() => t1 == t2);
		}

		[Test]
		[Ignore("This test will fail for demo purposes")]
		public void SequenceEqualButNotOperatorEquals()
		{
			object list = new List<int> {1, 2, 3};
			object array = new[] {1, 2, 3};
			PAssert.IsTrue(() => list == array);
		}

		[Test]
		[Ignore("This test will fail for demo purposes")]
		public void PrintingLinqStatements()
		{
			var list = Enumerable.Range(0, 150);
			PAssert.IsTrue(() => list.Where(x => x%2 == 0).Sum() == 0);
		}

		[Test]
		[Ignore("This test will fail for demo purposes")]
		public void PrintingLinqExpressionStatements()
		{
			var list = Enumerable.Range(0, 150);
			PAssert.IsTrue(() => (from l in list where l%2 == 0 select l).Sum() == 0);
		}

		[Test]
		[Ignore("This test will fail for demo purposes")]
		public void PrintingComplexLinqExpressionStatements()
		{
			var list = Enumerable.Range(0, 5);
			PAssert.IsTrue(() => (from x in list from y in list where x > y select x + "," + y).Count() == 0);
		}

		[Test]
		[Ignore("This test will fail for demo purposes")]
		public void PrintingEnumerablesWithNulls()
		{
			var list = new List<int?> {1, 2, null, 4, 5};
			PAssert.IsTrue(() => list.Sum() == null);
		}

		[Test]
		[Ignore("This test will fail for demo purposes")]
		public void PrintingUnaryNot()
		{
			var b = true;
			PAssert.IsTrue(() => !b);
		}

		[Test]
		[Ignore("This test will fail for demo purposes")]
		public void PrintingUnaryNegate()
		{
			var b = 5;
			PAssert.IsTrue(() => -b == 0);
		}

		[Test]
		[Ignore("This test will fail for demo purposes")]
		public void PrintingIsTest()
		{
			var b = new object();
			PAssert.IsTrue(() => b is string);
		}
	}

	// -----------------------------------------------------------------------
	// Copyright (c) Microsoft Corporation.  All rights reserved.
	// -----------------------------------------------------------------------
#if !CLR40

	// This is a very minimalistic implementation of Tuple'2 that allows us
	// to compile and work on versions of .Net eariler then 4.0.
	public struct Tuple<TItem1, TItem2>
	{
		public Tuple(TItem1 item1, TItem2 item2)
		{
			this = new Tuple<TItem1, TItem2>();
			this.Item1 = item1;
			this.Item2 = item2;
		}

		public TItem1 Item1 { get; private set; }
		public TItem2 Item2 { get; private set; }

		public override bool Equals(object obj)
		{
			if (obj is Tuple<TItem1, TItem2>)
			{
				Tuple<TItem1, TItem2> that = (Tuple<TItem1, TItem2>) obj;
				return object.Equals(this.Item1, that.Item1) && object.Equals(this.Item2, that.Item2);
			}
			else
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			return ((this.Item1 != null) ? this.Item1.GetHashCode() : 0) ^ ((this.Item2 != null) ? this.Item2.GetHashCode() : 0);
		}

		public static bool operator ==(Tuple<TItem1, TItem2> left, Tuple<TItem1, TItem2> right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Tuple<TItem1, TItem2> left, Tuple<TItem1, TItem2> right)
		{
			return !left.Equals(right);
		}
	}

	[Serializable]
	public class Tuple<T1> : IComparable
	{
		private T1 item1;

		public Tuple(T1 item1)
		{
			this.item1 = item1;
		}

		public T1 Item1
		{
			get { return item1; }
		}

		public int CompareTo(object obj)
		{
			return this.CompareTo(obj, Comparer<object>.Default);
		}

		private int CompareTo(object other, IComparer comparer)
		{
			var t = other as Tuple<T1>;
			if (t == null)
			{
				if (other == null) return 1;
				throw new ArgumentException();
			}

			return comparer.Compare(item1, t.item1);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj, EqualityComparer<object>.Default);
		}

		private bool Equals(object other, IEqualityComparer comparer)
		{
			var t = other as Tuple<T1>;
			if (t == null)
			{
				if (other == null) return false;
				throw new ArgumentException();
			}

			return comparer.Equals(item1, t.item1);
		}

		public override int GetHashCode()
		{
			return this.GetHashCode(EqualityComparer<object>.Default);
		}

		private int GetHashCode(IEqualityComparer comparer)
		{
			return comparer.GetHashCode(item1);
		}

		public override string ToString()
		{
			return String.Format("({0})", item1);
		}
	}
}

#endif
