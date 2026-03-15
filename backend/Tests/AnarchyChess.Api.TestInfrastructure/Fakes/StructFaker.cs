using Bogus;
using System.Linq.Expressions;
using System.Reflection;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public delegate void StructRule<T>(Faker faker, ref T value);

public delegate void StructSetter<T, TProp>(ref T obj, TProp value);

public class StructFaker<T>
    where T : struct
{
    private static readonly Dictionary<MemberInfo, Delegate> _setterCache = [];
    private static readonly MemberInfo[] _allMembers =
    [
        .. typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance).Cast<MemberInfo>(),
        .. typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance),
    ];

    private readonly Dictionary<MemberInfo, StructRule<T>> _rules = [];
    private bool _strictMode = false;

    public StructFaker<T> StrictMode(bool strictMode)
    {
        _strictMode = strictMode;
        return this;
    }

    private static StructSetter<T, TProp> CreateSetter<TProp>(MemberInfo member)
    {
        var obj = Expression.Parameter(typeof(T).MakeByRefType(), "obj");
        var value = Expression.Parameter(typeof(TProp), "value");

        Expression memberAccess = member switch
        {
            PropertyInfo p => Expression.Property(obj, p),
            FieldInfo f => Expression.Field(obj, f),
            _ => throw new NotSupportedException(),
        };

        Expression valueToAssign = value;
        if (value.Type != memberAccess.Type)
        {
            valueToAssign = Expression.Convert(value, memberAccess.Type);
        }

        Expression body = Expression.Assign(memberAccess, valueToAssign);
        return Expression.Lambda<StructSetter<T, TProp>>(body, obj, value).Compile();
    }

    public StructFaker<T> RuleFor<TProp>(
        Expression<Func<T, TProp>> property,
        Func<Faker, T, TProp> valueFactory
    )
    {
        MemberExpression memberExpr = property.Body switch
        {
            MemberExpression m => m,
            UnaryExpression u
                when u.NodeType == ExpressionType.Convert && u.Operand is MemberExpression m => m,
            _ => throw new ArgumentException("Expression must be a member access"),
        };
        MemberInfo member = memberExpr.Member;

        if (!_setterCache.TryGetValue(member, out var del))
        {
            del = CreateSetter<TProp>(member);
            _setterCache[member] = del;
        }

        var setter = (StructSetter<T, TProp>)del;
        _rules[member] = (f, ref obj) =>
        {
            var value = valueFactory(f, obj);
            setter(ref obj, value);
        };

        return this;
    }

    public StructFaker<T> RuleFor<TProperty>(
        Expression<Func<T, TProperty>> property,
        Func<Faker, TProperty> setter
    ) => RuleFor(property, (f, _) => setter(f));

    public StructFaker<T> RuleFor<TProperty>(
        Expression<Func<T, TProperty>> property,
        TProperty value
    ) => RuleFor(property, (_, _) => value);

    public T Generate()
    {
        ValidateStrictMode();

        Faker faker = new();

        T obj = new();
        foreach (var rule in _rules.Values)
        {
            rule(faker, ref obj);
        }
        return obj;
    }

    public List<T> Generate(int count)
    {
        List<T> result = new(count);
        for (int i = 0; i < count; i++)
        {
            result.Add(Generate());
        }
        return result;
    }

    private void ValidateStrictMode()
    {
        if (!_strictMode)
        {
            return;
        }

        MemberInfo[] missing = [.. _allMembers.Except(_rules.Keys)];
        if (missing.Length > 0)
        {
            throw new InvalidOperationException(
                $"No rules defined as required by strict mode for {string.Join(", ", missing.Select(m => m.Name))}"
            );
        }
    }
}
