using InLap.App.Domain;
using InLap.App.DTOs;

namespace InLap.App.Interfaces
{
    public interface ISummaryService
    {
        /// <summary>
        /// Composes a summary from a parsed race weekend.
        /// </summary>
        /// <param name="weekend">The parsed race weekend.</param>
        /// <param name="warnings">Warnings from parsing.</param>
        /// <returns>The composed summary.</returns>
        WeekendSummaryDto Compose(RaceWeekend weekend, DTOs.ParseWarnings warnings);
    }
}
