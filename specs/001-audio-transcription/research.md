# Research: Audio Transcription

## Decision: Use Local Files plus JSON Metadata

**Rationale**: The MVP needs to store original uploads, transcripts, downloads,
and simple status metadata for a local single-user workflow. A directory per job
with `metadata.json` keeps the implementation understandable and avoids a
database until querying or concurrency proves necessary.

**Alternatives considered**:
- SQLite: useful later for filtering, history, and stronger transactional
  behavior, but unnecessary for the first local workflow.
- In-memory state: simpler, but loses status and results across process restart.

## Decision: Keep Processing In-Process for MVP

**Rationale**: Queues are out of scope for v1. The upload endpoint can create a
job record, save the original file, and run the processing orchestration inside
the local API process. The service boundary keeps a later background worker
possible without changing endpoint contracts.

**Alternatives considered**:
- Background queue: better for long jobs, but explicitly out of scope.
- Blocking upload until completion: simplest for short files, but gives a worse
  status/result workflow and can time out for longer audio.

## Decision: Wrap OpenAI Behind Interfaces

**Rationale**: The feature depends on transcription and text improvement, but
tests must be deterministic and must verify error handling without real network
calls. Interfaces allow fake implementations in xUnit and keep provider-specific
details outside vertical slices.

**Alternatives considered**:
- Direct SDK calls in endpoint handlers: faster to write, but hard to test and
  spreads provider details across slices.
- Mock HTTP at a lower level only: useful for adapter tests, but feature tests
  still benefit from service-level fakes.

## Decision: Isolate FFmpeg Behind `IAudioProcessor`

**Rationale**: FFmpeg is required for conversion/chunking, but chunking can be a
later task. A dedicated audio service can probe or prepare audio now and own
future chunking without changing API contracts.

**Alternatives considered**:
- Calling FFmpeg directly from upload processing: simpler initially, but mixes
  process execution with feature orchestration.
- No FFmpeg boundary until chunking exists: risks redesign when chunking arrives.

## Decision: Use Swagger/OpenAPI as the API Contract

**Rationale**: The feature is an HTTP API with known endpoints. An OpenAPI file
documents request/response schemas, error responses, and download behavior before
implementation.

**Alternatives considered**:
- Markdown-only endpoint list: easy to read, but less precise for tests and
  client expectations.

## Decision: xUnit Test Layers

**Rationale**: Unit tests cover validation, state transitions, storage, exporter,
and transcript integrity rules. Integration tests with fakes cover endpoint
behavior without real OpenAI or FFmpeg calls.

**Alternatives considered**:
- Manual Swagger testing only: insufficient for constitution-required tests.
- End-to-end tests with real OpenAI by default: costly, unstable, and unsuitable
  for routine local validation.
