namespace AudioTranscriberAI.Api.Infrastructure.OpenAI;

public static class TranscriptImprovementPromptBuilder
{
    public static string Build(string rawTranscript) =>
        """
        Improve the transcript for punctuation, grammar, paragraph breaks, and readability.

        Rules:
        - Preserve the original meaning exactly.
        - Do not invent facts, names, numbers, dates, claims, or context.
        - Do not add explanations, summaries, speaker labels, or unsupported details.
        - Keep existing uncertainty markers and use [unclear] when a word or phrase cannot be confidently corrected.
        - Return only the improved transcript text.

        Raw transcript:
        """ + Environment.NewLine + rawTranscript;
}
