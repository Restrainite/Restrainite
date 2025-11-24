using System.Collections.ObjectModel;
using FrooxEngine;
using FrooxEngine.CommonAvatar;

namespace Restrainite.RestrictionTypes.Base;

public class AvatarAnchorParameter : IRestrictionParameter
{
    private SimpleState<AnchorList> AvatarAnchors { get; } = new(AnchorList.Empty);

    public bool Combine(IRestriction restriction, IEnumerable<IBaseState> states)
    {
        List<AvatarAnchor> anchors = [];
        foreach (var baseState in states)
        {
            if (baseState is not LocalBaseState<AvatarAnchor?> localState) continue;
            if (localState.Value == null) continue;
            if (anchors.Contains(localState.Value)) continue;
            anchors.Add(localState.Value);
        }

        anchors.Sort((a, b) => a.ReferenceID.CompareTo(b.ReferenceID));

        return AvatarAnchors.SetIfChanged(restriction, new AnchorList(anchors.AsReadOnly()));
    }

    public IBaseState CreateLocalState(DynamicVariableSpace dynamicVariableSpace,
        IDynamicVariableSpace dynamicVariableSpaceSync,
        IRestriction restriction)
    {
        return new LocalBaseState<AvatarAnchor>(null!, dynamicVariableSpace, dynamicVariableSpaceSync,
            restriction, false);
    }

    public void CreateStatusComponent(IRestriction restriction, Slot slot, string dynamicVariableSpaceName)
    {
        BaseRestriction.CreateStatusRefComponent<AnchorList, AvatarAnchor>(
            restriction, slot, dynamicVariableSpaceName, AvatarAnchors,
            a => a.GetRandomAnchor(slot.World)!
        );
    }

    public bool Contains(AvatarAnchor anchor)
    {
        return AvatarAnchors.Value.Anchors.Count == 0 || AvatarAnchors.Value.Anchors.Contains(anchor);
    }

    public AvatarAnchor? GetRandomAnchor(World world)
    {
        return AvatarAnchors.Value.GetRandomAnchor(world);
    }
}

internal sealed class AnchorList(ReadOnlyCollection<AvatarAnchor> anchors) : IEquatable<AnchorList>
{
    internal static readonly AnchorList Empty = new(new ReadOnlyCollection<AvatarAnchor>([]));

    private static readonly Random Random = new();

    internal ReadOnlyCollection<AvatarAnchor> Anchors { get; } = anchors;

    public bool Equals(AnchorList? other)
    {
        return other is not null && (ReferenceEquals(this, other) || Anchors.SequenceEqual(other.Anchors));
    }

    public override bool Equals(object? obj)
    {
        return obj is AnchorList objS && Equals(objS);
    }

    public override int GetHashCode()
    {
        return Anchors.GetHashCode();
    }

    public AvatarAnchor? GetRandomAnchor(World world)
    {
        if (Anchors.Count == 0) return null;
        var validAnchors = Anchors.Where(anchor => anchor.World == world).ToList();
        return validAnchors.Count == 0
            ? null
            : validAnchors[Random.Next(validAnchors.Count)];
    }
}