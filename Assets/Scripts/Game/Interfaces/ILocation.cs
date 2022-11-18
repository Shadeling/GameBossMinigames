

namespace MyGame
{

    public enum LocationType
    {
        None = -1,
        ResourceLocation = 0,
        Shop = 1,
        Boss = 2,

    }

    public interface ILocation : IVisualizable
    {
        public LocationType Type { get; }

        public int LocationLevel { get; }

        public float LocationFrequency { get; } //Вес, с которым локации появляются: чем больше вес, тем больше шанс
    }
}
