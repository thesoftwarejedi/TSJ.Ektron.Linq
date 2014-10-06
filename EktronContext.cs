using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ektron.Cms;
using TSJ.Ektron.Linq.Provider;
using Ektron.Cms.Framework;
using TSJ.Ektron.Linq.QueryWrapper;
using Ektron.Cms.Framework.Content;
using Ektron.Cms.Content;
using Ektron.Cms.Framework.Organization;
using Ektron.Cms.Framework.Calendar;
using Ektron.Cms.Organization;
using Ektron.Cms.Common.Calendar;
using Ektron.Cms.Settings.UrlAliasing.DataObjects;
using Ektron.Cms.Common;
using DumbThingIsAliasedNowMenuData = Ektron.Cms.Organization.MenuData;
using Ektron.Cms.Framework.Settings.UrlAliasing;
using Ektron.Cms.Framework.User;
using Ektron.Cms.User;

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

        //Would have been funnier for this to search for all sets of classes matching the standard suffix and generate this
        public Query<ContentAssetData> Assets { get; private set; }
        public Query<ContentData> Content { get; private set; }
        public Query<ContentCollectionData> Collections { get; private set; }
        public Query<LibraryData> Library { get; private set; }
        public Query<WebCalendarData> WebCalenders { get; private set; }
        public Query<WebEventData> WebEvents { get; private set; }
        public Query<FolderData> Folders { get; private set; }
        public Query<DumbThingIsAliasedNowMenuData> Menus { get; private set; }
        public Query<TaxonomyData> Taxonomy { get; private set; }
        public Query<AliasData> Aliases { get; private set; }
        public Query<UserData> Users { get; private set; }
        public Query<UserGroupData> UserGroups { get; private set; }

        private ApiAccessMode _mode;
        private int _contentLanguage;

        public EktronContext(ApiAccessMode mode = ApiAccessMode.LoggedInUser, int contentLanguage = int.MinValue)
        {
            //pasting this signature below because at this point of development my intellisense
            //stopped working and this stuff has become incredibly deep with abstraction which 
            //suprises me because it means that 90% of the code I've ended up writing is merely 
            //glue code to attach the 3 generic types that this class is comprised of with a 
            //supplied function, a little expressionvisitor, and a model type

            //Func<T, U, object> queryMethod, ApiAccessMode mode = ApiAccessMode.LoggedInUser, int contentLanguage = int.MinValue

            _mode = mode;
            _contentLanguage = contentLanguage;

            this.Assets = Glue<AssetManager, AssetCriteria, AssetProperty, ContentAssetData>((a, b) => a.GetList(b));
            //content has a custom visitor for the smartformid
            this.Content = Glue<ContentManager, ContentCriteria, ContentProperty, ContentData>((a, b) => a.GetList(b), new EktronContentExpressionVisitor());
            this.Collections = Glue<CollectionManager, CollectionCriteria, ContentCollectionProperty, ContentCollectionData>((a, b) => a.GetList(b));
            this.Library = Glue<LibraryManager, LibraryCriteria, LibraryProperty, LibraryData>((a, b) => a.GetList(b));
            this.WebCalenders = Glue<WebCalendarManager, WebCalendarCriteria, WebCalendarProperty, WebCalendarData>((a, b) => a.GetList(b));
            this.WebEvents = Glue<WebEventManager, WebEventCriteria, EventProperty, WebEventData>((a, b) => a.GetList(b));
            this.Folders = Glue<FolderManager, FolderCriteria, FolderProperty, FolderData>((a, b) => a.GetList(b));
            this.Menus = Glue<MenuManager, MenuCriteria, MenuProperty, DumbThingIsAliasedNowMenuData>((a, b) => a.GetMenuList(b)); //yeah, gotta be different
            this.Taxonomy = Glue<TaxonomyManager, TaxonomyCriteria, TaxonomyProperty, TaxonomyData>((a, b) => a.GetList(b));
            this.Aliases = Glue<AliasManager, AliasCriteria, AliasProperty, AliasData>((a, b) => a.GetList(b));
            this.Users = Glue<UserManager, UserCriteria, UserProperty, UserData>((a, b) => a.GetList(b));
        }

        /// <summary>
        /// Only once in my career have I needed a glue method, and I can't remember why.  I know this will give me
        /// a headache later so it's getting documented right here on the glue method
        /// </summary>
        /// <typeparam name="T">The Ektron Manager API class</typeparam>
        /// <typeparam name="U">The Criteria class used to query that manager</typeparam>
        /// <typeparam name="V">The Property class used within the criteria to query the manager</typeparam>
        /// <typeparam name="W">The type contained in the enumeration returned</typeparam>
        /// <param name="queryMethod">A lambda which gives you the Manager (T), and the criteria (U) and allows you to produce a Query&lt;W&gt; (call getlist?)</param>
        /// <returns></returns>
        private Query<W> Glue<T, U, V, W>(Func<T, U, object> queryMethod)
            where T : CmsApi<T>, new()
            where U : Criteria<V>, new()
        {
            return new Query<W>(new EktronQueryProvider<T, U, V>(queryMethod, _mode, _contentLanguage));
        }

        //went a step farther - try invoking T.GetList(U) - lolol I have no clue why I find this funny
        //perhaps it's because the damn GetList method isn't on an interface or on CmsApi
        //or maybe because I didn't bother using it.  
        private Query<W> Glue<T, U, V, W>()
            where T : CmsApi<T>, new()
            where U : Criteria<V>, new()
        {
            return Glue<T, U, V, W>((a,b) => typeof(T).GetMethod("GetList", new Type[] { typeof(U) }).Invoke(a, new[] {b}));
        }
        private Query<W> Glue<T, U, V, W>(Func<T, U, object> queryMethod, EktronExpressionVisitor<U, V> visitor)
            where T : CmsApi<T>, new()
            where U : Criteria<V>, new()
        {
            return new Query<W>(new EktronQueryProvider<T, U, V>(queryMethod, visitor, _mode, _contentLanguage));
        }

        public void Dispose()
        {
            //nothing for now
        }
    }
}
