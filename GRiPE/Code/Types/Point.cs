
namespace GRiPE.Code.Types
{
    public struct Point
    {
        public Vector2 position;
        public Vector4 color;
        public float size;

        public override string ToString()
        {
            return $"[{position}] [{color}] [{size}]";
        }
    }

    public class PointArray : FloatBackedArray<Point>
    {
        public override int FloatWidth => 7;

        protected override Point FromFloats(ReadOnlySpan<float> floats)
        {
            return new Point
            {
                position = new Vector2(floats),
                color = new Vector4(floats.Slice(2, 4)),
                size = floats[6]
            };
        }

        protected override void ToFloats(Point value, Span<float> floats)
        {
            value.position.CopyTo(floats);
            value.color.CopyTo(floats.Slice(2, 4));
            floats[6] = value.size;
        }
    }
}
