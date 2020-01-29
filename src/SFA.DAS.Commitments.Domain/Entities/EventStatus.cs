namespace SFA.DAS.Commitments.Domain.Entities
{
    public enum EventStatus : byte
    {
        None = 0,
        New = 1,
        Updated = 2,
        Removed = 3
    }
}