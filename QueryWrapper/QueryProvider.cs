﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

/**
    blatantly 'borrowed' from:
    http://blogs.msdn.com/b/mattwar/archive/2007/07/30/linq-building-an-iqueryable-provider-part-i.aspx

    provides a simplified starting point for query provider creation by handling most of the plumbing

    complete blog text posted in qeury.cs in case web version is gone
    */
namespace TSJ.Ektron.Linq.QueryWrapper
{
    internal abstract class QueryProvider : IQueryProvider
    {
        protected QueryProvider()
        {
        }

        IQueryable<S> IQueryProvider.CreateQuery<S>(Expression expression)
        {
            return new Query<S>(this, expression);
        }

        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {
            Type elementType = TypeSystem.GetElementType(expression.Type);
            try
            {
                return (IQueryable)Activator.CreateInstance(typeof(Query<>).MakeGenericType(elementType), new object[] { this, expression });
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }

        S IQueryProvider.Execute<S>(Expression expression)
        {
            return (S)this.Execute(expression);
        }

        object IQueryProvider.Execute(Expression expression)
        {
            return this.Execute(expression);
        }

        public abstract string GetQueryText(Expression expression);
        public abstract object Execute(Expression expression);
    }
}
