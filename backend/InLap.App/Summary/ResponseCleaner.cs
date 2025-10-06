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
            if (summary == null) return llmText;

            var allowedDrivers = new HashSet<string>(
                summary.Sessions.SelectMany(s => s.TopFinishers.Select(tf => tf.Driver))
                       .Concat(summary.Sessions.Where(s => s.BestLap != null).Select(s => s.BestLap!.Driver))
                       .Where(n => !string.IsNullOrWhiteSpace(n)),
                StringComparer.OrdinalIgnoreCase);

            var track = summary.Weekend.Track ?? string.Empty;

            var sentences = Regex.Split(llmText, @"(?<=[\.!?])\s+");
            var kept = new List<string>();
            foreach (var s in sentences)
            {
                var sentence = s.Trim();
                if (sentence.Length == 0) continue;

                var mentionsUnknownDriver = ExtractCapitalizedWords(sentence)
                    .Any(w => LooksLikeName(w) && !allowedDrivers.Contains(w));

                if (mentionsUnknownDriver) continue;

                if (!string.IsNullOrWhiteSpace(track))
                {
                    if (Regex.IsMatch(sentence, @"\btrack\b", RegexOptions.IgnoreCase) &&
                        !sentence.Contains(track, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }

                kept.Add(sentence);
            }

            return string.Join(" ", kept);
        }

        private static IEnumerable<string> ExtractCapitalizedWords(string s)
        {
            return Regex.Matches(s, @"\b[A-Z][a-z]+(?:\s+[A-Z][a-z]+)*\b")
                        .Cast<Match>()
                        .Select(m => m.Value);
        }

        private static bool LooksLikeName(string token)
        {
            var parts = token.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length >= 1 && parts.Length <= 3;
        }
    }
}
