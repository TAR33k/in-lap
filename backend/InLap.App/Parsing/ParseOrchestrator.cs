using System;
using System.Collections.Generic;
using InLap.App.Domain;
using InLap.App.DTOs;

namespace InLap.App.Parsing
{
    public static class ParseOrchestrator
    {
        public static (RaceWeekend weekend, ParseWarnings warnings) Parse(List<string> lines)
        {
            var warnings = new ParseWarnings();
            var weekend = new RaceWeekend();

            var (metadata, sessions, others) = TopLevelSplitter.Split(lines);

            MetadataParser.Apply(metadata, weekend, warnings);

            foreach (var block in sessions)
            {
                if (string.IsNullOrWhiteSpace(weekend.Game) || string.IsNullOrWhiteSpace(weekend.Track) || !weekend.Date.HasValue)
                {
                    MetadataParser.Apply(block.Lines, weekend, warnings);
                }

                var session = new Session { Type = ToSessionType(block.Name) };

                ResultsParser.ParseIntoSession(block.Lines, session, warnings);
                LapsByDriverParser.ParseIntoSession(block.Lines, session, warnings);
                IncidentsParser.ParseIntoSession(block.Lines, session);

                weekend.Sessions.Add(session);
            }

            return (weekend, warnings);
        }

        private static SessionType ToSessionType(string name)
        {
            name = name.Trim().ToLowerInvariant();
            if (name.StartsWith("practice")) return SessionType.Practice;
            if (name.StartsWith("qualify")) return SessionType.Qualify;
            if (name.StartsWith("race 2")) return SessionType.Race2;
            if (name.StartsWith("race 1") || name.StartsWith("race")) return SessionType.Race1;
            return SessionType.Unknown;
        }
    }
}
