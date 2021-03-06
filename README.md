Tsj.Ektron.Linq
===============

A QueryProvider for the Ektron v9.1 CMS system

I wasn't sure about how Ektron would feel about me putting the binaries 
in github, so you'll have to create a "references" folder off the root and 
plop all the binaries in there for the HintPath to see them.  

Quick example usage:

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


This early proof of concept is to show how it would be possible to wrap the
Ektron API into .NET's QueryProvider syntax.  This framework accepts an expression created
using simple lambda syntax and parses it into Criteria objects and calls to the Manager set
of APIs as appropriate.  If you have ever used the Entity Framework
this usage should be familiar to you.  The code currently blindly smashes together
The Property enums and Data classes hoping that the enum name matches a Data property.
Obviously a better way needs to be done - but this works for many situations, and
so I release it as an open idea.

NOTE: the "better way" to do this will be tracked using issue #1 in github. https://github.com/thesoftwarejedi/TSJ.Ektron.Linq

DEVELOPERS: 
- If you are cracking open this code, it'll be tough to follow from the entry to the real logic of the code.  The real code after all the glue code is located here: https://github.com/thesoftwarejedi/TSJ.Ektron.Linq/blob/master/Provider/EktronExpressionVisitor.cs#L113  That is where the expression is converted to an EktronCriteria.  Also, the line below is there the enum is parsed from the Data method - the pivotal assumption of the entire library.
- If you are tracking down an error, there is a 99% chance it is from this line: https://github.com/thesoftwarejedi/TSJ.Ektron.Linq/blob/master/Provider/EktronExpressionVisitor.cs#L172  That is where the bad assumption is made described above as needing a better way.

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
