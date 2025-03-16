namespace Lapo.Mud.Comparers;

public class CustomOperationComparer : IComparer<object>
{
    static readonly Dictionary<string, int> OperationOrder = new()
    {
        { "ADDED", 1 },
        { "REMOVED", 2 },
        { "UPDATED_OLD", 3 },
        { "UPDATED_NEW", 4 }
    };

    public int Compare(object? x, object? y)
    {
        if (x is string xStr && y is string yStr)
        {
            return OperationOrder[xStr].CompareTo(OperationOrder[yStr]);
        }
        return 0;
    }
}