using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GRiPE.Code.Renderer
{
    public struct Line
    {
        public Vector2 start;
        public Vector2 end;
        public Vector4 color;
    }

    public class LineArray : FloatBackedArray<Line>
    {
        public override int FloatWidth => 8;

        protected override Line FromFloats(ReadOnlySpan<float> floats)
        {
            return new Line
            {
                start = new(floats[..2]),
                end = new(floats[2..4]),
                color = new(floats[4..8])
            };
        }

        protected override void ToFloats(Line value, Span<float> floats)
        {
            value.start.CopyTo(floats[..2]);
            value.end.CopyTo(floats[2..4]);
            value.color.CopyTo(floats[4..8]);
        }
    }

    public class BasicLineRenderer : GRiPERenderer
    {
        CachedShader lineShader;
        WebGLBuffer? buffer;

        public LineArray Lines { get; } = new LineArray();

        int posLocation;
        int colorLocation;

        public override async Task InitResources(WebGLContext gl, WebGLShaderCache shaderCache)
        {
            if (!lineShader)
            {
                lineShader = await shaderCache.FetchShaderAsync(gl, "line.glsl");
                buffer = await gl.CreateBufferAsync();

                await gl.BindBufferAsync(BufferType.ARRAY_BUFFER, buffer);

                posLocation = await gl.GetAttribLocationAsync(lineShader.ShaderProgram, "pos");
                colorLocation = await gl.GetAttribLocationAsync(lineShader.ShaderProgram, "color");

                await gl.EnableVertexAttribArrayAsync((uint)posLocation);
                await gl.EnableVertexAttribArrayAsync((uint)colorLocation);
            }
        }

        public override async Task DestroyResources(WebGLContext gl, WebGLShaderCache shaderCache)
        {
            if (buffer != null)
            {
                await gl.DeleteBufferAsync(buffer);
            }
        }

        public override async Task Render(WebGLContext gl, WebGLShaderCache shaderCache)
        {
            await InitResources(gl, shaderCache);

            await gl.UseProgramAsync(lineShader.ShaderProgram);

            await gl.BindBufferAsync(BufferType.ARRAY_BUFFER, buffer);
            await gl.BufferDataAsync(BufferType.ARRAY_BUFFER, Lines.FloatArray, BufferUsageHint.DYNAMIC_DRAW);

            await gl.VertexAttribPointerAsync((uint)posLocation, 2, DataType.FLOAT, false, Lines.FloatWidth * 4, 0);
            await gl.VertexAttribPointerAsync((uint)colorLocation, 4, DataType.FLOAT, false, Lines.FloatWidth * 4, 16);

            await gl.DrawArraysAsync(Primitive.LINES, 0, Lines.Length);
        }
    }
}
