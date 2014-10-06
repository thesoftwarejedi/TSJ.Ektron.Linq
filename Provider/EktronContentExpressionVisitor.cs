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

    //merely to pull out the xmlconfigid from the lambda
    internal class EktronContentExpressionVisitor : EktronExpressionVisitor<ContentCriteria, ContentProperty>
    {

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
                    return base.VisitMember(m);
                }
            }
            else if (m.Expression != null && m.Member.DeclaringType == typeof(XmlConfigData) && m.Member.Name == "Id")
            {
                //if it's an xmlconfigid, let it through to hit the parent (xmlconfig)
                return Visit(m.Expression);
            }
            return base.Visit(m);
        }
    }
}
