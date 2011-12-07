namespace Simple.Testing.Framework
{
	using System;

	public interface ISpecificationRunListener
	{
		void OnAssemblyStart(AssemblyInfo assembly);
		void OnAssemblyEnd(AssemblyInfo assembly);
		void OnRunStart();
		void OnRunEnd();
		void OnSpecificationStart(SpecificationInfo specification);
		void OnSpecificationEnd(SpecificationInfo specification, RunResult result);
		void OnFatalError(Exception exception);
	}

	public class EmptySpecificationRunListener : ISpecificationRunListener
	{
		public void OnAssemblyStart(AssemblyInfo assembly)
		{
		}

		public void OnAssemblyEnd(AssemblyInfo assembly)
		{
		}

		public void OnRunStart()
		{
		}

		public void OnRunEnd()
		{
		}

		public void OnSpecificationStart(SpecificationInfo specification)
		{
		}

		public void OnSpecificationEnd(SpecificationInfo specification, RunResult result)
		{
		}

		public void OnFatalError(Exception exception)
		{
		}
	}

	public interface ISpecificationResultProvider
	{
		bool FailureOccurred { get; }
	}


	[Serializable]
	public class AssemblyInfo
	{
		public AssemblyInfo(string name, string location)
		{
			Name = name;
			Location = location;
		}

		public string Name { get; private set; }

		public string Location { get; private set; }

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}
			if (ReferenceEquals(this, obj))
			{
				return true;
			}
			if (obj.GetType() != typeof (AssemblyInfo))
			{
				return false;
			}
			return GetHashCode() == obj.GetHashCode();
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((Name != null ? Name.GetHashCode() : 0)*397) ^ (Location != null ? Location.GetHashCode() : 0);
			}
		}
	}
}