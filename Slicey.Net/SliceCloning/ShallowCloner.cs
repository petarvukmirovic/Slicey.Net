using Force.DeepCloner;

namespace Slicey.Net.SliceCloning
{
    internal class ShallowCloner : ICloner
    {
        public T Clone<T>(T source) => source.ShallowClone();
    }
}
