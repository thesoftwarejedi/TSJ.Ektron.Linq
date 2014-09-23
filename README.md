Tsj.Ektron.Linq
===============

A QueryProvider for the Ektron v9.1 CMS system

I wasn't sure about how Ektron would feel about me putting the binaries 
in github, so you'll have to create a "references" folder off the root and 
plop all the binaries in there for the HintPath to see them.  

This early proof of concept is to show how it would be possible to wrap the
Ektron API into .NET's QueryProvider syntax.  If you have ever used the Entity Framework
this usage should be familiar to you.  At this time, only "ContentData" is made available
but the framework is laid for other Manager objects from the Framework to be added
going forward.  I plan on adding them, and additional functionality, as needed.

The summary of EktronContext, which is our only public entry point, describes it best:

    /// <summary>
    /// This class is a work in progress.  Currently "skip" and "take" are 
    /// better thought of as "page" and "items per page" respectively
    /// 
    /// Where, OrderBy, OrderByDescending, Skip, and Take are the only methods translated to Ektron.
    /// Force execution via .AsEnumerable() before doing other work on the queryable
    /// 
    /// Debug thouroughly before using
    /// </summary>

Example:

            using (var ctx = new EktronContext())
            {
                                          //"a" here is a ContentData, and any of the properties
                                          //of ContentData can be used to build a query
                var list = ctx.Content.Where(a => a.XmlConfiguration.Id == 123123123 
                                                        && a.IsPublished == true)
                                      .OrderBy(a => a.DateCreated)
                                      .Take(5)
                                      .Skip(10);
                //to force the query to execute in ektron (perhaps to make unsupported
                //additional queries, groupings, etc)
                var executedList = list.AsEnumerable();
            }