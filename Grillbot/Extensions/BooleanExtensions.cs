namespace Grillbot.Extensions
{
    public static class BooleanExtensions
    {
        public static string TranslateToCz(this bool value)
        {
            return value ? "Ano" : "Ne";
        }
    }
}
