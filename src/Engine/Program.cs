// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2023.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime;
using System.Text.Json;
using System.Threading.Tasks;
using ErikTheCoder.MadChess.Core;
using ErikTheCoder.MadChess.Engine.Config;


namespace ErikTheCoder.MadChess.Engine;


public static class Program
{
    private const string _advancedConfigFilename = "MadChess.AdvancedConfig.json";
    private const string _advancedConfigResource = "ErikTheCoder.MadChess.Engine.MadChess.AdvancedConfig.json";
    private static readonly JsonSerializerOptions _jsonOptions;

    
    static Program()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };
    }


    // Attribute required to prevent Publish Profile from trimming Config constructors because it believes they're unreferenced code.
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(AdvancedConfig))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(LimitStrengthConfig))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(LimitStrengthEvalConfig))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(LimitStrengthSearchConfig))]
    public static async Task Main()
    {
        // Improve garbage collector performance at the cost of memory usage.
        // Engine should not allocate much memory when searching a position anyhow because it references pre-allocated objects.
        GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        // Load advanced configuration.
        //   Regular configuration is available via UCI engine options displayed by a GUI.
        //   Advanced configuration is available via a file located in same directory as MadChess.Engine.exe.
        // Segmenting configuration like this prevents casual users from becoming confused and overwhelmed by too many / too complex options.
        var advancedConfig = await LoadAdvancedConfig();

        await using (var inputStream = Console.OpenStandardInput())
        await using (var outputStream = Console.OpenStandardOutput())
        await using (var messenger = new Messenger(inputStream, outputStream))
        using (var uciStream = new UciStream(advancedConfig, messenger))
        {
            try
            {
                uciStream.Run();
            }
            catch (Exception exception)
            {
                uciStream.HandleException(exception);
            }
        }
    }

    private static async Task<AdvancedConfig> LoadAdvancedConfig()
    {
        if (File.Exists(_advancedConfigFilename))
        {
            // Load advanced config (potentially containing user-modified values) from file.
            await using (var stream = File.OpenRead(_advancedConfigFilename))
            {
                return await DeserializeConfigJson(stream);
            }
        }

        // Load advanced config (containing default values) from embedded resource.
        await using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(_advancedConfigResource))
        {
            return stream == null
                ? null
                : await DeserializeConfigJson(stream);
        }
    }


    private static async Task<AdvancedConfig> DeserializeConfigJson(Stream stream) => await JsonSerializer.DeserializeAsync<AdvancedConfig>(stream, _jsonOptions);
}