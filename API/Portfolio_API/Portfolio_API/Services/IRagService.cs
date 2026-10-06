using Portfolio_API.Dtos;

namespace Portfolio_API.Services
{
    public interface IRagService
    {
        Task<AskAiResponse> GetAnswerAsync(string query, CancellationToken ct);
    }
}
