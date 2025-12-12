//using Ganss.Xss;

//public static class Sanitizer
//{
//    private static readonly HtmlSanitizer _sanitizer = new HtmlSanitizer();

//    public static string? Clean(string? input)
//    {
//        if (string.IsNullOrWhiteSpace(input)) return input;

//        var trimmed = input.Trim();
//        trimmed = new string(trimmed.Where(c => !char.IsControl(c)).ToArray());

//        return _sanitizer.Sanitize(trimmed);
//    }
//}


using Ganss.Xss;

public static class Sanitizer
{
    private static readonly HtmlSanitizer _sanitizer;

    static Sanitizer()
    {
        _sanitizer = new HtmlSanitizer();

        // Allow basic harmless formatting
        _sanitizer.AllowedTags.Add("b");
        _sanitizer.AllowedTags.Add("strong");
        _sanitizer.AllowedTags.Add("i");
        _sanitizer.AllowedTags.Add("em");
        _sanitizer.AllowedTags.Add("u");
        _sanitizer.AllowedTags.Add("br");
        _sanitizer.AllowedTags.Add("p");

        // Ensure no script or event handlers get through
        _sanitizer.AllowedAttributes.Remove("onclick");
        _sanitizer.AllowedAttributes.Remove("onload");
    }

    public static string? Clean(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;

        // Trim + remove control chars
        var trimmed = new string(input.Trim().Where(c => !char.IsControl(c)).ToArray());

        return _sanitizer.Sanitize(trimmed);
    }
}
