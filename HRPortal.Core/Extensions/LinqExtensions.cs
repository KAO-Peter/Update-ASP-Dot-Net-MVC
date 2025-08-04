using LinqKit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

public static class LinqExtensions
{
    public static Expression<Func<T, bool>> OrLike<T>(this Expression<Func<T, bool>> source, string propertyName, object value)
    {
        Expression<Func<T, bool>> predicate = null;
        var linqParam = Expression.Parameter(typeof(T), propertyName);
        var linqProp = GetProperty<T>(linqParam, propertyName);
        switch (linqProp.Type.ToString())
        {
            case "System.Int32":
                predicate = source.Or(LikeLambdaDouble<T>(propertyName, Convert.ToDouble(value)));
                break;
            default:
                predicate = source.Or(LikeLambdaString<T>(propertyName, value.ToString()));
                break;
        }
        return predicate;
    }

    public static Expression<Func<T, bool>> LikeLambdaString<T>(string propertyName, string value)
    {
        var linqParam = Expression.Parameter(typeof(T), propertyName);
        var linqProp = GetProperty<T>(linqParam, propertyName);

        var containsFunc = Expression.Call(linqProp,
            typeof(string).GetMethod("Contains"),
            new Expression[] { Expression.Constant(value) });

        return Expression.Lambda<Func<T, bool>>(containsFunc,
                new ParameterExpression[] { linqParam });
    }

    public static Expression<Func<T, bool>> LikeLambdaDouble<T>(string propertyName, double? value)
    {
        string stringValue = (value == null) ? string.Empty : value.ToString();
        var linqParam = Expression.Parameter(typeof(T), propertyName);
        var linqProp = GetProperty<T>(linqParam, propertyName);

        var stringConvertMethodInfo =
            typeof(SqlFunctions).GetMethod("StringConvert", new Type[] { typeof(double?) });

        var stringContainsMethodInfo =
            typeof(String).GetMethod("Contains");

        return
            Expression.Lambda<Func<T, bool>>(
            Expression.Call(
                Expression.Call(
                    stringConvertMethodInfo,
                    Expression.Convert(
                        linqProp,
                        typeof(double?))),
                stringContainsMethodInfo,
                Expression.Constant(stringValue)),
            linqParam);
    }

    public static MemberExpression GetProperty<T>(ParameterExpression linqParam, string propertyName)
    {
        string[] propertyNames = propertyName.Split('.').ToArray();

        var linqProp = Expression.Property(linqParam, propertyNames[0]);

        for (int i = 1; i < propertyNames.Count(); i++)
        {
            linqProp = Expression.Property(linqProp, propertyNames[i]);
        }

        return linqProp;
    }

    private static MethodInfo containsMethod = typeof(string).GetMethod("Contains");
    private static MethodInfo startsWithMethod =
    typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) });
    private static MethodInfo endsWithMethod =
    typeof(string).GetMethod("EndsWith", new Type[] { typeof(string) });

    public static Expression<Func<T, bool>> GetExpression<T>(this Expression<Func<T, bool>> source, Op operation, string propertyName, object value)
    {
        ParameterExpression param = Expression.Parameter(typeof(T), "x");
        MemberExpression member = GetProperty<T>(param, propertyName);
        ConstantExpression constant;
        switch (member.Type.ToString())
        {
            case "System.Boolean":
                constant = Expression.Constant(Convert.ToBoolean(value));
                break;
            case "System.Int32":
                constant = Expression.Constant(Convert.ToInt32(value));
                break;
            case "System.Guid":
                constant = Expression.Constant(new Guid(value.ToString()));
                break;
            case "System.Nullable`1[System.Guid]":
                if (value.ToString() == "null")
                    constant = Expression.Constant(null, typeof(Guid?));
                else
                    constant = Expression.Constant(new Guid(value.ToString()), typeof(Guid?));
                break;
            default:
                constant = Expression.Constant(value);
                break;
        }
        Expression exp = null;
        switch (operation)
        {
            case Op.Equals:
                exp = Expression.Equal(member, constant);
                break;
            case Op.NotEqual:
                exp = Expression.NotEqual(member, constant);
                break;
            case Op.GreaterThan:
                exp = Expression.GreaterThan(member, constant);
                break;
            case Op.GreaterThanOrEqual:
                exp = Expression.GreaterThanOrEqual(member, constant);
                break;
            case Op.LessThan:
                exp = Expression.LessThan(member, constant);
                break;
            case Op.LessThanOrEqual:
                exp = Expression.LessThanOrEqual(member, constant);
                break;
            case Op.Contains:
                exp = Expression.Call(member, containsMethod, constant);
                break;
            case Op.StartsWith:
                exp = Expression.Call(member, startsWithMethod, constant);
                break;
            case Op.EndsWith:
                exp = Expression.Call(member, endsWithMethod, constant);
                break;
        }

        return Expression.Lambda<Func<T, bool>>(exp, param);
    }

    public enum Op
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