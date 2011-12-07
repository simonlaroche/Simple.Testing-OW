namespace Simple.Testing.Framework
{
	using System;

	[Serializable]
	public class SpecificationInfo
	{
		public SpecificationInfo(string containingType, string fieldName)
		{
			ContainingType = containingType;
			FieldName = fieldName;
		}

		public string ContainingType { get; set; }
		public string FieldName { get; set; }
	}
}