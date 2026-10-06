using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Portfolio_API.Services
{
    // Calls OpenAI's Chat Completions API (https://api.openai.com/v1/chat/completions).
    // The HttpClient (base address + Authorization header) is configured once in Program.cs
    // via AddHttpClient, using the key from configuration ("OpenAI:ApiKey" -- User Secrets
    // locally, environment variable/Key Vault in production; never committed to appsettings).
    public class OpenAiClient : ILlmClient
    {
        private const string Model = "gpt-4o-mini";

        private readonly HttpClient _http;
        private readonly ILogger<OpenAiClient> _logger;

        public OpenAiClient(HttpClient http, ILogger<OpenAiClient> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken ct)
        {
            var requestBody = new ChatCompletionRequest(
                Model,
                new[]
                {
                    new ChatMessage("system", systemPrompt),
                    new ChatMessage("user", userPrompt)
                },
                Temperature: 0.2);

            HttpResponseMessage response;
            try
            {
                response = await _http.PostAsJsonAsync("v1/chat/completions", requestBody, ct);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                _logger.LogError(ex, "OpenAI request failed");
                throw new LlmUnavailableException("The AI provider is currently unreachable.", ex);
            }

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("OpenAI returned {StatusCode}: {Body}", response.StatusCode, body);
                throw new LlmUnavailableException($"The AI provider returned an error ({(int)response.StatusCode}).");
            }

            var result = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(cancellationToken: ct);
            var answer = result?.Choices?.FirstOrDefault()?.Message?.Content;

            if (string.IsNullOrWhiteSpace(answer))
                throw new LlmUnavailableException("The AI provider returned an empty response.");

            return answer.Trim();
        }

        private record ChatCompletionRequest(
            [property: JsonPropertyName("model")] string Model,
            [property: JsonPropertyName("messages")] ChatMessage[] Messages,
            [property: JsonPropertyName("temperature")] double Temperature);

        private record ChatMessage(
            [property: JsonPropertyName("role")] string Role,
            [property: JsonPropertyName("content")] string Content);

        private record ChatCompletionResponse(
            [property: JsonPropertyName("choices")] ChatChoice[]? Choices);

        private record ChatChoice(
            [property: JsonPropertyName("message")] ChatMessage? Message);
    }
}
