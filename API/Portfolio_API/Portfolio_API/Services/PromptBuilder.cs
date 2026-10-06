using Portfolio_API.Models;

namespace Portfolio_API.Services
{
    public static class PromptBuilder
    {
        private const string SystemInstruction =
            "You are the AI assistant embedded in a personal portfolio website. " +
            "Answer the visitor's question using ONLY the information in the provided context. " +
            "If the context does not contain enough information to answer, say you don't have " +
            "that information -- do not guess or make anything up.";

        public static (string System, string User) Build(string query, IReadOnlyList<ContentChunk> contextChunks)
        {
            var contextBlock = string.Join(
                "\n\n",
                contextChunks.Select((c, i) => $"[{i + 1}] ({c.Section}): {c.Text}"));

            var userPrompt = $"Context:\n{contextBlock}\n\nQuestion: {query}";

            return (SystemInstruction, userPrompt);
        }
    }
}
