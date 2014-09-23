using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ektron.Cms;
using TSJ.Ektron.Linq.Provider;
using Ektron.Cms.Framework;
using TSJ.Ektron.Linq.QueryWrapper;

namespace TSJ.Ektron.Linq
{
        /// <summary>
    /// This class is a work in progress.  Currently "skip" and "take" are 
    /// better thought of as "page" and "items per page" respectively
    /// 
    /// Where, OrderBy, OrderByDescending, Skip, and Take are the only methods translated to Ektron.
    /// Force execution via .AsEnumerable() before doing other work on the queryable
    /// 
    /// Debug thouroughly before using
    /// </summary>
    public class EktronContext : IDisposable
    {
        private EktronContentQueryProvider _qp;

        public Query<ContentData> Content { get; private set; }

        public EktronContext()
        {
            _qp = new EktronContentQueryProvider();
            this.Content = new Query<ContentData>(_qp);
        }

        public EktronContext(ApiAccessMode mode)
        {
            _qp = new EktronContentQueryProvider(mode);
            this.Content = new Query<ContentData>(_qp);
        }

        public void Dispose()
        {
            //nothing for now
        }
    }
}
