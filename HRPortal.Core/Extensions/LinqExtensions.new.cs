using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using LinqKit;
using Microsoft.EntityFrameworkCore.SqlServer;

namespace HRPortal.Core.Extensions;

/// <summary>
/// 提供 LINQ 查詢的擴展方法
/// </summary>
public static class LinqExtensions
{
    private static readonly MethodInfo ContainsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
    private static readonly MethodInfo StartsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) })!;
    private static readonly MethodInfo EndsWithMethod = typeof(string).GetMethod("EndsWith", new[] { typeof(string) })!;

    /// <summary>
    /// 添加 OR Like 條件到現有表達式
    /// </summary>
    public static Expression<Func<T, bool>> OrLike<T>(
        this Expression<Func<T, bool>> source,
        string propertyName,
        object value)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(propertyName);
        ArgumentNullException.ThrowIfNull(value);

        var param = Expression.Parameter(typeof(T), propertyName);
        var property = GetProperty<T>(param, propertyName);

        return property.Type.Name switch
        {
            nameof(Int32) => source.Or(CreateNumericLikePredicate<T>(propertyName, Convert.ToDouble(value))),
            nameof(Double) => source.Or(CreateNumericLikePredicate<T>(propertyName, Convert.ToDouble(value))),
            nameof(String) => source.Or(CreateStringLikePredicate<T>(propertyName, value.ToString()!)),
            _ => throw new ArgumentException($"Unsupported property type: {property.Type.Name}")
        };
    }

    /// <summary>
    /// 創建字符串Like查詢表達式
    /// </summary>
    public static Expression<Func<T, bool>> CreateStringLikePredicate<T>(string propertyName, string value)
    {
        var param = Expression.Parameter(typeof(T), propertyName);
        var property = GetProperty<T>(param, propertyName);

        var containsExpression = Expression.Call(
            property,
            ContainsMethod,
            Expression.Constant(value));

        return Expression.Lambda<Func<T, bool>>(containsExpression, param);
    }

    /// <summary>
    /// 創建數值Like查詢表達式
    /// </summary>
    public static Expression<Func<T, bool>> CreateNumericLikePredicate<T>(string propertyName, double? value)
    {
        var stringValue = value?.ToString() ?? string.Empty;
        var param = Expression.Parameter(typeof(T), propertyName);
        var property = GetProperty<T>(param, propertyName);

        // 在 EF Core 中使用 EF.Functions.ToString 替代 SqlFunctions
        var toStringMethod = typeof(EF.Functions)
            .GetMethod("ToString", new[] { typeof(double?) })!;

        var contains = Expression.Call(
            Expression.Call(null, toStringMethod, 
                Expression.Convert(property, typeof(double?))),
            ContainsMethod,
            Expression.Constant(stringValue));

        return Expression.Lambda<Func<T, bool>>(contains, param);
    }

    /// <summary>
    /// 獲取屬性表達式，支援巢狀屬性
    /// </summary>
    public static MemberExpression GetProperty<T>(ParameterExpression param, string propertyPath)
    {
        return propertyPath.Split('.')
            .Aggregate((MemberExpression)Expression.Property(param, propertyPath.Split('.')[0]),
                (current, propertyName) => Expression.Property(current, propertyName));
    }

    /// <summary>
    /// 根據操作符創建查詢表達式
    /// </summary>
    public static Expression<Func<T, bool>> GetExpression<T>(
        this Expression<Func<T, bool>> source,
        ComparisonOperator operation,
        string propertyName,
        object value)
    {
        ArgumentNullException.ThrowIfNull(source);
        
        var param = Expression.Parameter(typeof(T), "x");
        var member = GetProperty<T>(param, propertyName);
        var constant = CreateTypedConstant(member.Type, value);

        var expression = operation switch
        {
            ComparisonOperator.Equals => Expression.Equal(member, constant),
            ComparisonOperator.NotEqual => Expression.NotEqual(member, constant),
            ComparisonOperator.GreaterThan => Expression.GreaterThan(member, constant),
            ComparisonOperator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(member, constant),
            ComparisonOperator.LessThan => Expression.LessThan(member, constant),
            ComparisonOperator.LessThanOrEqual => Expression.LessThanOrEqual(member, constant),
            ComparisonOperator.Contains => Expression.Call(member, ContainsMethod, constant),
            ComparisonOperator.StartsWith => Expression.Call(member, StartsWithMethod, constant),
            ComparisonOperator.EndsWith => Expression.Call(member, EndsWithMethod, constant),
            _ => throw new ArgumentException($"Unsupported operation: {operation}")
        };

        return Expression.Lambda<Func<T, bool>>(expression, param);
    }

    private static ConstantExpression CreateTypedConstant(Type memberType, object value)
    {
        if (value == null)
        {
            return Expression.Constant(null, memberType);
        }

        if (memberType == typeof(bool) || memberType == typeof(bool?))
            return Expression.Constant(Convert.ToBoolean(value), memberType);
        
        if (memberType == typeof(int) || memberType == typeof(int?))
            return Expression.Constant(Convert.ToInt32(value), memberType);
        
        if (memberType == typeof(Guid) || memberType == typeof(Guid?))
        {
            if (value.ToString() == "null")
                return Expression.Constant(null, typeof(Guid?));
            return Expression.Constant(Guid.Parse(value.ToString()!), memberType);
        }

        return Expression.Constant(value, memberType);
    }

    /// <summary>
    /// 比較運算符
    /// </summary>
    public enum ComparisonOperator
    {
        Equals,
        NotEqual,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual,
        Contains,
        StartsWith,
        EndsWith
    }
}
