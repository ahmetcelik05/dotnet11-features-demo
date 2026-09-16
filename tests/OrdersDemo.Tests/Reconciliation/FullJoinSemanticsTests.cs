namespace OrdersDemo.Tests.Reconciliation;

/// <summary>Documents observed behaviour of the .NET 11 join operators that the article relies on.</summary>
public sealed class FullJoinSemanticsTests
{
    [Fact]
    public void Observed_order_is_outer_sequence_first_then_unmatched_inner()
    {
        int[] outer = [1, 2, 3];
        int[] inner = [3, 4, 2];

        var pairs = outer.FullJoin(inner, o => o, i => i).ToArray();

        // Matches follow the outer order; inner-only rows are appended at the end.
        Assert.Equal([(1, 0), (2, 2), (3, 3), (0, 4)], pairs);
    }

    [Fact]
    public void Value_type_default_is_indistinguishable_from_a_real_zero()
    {
        // (1, 0): is 0 "no match" or an actual inner value of 0? Use nullable or reference types,
        // or the resultSelector overload, when that distinction matters.
        int[] outer = [1];
        int[] inner = [0];

        var pairs = outer.FullJoin(inner, o => o, i => i).ToArray();

        Assert.Equal([(1, 0), (0, 0)], pairs);
    }

    [Fact]
    public void Null_keys_never_match_each_other()
    {
        (string? Key, string Name)[] outer = [(null, "outer-null")];
        (string? Key, string Name)[] inner = [(null, "inner-null")];

        var pairs = outer.FullJoin(inner, o => o.Key, i => i.Key).ToArray();

        Assert.Equal(2, pairs.Length);
        Assert.All(pairs, pair => Assert.True(pair.Outer == default || pair.Inner == default));
    }

    [Fact]
    public void Selector_less_GroupJoin_returns_groupings_keyed_by_the_outer_element()
    {
        (int Id, string Name)[] customers = [(1, "Ada"), (2, "Grace")];
        (int CustomerId, decimal Total)[] orders = [(1, 10m), (1, 20m)];

        var grouped = customers
            .GroupJoin(orders, c => c.Id, o => o.CustomerId)
            .Select(group => (group.Key.Name, Count: group.Count()))
            .ToArray();

        Assert.Equal([("Ada", 2), ("Grace", 0)], grouped);
    }

    [Fact]
    public void Selector_less_Join_returns_tuples()
    {
        (int Id, string Name)[] customers = [(1, "Ada")];
        (int CustomerId, decimal Total)[] orders = [(1, 10m), (1, 20m)];

        var joined = customers.Join(orders, c => c.Id, o => o.CustomerId).ToArray();

        Assert.Equal([((1, "Ada"), (1, 10m)), ((1, "Ada"), (1, 20m))], joined);
    }
}
