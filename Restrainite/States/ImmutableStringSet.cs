using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Restrainite.States;

public class ImmutableStringSet(IImmutableSet<string> immutableSet)
    : IImmutableSet<string>, IEquatable<ImmutableStringSet>
{
    public static readonly ImmutableStringSet Empty = new(ImmutableHashSet<string>.Empty);

    public bool Equals(ImmutableStringSet other)
    {
        return immutableSet.SetEquals(other);
    }

    public IEnumerator<string> GetEnumerator()
    {
        return immutableSet.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => immutableSet.Count;

    public IImmutableSet<string> Clear()
    {
        return immutableSet.Clear();
    }

    public bool Contains(string value)
    {
        return immutableSet.Contains(value);
    }

    public IImmutableSet<string> Add(string value)
    {
        return immutableSet.Add(value);
    }

    public IImmutableSet<string> Remove(string value)
    {
        return immutableSet.Remove(value);
    }

    public bool TryGetValue(string equalValue, out string actualValue)
    {
        return immutableSet.TryGetValue(equalValue, out actualValue);
    }

    public IImmutableSet<string> Intersect(IEnumerable<string> other)
    {
        return immutableSet.Intersect(other);
    }

    public IImmutableSet<string> Except(IEnumerable<string> other)
    {
        return immutableSet.Except(other);
    }

    public IImmutableSet<string> SymmetricExcept(IEnumerable<string> other)
    {
        return immutableSet.SymmetricExcept(other);
    }

    public IImmutableSet<string> Union(IEnumerable<string> other)
    {
        return immutableSet.Union(other);
    }

    public bool SetEquals(IEnumerable<string> other)
    {
        return immutableSet.SetEquals(other);
    }

    public bool IsProperSubsetOf(IEnumerable<string> other)
    {
        return immutableSet.IsProperSubsetOf(other);
    }

    public bool IsProperSupersetOf(IEnumerable<string> other)
    {
        return immutableSet.IsProperSupersetOf(other);
    }

    public bool IsSubsetOf(IEnumerable<string> other)
    {
        return immutableSet.IsSubsetOf(other);
    }

    public bool IsSupersetOf(IEnumerable<string> other)
    {
        return immutableSet.IsSupersetOf(other);
    }

    public bool Overlaps(IEnumerable<string> other)
    {
        return immutableSet.Overlaps(other);
    }

    public static implicit operator ImmutableStringSet(ImmutableHashSet<string> immutableHashSet)
    {
        return new ImmutableStringSet(immutableHashSet);
    }

    public static ImmutableStringSet From(string? immutableSet)
    {
        var splitArray = immutableSet?.Split(',') ?? [];
        return splitArray.Select(t => t.Trim())
            .Where(trimmed => trimmed.Length != 0)
            .ToImmutableHashSet();
    }

    public override string ToString()
    {
        return immutableSet.Aggregate<string, string>("",
            (prev, curr) => prev + (prev.Length > 0 ? "," : "") + curr);
    }
}