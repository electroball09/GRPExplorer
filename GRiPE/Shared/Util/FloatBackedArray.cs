
using System.Collections;

namespace GRiPE.Code.Util
{
    public abstract class FloatBackedArray<T> : IEnumerable<T>
    {
        public float[] FloatArray => array;
        public int Length => numItems;

        private float[] array;
        private int numItems = 0;

        protected abstract int FloatWidth { get; }
        protected abstract T FromFloats(ReadOnlySpan<float> floats);
        protected abstract void ToFloats(T value, Span<float> floats);

        public FloatBackedArray()
        {
            array = new float[FloatWidth * 4];
        }

        public T this[int index]
        {
            get => FromFloats(new ReadOnlySpan<float>(array, index * FloatWidth, FloatWidth));
            set => ToFloats(value, new Span<float>(array, index * FloatWidth, FloatWidth));
        }

        public void Add(T item)
        {
            numItems++;
            if (array.Length < numItems * FloatWidth)
            {
                Array.Resize(ref array, array.Length * 2);
            }
        }

        public void Remove(int index)
        {
            int actualIndex = index * FloatWidth;
            Array.Copy(array, actualIndex + FloatWidth, array, actualIndex, array.Length - (actualIndex + FloatWidth));
            numItems--;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < numItems; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
