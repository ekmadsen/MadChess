using System.Text.Json.Serialization;
using ErikTheCoder.MadChess.Engine.Config;


namespace ErikTheCoder.MadChess.Engine;

[JsonSerializable(typeof(AdvancedConfig))]
internal sealed partial class JsonContext : JsonSerializerContext
{
}