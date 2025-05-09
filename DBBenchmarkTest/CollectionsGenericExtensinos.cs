public static class CollectionsGenericExtensinos
{
    public static T Percentile<T>(this List<T> list, float percentile)
    {
        list.Sort();
        int index = (int)Math.Ceiling(percentile * list.Count) - 1;
        index = Math.Min(Math.Max(index, 0), list.Count - 1);
        return list[index];
    }
}
