
using System.Collections;

namespace GRiPE.Code.Util
{
    public class SerializedVector2Array : FloatBackedArray<Vector2> //: IEnumerable<Vector2>
    {
        protected override int FloatWidth => 2;
        protected override Vector2 FromFloats(ReadOnlySpan<float> floats) => new Vector2(floats);
        protected override void ToFloats(Vector2 value, Span<float> floats) => value.CopyTo(floats);
        //float[] array = new float[0];
        //public float[] BackingArray => array;

        //public int Length => array.Length / 2;

        //public Vector2 this[int index]
        //{
        //    get => new(new ReadOnlySpan<float>(array, index * 2, 2));
        //    set => value.CopyTo(array, index * 2);
        //}

        //public void Add(Vector2 vector)
        //{
        //    Array.Resize(ref array, array.Length + 2);
        //    this[Length - 1] = vector;
        //}

        //public void Remove(int index)
        //{
        //    int actualIndex = index * 2;
        //    float[] newArr = new float[array.Length - 2];
        //    Array.Copy(array, 0, newArr, 0, actualIndex);
        //    Array.Copy(array, actualIndex + 2, newArr, actualIndex, array.Length - (actualIndex + 2));
        //    array = newArr;
        //}

        //public IEnumerator<Vector2> GetEnumerator()
        //{
        //    for (int i = 0; i < array.Length / 2; i++)
        //    {
        //        yield return this[i];
        //    }
        //}

        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    return this.GetEnumerator();
        //}
    }
}
