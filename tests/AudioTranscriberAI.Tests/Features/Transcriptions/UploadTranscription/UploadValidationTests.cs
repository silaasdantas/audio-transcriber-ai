using AudioTranscriberAI.Api.Features.Transcriptions.UploadTranscription;
using AudioTranscriberAI.Api.Infrastructure.Configuration;

namespace AudioTranscriberAI.Tests.Features.Transcriptions.UploadTranscription;

public sealed class UploadValidationTests
{
    private static readonly TranscriptionOptions Options = new()
    {
        MaxUploadBytes = 100
    };

    [Theory]
    [InlineData("sample.mp3", "mp3")]
    [InlineData("sample.WAV", "wav")]
    [InlineData("sample.M4a", "m4a")]
    public void Validate_accepts_supported_extensions_case_insensitively(string fileName, string expectedExtension)
    {
        var result = UploadTranscriptionValidator.Validate(fileName, 10, Options);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedExtension, result.Value!.Extension);
    }

    [Theory]
    [InlineData("sample.txt")]
    [InlineData("sample")]
    [InlineData("")]
    public void Validate_rejects_unsupported_or_missing_extensions(string fileName)
    {
        var result = UploadTranscriptionValidator.Validate(fileName, 10, Options);

        Assert.False(result.IsSuccess);
        Assert.Equal("upload.unsupported_format", result.Error!.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_rejects_empty_files(long sizeBytes)
    {
        var result = UploadTranscriptionValidator.Validate("sample.mp3", sizeBytes, Options);

        Assert.False(result.IsSuccess);
        Assert.Equal("upload.empty_file", result.Error!.Code);
    }

    [Fact]
    public void Validate_rejects_files_over_configured_limit()
    {
        var result = UploadTranscriptionValidator.Validate("sample.mp3", 101, Options);

        Assert.False(result.IsSuccess);
        Assert.Equal("upload.file_too_large", result.Error!.Code);
    }
}
