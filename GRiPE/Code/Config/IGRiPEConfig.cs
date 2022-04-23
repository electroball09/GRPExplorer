using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Config.Net;
using Config.Net.Json;

namespace GRiPE.Code.Config
{
    public interface IGRiPEConfig
    {
        [Option(DefaultValue = false)] public bool Maximized { get; set; }
        [Option(DefaultValue = 0d)] public double WindowLeft { get; set; }
        [Option(DefaultValue = 0d)] public double WindowTop { get; set; }
        [Option(DefaultValue = 800d)] public double WindowWidth { get; set; }
        [Option(DefaultValue = 450d)] public double WindowHeight { get; set; }
    }

    public static class GRiPEConfig
    {
        public static readonly string ConfigPath = 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GRiPE/");
        public const string ConfigFileName = "GRiPE.config.json";

        public static IGRiPEConfig Config { get; }

        static GRiPEConfig()
        {
            string filePath = Path.Combine(ConfigPath, ConfigFileName);

            Config = new ConfigurationBuilder<IGRiPEConfig>()
                .UseCommandLineArgs()
                .UseJsonFile(filePath)
                .UseEnvironmentVariables()
                .Build();
        }
    }
}
