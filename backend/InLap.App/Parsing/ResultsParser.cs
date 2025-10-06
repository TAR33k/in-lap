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

        private static readonly Regex BestLapLine = new(
            pattern: @"^Best lap,\s*""?(?<driver>[^\(""]+)\s*\((?<time>\d{1,2}:\d{2}\.\d{3,4})\)""?",
            options: RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static void ParseIntoSession(List<string> lines, Session session, ParseWarnings warnings)
        {
            if (lines.Count == 0) return;

            if (session.BestLap == null)
            {
                foreach (var l in lines)
                {
                    var m = BestLapLine.Match(l);
                    if (m.Success)
                    {
                        session.BestLap = new BestLapInfo
                        {
                            Driver = m.Groups["driver"].Value.Trim('"', ' '),
                            Time = m.Groups["time"].Value
                        };
                        break;
                    }
                }
            }

            int headerIdx = lines.FindIndex(l => l.IndexOf("Pos", StringComparison.OrdinalIgnoreCase) >= 0
                                               && l.IndexOf("Driver", StringComparison.OrdinalIgnoreCase) >= 0);
            if (headerIdx >= 0)
            {
                var headerCells = CsvSplit.Split(lines[headerIdx]).Select(c => c.Trim().Trim('"')).ToArray();
                int posIdx = Array.FindIndex(headerCells, h => h.Equals("Pos", StringComparison.OrdinalIgnoreCase));
                int driverIdx = Array.FindIndex(headerCells, h => h.Equals("Driver", StringComparison.OrdinalIgnoreCase));
                int timeIdx = Array.FindIndex(headerCells, h => h.StartsWith("Time", StringComparison.OrdinalIgnoreCase));
                if (posIdx < 0 || driverIdx < 0)
                {
                    return;
                }

                for (int i = headerIdx + 1; i < lines.Count; i++)
                {
                    var row = lines[i];
                    if (string.IsNullOrWhiteSpace(row)) break;

                    var cells = CsvSplit.Split(row).Select(c => c.Trim().Trim('"')).ToArray();
                    if (cells.Length <= Math.Max(posIdx, driverIdx)) continue;

                    if (!int.TryParse(cells[posIdx], out var pos))
                    {
                        continue;
                    }

                    string driver = cells[driverIdx];

                    string? gap = null;
                    if (pos > 1 && timeIdx >= 0 && timeIdx < cells.Length)
                    {
                        var timeCell = cells[timeIdx];
                        if (!string.IsNullOrEmpty(timeCell))
                        {
                            timeCell = timeCell.Replace("\u00A0", " ");
                            timeCell = timeCell.Trim();
                            while (timeCell.Length > 0 && (timeCell[0] == '\'' || char.IsWhiteSpace(timeCell[0])))
                            {
                                timeCell = timeCell.Substring(1);
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(timeCell) && (timeCell.StartsWith("+") || Regex.IsMatch(timeCell, @"^\d{1,2}:\d{2}\.\d{3,4}$")))
                        {
                            gap = timeCell;
                        }
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

            if (session.BestLap == null && session.TopFinishers.Count > 0)
            {
                var bestLapMatch = lines.SelectMany(l => Regex.Matches(l, @"(?<time>\d{1,2}:\d{2}\.\d{3,4})").Cast<Match>())
                                         .Select(m => m.Groups["time"].Value)
                                         .FirstOrDefault();
                if (bestLapMatch != null)
                {
                    session.BestLap = new BestLapInfo { Driver = session.TopFinishers[0].Driver, Time = bestLapMatch };
                }
            }
        }
    }
}
