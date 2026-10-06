using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Portfolio_API.Data;
using Portfolio_API.Dtos;
using Portfolio_API.Models;

namespace Portfolio_API.Services
{
    // Deliberately simple keyword-overlap retrieval instead of embeddings/a vector DB --
    // justified in Docs/02-High-Level-Design.md section 6: content volume is personal-bio
    // scale, so this is cheap, dependency-free, and good enough. Revisit only if content
    // volume grows enough to need semantic search.
    public class RagService : IRagService
    {
        private const string CacheKey = "content-chunks";
        private const int TopN = 5;

        private const string NoMatchFallbackAnswer =
            "I don't have specific information about that in my profile yet. Try asking about " +
            "my skills, experience, projects, or education.";

        private static readonly char[] WordSeparators =
            { ' ', '\t', '\n', '\r', '.', ',', '!', '?', ';', ':', '"', '\'', '(', ')' };

        // Filtered out of both the query and chunk text before scoring -- without this, common
        // words like "is"/"the"/"on" match almost any English sentence and the no-match fallback
        // (bestScore == 0) never triggers, even for genuinely off-topic questions.
        private static readonly HashSet<string> StopWords = new()
        {
            "a", "an", "the", "is", "are", "was", "were", "be", "been", "being",
            "what", "which", "who", "whom", "this", "that", "these", "those",
            "on", "in", "at", "for", "to", "of", "with", "and", "or", "but",
            "do", "does", "did", "he", "she", "it", "his", "her", "its",
            "have", "has", "had", "i", "you", "your", "my", "me", "we", "us",
            "our", "their", "they", "them", "can", "could", "will", "would",
            "should", "about", "like", "how", "when", "where", "why"
        };

        private readonly ApplicationDbContext _db;
        private readonly ILlmClient _llm;
        private readonly IMemoryCache _cache;
        private readonly ILogger<RagService> _logger;

        public RagService(ApplicationDbContext db, ILlmClient llm, IMemoryCache cache, ILogger<RagService> logger)
        {
            _db = db;
            _llm = llm;
            _cache = cache;
            _logger = logger;
        }

        public async Task<AskAiResponse> GetAnswerAsync(string query, CancellationToken ct)
        {
            var chunks = await _cache.GetOrCreateAsync(CacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                return await _db.ContentChunks.AsNoTracking().ToListAsync(ct);
            }) ?? new List<ContentChunk>();

            var ranked = RankByKeywordOverlap(chunks, query, TopN);
            var topChunks = ranked.Select(r => r.Chunk).ToList();
            var bestScore = ranked.Count > 0 ? ranked[0].Score : 0;

            string answer;
            bool usedLlm;
            try
            {
                var (systemPrompt, userPrompt) = PromptBuilder.Build(query, topChunks);
                answer = await _llm.CompleteAsync(systemPrompt, userPrompt, ct);
                usedLlm = true;
            }
            catch (LlmUnavailableException ex)
            {
                // No LLM configured yet (or the provider is down) -- fall back to returning the
                // best-matching content directly instead of failing the request. This keeps the
                // feature usable end-to-end today, and automatically switches to real generated
                // answers the moment a working OpenAI key is in place -- no flag to flip.
                _logger.LogWarning(ex, "LLM unavailable, falling back to extractive answer");
                answer = bestScore > 0 ? topChunks[0].Text : NoMatchFallbackAnswer;
                usedLlm = false;
            }

            var sources = usedLlm || bestScore > 0
                ? topChunks.Select(c => c.Section).Distinct().ToList()
                : new List<string>();

            return new AskAiResponse(answer, sources);
        }

        private static List<(ContentChunk Chunk, int Score)> RankByKeywordOverlap(
            List<ContentChunk> chunks, string query, int take)
        {
            var queryTokens = Tokenize(query);

            return chunks
                .Select(c => (Chunk: c, Score: Tokenize(c.Text).Count(queryTokens.Contains)))
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Chunk.DisplayOrder)
                .Take(take)
                .ToList();
        }

        private static HashSet<string> Tokenize(string text) =>
            text.ToLowerInvariant()
                .Split(WordSeparators, StringSplitOptions.RemoveEmptyEntries)
                .Where(token => !StopWords.Contains(token))
                .ToHashSet();
    }
}
