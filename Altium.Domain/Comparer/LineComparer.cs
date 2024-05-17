namespace Altium.Domain.Comparer
{
    public record LineComparer : IComparer<LineInfo>
    {
        public int Compare(LineInfo? x, LineInfo? y)
        {
            int lineComparison = string.Compare(x.StringPart, y.StringPart);

            if (lineComparison == 0)
            {
                return x.IntPart.CompareTo(y.IntPart);
            }

            return lineComparison;
        }
    }
}
