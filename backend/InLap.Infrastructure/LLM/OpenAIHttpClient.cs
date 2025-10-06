using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using InLap.App.Interfaces;
using Microsoft.Extensions.Configuration;

namespace InLap.Infrastructure.LLM
{
    public class OpenAIHttpClient : ILLMClient
    {
        private readonly HttpClient _http;
        private readonly string _model;

        public OpenAIHttpClient(HttpClient httpClient, IConfiguration configuration)
        {
            _http = httpClient;
            _model = configuration["OPENAI_MODEL"] ?? "gpt-4o-mini";
        }

        public async Task<string> CompleteAsync(string prompt, string systemPrompt, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(prompt)) throw new ArgumentException("Prompt is required.", nameof(prompt));
            systemPrompt ??= "Only use provided summary. Do not invent facts. If uncertain, omit.";

            var payload = new
            {
                model = _model,
                messages = new object[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = prompt }
                },
                temperature = 0.2,
                max_tokens = 800
            };

            using var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            using var resp = await _http.PostAsync("/chat/completions", content, ct);
            resp.EnsureSuccessStatusCode();

            using var stream = await resp.Content.ReadAsStreamAsync(ct);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

            var root = doc.RootElement;
            if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                var first = choices[0];
                if (first.TryGetProperty("message", out var msg) && msg.TryGetProperty("content", out var contentProp))
                {
                    return contentProp.GetString() ?? string.Empty;
                }
            }

            return string.Empty;
        }
    }
}
