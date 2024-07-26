namespace Slicey.Net.SliceCloning
{
    public enum CloningLevel
    {
        NoCloning,
        ShallowCloning,
        DeepCloning
    }

    internal static class CloningLevelExtensions
    {
        internal static ICloner ToCloner(this CloningLevel cloningLevel) => cloningLevel switch
        {
            CloningLevel.NoCloning => new NoCloner(),
            CloningLevel.ShallowCloning => new ShallowCloner(),
            CloningLevel.DeepCloning => new DeepCloner(),
            _ => throw new NotSupportedException()
        };
    }
}
