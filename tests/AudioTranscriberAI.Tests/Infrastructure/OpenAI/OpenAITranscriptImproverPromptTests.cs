using AudioTranscriberAI.Api.Infrastructure.OpenAI;

namespace AudioTranscriberAI.Tests.Infrastructure.OpenAI;

public sealed class OpenAITranscriptImproverPromptTests
{
    [Fact]
    public void Build_instructs_model_to_preserve_meaning_and_mark_uncertainty()
    {
        const string rawTranscript = "maria said the invoice was 42 reais and [unclear]";

        var prompt = TranscriptImprovementPromptBuilder.Build(rawTranscript);

        Assert.Contains("preserve the original meaning", prompt, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("do not invent", prompt, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("do not add", prompt, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("[unclear]", prompt, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(rawTranscript, prompt, StringComparison.Ordinal);
    }
}
