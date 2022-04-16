
using System.Collections;

namespace GRiPE.Code.Util
{
    public  class SerializedVector2Array : IEnumerable<Vector2>
    {
        float[] array = new float[0];
        public float[] BackingArray => array;

        public int Length => array.Length / 2;

        public Vector2 this[int index]
        {
            get => new Vector2(array[index * 2], array[index * 2 + 1]);
            set
            {
                array[index * 2] = value.X;
                array[index * 2 + 1] = value.Y;
            }
        }

        public void Add(Vector2 vector)
        {
            float[] newArr = new float[array.Length + 2];
            Array.Copy(array, 0, newArr, 0, array.Length);
            newArr[^2] = vector.X;
            newArr[^1] = vector.Y;
            array = newArr;
        }

        public void Remove(int index)
        {
            int actualIndex = index * 2;
            float[] newArr = new float[array.Length - 2];
            Array.Copy(array, 0, newArr, 0, actualIndex);
            Array.Copy(array, actualIndex + 2, newArr, actualIndex, array.Length - (actualIndex + 2));
            array = newArr;
        }

        public IEnumerator<Vector2> GetEnumerator()
        {
            for (int i = 0; i < array.Length / 2; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
