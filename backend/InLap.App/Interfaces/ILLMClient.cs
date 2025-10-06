using System.Threading;
using System.Threading.Tasks;

namespace InLap.App.Interfaces
{
    public interface ILLMClient
    {
        /// <summary>
        /// Completes a prompt using the specified system prompt.
        /// </summary>
        /// <param name="prompt">The prompt to complete.</param>
        /// <param name="systemPrompt">The system prompt to use.</param>
        /// <param name="ct">Cancellation token.</param>
        Task<string> CompleteAsync(string prompt, string systemPrompt, CancellationToken ct = default);
    }
}
