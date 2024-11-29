using System.Globalization;
using System.Text;

namespace Harrison314Blog;

internal static class StringExtensions
{
    public static string RemoveDiacritics(this string str)
    {
        System.Diagnostics.Debug.Assert(str != null);

        string normalized = str.Normalize(NormalizationForm.FormD);
        StringBuilder builder = new StringBuilder(str.Length);

        foreach (char c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }
}
