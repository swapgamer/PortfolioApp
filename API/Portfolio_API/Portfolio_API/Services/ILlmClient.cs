namespace Portfolio_API.Services
{
    public interface ILlmClient
    {
        Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken ct);
    }

    // Thrown when the LLM provider is unreachable, times out, or returns an error.
    // AskAiController maps this to a 502/504 response instead of leaking the raw exception.
    public class LlmUnavailableException : Exception
    {
        public LlmUnavailableException(string message, Exception? inner = null) : base(message, inner) { }
    }
}
