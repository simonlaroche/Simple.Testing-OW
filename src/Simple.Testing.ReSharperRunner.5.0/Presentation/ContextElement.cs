using System;
using System.Collections.Generic;

using JetBrains.Application;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.Text;

namespace Simple.Testing.ReSharperRunner.Presentation
{
  internal class ContextElement : Element
  {
    readonly string _assemblyLocation;

    public ContextElement(IUnitTestProvider provider,
                          ProjectModelElementEnvoy projectEnvoy,
                          string typeName,
                          string assemblyLocation,
                          bool isIgnored)
      : base(provider, null, projectEnvoy, typeName, isIgnored)
    {
      _assemblyLocation = assemblyLocation;

    }

    public override string ShortName
    {
      get { return GetKind(); }
    }

    public string AssemblyLocation
    {
      get { return _assemblyLocation; }
    }

    public override bool Matches(string filter, IdentifierMatcher matcher)
    {
      return matcher.Matches(GetTypeClrName());
    }

    public override string GetTitle()
    {
      return new CLRTypeName(GetTypeClrName()).ShortName;
    }

    public override IDeclaredElement GetDeclaredElement()
    {
      ISolution solution = GetSolution();
      if (solution == null)
      {
        return null;
      }

      using (ReadLockCookie.Create())
      {
        IDeclarationsScope scope = DeclarationsScopeFactory.SolutionScope(solution, false);
        IDeclarationsCache cache = PsiManager.GetInstance(solution).GetDeclarationsCache(scope, true);
        return cache.GetTypeElementByCLRName(GetTypeClrName());
      }
    }

    public override string GetKind()
    {
      return "Context";
    }

    public override bool Equals(object obj)
    {
      if (base.Equals(obj))
      {
        ContextElement other = (ContextElement) obj;
        return Equals(AssemblyLocation, other.AssemblyLocation);
      }

      return false;
    }

    public override int GetHashCode()
    {
      int result = base.GetHashCode();
      result = 29 * result + AssemblyLocation.GetHashCode();
      return result;
    }
  }
}