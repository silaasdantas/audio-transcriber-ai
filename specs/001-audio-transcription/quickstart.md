# Quickstart: Audio Transcription

## Prerequisites

- .NET 8 SDK installed.
- FFmpeg installed and available on `PATH`, or configured through application
  settings.
- OpenAI API key available in an environment variable.
- A small MP3, WAV, or M4A sample file for local testing.

## Configuration

Set secrets through environment variables:

```powershell
$env:OPENAI_API_KEY = "your-api-key"
```

Non-secret local defaults belong in `src/AudioTranscriberAI.Api/appsettings.json`:

```json
{
  "Transcriptions": {
    "StorageRoot": "data/transcriptions",
    "MaxUploadBytes": 104857600,
    "FfmpegPath": "ffmpeg",
    "DownloadFilePrefix": "transcript"
  },
  "OpenAI": {
    "TranscriptionModel": "whisper-1",
    "TextModel": "gpt-4.1-mini"
  }
}
```

## Run Locally

```powershell
dotnet restore
dotnet build
dotnet run --project src/AudioTranscriberAI.Api
```

Open Swagger in the browser at the URL printed by the API, typically:

```text
https://localhost:5001/swagger
```

## Smoke Test Flow

Upload a valid audio file:

```powershell
curl.exe -X POST "https://localhost:5001/api/transcriptions" `
  -F "file=@C:\path\to\sample.mp3"
```

Check status:

```powershell
curl.exe "https://localhost:5001/api/transcriptions/{id}"
```

Read raw transcript:

```powershell
curl.exe "https://localhost:5001/api/transcriptions/{id}/raw"
```

Read improved transcript:

```powershell
curl.exe "https://localhost:5001/api/transcriptions/{id}/improved"
```

Download improved Markdown:

```powershell
curl.exe -L "https://localhost:5001/api/transcriptions/{id}/download?type=improved" `
  -o improved-transcript.md
```

## Test

```powershell
dotnet test
```

Expected coverage includes upload validation, unsupported formats, oversized
files, fake OpenAI and FFmpeg failures, missing API key behavior, JSON metadata
behavior, raw/improved Markdown downloads, and transcript integrity rules.
