using System;
using System.Collections.Generic;
using InLap.App.Domain;

namespace InLap.App.Parsing
{
    public static class IncidentsParser
    {
        public static void ParseIntoSession(List<string> lines, Session session)
        {
            foreach (var raw in lines)
            {
                var line = raw.Trim();
                if (string.IsNullOrEmpty(line)) continue;
                if (line.Contains(',') && !line.EndsWith('.')) continue;

                if (line.Length > 6 && line.EndsWith(".", StringComparison.Ordinal))
                {
                    session.NotableIncidents.Add(new Incident { Description = line });
                }
            }
        }
    }
}
