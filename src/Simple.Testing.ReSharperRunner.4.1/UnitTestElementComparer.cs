using System;
using System.Collections.Generic;

#if RESHARPER_5 || RESHARPER_6
using JetBrains.ReSharper.UnitTestFramework;
#else
using JetBrains.ReSharper.UnitTestExplorer;
#endif
using Simple.Testing.ReSharperRunner.Presentation;

namespace Simple.Testing.ReSharperRunner
{
	using Presentation;

#if RESHARPER_6  
  internal class UnitTestElementComparer : Comparer<IUnitTestElement>
  {

    public override int Compare(IUnitTestElement x, IUnitTestElement y)
    {
#else
  internal class UnitTestElementComparer : Comparer<UnitTestElement>
  {

    public override int Compare(UnitTestElement x, UnitTestElement y)
    {
#endif
      if (Equals(x, y))
      {
        return 0;
      }

      if ((x is ContextSpecificationElement) && y is ContextElement)
      {
        return -1;
      }

      if (x is ContextElement && (y is ContextSpecificationElement))
      {
        return 1;
      }

      if (x is ContextSpecificationElement)
      {
        return 1;
      }

      if (y is ContextSpecificationElement)
      {
        return -1;
      }
#if RESHARPER_6
      return StringComparer.CurrentCultureIgnoreCase.Compare(x.ShortName, y.ShortName);
#else
      return StringComparer.CurrentCultureIgnoreCase.Compare(x.GetTitle(), y.GetTitle());
#endif
    }

  }
}