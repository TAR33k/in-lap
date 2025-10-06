using System;
using System.Collections.Generic;

namespace InLap.App.Domain
{
    public class RaceWeekend
    {
        public string Game { get; set; } = string.Empty;
        public string Track { get; set; } = string.Empty;
        public DateTime? Date { get; set; }

        public List<Session> Sessions { get; set; } = new();
    }
}
