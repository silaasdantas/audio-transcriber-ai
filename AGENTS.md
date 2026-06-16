<!-- SPECKIT START -->
For additional context about technologies to be used, project structure,
shell commands, and other important information, read
specs/001-audio-transcription/plan.md
<!-- SPECKIT END -->

# Project Notes for Agents

This repository contains a local .NET 8 Web API MVP for audio transcription.
The active feature spec is `specs/001-audio-transcription/`.

## Current Implementation State

- User Story 1 is implemented: upload MP3/WAV/M4A audio, persist the original
  file and metadata, generate `raw.txt`, and expose `GET /api/transcriptions/{id}/raw`.
- User Story 2 is implemented: improve the raw transcript with OpenAI, persist
  `improved.txt`, and expose `GET /api/transcriptions/{id}/improved`.
- User Story 3 is not complete yet: status and Markdown download endpoints are
  still planned tasks.

## Local Commands

Run these from the repository root:

```powershell
dotnet restore AudioTranscriberAI.sln
dotnet build AudioTranscriberAI.sln
dotnet test AudioTranscriberAI.sln
dotnet run --project src/AudioTranscriberAI.Api/AudioTranscriberAI.Api.csproj
```

Set the OpenAI API key before running real transcription:

```powershell
$env:OPENAI_API_KEY="your-openai-api-key"
```

## Important Implementation Details

- Do not commit API keys, generated audio data, transcript contents, or session
  rollout JSONL files.
- Uploads are stored locally under the configured `Transcriptions:StorageRoot`.
- The upload endpoint reads multipart form data manually from
  `request.ReadFormAsync()` and expects a form file named `file`.
- Swagger must document the upload body as `multipart/form-data` with a `file`
  binary property; otherwise Swagger UI can send an empty multipart request.
- The current audio processor is an MVP pass-through boundary. Chunking is
  intentionally deferred behind `IAudioProcessor`.
- The current transcript improvement step sends the full raw transcript to the
  configured OpenAI text model and must preserve original meaning.
