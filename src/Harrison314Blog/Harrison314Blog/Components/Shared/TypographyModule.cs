using System.Text.RegularExpressions;

namespace Harrison314Blog.Components.Shared;

internal static partial class TypographyModule
{
    [GeneratedRegex(@"(\s|^)(z|zo|bez|na|po|od|do|pri|pre|so|miesto|o|v|s|za|a|i|ani|aj|najprv|potom|ešte|ale|no|lež|jednako|alebo|buď|či|že|aby|čo|aký|ktorý|kde|keď|kým|kde|k|ku|čo|akoby|lebo|pretože|predsa)(\s+)([^\p{Cc}\p{Cf}\p{Z}]+)", RegexOptions.IgnoreCase)]
    private static partial Regex GetRegex();

    public static string UseTypography(string html)
    {
        string text = GetRegex().Replace(html, "$1$2&#160;$4");
        text = text.Replace("...", "&#8230;");

        return text;
    }
}
