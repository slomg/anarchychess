using AnarchyChess.Api.Shared.Models;
using AwesomeAssertions;

namespace AnarchyChess.Api.Unit.Tests;

public class BoundedSetTests
{
    [Fact]
    public void TryAdd_adds_item_when_set_is_empty()
    {
        BoundedSet<int> set = new(3);
        bool result = set.TryAdd(1);

        result.Should().BeTrue();
        set.Has(1).Should().BeTrue();
    }

    [Fact]
    public void TryAdd_returns_false_when_item_already_exists_in_set()
    {
        BoundedSet<int> set = new(3);
        set.TryAdd(1);

        bool result = set.TryAdd(1);

        result.Should().BeFalse();
    }

    [Fact]
    public void TryAdd_removes_oldest_item_when_max_size_exceeded()
    {
        BoundedSet<int> set = new(3);
        set.TryAdd(1);
        set.TryAdd(2);
        set.TryAdd(3);

        set.TryAdd(4);

        set.Has(1).Should().BeFalse();
        set.Has(2).Should().BeTrue();
        set.Has(3).Should().BeTrue();
        set.Has(4).Should().BeTrue();
    }

    [Fact]
    public void Has_returns_false_for_item_not_in_set()
    {
        BoundedSet<string> set = new(2);

        set.Has("hello").Should().BeFalse();
    }

    [Fact]
    public void BoundedSet_maintains_order_when_items_removed_due_to_max_size()
    {
        BoundedSet<int> set = new(2);
        set.TryAdd(10);
        set.TryAdd(20);
        set.TryAdd(30);

        set.Has(10).Should().BeFalse();
        set.Has(20).Should().BeTrue();
        set.Has(30).Should().BeTrue();
    }

    [Fact]
    public void TryAdd_can_add_multiple_items_up_to_max_size()
    {
        BoundedSet<int> set = new(3);
        set.TryAdd(1).Should().BeTrue();
        set.TryAdd(2).Should().BeTrue();
        set.TryAdd(3).Should().BeTrue();

        set.Has(1).Should().BeTrue();
        set.Has(2).Should().BeTrue();
        set.Has(3).Should().BeTrue();
    }
}
