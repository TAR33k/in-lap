using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using InLap.App.Domain;
using InLap.App.DTOs;

namespace InLap.App.Parsing
{
    public static class ResultsParser
    {
        private static readonly Regex CsvSplit = new(
            pattern: ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)",
            options: RegexOptions.Compiled);

        public static void ParseIntoSession(List<string> lines, Session session, ParseWarnings warnings)
        {
            if (lines.Count == 0) return;

            int headerIdx = lines.FindIndex(l => l.IndexOf("Pos", StringComparison.OrdinalIgnoreCase) >= 0
                                               && l.IndexOf("Driver", StringComparison.OrdinalIgnoreCase) >= 0);
            if (headerIdx >= 0)
            {
                for (int i = headerIdx + 1; i < lines.Count; i++)
                {
                    var row = lines[i];
                    if (string.IsNullOrWhiteSpace(row)) break;

                    var cells = CsvSplit.Split(row).Select(c => c.Trim().Trim('"')).ToArray();
                    if (cells.Length < 2) continue;

                    int pos;
                    if (!int.TryParse(cells[0], out pos))
                    {
                        continue;
                    }

                    string driver = cells.FirstOrDefault(c => !int.TryParse(c, out _)) ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(driver)) continue;

                    string? gap = null;
                    foreach (var c in cells)
                    {
                        if (c.StartsWith("+")) { gap = c; break; }
                        if (Regex.IsMatch(c, @"^\d{1,2}:\d{2}\.\d{3,4}$")) { gap = c; }
                    }

                    session.TopFinishers.Add(new TopFinisher
                    {
                        Pos = pos,
                        Driver = driver,
                        Gap = pos == 1 ? null : gap
                    });

                    if (session.TopFinishers.Count >= 10) break;
                }
            }

            var bestLapMatch = lines.SelectMany(l => Regex.Matches(l, @"(?<time>\d{1,2}:\d{2}\.\d{3,4})").Cast<Match>())
                                     .Select(m => m.Groups["time"].Value)
                                     .FirstOrDefault();
            if (bestLapMatch != null && session.BestLap == null && session.TopFinishers.Count > 0)
            {
                session.BestLap = new BestLapInfo { Driver = session.TopFinishers[0].Driver, Time = bestLapMatch };
            }
        }
    }
}
