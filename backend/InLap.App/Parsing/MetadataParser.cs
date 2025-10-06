using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using InLap.App.Domain;
using InLap.App.DTOs;

namespace InLap.App.Parsing
{
    public static class MetadataParser
    {
        private static readonly Regex KeyValue = new(
            pattern: "^(?<key>[A-Za-z ]+):\\s*\\\"?(?<value>.*?)\\\"?$",
            options: RegexOptions.Compiled);

        public static void Apply(List<string> metadataLines, RaceWeekend weekend, ParseWarnings warnings)
        {
            foreach (var line in metadataLines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var m = KeyValue.Match(line);
                if (!m.Success) continue;

                var key = m.Groups["key"].Value.Trim();
                var value = m.Groups["value"].Value.Trim();

                switch (key.ToLowerInvariant())
                {
                    case "game":
                        weekend.Game = value.Trim('"');
                        break;
                    case "track":
                        weekend.Track = value.Trim('"');
                        break;
                    case "date":
                        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt))
                        {
                            weekend.Date = dt;
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
