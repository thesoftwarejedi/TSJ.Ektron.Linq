using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Ektron.Cms.Common;
using Ektron.Cms.Content;
using Ektron.Cms.Framework.Content;
using Ektron.Cms.Framework;
using Ektron.Cms;

namespace TSJ.Ektron.Linq.Provider
{

    //theres an awful lot of assumptions in here!
    internal class EktronExpressionVisitor<T,U> : ExpressionVisitor where T : Criteria<U>, new()
    {

        T _criteria;
        int _take = 0;
        int _skip = 0;
        bool _paging = false;

        internal EktronExpressionVisitor(T criteria)
        {
            _criteria = criteria;
        }

        internal EktronExpressionVisitor()
        {
            _criteria = new T();
        }

        internal T Translate(Expression ex) 
        {
            this.Visit(ex);
            //apply the take/skip
            if (_paging)
            {
                _criteria.PagingInfo = new PagingInfo(_take, _skip + 1);
            }
            return _criteria;
        }

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            return e;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Where")
            {
                this.Visit(m.Arguments[0]);
                LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                this.Visit(lambda.Body);
                return m;
            }
            else if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "OrderBy")
            {
                this.Visit(m.Arguments[0]);
                LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                try
                {
                    var memberName = ((MemberExpression)lambda.Body).Member.Name;
                    _criteria.OrderByDirection = EkEnumeration.OrderByDirection.Ascending;
                    _criteria.OrderByField = (U)Enum.Parse(typeof(U), memberName);
                }
                catch
                {
                    throw new NotSupportedException(string.Format("OrderBy '{0}' is not supported", lambda.Body));
                }
                return m;
            }
            else if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "OrderByDescending")
            {
                this.Visit(m.Arguments[0]);
                LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                try
                {
                    var memberName = ((MemberExpression)lambda.Body).Member.Name;
                    _criteria.OrderByDirection = EkEnumeration.OrderByDirection.Descending;
                    _criteria.OrderByField = (U)Enum.Parse(typeof(U), memberName);
                }
                catch
                {
                    throw new NotSupportedException(string.Format("OrderByDescending '{0}' is not supported", lambda.Body));
                }
                return m;
            }
            else if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Take")
            {
                this.Visit(m.Arguments[0]);
                _paging = true;
                _take += (int)((ConstantExpression)m.Arguments[1]).Value;
                return m;
            }
            else if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Skip")
            {
                this.Visit(m.Arguments[0]);
                _paging = true;
                _skip += (int)((ConstantExpression)m.Arguments[1]).Value;
                return m;
            }
            throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            Expression left = this.Visit(b.Left);
            Expression right = this.Visit(b.Right);
            CriteriaFilterOperator op;
            switch (b.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    //assume these were also binary
                    return b;
                case ExpressionType.Equal:
                    op = CriteriaFilterOperator.EqualTo;
                    break;
                case ExpressionType.NotEqual:
                    op = CriteriaFilterOperator.NotEqualTo;
                    break;
                case ExpressionType.LessThan:
                    op = CriteriaFilterOperator.LessThan;
                    break;
                case ExpressionType.LessThanOrEqual:
                    op = CriteriaFilterOperator.LessThanOrEqualTo;
                    break;
                case ExpressionType.GreaterThan:
                    op = CriteriaFilterOperator.GreaterThan;
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    op = CriteriaFilterOperator.GreaterThanOrEqualTo;
                    break;
                default:
                    throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", b.NodeType));
            }
            if (left.CanReduce) left = left.Reduce();
            if (right.CanReduce) right = right.Reduce();
            if (left.NodeType == ExpressionType.Constant && right.NodeType == ExpressionType.Constant)
            {
                var leftVal = ((ConstantExpression)left).Value;
                var rightVal = ((ConstantExpression)right).Value;
                if (leftVal.GetType() == typeof(U))
                {
                    _criteria.AddFilter((U)leftVal, op, rightVal);
                }
                else if (rightVal.GetType() == typeof(U))
                {
                    _criteria.AddFilter((U)rightVal, op, leftVal);
                }
                return b;
            }

            throw new NotSupportedException(string.Format("The binary expression '{0}' is not supported", b.Method.Name));
        }

        protected override Expression VisitMember(MemberExpression m)
        {
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
            {
                var memberName = m.Member.Name;
                //the member name called on the lambda parameter is the same as the name on 
                //the U type (ContentProperty, MenuProperty, etc...)
                return Expression.Constant(Enum.Parse(typeof(U), memberName));
            } 
            throw new NotSupportedException(string.Format("The member '{0}' is not supported", m.Member.Name));
        }
    }
}
