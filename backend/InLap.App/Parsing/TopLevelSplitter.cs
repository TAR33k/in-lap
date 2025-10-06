using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace InLap.App.Parsing
{
    public record NamedBlock(string Name, List<string> Lines);

    public static class TopLevelSplitter
    {
        private static readonly Regex SessionHeader = new(
            pattern: @"^(Practice|Qualify|Race\s*1|Race\s*2)\b.*",
            options: RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static (List<string> metadata, List<NamedBlock> sessions, List<string> others) Split(List<string> lines)
        {
            var metadata = new List<string>();
            var sessions = new List<NamedBlock>();
            var others = new List<string>();

            NamedBlock? current = null;
            bool metadataOpen = true;

            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line))
                {
                    Add(line);
                    continue;
                }

                if (SessionHeader.IsMatch(line))
                {
                    metadataOpen = false;
                    current = new NamedBlock(NormalizeSessionName(line), new List<string>());
                    sessions.Add(current);
                    continue;
                }

                Add(line);
            }

            return (metadata, sessions, others);

            void Add(string l)
            {
                if (current != null)
                {
                    current.Lines.Add(l);
                }
                else if (metadataOpen)
                {
                    metadata.Add(l);
                }
                else
                {
                    others.Add(l);
                }
            }
        }

        private static string NormalizeSessionName(string header)
        {
            header = header.Trim();
            if (header.StartsWith("Race", StringComparison.OrdinalIgnoreCase))
            {
                return header.Contains("2") ? "Race 2" : "Race 1";
            }
            if (header.StartsWith("Qualify", StringComparison.OrdinalIgnoreCase)) return "Qualify";
            if (header.StartsWith("Practice", StringComparison.OrdinalIgnoreCase)) return "Practice";
            return header;
        }
    }
}
