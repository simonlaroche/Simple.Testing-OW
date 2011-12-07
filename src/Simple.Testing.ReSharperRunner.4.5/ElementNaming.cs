
namespace Simple.Testing.ReSharperRunner
{
	using JetBrains.ReSharper.Psi;
	using JetBrains.ReSharper.Psi.Naming.Elements;
	
	[NamedElementsBag(null)]
  public class ElementNaming : ElementKindOfElementType
  {
    public static readonly IElementKind Context = new ElementNaming("Machine.Specifications_Context",
                                                                         "Machine.Specifications context (class containing specifications)",
                                                                         IsContext);

    public static readonly IElementKind ContextBase = new ElementNaming("Machine.Specifications_ContextBase",
                                                                             "Machine.Specifications context base class",
                                                                             IsContextBase);

    public static readonly IElementKind Specification = new ElementNaming("Machine.Specifications_Specification",
                                                                               "Machine.Specifications specification (field of type It)",
                                                                               IsSpecification);

    public static readonly IElementKind SupportingField =
      new ElementNaming("Machine.Specifications_SupportingField",
                             "Machine.Specifications supporting field (Establish, Because, Cleanup)",
                             IsSupportingField);

    public static readonly IElementKind Fields = new ElementNaming("Machine.Specifications_Field",
                                                                        "Machine.Specifications non-supporting field",
                                                                        IsField);

    public static readonly IElementKind Constants = new ElementNaming("Machine.Specifications_Constant",
                                                                           "Machine.Specifications constant",
                                                                           IsConstant);

    public static readonly IElementKind Locals = new ElementNaming("Machine.Specifications_Local",
                                                                        "Machine.Specifications local",
                                                                        IsLocal);

	protected ElementNaming(string name, string presentableName, System.Func<IDeclaredElement, bool> isApplicable)
      : base(name, presentableName, isApplicable)
    {
    }

#if RESHARPER_5
    public override PsiLanguageType Language
    {
      get { return PsiLanguageType.ANY; }
    }
#endif

    static bool IsContext(IDeclaredElement declaredElement)
    {
      return declaredElement.IsContext();
    }

    static bool IsContextBase(IDeclaredElement declaredElement)
    {
      return declaredElement.IsContextBaseClass();
    }

    static bool IsSpecification(IDeclaredElement declaredElement)
    {
      return declaredElement.IsSpecification() && IsInSpecificationContainer(declaredElement);
    }

    static bool IsSupportingField(IDeclaredElement declaredElement)
    {
      return declaredElement.IsSupportingField() && IsInSpecificationContainer(declaredElement);
    }

    static bool IsField(IDeclaredElement declaredElement)
    {
      return declaredElement.IsField() && IsInSpecificationContainer(declaredElement);
    }

    static bool IsConstant(IDeclaredElement declaredElement)
    {
      return declaredElement.IsConstant() && IsInSpecificationContainer(declaredElement);
    }

    static bool IsLocal(IDeclaredElement declaredElement)
    {
      return declaredElement.IsLocal() && IsInSpecificationContainer(declaredElement);
    }

    /// <summary>
    /// Determines if the declared element is contained in a MSpec-related container type,
    /// i.e.: Context, context base class, class with <see cref="BehaviorsAttribute" />.
    /// </summary>
    static bool IsInSpecificationContainer(IDeclaredElement declaredElement)
    {
#if RESHARPER_6
      ITypeElement containingType = null;
      if (declaredElement is ITypeMember)
        containingType = ((ITypeMember) declaredElement).GetContainingType();
      else if (declaredElement is ITypeElement)
        containingType = (ITypeElement) declaredElement;
#else
      var containingType = declaredElement.GetContainingType();
#endif
      return IsContext(containingType) || containingType.IsBehaviorContainer() || IsContextBase(containingType);
    }
  }
}