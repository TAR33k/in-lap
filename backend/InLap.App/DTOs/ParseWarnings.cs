using System.Collections.Generic;

namespace InLap.App.DTOs
{
    public class ParseWarning
    {
        public int? LineNumber { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class ParseWarnings
    {
        public List<ParseWarning> Items { get; set; } = new();

        public void Add(int? lineNumber, string message)
        {
            Items.Add(new ParseWarning { LineNumber = lineNumber, Message = message });
        }

        public bool HasWarnings => Items.Count > 0;
    }
}
