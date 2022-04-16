using System.Collections.ObjectModel;

namespace GRiPE.Code.Renderer
{
    public struct CachedShader
    {
        public WebGLProgram ShaderProgram { get; }
        public (WebGLShader vert, WebGLShader frag) CompiledShader { get; }
        public (string vert, string frag) ShaderSources { get; }

        public ReadOnlyDictionary<string, int> AttributeLocations { get; }
        public ReadOnlyDictionary<string, WebGLUniformLocation> UniformLocations { get; }

        internal CachedShader(WebGLProgram program, 
            (WebGLShader vert, WebGLShader frag) shader, 
            (string vert, string frag) sources,
            ReadOnlyDictionary<string, int> locations,
            ReadOnlyDictionary<string, WebGLUniformLocation> uniforms)
        {
            ShaderProgram = program;
            CompiledShader = shader;
            ShaderSources = sources;
            AttributeLocations = locations;
            UniformLocations = uniforms;
        }

        public static implicit operator bool(CachedShader s)
        {
            return s.ShaderProgram != null;
        }
    }

    public class WebGLShaderCache
    {
        enum ShaderSourceReadTarget
        {
            Both, Vert, Frag
        }

        public const string SHADER_FOLDER = "shader/";

        Dictionary<string, (string vert, string frag)> cachedShaderSources = new();
        Dictionary<string, CachedShader> cachedShaders = new();

        readonly HttpClient client;

        public WebGLShaderCache(HttpClient httpClient)
        {
            client = httpClient;
        }

        public async Task<CachedShader> FetchShaderAsync(WebGLContext gl, string fileName)
        {
            if (!fileName.EndsWith(".glsl"))
                fileName += ".glsl";

            if (cachedShaders.TryGetValue(fileName, out var cachedShader))
                return cachedShader;

            var sources = await LoadShaderSource(gl, fileName);
            var compiledShaders = await CompileShader(gl, sources);
            var program = await CreateShaderProgram(gl, compiledShaders);

            var locations = await GetAttribAndUniformLocations(gl, program);

            Console.WriteLine($"Compiled shader {fileName}");

            CachedShader newShader = new CachedShader(program, compiledShaders, sources, 
                new ReadOnlyDictionary<string, int>(locations.attribLocations), 
                new ReadOnlyDictionary<string, WebGLUniformLocation>(locations.uniformLocations));

            cachedShaders.Add(fileName, newShader);
            return newShader;
        }

        private async Task<(Dictionary<string, int> attribLocations, Dictionary<string, WebGLUniformLocation> uniformLocations)> 
            GetAttribAndUniformLocations(WebGLContext gl, WebGLProgram program)
        {

            Dictionary<string, int> attribLocations = new();
            Dictionary<string, WebGLUniformLocation> uniformLocations = new();
            await Task.CompletedTask;
            //uint numAttribs = await gl.GetProgramParameterAsync<uint>(program, ProgramParameter.ACTIVE_ATTRIBUTES);
            //uint numUniforms = await gl.GetProgramParameterAsync<uint>(program, ProgramParameter.ACTIVE_UNIFORMS);
            //Console.WriteLine($"numAttribs: {numAttribs}  numUniforms: {numUniforms}");
            //for (uint i = 0; i < numAttribs; ++i)
            //{
            //    var attrib = await gl.GetActiveAttribAsync(program, i);
            //    var location = await gl.GetAttribLocationAsync(program, attrib.Name);
            //    Console.WriteLine($"{i} - attrib {attrib.Name} - type {attrib.Type} - location {location}");
            //    attribLocations.Add(attrib.Name, location);
            //}
            //for (uint i = 0; i < numUniforms; i++)
            //{
            //    var uniform = await gl.GetActiveUniformAsync(program, i);
            //    var location = await gl.GetUniformLocationAsync(program, uniform.Name);
            //    uniformLocations.Add(uniform.Name, location);
            //}

            return (attribLocations, uniformLocations);
        }

        private async Task<WebGLProgram> CreateShaderProgram(WebGLContext gl, (WebGLShader vert, WebGLShader frag) compiledShader)
        {
            var program = await gl.CreateProgramAsync();
            await gl.AttachShaderAsync(program, compiledShader.vert);
            await gl.AttachShaderAsync(program, compiledShader.frag);
            await gl.LinkProgramAsync(program);
            LogInfo("program", await gl.GetProgramInfoLogAsync(program));

            return program;
        }

        private async Task<(WebGLShader vert, WebGLShader frag)> CompileShader(WebGLContext gl, (string vert, string frag) shaderSources)
        {
            var vert = await gl.CreateShaderAsync(ShaderType.VERTEX_SHADER);
            await gl.ShaderSourceAsync(vert, shaderSources.vert);
            await gl.CompileShaderAsync(vert);
            LogInfo("vert", await gl.GetShaderInfoLogAsync(vert));

            var frag = await gl.CreateShaderAsync(ShaderType.FRAGMENT_SHADER);
            await gl.ShaderSourceAsync(frag, shaderSources.frag);
            await gl.CompileShaderAsync(frag);
            LogInfo("frag", await gl.GetShaderInfoLogAsync(frag));

            return (vert, frag);
        }

        private async Task<(string vert, string frag)> LoadShaderSource(WebGLContext gl, string fileName)
        {
            if (cachedShaderSources.TryGetValue(fileName, out var shaderSource))
            {
                Console.WriteLine($"Found cached shader source for {fileName}");
                return shaderSource;
            }

            string path = Path.Combine(SHADER_FOLDER, fileName);
            Console.WriteLine($"shader source path: {path}");
            //var lines = await File.ReadAllLinesAsync(fileName);

            //wtf even is this lmao
            var raw = await client.GetStringAsync(path);
            StringReader sr = new StringReader(raw);
            IEnumerable<string> ReadLines()
            {
                string? line = null;
                while ((line = sr.ReadLine()) != null)
                    yield return line;
            }

            ShaderSourceReadTarget target = ShaderSourceReadTarget.Both;
            StringBuilder vert = new();
            StringBuilder frag = new();
            
            foreach (var line in ReadLines())
            {
                var newTarget = line switch
                {
                    "##vert" => ShaderSourceReadTarget.Vert,
                    "##frag" => ShaderSourceReadTarget.Frag,
                    "##both" => ShaderSourceReadTarget.Both,
                    _ => target
                };

                if (target != newTarget)
                {
                    target = newTarget;
                    continue;
                }

                if (target == ShaderSourceReadTarget.Vert || target == ShaderSourceReadTarget.Both)
                    vert.AppendLine(line);
                if (target == ShaderSourceReadTarget.Frag || target == ShaderSourceReadTarget.Both)
                    frag.AppendLine(line);
            }

            var sources = (vert.ToString(), frag.ToString());
            cachedShaderSources.Add(fileName, sources);
            return sources;
        }

        private void LogInfo(string stageName, string info)
        {
            if (!string.IsNullOrEmpty(info))
            {
                Console.WriteLine($"Received logs for stage {stageName}\n\r{info}");
            }
        }
    }
}
