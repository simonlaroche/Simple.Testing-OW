extern alias resharper;
using System;
using System.Collections.Generic;

using resharper::JetBrains.Application;
using resharper::JetBrains.ProjectModel;
using resharper::JetBrains.ReSharper.Psi;
using resharper::JetBrains.ReSharper.Psi.Caches;
using resharper::JetBrains.ReSharper.UnitTestFramework;
using resharper::JetBrains.Text;

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

    public override bool Matches(string filter, resharper::JetBrains.Text.IdentifierMatcher matcher)
    {
      return matcher.Matches(GetTypeClrName());
    }

    public override string GetTitle()
    {
      return new resharper::JetBrains.ReSharper.Psi.CLRTypeName(GetTypeClrName()).ShortName;
    }

    public override resharper::JetBrains.ReSharper.Psi.IDeclaredElement GetDeclaredElement()
    {
      ISolution solution = GetSolution();
      if (solution == null)
      {
        return null;
      }

      using (resharper::JetBrains.Application.ReadLockCookie.Create())
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