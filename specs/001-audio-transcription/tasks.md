---
description: "Implementation tasks for 001-audio-transcription"
---

# Tasks: Audio Transcription

**Input**: Design documents from `/specs/001-audio-transcription/`

**Prerequisites**: [plan.md](./plan.md), [spec.md](./spec.md), [research.md](./research.md), [data-model.md](./data-model.md), [contracts/openapi.yaml](./contracts/openapi.yaml), [quickstart.md](./quickstart.md)

**Tests**: Required by the project constitution. Write tests before implementation tasks in each user story where practical.

**Scope**: Backend API and local file processing only. No frontend, database, authentication, queues, cloud storage, or SaaS features.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel because it touches different files and has no dependency on incomplete tasks
- **[Story]**: User story label for story-specific work only: `[US1]`, `[US2]`, `[US3]`
- Every task names the file or directory to create or change

## Phase 1: Project Setup

**Purpose**: Create the backend API and test project skeleton.

- [X] T001 Create solution file `AudioTranscriberAI.sln` at repository root
- [X] T002 Create .NET 8 Web API project in `src/AudioTranscriberAI.Api/AudioTranscriberAI.Api.csproj`
- [X] T003 Create xUnit test project in `tests/AudioTranscriberAI.Tests/AudioTranscriberAI.Tests.csproj`
- [X] T004 Add project reference from `tests/AudioTranscriberAI.Tests/AudioTranscriberAI.Tests.csproj` to `src/AudioTranscriberAI.Api/AudioTranscriberAI.Api.csproj`
- [X] T005 Configure Swagger/OpenAPI services in `src/AudioTranscriberAI.Api/Program.cs`
- [X] T006 [P] Create vertical slice folder structure under `src/AudioTranscriberAI.Api/Features/Transcriptions/`
- [X] T007 [P] Create infrastructure folder structure under `src/AudioTranscriberAI.Api/Infrastructure/`
- [X] T008 [P] Create test folder structure under `tests/AudioTranscriberAI.Tests/Features/Transcriptions/`, `tests/AudioTranscriberAI.Tests/Integration/`, `tests/AudioTranscriberAI.Tests/Contract/`, and `tests/AudioTranscriberAI.Tests/Fakes/`

---

## Phase 2: Configuration and Foundational Model

**Purpose**: Add configuration, domain types, storage abstractions, provider abstractions, and error primitives required by all stories.

- [X] T009 Create transcription configuration defaults in `src/AudioTranscriberAI.Api/appsettings.json`
- [X] T010 Create options classes `TranscriptionOptions` and `OpenAIOptions` in `src/AudioTranscriberAI.Api/Infrastructure/Configuration/`
- [X] T011 Register options binding and validation for upload size, storage root, FFmpeg path, and OpenAI settings in `src/AudioTranscriberAI.Api/Program.cs`
- [X] T012 [P] Create `TranscriptionJob`, `TranscriptionStatus`, `AudioUpload`, `TranscriptArtifact`, and `DownloadArtifact` domain records in `src/AudioTranscriberAI.Api/Features/Transcriptions/Shared/`
- [X] T013 [P] Create service result and error types in `src/AudioTranscriberAI.Api/Features/Transcriptions/Shared/TranscriptionErrors.cs`
- [X] T014 [P] Create `ITranscriptionStore` interface in `src/AudioTranscriberAI.Api/Features/Transcriptions/Shared/ITranscriptionStore.cs`
- [X] T015 [P] Create `ILocalFileStorage` interface in `src/AudioTranscriberAI.Api/Features/Transcriptions/Shared/ILocalFileStorage.cs`
- [X] T016 [P] Create `IAudioProcessor` interface in `src/AudioTranscriberAI.Api/Features/Transcriptions/Shared/IAudioProcessor.cs`
- [X] T017 [P] Create `ITranscriptionService` interface in `src/AudioTranscriberAI.Api/Features/Transcriptions/Shared/ITranscriptionService.cs`
- [X] T018 [P] Create `ITranscriptImprover` interface in `src/AudioTranscriberAI.Api/Features/Transcriptions/Shared/ITranscriptImprover.cs`
- [X] T019 [P] Create `ITranscriptExporter` interface in `src/AudioTranscriberAI.Api/Features/Transcriptions/Shared/ITranscriptExporter.cs`
- [X] T020 [P] Create `ITranscriptionJobProcessor` interface in `src/AudioTranscriberAI.Api/Features/Transcriptions/Shared/ITranscriptionJobProcessor.cs`
- [X] T021 Create RFC 7807 problem details mapping helpers in `src/AudioTranscriberAI.Api/Infrastructure/Errors/`
- [X] T022 Register foundational services and interface placeholders in `src/AudioTranscriberAI.Api/Program.cs`

---

## Phase 3: User Story 1 - Upload Audio and Receive Raw Transcript (Priority: P1)

**Goal**: A user uploads a supported MP3, WAV, or M4A file and receives a raw transcript.

**Independent Test**: Upload a valid audio file through `POST /api/transcriptions`, verify the original file is stored locally, metadata is created, processing completes, and raw transcript text is available.

### Tests for User Story 1

- [X] T023 [P] [US1] Add upload validation unit tests in `tests/AudioTranscriberAI.Tests/Features/Transcriptions/UploadTranscription/UploadValidationTests.cs`
- [X] T024 [P] [US1] Add JSON metadata store tests in `tests/AudioTranscriberAI.Tests/Infrastructure/Storage/JsonTranscriptionStoreTests.cs`
- [X] T025 [P] [US1] Add local file storage tests in `tests/AudioTranscriberAI.Tests/Infrastructure/Storage/LocalFileStorageTests.cs`
- [X] T026 [P] [US1] Add raw transcription orchestration tests with fake audio and fake transcription services in `tests/AudioTranscriberAI.Tests/Features/Transcriptions/TranscriptionJobProcessorTests.cs`
- [X] T027 [P] [US1] Add API integration tests for upload and raw result endpoints in `tests/AudioTranscriberAI.Tests/Integration/TranscriptionsUploadAndRawTests.cs`

### Implementation for User Story 1

- [X] T028 [US1] Implement upload request validation in `src/AudioTranscriberAI.Api/Features/Transcriptions/UploadTranscription/UploadTranscriptionValidator.cs`
- [X] T029 [US1] Implement `JsonTranscriptionStore` in `src/AudioTranscriberAI.Api/Infrastructure/Storage/JsonTranscriptionStore.cs`
- [X] T030 [US1] Implement `LocalFileStorage` in `src/AudioTranscriberAI.Api/Infrastructure/Storage/LocalFileStorage.cs`
- [X] T031 [US1] Implement MVP `FfmpegAudioProcessor` probe/prepare behavior in `src/AudioTranscriberAI.Api/Infrastructure/Audio/FfmpegAudioProcessor.cs`
- [X] T032 [US1] Implement OpenAI transcription adapter in `src/AudioTranscriberAI.Api/Infrastructure/OpenAI/OpenAITranscriptionService.cs`
- [X] T033 [US1] Implement transcription job orchestration for upload, raw transcription, metadata updates, and failed state handling in `src/AudioTranscriberAI.Api/Features/Transcriptions/Shared/TranscriptionJobProcessor.cs`
- [X] T034 [US1] Implement upload endpoint `POST /api/transcriptions` in `src/AudioTranscriberAI.Api/Features/Transcriptions/UploadTranscription/UploadTranscriptionEndpoint.cs`
- [X] T035 [US1] Implement raw transcript endpoint `GET /api/transcriptions/{id}/raw` in `src/AudioTranscriberAI.Api/Features/Transcriptions/GetRawTranscript/GetRawTranscriptEndpoint.cs`
- [X] T036 [US1] Register US1 concrete services and endpoints in `src/AudioTranscriberAI.Api/Program.cs`

**Checkpoint**: US1 is complete when upload validation works, original files and metadata are stored locally, raw transcription is persisted, and `/raw` returns transcript text.

---

## Phase 4: User Story 2 - Improve Transcript Without Changing Meaning (Priority: P2)

**Goal**: A user receives an improved transcript with better punctuation, grammar, paragraphs, and readability without invented content.

**Independent Test**: Start from a raw transcript, run improvement with a fake text model, and verify the improved transcript preserves meaning and marks uncertainty as `[unclear]`.

### Tests for User Story 2

- [ ] T037 [P] [US2] Add transcript improvement prompt tests in `tests/AudioTranscriberAI.Tests/Infrastructure/OpenAI/OpenAITranscriptImproverPromptTests.cs`
- [ ] T038 [P] [US2] Add transcript integrity tests for preserving names, numbers, claims, and `[unclear]` markers in `tests/AudioTranscriberAI.Tests/Features/Transcriptions/TranscriptIntegrityTests.cs`
- [ ] T039 [P] [US2] Add improved transcript endpoint integration tests in `tests/AudioTranscriberAI.Tests/Integration/ImprovedTranscriptTests.cs`

### Implementation for User Story 2

- [ ] T040 [US2] Implement OpenAI text improvement adapter in `src/AudioTranscriberAI.Api/Infrastructure/OpenAI/OpenAITranscriptImprover.cs`
- [ ] T041 [US2] Add transcript improvement prompt builder in `src/AudioTranscriberAI.Api/Infrastructure/OpenAI/TranscriptImprovementPromptBuilder.cs`
- [ ] T042 [US2] Extend `TranscriptionJobProcessor` in `src/AudioTranscriberAI.Api/Features/Transcriptions/Shared/TranscriptionJobProcessor.cs` to persist `improved.txt` after raw transcription succeeds
- [ ] T043 [US2] Implement improved transcript endpoint `GET /api/transcriptions/{id}/improved` in `src/AudioTranscriberAI.Api/Features/Transcriptions/GetImprovedTranscript/GetImprovedTranscriptEndpoint.cs`
- [ ] T044 [US2] Register US2 concrete services and endpoint in `src/AudioTranscriberAI.Api/Program.cs`

**Checkpoint**: US2 is complete when improved transcripts are persisted, preserve meaning, use `[unclear]` for uncertainty, and `/improved` returns the improved text.

---

## Phase 5: User Story 3 - Track Status and Download Transcripts (Priority: P3)

**Goal**: A user checks processing status and downloads raw or improved transcript output as Markdown.

**Independent Test**: Upload a valid file, query status, then download raw and improved transcript files in Markdown format after completion.

### Tests for User Story 3

- [ ] T045 [P] [US3] Add status endpoint integration tests in `tests/AudioTranscriberAI.Tests/Integration/TranscriptionStatusTests.cs`
- [ ] T046 [P] [US3] Add transcript exporter unit tests for Markdown output in `tests/AudioTranscriberAI.Tests/Infrastructure/Export/TranscriptExporterTests.cs`
- [ ] T047 [P] [US3] Add download endpoint integration tests for raw and improved Markdown in `tests/AudioTranscriberAI.Tests/Integration/TranscriptDownloadTests.cs`
- [ ] T048 [P] [US3] Add OpenAPI contract tests for all five endpoints in `tests/AudioTranscriberAI.Tests/Contract/OpenApiContractTests.cs`

### Implementation for User Story 3

- [ ] T049 [US3] Implement status response mapping in `src/AudioTranscriberAI.Api/Features/Transcriptions/GetTranscriptionStatus/GetTranscriptionStatusResponse.cs`
- [ ] T050 [US3] Implement status endpoint `GET /api/transcriptions/{id}` in `src/AudioTranscriberAI.Api/Features/Transcriptions/GetTranscriptionStatus/GetTranscriptionStatusEndpoint.cs`
- [ ] T051 [US3] Implement Markdown transcript exporter in `src/AudioTranscriberAI.Api/Infrastructure/Export/TranscriptExporter.cs`
- [ ] T052 [US3] Implement download request validation in `src/AudioTranscriberAI.Api/Features/Transcriptions/DownloadTranscript/DownloadTranscriptValidator.cs`
- [ ] T053 [US3] Implement download endpoint `GET /api/transcriptions/{id}/download?type=raw|improved` in `src/AudioTranscriberAI.Api/Features/Transcriptions/DownloadTranscript/DownloadTranscriptEndpoint.cs`
- [ ] T054 [US3] Update Swagger metadata for status, raw, improved, and download endpoints in `src/AudioTranscriberAI.Api/Features/Transcriptions/`
- [ ] T055 [US3] Register US3 concrete services and endpoints in `src/AudioTranscriberAI.Api/Program.cs`

**Checkpoint**: US3 is complete when status reports job metadata and downloads return correct raw/improved Markdown files.

---

## Phase 6: Error Handling and Security Hardening

**Purpose**: Make validation, processing, and logging failures safe and predictable across all endpoints.

- [ ] T056 [P] Add safe logging tests that assert API keys, transcript contents, audio contents, and sensitive local paths are not logged in `tests/AudioTranscriberAI.Tests/Infrastructure/Errors/SafeLoggingTests.cs`
- [ ] T057 [P] Add provider failure tests for transcription and improvement failures in `tests/AudioTranscriberAI.Tests/Features/Transcriptions/ProviderFailureTests.cs`
- [ ] T058 [P] Add FFmpeg failure tests in `tests/AudioTranscriberAI.Tests/Infrastructure/Audio/FfmpegAudioProcessorTests.cs`
- [ ] T059 [P] Add malformed metadata and storage failure tests in `tests/AudioTranscriberAI.Tests/Infrastructure/Storage/StorageFailureTests.cs`
- [ ] T060 [P] Add missing `OPENAI_API_KEY` configuration tests in `tests/AudioTranscriberAI.Tests/Infrastructure/Configuration/OpenAIOptionsValidationTests.cs`
- [ ] T061 Implement sanitized exception logging helpers in `src/AudioTranscriberAI.Api/Infrastructure/Errors/SafeErrorLogger.cs`
- [ ] T062 Implement consistent problem details responses for validation, not found, conflict, missing API key, processing failure, and unexpected errors in `src/AudioTranscriberAI.Api/Infrastructure/Errors/ProblemDetailsMapper.cs`
- [ ] T063 Update all transcription endpoints in `src/AudioTranscriberAI.Api/Features/Transcriptions/` to use centralized problem details mapping
- [ ] T064 Implement safe startup or request-time failure behavior for missing `OPENAI_API_KEY` in `src/AudioTranscriberAI.Api/Infrastructure/Configuration/OpenAIOptions.cs`
- [ ] T065 Update `JsonTranscriptionStore` in `src/AudioTranscriberAI.Api/Infrastructure/Storage/JsonTranscriptionStore.cs` to handle malformed metadata and atomic writes

---

## Phase 7: Final Tests and Manual Validation Checklist

**Purpose**: Verify the backend MVP end to end without adding frontend or database work.

- [ ] T066 Run `dotnet format` and fix formatting issues in `src/AudioTranscriberAI.Api/` and `tests/AudioTranscriberAI.Tests/`
- [ ] T067 Run `dotnet test` and fix failing tests in `src/AudioTranscriberAI.Api/` and `tests/AudioTranscriberAI.Tests/`
- [ ] T068 Run `dotnet build` and fix warnings or errors in `AudioTranscriberAI.sln`
- [ ] T069 Manually verify Swagger lists all five expected endpoints in `src/AudioTranscriberAI.Api/Program.cs`
- [ ] T070 Manually upload a valid MP3 file through Swagger or curl and verify local files under `data/transcriptions/{id}/`
- [ ] T071 Manually verify unsupported format and oversized upload errors through `POST /api/transcriptions`
- [ ] T072 Manually verify missing `OPENAI_API_KEY` returns a safe configuration error without exposing secrets
- [ ] T073 Manually verify `GET /api/transcriptions/{id}` returns `Pending`, `Processing`, `Completed`, or `Failed`
- [ ] T074 Manually verify `GET /api/transcriptions/{id}/raw` returns raw transcript text after processing
- [ ] T075 Manually verify `GET /api/transcriptions/{id}/improved` returns improved transcript text preserving original meaning
- [ ] T076 Manually verify `GET /api/transcriptions/{id}/download?type=raw` downloads raw Markdown output
- [ ] T077 Manually verify `GET /api/transcriptions/{id}/download?type=improved` downloads improved Markdown output
- [ ] T078 Update `specs/001-audio-transcription/quickstart.md` if any local run command, port, or configuration key differs from implementation

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 Setup**: No dependencies.
- **Phase 2 Configuration and Foundational Model**: Depends on Phase 1.
- **Phase 3 US1**: Depends on Phase 2 and delivers the MVP raw transcription path.
- **Phase 4 US2**: Depends on US1 raw transcript persistence.
- **Phase 5 US3**: Depends on US1 for status/raw download and US2 for improved download.
- **Phase 6 Error Handling and Security Hardening**: Depends on endpoint and infrastructure surfaces from US1-US3.
- **Phase 7 Final Tests and Manual Validation**: Depends on all implementation phases.

### User Story Dependencies

- **US1**: Independent MVP slice after foundational work.
- **US2**: Requires raw transcript artifact from US1.
- **US3**: Requires metadata from US1 and improved transcript from US2 for the full download workflow.

### Parallel Opportunities

- T006-T008 can run in parallel after project creation.
- T012-T020 can run in parallel after configuration files exist.
- US1 tests T023-T027 can run in parallel.
- US2 tests T037-T039 can run in parallel after US1 artifacts are defined.
- US3 tests T045-T048 can run in parallel after endpoint contracts are stable.
- Error hardening tests T056-T060 can run in parallel.

## Parallel Example: User Story 1

```text
Task: "T023 [P] [US1] Add upload validation unit tests in tests/AudioTranscriberAI.Tests/Features/Transcriptions/UploadTranscription/UploadValidationTests.cs"
Task: "T024 [P] [US1] Add JSON metadata store tests in tests/AudioTranscriberAI.Tests/Infrastructure/Storage/JsonTranscriptionStoreTests.cs"
Task: "T025 [P] [US1] Add local file storage tests in tests/AudioTranscriberAI.Tests/Infrastructure/Storage/LocalFileStorageTests.cs"
Task: "T027 [P] [US1] Add API integration tests for upload and raw result endpoints in tests/AudioTranscriberAI.Tests/Integration/TranscriptionsUploadAndRawTests.cs"
```

## Parallel Example: User Story 2

```text
Task: "T037 [P] [US2] Add transcript improvement prompt tests in tests/AudioTranscriberAI.Tests/Infrastructure/OpenAI/OpenAITranscriptImproverPromptTests.cs"
Task: "T038 [P] [US2] Add transcript integrity tests for preserving names, numbers, claims, and [unclear] markers in tests/AudioTranscriberAI.Tests/Features/Transcriptions/TranscriptIntegrityTests.cs"
Task: "T039 [P] [US2] Add improved transcript endpoint integration tests in tests/AudioTranscriberAI.Tests/Integration/ImprovedTranscriptTests.cs"
```

## Parallel Example: User Story 3

```text
Task: "T045 [P] [US3] Add status endpoint integration tests in tests/AudioTranscriberAI.Tests/Integration/TranscriptionStatusTests.cs"
Task: "T046 [P] [US3] Add transcript exporter unit tests for Markdown output in tests/AudioTranscriberAI.Tests/Infrastructure/Export/TranscriptExporterTests.cs"
Task: "T047 [P] [US3] Add download endpoint integration tests for raw and improved Markdown in tests/AudioTranscriberAI.Tests/Integration/TranscriptDownloadTests.cs"
Task: "T048 [P] [US3] Add OpenAPI contract tests for all five endpoints in tests/AudioTranscriberAI.Tests/Contract/OpenApiContractTests.cs"
```

## Implementation Strategy

### MVP First

1. Complete Phase 1 and Phase 2.
2. Complete Phase 3 for US1 only.
3. Stop and validate upload, local storage, metadata, raw transcription, and `/raw`.

### Incremental Delivery

1. Add US2 to produce improved transcripts while preserving meaning.
2. Add US3 to expose status and download workflows.
3. Complete Phase 6 before considering the backend feature done.
4. Complete Phase 7 manual validation using the quickstart flow.

## Notes

- Do not implement frontend work in this feature.
- Do not add a database unless JSON metadata cannot satisfy a specific task.
- Do not add queues, authentication, accounts, payments, S3, cloud deployment, real-time transcription, speaker diarization, or advanced audio editing.
- Keep OpenAI and FFmpeg calls behind interfaces so tests use fakes by default.
