using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using InLap.App.DTOs;

namespace InLap.App.Parsing
{
    public static class LinesTokenizer
    {
        public static List<string> Tokenize(Stream csvStream, ParseWarnings warnings)
        {
            if (csvStream == null) throw new ArgumentNullException(nameof(csvStream));
            if (warnings == null) throw new ArgumentNullException(nameof(warnings));

            csvStream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(csvStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);
            var lines = new List<string>();
            string? line;
            int lineNo = 0;
            while ((line = reader.ReadLine()) != null)
            {
                lineNo++;
                var trimmed = line.Trim();
                if (trimmed.Length == 0) { lines.Add(string.Empty); continue; }

                if (trimmed.All(ch => ch == '+' || ch == '-' || ch == '='))
                {
                    continue;
                }

                if (trimmed.StartsWith("'"))
                {
                    trimmed = trimmed.TrimStart('\'', ' ');
                }

                lines.Add(trimmed);
            }
            return lines;
        }
    }
}
