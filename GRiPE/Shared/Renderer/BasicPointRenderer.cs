
namespace GRiPE.Code.Renderer
{
    public class BasicPointRenderer : GRiPERenderer
    {
        private CachedShader pointShader;

        public SerializedVector2Array PointLocations { get; } = new();

        public override async Task InitResources(WebGLContext gl, WebGLShaderCache shaderCache)
        {
            if (!pointShader)
            {
                pointShader = await shaderCache.FetchShaderAsync(gl, "point");
            }
        }

        public override async Task Render(WebGLContext gl, WebGLShaderCache shaderCache)
        {
            await InitResources(gl, shaderCache);

            await gl.UseProgramAsync(pointShader.ShaderProgram);

            var buffer = await gl.CreateBufferAsync();
            await gl.BindBufferAsync(BufferType.ARRAY_BUFFER, buffer);
            await gl.BufferDataAsync(BufferType.ARRAY_BUFFER, PointLocations.BackingArray, BufferUsageHint.DYNAMIC_DRAW);

            var index = await gl.GetAttribLocationAsync(pointShader.ShaderProgram, "pos");
            await gl.VertexAttribPointerAsync((uint)index, 2, DataType.FLOAT, false, 0, 0);
            await gl.EnableVertexAttribArrayAsync((uint)index);

            await gl.DrawArraysAsync(Primitive.POINTS, 0, PointLocations.Length);

            await gl.BindBufferAsync(BufferType.ARRAY_BUFFER, null);
        }
    }
}
