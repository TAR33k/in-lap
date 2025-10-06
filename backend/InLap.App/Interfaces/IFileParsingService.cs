using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using InLap.App.Domain;
using InLap.App.DTOs;

namespace InLap.App.Interfaces
{
    public interface IFileParsingService
    {
        /// <summary>
        /// Parses a CSV stream into a RaceWeekend object.
        /// </summary>
        /// <param name="csv">The CSV stream to parse.</param>
        /// <param name="ct">Cancellation token.</param>
        Task<(RaceWeekend weekend, ParseWarnings warnings)> ParseAsync(Stream csv, CancellationToken ct = default);
    }
}
