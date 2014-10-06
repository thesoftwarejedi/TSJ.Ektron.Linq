using Ektron.Cms.Common;
using Ektron.Cms.Content;
using Ektron.Cms.Framework;
using Ektron.Cms.Framework.Content;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TSJ.Ektron.Linq.QueryWrapper;

namespace TSJ.Ektron.Linq.Provider
{
    internal class EktronQueryProvider<T, U, V> : QueryProvider
        where T : CmsApi<T>, new()
        where U : Criteria<V>, new()
    {

        protected T Manager { get; private set; }
        private Func<T, U, object> _queryMethod;
        private EktronExpressionVisitor<U, V> _visitor;

        internal EktronQueryProvider(Func<T, U, object> queryMethod, ApiAccessMode mode = ApiAccessMode.LoggedInUser, int contentLanguage = int.MinValue)
        {
            _queryMethod = queryMethod;
            if (!Debugger.IsAttached)
            {
                Manager = new T();
                Manager.ApiMode = mode;
                if (contentLanguage != int.MinValue)
                    Manager.ContentLanguage = contentLanguage;
            }
        }

        internal EktronQueryProvider(Func<T, U, object> queryMethod, EktronExpressionVisitor<U, V> visitor, ApiAccessMode mode = ApiAccessMode.LoggedInUser, int contentLanguage = int.MinValue)
            : this(queryMethod, mode, contentLanguage)
        {
            _visitor = visitor;
        }

        public override object Execute(Expression expression)
        {
            expression = Evaluator.PartialEval(expression); //evals anything w/o a param, useful for local vars in tree
            U criteria = _visitor.Translate(expression);
            return _queryMethod(Manager, criteria);
        }

        public override string GetQueryText(Expression expression)
        {
            return "not available";
        }
    }
}
