
namespace GRiPE.Code.Renderer
{
    public class BasicPointRenderer : GRiPERenderer
    {
        private CachedShader pointShader;

        public PointArray Points { get; } = new PointArray();
        WebGLBuffer? buffer;

        int posLocation;
        int colorLocation;
        int sizeLocation;

        public override async Task InitResources(WebGLContext gl, WebGLShaderCache shaderCache)
        {
            if (!pointShader)
            {
                pointShader = await shaderCache.FetchShaderAsync(gl, "point");
                buffer = await gl.CreateBufferAsync();

                await gl.BindBufferAsync(BufferType.ARRAY_BUFFER, buffer);

                posLocation = await gl.GetAttribLocationAsync(pointShader.ShaderProgram, "pos");
                colorLocation = await gl.GetAttribLocationAsync(pointShader.ShaderProgram, "color");
                sizeLocation = await gl.GetAttribLocationAsync(pointShader.ShaderProgram, "size");

                await gl.EnableVertexAttribArrayAsync((uint)posLocation);
                await gl.EnableVertexAttribArrayAsync((uint)colorLocation);
                await gl.EnableVertexAttribArrayAsync((uint)sizeLocation);
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

            await gl.UseProgramAsync(pointShader.ShaderProgram);

            await gl.BufferDataAsync(BufferType.ARRAY_BUFFER, Points.FloatArray, BufferUsageHint.DYNAMIC_DRAW);

            await gl.VertexAttribPointerAsync((uint)posLocation, 2, DataType.FLOAT, false, Points.FloatWidth * 4, 0);
            await gl.VertexAttribPointerAsync((uint)colorLocation, 4, DataType.FLOAT, false, Points.FloatWidth * 4, 8);
            await gl.VertexAttribPointerAsync((uint)sizeLocation, 1, DataType.FLOAT, false, Points.FloatWidth * 4, 24);

            await gl.DrawArraysAsync(Primitive.POINTS, 0, Points.Length);

            await gl.FlushAsync();
        }
    }
}
