using System.Text.RegularExpressions;

namespace ArchUnitNet.Common.PatternMatching;

/// <summary>
/// Glob pattern matching for file paths.
/// Examples: "src/**/*.cs", "tests/*/Error.cs", "**/{internal}/**"
/// Uses regex-based implementation for broad framework support.
/// </summary>
public class GlobPattern
{
    private readonly Regex _regex;
    private readonly string _pattern;

    public GlobPattern(string pattern)
    {
        _pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
        _regex = CreateGlobRegex(_pattern);
    }

    public bool Matches(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        // Normalize path to use forward slashes
        var normalizedPath = path.Replace('\\', '/');
        return _regex.IsMatch(normalizedPath);
    }

    private static Regex CreateGlobRegex(string pattern)
    {
        // Normalize pattern to use forward slashes
        var normalizedPattern = pattern.Replace('\\', '/');

        // Escape regex special chars except *, ?, [, ], {, }
        var regexPattern = "^";
        var i = 0;

        while (i < normalizedPattern.Length)
        {
            var ch = normalizedPattern[i];

            if (ch == '*')
            {
                if (i + 1 < normalizedPattern.Length && normalizedPattern[i + 1] == '*')
                {
                    if (i + 2 < normalizedPattern.Length && normalizedPattern[i + 2] == '/')
                    {
                        // **/ matches zero or more directories
                        regexPattern += "(?:.*/)?";
                        i += 3;
                    }
                    else if (i + 2 == normalizedPattern.Length)
                    {
                        // ** at end matches everything
                        regexPattern += ".*";
                        i += 2;
                    }
                    else
                    {
                        // ** in middle: match any chars including /
                        regexPattern += ".*";
                        i += 2;
                    }
                }
                else
                {
                    // * matches anything except /
                    regexPattern += "[^/]*";
                    i++;
                }
            }
            else if (ch == '?')
            {
                // ? matches any single char except /
                regexPattern += "[^/]";
                i++;
            }
            else if (ch == '[')
            {
                // Character class [abc] or [!abc]
                var j = i + 1;
                var hasNot = false;

                if (j < normalizedPattern.Length && normalizedPattern[j] == '!')
                {
                    hasNot = true;
                    j++;
                }

                while (j < normalizedPattern.Length && normalizedPattern[j] != ']')
                    j++;

                if (j < normalizedPattern.Length)
                {
                    var charClass = normalizedPattern.Substring(i + 1 + (hasNot ? 1 : 0), j - i - 2 - (hasNot ? 1 : 0));
                    regexPattern += hasNot ? $"[^{Regex.Escape(charClass)}]" : $"[{Regex.Escape(charClass)}]";
                    i = j + 1;
                }
                else
                {
                    regexPattern += Regex.Escape("[");
                    i++;
                }
            }
            else
            {
                // Escape regex special characters
                regexPattern += Regex.Escape(ch.ToString());
                i++;
            }
        }

        regexPattern += "$";
        return new Regex(regexPattern, RegexOptions.IgnoreCase);
    }

    public override string ToString() => _pattern;
}
