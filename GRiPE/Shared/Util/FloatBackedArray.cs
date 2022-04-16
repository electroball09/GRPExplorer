
using System.Collections;

namespace GRiPE.Code.Util
{
    public delegate void Modifier<T>(ref T item);

    public abstract class FloatBackedArray<T> : IEnumerable<T>
    {
        public float[] FloatArray => array;
        public int Length => numItems;

        public int ArrayResizeModifier => FloatWidth * 4;

        private float[] array;
        private int numItems = 0;

        public abstract int FloatWidth { get; }
        protected abstract T FromFloats(ReadOnlySpan<float> floats);
        protected abstract void ToFloats(T value, Span<float> floats);

        public FloatBackedArray()
        {
            array = new float[ArrayResizeModifier];
        }

        public T this[int index]
        {
            get => FromFloats(new ReadOnlySpan<float>(array, index * FloatWidth, FloatWidth));
            set => ToFloats(value, new Span<float>(array, index * FloatWidth, FloatWidth));
        }

        public void Add(T item)
        {
            if (array.Length < (numItems + 1) * FloatWidth)
            {
                Array.Resize(ref array, array.Length + ArrayResizeModifier);
            }
            this[numItems++] = item;
        }

        public void Remove(int index)
        {
            int actualIndex = index * FloatWidth;
            Array.Copy(array, actualIndex + FloatWidth, array, actualIndex, array.Length - (actualIndex + FloatWidth));
            numItems--;
        }

        public void Modify(int index, Modifier<T> modifier)
        {
            T value = this[index];
            modifier(ref value);
            this[index] = value;
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
