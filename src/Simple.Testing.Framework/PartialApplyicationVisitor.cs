

namespace Simple.Testing.Framework
{
	using System;
	using System.Linq.Expressions;
	using Linq;

	public sealed class PartialApplicationVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _paramExprToReplace;
        private readonly ConstantExpression _valuetoApply;

        private PartialApplicationVisitor(ParameterExpression paramExprToReplace, ConstantExpression valueToApply)
        {
            _paramExprToReplace = paramExprToReplace;
            _valuetoApply = valueToApply;
        }

        public static Expression<Func<bool>> Apply<T>(Expression<Func<T,bool>> expr, object value)
        {
            var paramExprToReplace = expr.Parameters[0];
            var valueToApply = Expression.Constant(value, value.GetType());
            var visitor = new PartialApplicationVisitor(paramExprToReplace, valueToApply);

            var oldBody = expr.Body;
            var newBody = visitor.Visit(oldBody);
            return Expression.Lambda<Func<bool>>(newBody);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
        	Expression result = null;
			if(node == _paramExprToReplace)
			{
				result = _valuetoApply;
			}
			else
			{
				result = base.VisitParameter(node);	
			}
        	return result;
        }
    }

}
