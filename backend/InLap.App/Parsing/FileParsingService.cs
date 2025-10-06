using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using InLap.App.Domain;
using InLap.App.DTOs;
using InLap.App.Interfaces;

namespace InLap.App.Parsing
{
    public class FileParsingService : IFileParsingService
    {
        public async Task<(RaceWeekend weekend, ParseWarnings warnings)> ParseAsync(Stream csv, CancellationToken ct = default)
        {
            if (csv == null) throw new ArgumentNullException(nameof(csv));

            await using var ms = new MemoryStream();
            await csv.CopyToAsync(ms, ct);
            ms.Seek(0, SeekOrigin.Begin);

            var warnings = new ParseWarnings();
            var lines = LinesTokenizer.Tokenize(ms, warnings);
            var (weekend, warn) = ParseOrchestrator.Parse(lines);

            foreach (var w in warn.Items)
                warnings.Items.Add(w);

            return (weekend, warnings);
        }
    }
}
