extern alias resharper;
using System.Collections.Generic;

using resharper::JetBrains.ReSharper.Psi;
using resharper::JetBrains.ReSharper.Psi.Tree;
#if RESHARPER_5 || RESHARPER_6
using resharper::JetBrains.ReSharper.UnitTestFramework;
#else
using resharper::JetBrains.ReSharper.UnitTestExplorer;
#endif

using Simple.Testing.ReSharperRunner.Factories;

namespace Simple.Testing.ReSharperRunner.Explorers.ElementHandlers
{
	using Factories;

	internal class ContextSpecificationElementHandler : IElementHandler
	{
		readonly ContextSpecificationFactory _contextSpecificationFactory;

		public ContextSpecificationElementHandler(ContextSpecificationFactory contextSpecificationFactory)
		{
			_contextSpecificationFactory = contextSpecificationFactory;
		}

#if RESHARPER_6
    public bool Accepts(ITreeNode element)
#else
		public bool Accepts(IElement element)
#endif
		{
			IDeclaration declaration = element as IDeclaration;
			if (declaration == null)
			{
				return false;
			}

			return declaration.DeclaredElement.IsSpecification();
		}

#if RESHARPER_6
    public IEnumerable<UnitTestElementDisposition> AcceptElement(ITreeNode element, IFile file)
#else
		public IEnumerable<UnitTestElementDisposition> AcceptElement(IElement element, IFile file)
#endif
		{
			IDeclaration declaration = (IDeclaration)element;
			var contextSpecificationElement =
			  _contextSpecificationFactory.CreateContextSpecification(declaration.DeclaredElement);

			if (contextSpecificationElement == null)
			{
				yield break;
			}

			yield return new UnitTestElementDisposition(contextSpecificationElement,
#if RESHARPER_6
			resharper::JetBrains.ReSharper.Psi.PsiSourceFileExtensions.ToProjectFile(file.GetSourceFile()),
#else
 file.ProjectFile,
#endif
 declaration.GetNavigationRange().TextRange,
#if RESHARPER_6
		resharper::JetBrains.ReSharper.Psi.Tree.DeclarationExtensions.GetNameDocumentRange(declaration).TextRange);
#else
		ElementExtensions.GetDocumentRange(declaration).TextRange);
#endif
		}

#if RESHARPER_6
    public void Cleanup(ITreeNode element)
    {
    }
#endif
	}
}