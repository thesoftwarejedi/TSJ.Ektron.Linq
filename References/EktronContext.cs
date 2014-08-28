using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ektron.Cms;
using R2i.Ektron.Linq.Provider;
using R2Integrated.Utilities.Linq;

namespace R2i.Ektron.Linq
{
    public class EktronContext
    {
        private EktronQueryProvider _qp = new EktronQueryProvider();

        public Query<ContentData> Content { get; private set; }

        public EktronContext()
        {
            this.Content = new Query<ContentData>(_qp);
        }

        public Query<T> GetSmartForm<T>() where T : new()
        {
            return new Query<T>(_qp);
        }

    }
}
