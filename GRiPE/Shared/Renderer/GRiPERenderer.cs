using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blazor.Extensions.Canvas;
using Blazor.Extensions.Canvas.Model;
using Blazor.Extensions.Canvas.WebGL;

namespace GRiPE.Code.Renderer
{
    public abstract class GRiPERenderer
    {
        public abstract Task InitResources(WebGLContext gl, WebGLShaderCache shaderCache);
        public abstract Task DestroyResources(WebGLContext gl, WebGLShaderCache shaderCache);
        public abstract Task Render(WebGLContext gl, WebGLShaderCache shaderCache);
    }
}
