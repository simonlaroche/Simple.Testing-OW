using JetBrains.ReSharper.Psi;

namespace Simple.Testing.ReSharperRunner
{
  internal static partial class PsiExtensions
  {
    static bool IsInvalid(this IExpressionType type)
    {
      return type == null || !type.IsValid;
    }
  }
}