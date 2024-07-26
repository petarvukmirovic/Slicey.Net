namespace Slicey.Net.SliceCloning
{
    internal interface ICloner
    {
        public T Clone<T>(T source);
    }
}
