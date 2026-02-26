using BenchmarkDotNet.Attributes;
using System.Text.RegularExpressions;

[MemoryDiagnoser]
[Config(typeof(BenchmarkConfig))]
public partial class RegexBenchmarks
{
    // ── Compiled/generated regexes (proposed) ───────────────────────────────
    [GeneratedRegex(@"<(.|\n)*?>", RegexOptions.None)]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex(@"^([a-zA-Z0-9_\-\.\+]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"[^a-z0-9\s-]")]
    private static partial Regex UrlInvalidCharsRegex();

    [GeneratedRegex(@"[\s-]+")]
    private static partial Regex UrlSpacesHyphensRegex();

    // ── Test inputs ──────────────────────────────────────────────────────────

    private const string HtmlInput = "<p>Hello <strong>World</strong>, visit <a href=\"https://example.com\">here</a>.</p>";
    private const string ValidEmail = "user.name+tag@example.co.uk";
    private const string InvalidEmail = "not-an-email";
    private const string UrlInput = "Hello World! This is a URL-friendly string with accents & special chars.";

    // ── StripHTML ─────────────────────────────────────────────────────────────

    [Benchmark(Baseline = true, Description = "StripHTML — new Regex per call")]
    public string StripHtml_Current()
    {
        string pattern = @"<(.|\n)*?>";
        return Regex.Replace(HtmlInput, pattern, string.Empty);
    }

    [Benchmark(Description = "StripHTML — [GeneratedRegex]")]
    public string StripHtml_Proposed()
    {
        return HtmlTagRegex().Replace(HtmlInput, string.Empty);
    }

    // ── IsValidEmailAddress ───────────────────────────────────────────────────

    [Benchmark(Description = "IsValidEmail (valid) — new Regex per call")]
    public bool IsValidEmail_Current_Valid()
    {
        string emailRegex = @"^([a-zA-Z0-9_\-\.\+]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
        return Regex.Match(ValidEmail, emailRegex, RegexOptions.IgnoreCase).Success;
    }

    [Benchmark(Description = "IsValidEmail (valid) — [GeneratedRegex]")]
    public bool IsValidEmail_Proposed_Valid()
    {
        return EmailRegex().IsMatch(ValidEmail);
    }

    // ── UrlFriendly (two Regex.Replace calls) ────────────────────────────────

    [Benchmark(Description = "UrlFriendly — new Regex per call")]
    public string UrlFriendly_Current()
    {
        string str = UrlInput.ToLower();
        str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
        str = Regex.Replace(str, @"[\s-]+", "-");
        str = str.Trim("- ".ToCharArray());
        return str;
    }

    [Benchmark(Description = "UrlFriendly — [GeneratedRegex]")]
    public string UrlFriendly_Proposed()
    {
        string str = UrlInput.ToLower();
        str = UrlInvalidCharsRegex().Replace(str, "");
        str = UrlSpacesHyphensRegex().Replace(str, "-");
        str = str.Trim("- ".ToCharArray());
        return str;
    }
}
