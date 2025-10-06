namespace InLap.App.Domain
{
    public class LapRecord
    {
        public int Lap { get; set; }
        public string Time { get; set; } = string.Empty;
        public string? Sector1 { get; set; }
        public string? Sector2 { get; set; }
        public string? Sector3 { get; set; }
        public bool Pit { get; set; }
        public string? Compound { get; set; }
        public int Cuts { get; set; }
    }
}
