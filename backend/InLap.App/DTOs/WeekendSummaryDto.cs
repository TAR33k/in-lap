using System;
using System.Collections.Generic;

namespace InLap.App.DTOs
{
    public class WeekendSummaryDto
    {
        public WeekendInfo Weekend { get; set; } = new();
        public List<SessionSummary> Sessions { get; set; } = new();
        public ConfidenceInfo Confidence { get; set; } = new();
    }

    public class WeekendInfo
    {
        public string Game { get; set; } = string.Empty;
        public string Track { get; set; } = string.Empty;
        public string? Date { get; set; }
    }

    public class SessionSummary
    {
        public string Type { get; set; } = string.Empty;
        public Dictionary<string, object> Metadata { get; set; } = new();
        public List<TopFinisherSummary> TopFinishers { get; set; } = new();
        public BestLapSummary? BestLap { get; set; }
        public List<string> NotableIncidents { get; set; } = new();
        public List<string> Notes { get; set; } = new();
    }

    public class TopFinisherSummary
    {
        public int Pos { get; set; }
        public string Driver { get; set; } = string.Empty;
        public string? Gap { get; set; }
    }

    public class BestLapSummary
    {
        public string Driver { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
    }

    public class ConfidenceInfo
    {
        public string TopFinishers { get; set; } = "medium";
        public string Incidents { get; set; } = "medium";
    }
}
