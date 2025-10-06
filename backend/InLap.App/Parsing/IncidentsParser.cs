using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Text.RegularExpressions;
using InLap.App.Domain;

namespace InLap.App.Parsing
{
    public static class IncidentsParser
    {
        private static readonly Regex CsvSplit = new(
            pattern: ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)",
            options: RegexOptions.Compiled);

        private static readonly Regex ImpactSpeed = new(
            pattern: @"impact\s*[^0-9A-Za-z]{0,3}\s*speed\s*[:=]\s*(?<v>[0-9]{1,3}(?:[\.,][0-9]{3})*(?:[\.,][0-9]+)?|[0-9]+(?:[\.,][0-9]+)?)",
            options: RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static void ParseIntoSession(List<string> lines, Session session)
        {
            if (lines == null || lines.Count == 0) return;
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var collected = new List<(string Desc, double Speed)>();

            foreach (var raw in lines)
            {
                if (string.IsNullOrWhiteSpace(raw)) continue;

                var line = raw.Trim();
                line = line.Replace('\u00A0'.ToString(), " ")
                           .Replace('\u2007'.ToString(), " ")
                           .Replace('\u202F'.ToString(), " ");

                if (line.IndexOf("Impact speed", StringComparison.OrdinalIgnoreCase) < 0) continue;

                var cells = CsvSplit.Split(line);
                string msg = string.Empty;
                if (cells.Length >= 2)
                {
                    msg = cells[1].Trim().Trim('"');
                }
                else
                {
                    var qm = Regex.Match(line, "\"(?<msg>[^\"]+)\"");
                    if (qm.Success)
                        msg = qm.Groups["msg"].Value.Trim();
                }
                if (string.IsNullOrEmpty(msg) || msg.Length < 12) continue;

                var sp = ImpactSpeed.Match(msg);
                if (!sp.Success)
                {
                    sp = ImpactSpeed.Match(line);
                    if (!sp.Success) continue;
                }

                var rawVal = sp.Groups["v"].Value.Replace(",", ".");
                if (rawVal.IndexOf('.') >= 0 && rawVal.IndexOf(',') >= 0)
                {
                    int lastSep = Math.Max(rawVal.LastIndexOf('.'), rawVal.LastIndexOf(','));
                    var sb = new System.Text.StringBuilder();
                    for (int i = 0; i < rawVal.Length; i++)
                    {
                        char ch = rawVal[i];
                        if ((ch == '.' || ch == ',') && i != lastSep) continue;
                        sb.Append(ch == ',' ? '.' : ch);
                    }
                    rawVal = sb.ToString();
                }

                if (double.TryParse(rawVal, NumberStyles.Float, CultureInfo.InvariantCulture, out var val))
                {
                    if (val > 60.0 && seen.Add(msg))
                        collected.Add((msg, val));
                }
            }

            var top = collected
                .OrderByDescending(x => x.Speed)
                .Take(10)
                .Select(x => new Incident { Description = x.Desc })
                .ToList();

            session.NotableIncidents.Clear();
            session.NotableIncidents.AddRange(top);
        }
    }
}
