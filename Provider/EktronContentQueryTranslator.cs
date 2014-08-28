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
    internal class EktronContentQueryTranslator : ExpressionVisitor
    {

        ContentCriteria criteria;
        int take = 0;
        int skip = 0;
        bool paging = false;

        internal EktronContentQueryTranslator()
        {
        }

        internal ContentCriteria Translate(Expression ex) 
        {
            criteria = new ContentCriteria();
            this.Visit(ex);
            //apply the take/skip
            if (paging)
            {
                criteria.PagingInfo = new PagingInfo(take, skip + 1);
            }
            return criteria;
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
                    criteria.OrderByDirection = EkEnumeration.OrderByDirection.Ascending;
                    criteria.OrderByField = (ContentProperty)Enum.Parse(typeof(ContentProperty), memberName);
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
                    criteria.OrderByDirection = EkEnumeration.OrderByDirection.Descending;
                    criteria.OrderByField = (ContentProperty)Enum.Parse(typeof(ContentProperty), memberName);
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
                paging = true;
                take += (int)((ConstantExpression)m.Arguments[1]).Value;
                return m;
            }
            else if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Skip")
            {
                this.Visit(m.Arguments[0]);
                paging = true;
                skip += (int)((ConstantExpression)m.Arguments[1]).Value;
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
                if (leftVal.GetType() == typeof(ContentProperty))
                {
                    criteria.AddFilter((ContentProperty)leftVal, op, rightVal);
                }
                else if (rightVal.GetType() == typeof(ContentProperty))
                {
                    criteria.AddFilter((ContentProperty)rightVal, op, leftVal);
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
                if (memberName == "XmlConfiguration")
                {
                    return Expression.Constant(ContentProperty.XmlConfigurationId);
                }
                else
                {
                    //the member name called on the contentdata is the same as the name on contentproperty
                    return Expression.Constant(Enum.Parse(typeof(ContentProperty), memberName));
                }
            } 
            else if (m.Expression != null && m.Member.DeclaringType == typeof(XmlConfigData) && m.Member.Name == "Id")
            {
                //hit the parent (xmlconfig)
                return this.Visit(m.Expression);
            }
            throw new NotSupportedException(string.Format("The member '{0}' is not supported", m.Member.Name));
        }
    }
}
