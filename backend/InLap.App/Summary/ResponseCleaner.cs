using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using InLap.App.DTOs;

namespace InLap.App.Summary
{
    public class ResponseCleaner
    {
        public string Clean(string llmText, WeekendSummaryDto summary)
        {
            if (string.IsNullOrWhiteSpace(llmText)) return string.Empty;

            var sanitized = Regex.Replace(llmText, "[\u0000-\u0008\u000B\u000C\u000E-\u001F\u007F]", string.Empty);
            sanitized = sanitized.Replace("\r\n", "\n").Replace("\r", "\n");
            sanitized = Regex.Replace(sanitized, @"(?im)^\s*HEADLINE\s*(?:—|-|:)?\s*$", "HEADLINE —", RegexOptions.None);
            sanitized = Regex.Replace(sanitized, @"(?im)^\s*LEAD\s*(?:—|-|:)?\s*$", "LEAD —", RegexOptions.None);
            sanitized = Regex.Replace(sanitized, @"(?im)^\s*BODY\s*(?:—|-|:)?\s*$", "BODY —", RegexOptions.None);
            sanitized = Regex.Replace(sanitized, @"(?im)^\s*QUICK FACTS\s*(?:—|-|:)?\s*$", "QUICK FACTS —", RegexOptions.None);

            sanitized = Regex.Replace(sanitized, @"(?m)^HEADLINE —\s*", "HEADLINE —\n");
            sanitized = Regex.Replace(sanitized, @"(?m)^LEAD —\s*", "LEAD —\n");
            sanitized = Regex.Replace(sanitized, @"(?m)^BODY —\s*", "BODY —\n");
            sanitized = Regex.Replace(sanitized, @"(?m)^QUICK FACTS —\s*", "QUICK FACTS —\n");

            var headline = ExtractSection(sanitized, "HEADLINE");
            var lead = ExtractSection(sanitized, "LEAD");
            var body = ExtractSection(sanitized, "BODY");
            var facts = ExtractSection(sanitized, "QUICK FACTS");

            var bodyParas = SplitParagraphs(body);
            var qualify = bodyParas.Where(p => Regex.IsMatch(p, @"\bQualif", RegexOptions.IgnoreCase)).ToList();
            var race1 = bodyParas.Where(p => Regex.IsMatch(p, @"\bRace\s*1\b|\bRace1\b", RegexOptions.IgnoreCase)).ToList();
            var race2 = bodyParas.Where(p => Regex.IsMatch(p, @"\bRace\s*2\b|\bRace2\b", RegexOptions.IgnoreCase)).ToList();
            var others = bodyParas.Except(qualify.Concat(race1).Concat(race2)).ToList();
            var reorderedBody = string.Join("\n\n", qualify.Concat(race1).Concat(race2).Concat(others));

            var factLines = facts.Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0).ToList();
            for (int i = 0; i < factLines.Count; i++)
            {
                if (!factLines[i].StartsWith("- ")) factLines[i] = "- " + factLines[i].TrimStart('-', ' ');
            }
            var normalizedFacts = string.Join("\n", factLines);

            var result = string.Join("\n\n", new[]
            {
                headline.Trim(),
                lead.Trim(),
                "BODY —\n" + reorderedBody.Trim(),
                "QUICK FACTS —\n" + normalizedFacts.Trim()
            }.Where(s => !string.IsNullOrWhiteSpace(s)));

            var finalText = result.Trim();
            if (string.IsNullOrWhiteSpace(finalText) || finalText.Equals("BODY —\n\nQUICK FACTS —", StringComparison.Ordinal))
                return sanitized.Trim();

            return finalText;
        }

        private static string ExtractSection(string text, string label)
        {
            var pattern = $@"(?is-m)^{label}\s+—\s*\n(?<content>.*?)(?=^(?:HEADLINE|LEAD|BODY|QUICK FACTS)\s+—\s*$|\z)";
            var m = Regex.Match(text, pattern);
            if (m.Success) return $"{label} —\n" + m.Groups["content"].Value.Trim();
            return string.Empty;
        }

        private static List<string> SplitParagraphs(string section)
        {
            if (string.IsNullOrWhiteSpace(section)) return new List<string>();
            var content = Regex.Replace(section, @"^BODY\s+—\s*\n", string.Empty, RegexOptions.IgnoreCase);
            var parts = Regex.Split(content.Trim(), @"\n\s*\n").Select(p => p.Trim()).Where(p => p.Length > 0).ToList();
            return parts;
        }
    }
}
