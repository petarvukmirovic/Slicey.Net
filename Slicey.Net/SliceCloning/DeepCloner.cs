using Force.DeepCloner;

namespace Slicey.Net.SliceCloning
{
    internal class DeepCloner : ICloner
    {
        public T Clone<T>(T source) => source.DeepClone();
    }
}
