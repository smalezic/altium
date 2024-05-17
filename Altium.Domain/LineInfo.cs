namespace Altium.Domain
{
    public record LineInfo : LineBase
    {
        public int IntPart { get; init; }
        public string StringPart { get; init; } = string.Empty;
    }
}
