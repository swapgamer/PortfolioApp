namespace Portfolio_API.Dtos
{
    public record AskAiResponse(string Answer, IReadOnlyList<string> Sources);
}
