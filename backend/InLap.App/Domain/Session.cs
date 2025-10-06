using System.Collections.Generic;

namespace InLap.App.Domain
{
    public class Session
    {
        public SessionType Type { get; set; } = SessionType.Unknown;

        public Dictionary<string, string> Metadata { get; set; } = new();

        public List<TopFinisher> TopFinishers { get; set; } = new();

        public BestLapInfo? BestLap { get; set; }

        public List<Incident> NotableIncidents { get; set; } = new();

        public List<string> Notes { get; set; } = new();

        public Dictionary<string, List<LapRecord>> LapsByDriver { get; set; } = new();
    }

    public class TopFinisher
    {
        public int Pos { get; set; }
        public string Driver { get; set; } = string.Empty;
        public string? Gap { get; set; }
    }

    public class BestLapInfo
    {
        public string Driver { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
    }
}
