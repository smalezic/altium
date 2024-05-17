namespace Altium.Domain
{
    public record LineInfoReader : LineBase
    {
        public int ReaderId { get; init; }
    }
}
