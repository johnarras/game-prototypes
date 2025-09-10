namespace Genrpg.Shared.Effects.Interfaces
{
    public interface IEffect
    {
        public long EntityTypeId { get; set; }

        public long Quantity { get; set; }

        public long EntityId { get; set; }
    }
}
