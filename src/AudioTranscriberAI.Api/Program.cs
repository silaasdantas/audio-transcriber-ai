using AudioTranscriberAI.Api.Features.Transcriptions.GetRawTranscript;
using AudioTranscriberAI.Api.Features.Transcriptions.Shared;
using AudioTranscriberAI.Api.Features.Transcriptions.UploadTranscription;
using AudioTranscriberAI.Api.Infrastructure.Audio;
using AudioTranscriberAI.Api.Infrastructure.Configuration;
using AudioTranscriberAI.Api.Infrastructure.OpenAI;
using AudioTranscriberAI.Api.Infrastructure.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration["OpenAI:ApiKey"] =
    Environment.GetEnvironmentVariable("OPENAI_API_KEY")
    ?? builder.Configuration["OpenAI:ApiKey"];

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<ITranscriptionStore, JsonTranscriptionStore>();
builder.Services.AddSingleton<ILocalFileStorage, LocalFileStorage>();
builder.Services.AddSingleton<IAudioProcessor, FfmpegAudioProcessor>();
builder.Services.AddScoped<ITranscriptionJobProcessor, TranscriptionJobProcessor>();
builder.Services.AddHttpClient<ITranscriptionService, OpenAITranscriptionService>();

builder.Services.AddOptions<TranscriptionOptions>()
    .Bind(builder.Configuration.GetSection(TranscriptionOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.StorageRoot), "Storage root is required.")
    .Validate(options => options.MaxUploadBytes > 0, "Maximum upload size must be greater than zero.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.FfmpegPath), "FFmpeg path is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.DownloadFilePrefix), "Download file prefix is required.");

builder.Services.AddOptions<OpenAIOptions>()
    .Bind(builder.Configuration.GetSection(OpenAIOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.TranscriptionModel), "OpenAI transcription model is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.TextModel), "OpenAI text model is required.");

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

app.MapGet("/", () => Results.Redirect("/swagger"))
    .ExcludeFromDescription();

app.MapUploadTranscription();
app.MapGetRawTranscript();

app.Run();

public partial class Program;
