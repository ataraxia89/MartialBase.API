// <copyright file="PredicateBuilder.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Tools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Linq.Expressions;

namespace MartialBase.API.Tools
{
    public static class PredicateBuilder
    {
        public static Expression<Func<T, bool>> True<T>() => f => true;

        public static Expression<Func<T, bool>> False<T>() => f => false;

        public static Expression<Func<T, bool>> Or<T>(
            this Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            InvocationExpression invokedExpr = Expression.Invoke(expr2, expr1.Parameters);

            return Expression.Lambda<Func<T, bool>>(
                Expression.OrElse(expr1.Body, invokedExpr),
                expr1.Parameters);
        }

        public static Expression<Func<T, bool>> And<T>(
            this Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            InvocationExpression invokedExpr = Expression.Invoke(expr2, expr1.Parameters);

            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
        }
    }
}
