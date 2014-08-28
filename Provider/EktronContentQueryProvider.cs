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
    internal class EktronContentQueryProvider : QueryProvider
    {

        ContentManager _cm;

        internal EktronContentQueryProvider()
        {
            //_cm = new ContentManager();
        }

        internal EktronContentQueryProvider(ApiAccessMode mode)
        {
           // _cm = new ContentManager(mode);
        }

        public override object Execute(Expression expression)
        {
            expression = Evaluator.PartialEval(expression); //evals anything w/o a param, useful for local vars in tree
            ContentCriteria cc = new EktronContentQueryTranslator().Translate(expression);
            return _cm.GetList(cc);
        }

        public override string GetQueryText(Expression expression)
        {
            return "not available";
        }
    }
}
