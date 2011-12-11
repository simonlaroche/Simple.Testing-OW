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

	internal class ContextElementHandler : IElementHandler
	{
		readonly ContextFactory _contextFactory;

		public ContextElementHandler(ContextFactory contextFactory)
		{
			_contextFactory = contextFactory;
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

			return declaration.DeclaredElement.IsContext();
		}

#if RESHARPER_6
    public IEnumerable<UnitTestElementDisposition> AcceptElement(ITreeNode element, IFile file)
#else
		public IEnumerable<resharper::JetBrains.ReSharper.UnitTestFramework.UnitTestElementDisposition> AcceptElement(IElement element, IFile file)
#endif
		{
			IDeclaration declaration = (IDeclaration)element;
			var contextElement = _contextFactory.CreateContext((ITypeElement)declaration.DeclaredElement);

			if (contextElement == null)
			{
				yield break;
			}

			yield return new resharper::JetBrains.ReSharper.UnitTestFramework.UnitTestElementDisposition(contextElement,
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
      var declaration = (IDeclaration)element;
      _contextFactory.UpdateChildState((IClass)declaration.DeclaredElement);
    }
#endif
	}
}