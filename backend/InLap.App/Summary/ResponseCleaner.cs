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
            sanitized = Regex.Replace(sanitized, @"(?im)^\s*HEADLINE\s*(?:—|-|:)?\s*(.*)$", m =>
            {
                var tail = m.Groups[1].Value.Trim();
                return tail.Length > 0 ? $"HEADLINE —\n{tail}" : "HEADLINE —";
            });
            sanitized = Regex.Replace(sanitized, @"(?im)^\s*LEAD\s*(?:—|-|:)?\s*(.*)$", m =>
            {
                var tail = m.Groups[1].Value.Trim();
                return tail.Length > 0 ? $"LEAD —\n{tail}" : "LEAD —";
            });
            sanitized = Regex.Replace(sanitized, @"(?im)^\s*BODY\s*(?:—|-|:)?\s*(.*)$", m =>
            {
                var tail = m.Groups[1].Value.Trim();
                return tail.Length > 0 ? $"BODY —\n{tail}" : "BODY —";
            });
            sanitized = Regex.Replace(sanitized, @"(?im)^\s*QUICK FACTS\s*(?:—|-|:)?\s*(.*)$", m =>
            {
                var tail = m.Groups[1].Value.Trim();
                return tail.Length > 0 ? $"QUICK FACTS —\n{tail}" : "QUICK FACTS —";
            });

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

            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(headline)) parts.Add(headline.Trim());
            if (!string.IsNullOrWhiteSpace(lead)) parts.Add(lead.Trim());
            if (!string.IsNullOrWhiteSpace(reorderedBody)) parts.Add("BODY —\n" + reorderedBody.Trim());
            if (!string.IsNullOrWhiteSpace(normalizedFacts)) parts.Add("QUICK FACTS —\n" + normalizedFacts.Trim());

            var result = string.Join("\n\n", parts);

            var finalText = result.Trim();
            finalText = StripTrailingHeaders(finalText).Trim();

            if (string.IsNullOrWhiteSpace(finalText) || finalText.Equals("BODY —\n\nQUICK FACTS —", StringComparison.Ordinal))
                return StripTrailingHeaders(sanitized).Trim();

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

        private static string StripTrailingHeaders(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text ?? string.Empty;
            var cleaned = Regex.Replace(text, @"(?mis)(?:\n\s*)*(?:^|\n)(?:HEADLINE|LEAD|BODY|QUICK FACTS)\s+—\s*\z", string.Empty);
            cleaned = Regex.Replace(cleaned, @"\n{3,}", "\n\n");
            return cleaned;
        }
    }
}
