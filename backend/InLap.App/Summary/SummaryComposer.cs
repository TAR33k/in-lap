using System;
using System.Collections.Generic;
using System.Linq;
using InLap.App.Domain;
using InLap.App.DTOs;
using InLap.App.Interfaces;

namespace InLap.App.Summary
{
    public class SummaryComposer : ISummaryService
    {
        public WeekendSummaryDto Compose(RaceWeekend weekend, ParseWarnings warnings)
        {
            if (weekend == null) throw new ArgumentNullException(nameof(weekend));

            var dto = new WeekendSummaryDto
            {
                Weekend = new WeekendInfo
                {
                    Game = weekend.Game,
                    Track = weekend.Track,
                    Date = weekend.Date?.ToString("yyyy-MM-dd")
                },
                Sessions = new List<SessionSummary>()
            };

            foreach (var s in weekend.Sessions)
            {
                var ss = new SessionSummary
                {
                    Type = MapSessionType(s.Type),
                    Metadata = s.Metadata.ToDictionary(kv => kv.Key, kv => (object)kv.Value),
                    TopFinishers = s.TopFinishers
                        .OrderBy(tf => tf.Pos)
                        .Select(tf => new TopFinisherSummary
                    {
                        Pos = tf.Pos,
                        Driver = tf.Driver,
                        Gap = tf.Gap
                    }).ToList(),
                    BestLap = s.BestLap == null ? null : new BestLapSummary { Driver = s.BestLap.Driver, Time = s.BestLap.Time },
                    NotableIncidents = s.NotableIncidents.Select(i => i.Description).ToList(),
                    Notes = s.Notes.ToList()
                };

                dto.Sessions.Add(ss);
            }

            var order = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["Practice"] = 0,
                ["Qualify"] = 1,
                ["Race1"] = 2,
                ["Race2"] = 3
            };
            dto.Sessions = dto.Sessions
                .OrderBy(s => order.TryGetValue(s.Type, out var o) ? o : 99)
                .ToList();

            dto.Confidence = new ConfidenceInfo
            {
                TopFinishers = ComputeConfidence(warnings, dto.Sessions.Any(x => x.TopFinishers.Count >= 3)),
                Incidents = ComputeConfidence(warnings, dto.Sessions.Any(x => x.NotableIncidents.Count > 0))
            };

            return dto;
        }

        private static string MapSessionType(SessionType t) => t switch
        {
            SessionType.Practice => "Practice",
            SessionType.Qualify => "Qualify",
            SessionType.Race1 => "Race1",
            SessionType.Race2 => "Race2",
            _ => "Unknown"
        };

        private static string ComputeConfidence(ParseWarnings warnings, bool hasData)
        {
            var count = warnings?.Items?.Count ?? 0;
            if (!hasData) return count == 0 ? "medium" : (count < 5 ? "low" : "low");
            if (count == 0) return "high";
            if (count < 5) return "medium";
            return "low";
        }
    }
}
