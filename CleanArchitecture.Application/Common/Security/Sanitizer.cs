using Ganss.Xss;

public static class Sanitizer
{
    private static readonly HtmlSanitizer _sanitizer = new HtmlSanitizer();

    public static string? Clean(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;

        var trimmed = input.Trim();
        trimmed = new string(trimmed.Where(c => !char.IsControl(c)).ToArray());

        return _sanitizer.Sanitize(trimmed);
    }
}
