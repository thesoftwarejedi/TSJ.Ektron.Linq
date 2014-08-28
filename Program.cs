using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSJ.Ektron.Linq
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var ctx = new EktronContext())
            {
                var list = ctx.Content.Where(a => a.XmlConfiguration.Id == 123123123 
                                                        && a.IsPublished == true)
                                      .OrderBy(a => a.DateCreated)
                                      .Take(5)
                                      .Skip(10);
                list.ToArray();
            }
        }
    }
}
