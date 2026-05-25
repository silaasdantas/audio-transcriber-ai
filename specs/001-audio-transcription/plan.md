# Implementation Plan: Audio Transcription

**Branch**: `001-audio-transcription` | **Date**: 2026-05-25 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-audio-transcription/spec.md`

## Summary

Build the first local MVP slice for uploading MP3, WAV, or M4A audio,
transcribing it with OpenAI, improving the transcript while preserving meaning,
and returning raw/improved transcript downloads as Markdown. Implement as
a .NET 8 Web API using Vertical Slice Architecture, local filesystem storage,
JSON metadata files, xUnit tests, FFmpeg behind an audio processing service, and
Swagger/OpenAPI documentation.

## Technical Context

**Language/Version**: .NET 8 / C#

**Primary Dependencies**: ASP.NET Core Web API, Swagger/OpenAPI, OpenAI API integration, FFmpeg process integration

**Storage**: Local filesystem for original audio, raw transcripts, improved transcripts, downloads, and JSON metadata

**Persistence**: Start without a database. Store one JSON metadata file per transcription job. Revisit SQLite only if querying, concurrency, or recovery needs exceed simple file metadata.

**Testing**: xUnit with focused unit tests, API contract/integration tests, and fake service implementations for OpenAI and FFmpeg boundaries

**Target Platform**: Local .NET 8 Web API runtime

**Project Type**: Web API using Vertical Slice Architecture

**Configuration**: `appsettings.json` for non-secret defaults; environment variables for `OPENAI_API_KEY`, storage root overrides, FFmpeg path, upload limits, and model names

**API Documentation**: Swagger/OpenAPI exposed in local development

**Performance Goals**: Valid uploads receive an accepted response quickly while processing runs in-process; status reflects progress until completion or failure. MVP targets one request-driven job at a time per process and avoids real-time streaming.

**Constraints**: No authentication, users, payments, queues, cloud deployment, S3 storage, speaker diarization, real-time transcription, or advanced audio editing. Do not log API keys, uploaded audio contents, transcript contents, or sensitive local paths.

**Scale/Scope**: Local single-user MVP. Default upload size limit is 100 MB. Chunking is prepared through service boundaries but can be implemented as a separate task if needed.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- MVP scope: PASS. Plan covers local audio upload, transcription, transcript
  improvement, and Markdown export only.
- Architecture: PASS. Backend is .NET 8 Web API with vertical feature folders
  for endpoint, request/response contracts, validation, handler logic, and tests.
- Audio processing: PASS. MP3/WAV/M4A files are stored locally and FFmpeg is
  isolated behind `IAudioProcessor`.
- AI integrity: PASS. OpenAI transcription and improvement are behind service
  interfaces; improvement prompt and tests require preserving meaning and using
  `[unclear]` for uncertain content.
- Quality gate: PASS. xUnit coverage is planned for validation, error handling,
  service failures, file failures, download formats, and transcript integrity.

Post-design re-check: PASS. Research, data model, contracts, and quickstart keep
the implementation inside the constitution and require tests before build-out.

## Architecture

The API is organized as a vertical `Transcriptions` feature. Each endpoint keeps
its request/response DTOs, validation, handler, and endpoint mapping close to the
feature. Infrastructure code is limited to reusable local storage, JSON metadata,
OpenAI clients, FFmpeg adapter, clock/id helpers, and problem details mapping.

Core boundaries:

- `ITranscriptionStore`: reads/writes job metadata and transcript artifact paths.
- `ILocalFileStorage`: stores original uploads and generated download files.
- `IAudioProcessor`: validates/prepares audio and later hosts chunking support.
- `ITranscriptionService`: sends audio to the transcription provider.
- `ITranscriptImprover`: improves raw transcript while preserving meaning.
- `ITranscriptExporter`: creates Markdown download content.
- `ITranscriptionJobProcessor`: orchestrates upload, transcription, improvement,
  persistence, and state transitions.

Processing runs in-process for the MVP after upload acceptance. No queue is added.
If processing becomes long-running enough to require background execution, that
must be planned as a later feature because queues are out of scope for v1.

## Data Flow

1. Client submits `POST /api/transcriptions` with `multipart/form-data` file.
2. Upload slice validates extension, content length, empty file, and configured
   size limit before any processing.
3. Original file is saved locally under `data/transcriptions/{id}/original/`.
4. Metadata JSON is created with status `Pending`, then updated to `Processing`.
5. `IAudioProcessor` probes/prepares audio through FFmpeg. Initial MVP may pass
   through supported audio unchanged when no conversion is needed.
6. `ITranscriptionService` sends the prepared audio to OpenAI transcription.
7. Raw transcript is written to `raw.txt`; metadata stores status progress.
8. `ITranscriptImprover` sends the raw transcript for correction with explicit
   instructions to preserve meaning and mark uncertainty as `[unclear]`.
9. Improved transcript is written to `improved.txt`.
10. Metadata is updated to `Completed`; failures update status to `Failed` with
    a safe user-facing message and a non-secret internal error code.
11. Status, raw, improved, and download endpoints read metadata and artifacts
    from local storage.

## Project Structure

### Documentation (this feature)

```text
specs/001-audio-transcription/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── openapi.yaml
└── tasks.md
```

### Source Code (repository root)

```text
src/
├── AudioTranscriberAI.Api/
│   ├── Program.cs
│   ├── appsettings.json
│   ├── Features/
│   │   └── Transcriptions/
│   │       ├── UploadTranscription/
│   │       ├── GetTranscriptionStatus/
│   │       ├── GetRawTranscript/
│   │       ├── GetImprovedTranscript/
│   │       ├── DownloadTranscript/
│   │       └── Shared/
│   └── Infrastructure/
│       ├── Audio/
│       ├── OpenAI/
│       ├── Storage/
│       ├── Export/
│       └── Errors/

tests/
└── AudioTranscriberAI.Tests/
    ├── Features/
    │   └── Transcriptions/
    ├── Contract/
    ├── Integration/
    └── Fakes/

data/
└── transcriptions/
    └── {id}/
        ├── metadata.json
        ├── original/
        ├── working/
        ├── raw.txt
        ├── improved.txt
        └── downloads/
```

**Structure Decision**: Use one API project and one xUnit test project. Keep
feature behavior in vertical slices under `Features/Transcriptions`; keep only
provider and filesystem adapters under `Infrastructure`.

## API Contract

The canonical contract is [contracts/openapi.yaml](./contracts/openapi.yaml).

Endpoints:

- `POST /api/transcriptions`: accepts `multipart/form-data` file upload and
  returns `202 Accepted` with transcription id and status URL.
- `GET /api/transcriptions/{id}`: returns job status and metadata.
- `GET /api/transcriptions/{id}/raw`: returns raw transcript text after raw
  transcription exists.
- `GET /api/transcriptions/{id}/improved`: returns improved transcript text
  after improvement exists.
- `GET /api/transcriptions/{id}/download?type=raw|improved`:
  returns a downloadable Markdown transcript file.

## Error Handling Strategy

- Use RFC 7807-style problem details for API errors.
- Validation failures return `400 Bad Request` with safe messages for unsupported
  format, empty file, missing file, and file too large.
- Unknown ids return `404 Not Found`.
- Requests for transcripts that are not ready return `409 Conflict` with current
  status.
- Processing failures update metadata to `Failed` and return safe status details
  without secrets, transcript contents, uploaded audio contents, or sensitive
  local paths.
- External provider, FFmpeg, and file I/O errors are logged with correlation/job
  id, sanitized category, and exception type. API keys and full content payloads
  are never logged.
- Unexpected API errors return `500 Internal Server Error` with a generic message
  and logged internal details.

## Testing Strategy

- Unit tests for upload validation: allowed extensions, mixed-case extensions,
  unsupported formats, empty files, missing files, and size limit.
- Unit tests for state transitions: `Pending`, `Processing`, `Completed`,
  `Failed`.
- Unit tests for transcript improvement rules: preserves names/numbers/claims,
  marks ambiguous content as `[unclear]`, rejects or flags suspicious provider
  output that adds unsupported meaning.
- Unit tests for exporters: raw/improved Markdown output names, content type,
  and body.
- Unit tests for JSON metadata store: create, read, update, not found, malformed
  metadata, and write failure behavior.
- Integration tests using `WebApplicationFactory` and fake OpenAI/FFmpeg services
  for upload/status/raw/improved/download endpoints.
- Contract tests assert the OpenAPI paths, response codes, and schemas for all
  expected endpoints.
- Error-path tests cover OpenAI failures, FFmpeg failures, storage failures, and
  safe logging behavior with no API keys or sensitive content.

## Implementation Phases

1. Project setup: create .NET 8 API project, xUnit test project, Swagger, config
   binding, options validation, and dependency injection.
2. Storage foundation: implement local folder layout, metadata JSON store,
   transcript artifact paths, and sanitized logging.
3. Upload/status slice: implement upload validation, original file persistence,
   metadata creation, and status retrieval.
4. Provider boundaries: define OpenAI transcription/improvement interfaces and
   fake test implementations; wire real OpenAI adapters behind configuration.
5. Audio boundary: define FFmpeg audio processor service and MVP probe/prepare
   behavior with future chunking extension points.
6. Processing orchestration: run transcription then improvement, persist raw and
   improved artifacts, and manage failure states.
7. Result/download slices: implement raw, improved, and download endpoints with
   Markdown exporters.
8. Test hardening: complete validation, integration, provider failure, storage
   failure, transcript integrity, and OpenAPI contract coverage.
9. Local verification: run quickstart flow with a small sample audio file and
   confirm Swagger documents the API.

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| OpenAI transcription or improvement fails | User cannot complete transcript workflow | Store failed status, return safe message, log sanitized failure, and keep original upload for retry-oriented future work |
| Improved transcript invents content | Breaks user trust and violates constitution | Use strict improvement prompt, targeted transcript integrity tests, and provider wrapper checks for suspicious additions |
| Large files exceed provider or local limits | Processing fails late or consumes too many resources | Validate size before processing and isolate future chunking behind `IAudioProcessor` |
| FFmpeg missing or misconfigured | Audio preparation fails | Validate FFmpeg path at startup or first use, return safe failed status, and document local setup in quickstart |
| Local storage write/read failure | Uploads or results are lost | Fail safely, update metadata when possible, log sanitized file operation category, and keep storage paths configurable |
| In-process processing blocks longer requests | Poor local UX for longer files | Keep MVP simple; prepare `ITranscriptionJobProcessor` boundary for later background execution without adding queues now |
| JSON metadata becomes corrupted | Status/result endpoints cannot read job state | Write atomically where possible, handle malformed metadata as failed/unavailable, and test corrupted metadata behavior |

## Complexity Tracking

No constitution violations. JSON metadata is chosen instead of SQLite to preserve
MVP simplicity; SQLite remains deferred until persistence needs justify it.
