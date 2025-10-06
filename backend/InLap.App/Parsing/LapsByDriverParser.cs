using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using InLap.App.Domain;
using InLap.App.DTOs;

namespace InLap.App.Parsing
{
    public static class LapsByDriverParser
    {
        private static readonly Regex CsvSplit = new(
            pattern: ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)",
            options: RegexOptions.Compiled);

        public static void ParseIntoSession(List<string> lines, Session session, ParseWarnings warnings)
        {
            string? currentDriver = null;
            foreach (var raw in lines)
            {
                var line = raw.Trim();
                if (string.IsNullOrEmpty(line)) continue;

                if (!line.Contains(','))
                {
                    if (line.Equals("No laps for this driver", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    currentDriver = line.Trim('"');
                    if (!session.LapsByDriver.ContainsKey(currentDriver))
                        session.LapsByDriver[currentDriver] = new List<LapRecord>();
                    continue;
                }

                if (currentDriver != null)
                {
                    var cells = CsvSplit.Split(line).Select(c => c.Trim().Trim('"')).ToArray();
                    if (cells.Length == 0) continue;

                    if (!int.TryParse(cells[0], out var lapNo))
                    {
                        continue;
                    }

                    int cutsVal = 0;
                    if (cells.Length > 9)
                    {
                        var cutsCell = cells[9];
                        var m = Regex.Match(cutsCell, @"^(?<n>\d+)");
                        if (m.Success && int.TryParse(m.Groups["n"].Value, out var n)) cutsVal = n;
                    }

                    var rec = new LapRecord
                    {
                        Lap = lapNo,
                        Time = cells.Length > 4 ? cells[4] : string.Empty,
                        Sector1 = cells.Length > 5 ? cells[5] : null,
                        Sector2 = cells.Length > 6 ? cells[6] : null,
                        Sector3 = cells.Length > 7 ? cells[7] : null,
                        Compound = cells.Length > 8 ? cells[8] : null,
                        Cuts = cutsVal,
                        Pit = cells.Length > 2 && (cells[2].Equals("yes", StringComparison.OrdinalIgnoreCase) || cells[2] == "1")
                    };

                    session.LapsByDriver[currentDriver].Add(rec);
                }
            }
        }
    }
}
