using JetBrains.Annotations;


namespace ErikTheCoder.MadChess.Core.Utilities
{
    public static class ExtensionMethods
    {
        [UsedImplicitly]
        [ContractAnnotation("text: null => true")]
        public static bool IsNullOrEmpty(this string text) => string.IsNullOrEmpty(text);
    }
}
