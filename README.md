# AudioTranscriberAI

AudioTranscriberAI is a local MVP for uploading audio files, generating raw transcripts, improving those transcripts with AI, and storing the results on the local filesystem.

The project is currently a .NET 8 Web API. It supports MP3, WAV, and M4A uploads, sends audio to OpenAI for transcription, improves the transcript for readability while preserving meaning, and exposes endpoints for raw and improved transcript text.

This MVP intentionally does not include a frontend, authentication, users, payments, background queues, cloud storage, a database, or SaaS deployment features.

## Current Capabilities

- Upload an audio file with `POST /api/transcriptions`.
- Store original audio, metadata, and transcript artifacts under `data/transcriptions`.
- Generate and persist a raw transcript as `raw.txt`.
- Generate and persist an improved transcript as `improved.txt`.
- Read the raw transcript with `GET /api/transcriptions/{id}/raw`.
- Read the improved transcript with `GET /api/transcriptions/{id}/improved`.
- Explore the API through Swagger in local development.

Status and Markdown download endpoints are part of the planned MVP workflow, but should not be treated as completed unless the corresponding implementation tasks are finished.

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- FFmpeg installed and available on `PATH`, or configured through `Transcriptions:FfmpegPath`
- An OpenAI API key set in the `OPENAI_API_KEY` environment variable
- Internet access for OpenAI API calls
- Optional: a small `.mp3`, `.wav`, or `.m4a` file for manual testing

## Configuration

Default non-secret configuration lives in:

```text
src/AudioTranscriberAI.Api/appsettings.json
```

The main settings are:

| Setting | Purpose |
| --- | --- |
| `Transcriptions:StorageRoot` | Local folder used for uploaded files, metadata, and transcript artifacts. Defaults to `data/transcriptions`. |
| `Transcriptions:MaxUploadBytes` | Maximum accepted upload size in bytes. |
| `Transcriptions:FfmpegPath` | FFmpeg executable path. Defaults to `ffmpeg`, which assumes it is available on `PATH`. |
| `Transcriptions:DownloadFilePrefix` | Prefix reserved for generated transcript download file names. |
| `OpenAI:TranscriptionModel` | OpenAI audio transcription model. |
| `OpenAI:TextModel` | OpenAI text model used to improve transcripts. |

Secrets should not be committed to `appsettings.json`. Set the API key through the environment:

```powershell
$env:OPENAI_API_KEY="your-openai-api-key"
```

## Run Locally

From the repository root:

```powershell
dotnet restore AudioTranscriberAI.sln
dotnet build AudioTranscriberAI.sln
dotnet test AudioTranscriberAI.sln
dotnet run --project src/AudioTranscriberAI.Api/AudioTranscriberAI.Api.csproj
```

When the API starts, ASP.NET Core prints the local URL, such as `http://localhost:5000` or `https://localhost:7000`. Open Swagger by adding `/swagger`:

```text
http://localhost:5000/swagger
```

Swagger is usually the easiest way to test multipart file upload from a local browser.

## API Examples

Upload a supported audio file:

```bash
curl -X POST "http://localhost:5000/api/transcriptions" \
  -F "file=@sample.mp3"
```

The response returns an `id`. Use that id to read the raw transcript:

```bash
curl "http://localhost:5000/api/transcriptions/{id}/raw"
```

Read the improved transcript:

```bash
curl "http://localhost:5000/api/transcriptions/{id}/improved"
```

On Windows PowerShell, `curl` is often an alias for `Invoke-WebRequest`; Swagger may be simpler for upload testing. If you want to use the real curl executable, run `curl.exe`.

## Local Data Layout

Each transcription job is stored under the configured storage root:

```text
data/transcriptions/{id}/
|-- metadata.json
|-- original/
|-- raw.txt
`-- improved.txt
```

Generated local data is ignored by Git.

## Development Notes

The API is organized around vertical transcription slices under `Features/Transcriptions`, with infrastructure adapters for local storage, FFmpeg, and OpenAI under `Infrastructure`.

Automated tests live in:

```text
tests/AudioTranscriberAI.Tests
```

Run the test suite with:

```powershell
dotnet test AudioTranscriberAI.sln
```
