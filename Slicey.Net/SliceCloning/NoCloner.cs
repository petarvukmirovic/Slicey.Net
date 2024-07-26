namespace Slicey.Net.SliceCloning
{
    internal class NoCloner : ICloner
    {
        public T Clone<T>(T source) => source;
    }
}
