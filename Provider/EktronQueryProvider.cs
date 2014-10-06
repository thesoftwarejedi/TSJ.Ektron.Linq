using Ektron.Cms.Common;
using Ektron.Cms.Content;
using Ektron.Cms.Framework;
using Ektron.Cms.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TSJ.Ektron.Linq.QueryWrapper;

namespace TSJ.Ektron.Linq.Provider
{
    internal class EktronQueryProvider<T, U, V> : QueryProvider where T : CmsApi<T>, new()
                                                                where U : Criteria<V>, new()
    {

        protected T Manager { get; private set; }
        private Func<T, U, object> _queryMethod;

        internal EktronQueryProvider(Func<T, U, object> queryMethod, ApiAccessMode mode = ApiAccessMode.LoggedInUser, int contentLanguage = int.MinValue)
        {
            _queryMethod = queryMethod;
            Manager = new T();
            Manager.ApiMode = mode;
            if (contentLanguage != int.MinValue)
                Manager.ContentLanguage = contentLanguage;
        }

        public override object Execute(Expression expression)
        {
            expression = Evaluator.PartialEval(expression); //evals anything w/o a param, useful for local vars in tree
            U criteria = new EktronExpressionVisitor<U, V>().Translate(expression);
            return _queryMethod(Manager, criteria);
        }

        public override string GetQueryText(Expression expression)
        {
            return "not available";
        }
    }
}
