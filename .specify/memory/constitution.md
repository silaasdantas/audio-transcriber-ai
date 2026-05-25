<!--
Sync Impact Report
Version change: template -> 1.0.0
Modified principles:
- Template principle 1 -> I. MVP Scope Discipline
- Template principle 2 -> II. .NET 8 Vertical Slices
- Template principle 3 -> III. Local Audio Processing
- Template principle 4 -> IV. AI Transcript Integrity
- Template principle 5 -> V. Validation, Errors, and Tests
Added sections:
- MVP Technology Boundaries
- Development Workflow
Removed sections:
- Placeholder SECTION_2_NAME
- Placeholder SECTION_3_NAME
Templates requiring updates:
- updated: .specify/templates/plan-template.md
- updated: .specify/templates/spec-template.md
- updated: .specify/templates/tasks-template.md
- reviewed: .specify/templates/checklist-template.md
- not present: .specify/templates/commands/*.md
Runtime guidance reviewed:
- reviewed: AGENTS.md
Follow-up TODOs: none
-->

# AudioTranscriberAI Constitution

## Core Principles

### I. MVP Scope Discipline
AudioTranscriberAI MUST remain a simple, focused MVP for local audio upload,
AI transcription, AI-assisted transcript improvement, and TXT or Markdown export.
The first version MUST NOT include authentication, payments, background queues,
cloud storage, multi-tenant SaaS features, or unrelated product surfaces. Any
feature proposal that expands beyond this workflow MUST be rejected or deferred.

Rationale: A narrow MVP makes the transcription path testable, shippable, and
easy to reason about before operational complexity is introduced.

### II. .NET 8 Vertical Slices
The backend MUST be implemented as a .NET 8 Web API organized with Vertical
Slice Architecture. Each feature slice MUST own its request/response contracts,
validation, handler/application logic, and tests unless shared infrastructure is
clearly justified. Cross-cutting abstractions MUST stay minimal and exist only
when multiple slices have demonstrated the same need.

Rationale: Vertical slices keep the system aligned with user workflows and avoid
premature layers that slow MVP delivery.

### III. Local Audio Processing
The MVP MUST accept local WAV, MP3, and M4A uploads, store files locally, and use
FFmpeg for audio conversion and chunking. SQLite MAY be introduced only when a
feature requires persistence that local files alone cannot satisfy. Cloud
storage, distributed processing, and queue-backed pipelines MUST NOT be added in
the first version.

Rationale: Local storage and FFmpeg provide the simplest reliable path for
handling real audio files while keeping deployment and failure modes contained.

### IV. AI Transcript Integrity
The system MUST use the OpenAI API for transcription and transcript improvement.
Transcript improvement MUST preserve the original meaning and MUST NOT invent,
add, or infer content that is not supported by the transcription. When content is
unclear, the system MUST preserve uncertainty rather than fabricate clarity.
Prompts, tests, and review criteria MUST explicitly protect against hallucinated
transcript content.

Rationale: Users rely on transcripts as records of audio; invented content breaks
trust and can create harmful downstream errors.

### V. Validation, Errors, and Tests
Every feature MUST include input validation, user-meaningful error handling, and
automated tests. Tests MUST cover successful paths, validation failures, external
API failure handling, FFmpeg or file-processing failures where relevant, and any
rules that protect transcript meaning. No feature is complete until its tests can
be run by the repository's documented test command.

Rationale: Audio processing and AI calls fail in practical ways; explicit
validation, error behavior, and tests keep those failures controlled.

## MVP Technology Boundaries

- Backend: .NET 8 Web API.
- Architecture: Vertical Slice Architecture by feature workflow.
- AI provider: OpenAI API for transcription and text improvement.
- Audio tooling: FFmpeg for conversion and chunking.
- Supported input formats: WAV, MP3, and M4A.
- Export formats: TXT and Markdown.
- MVP storage: local filesystem; SQLite only when persistence is necessary.
- Explicitly out of scope for v1: authentication, payments, queues, cloud
  storage, multi-tenant SaaS capabilities, and unrelated collaboration features.

## Development Workflow

Plans MUST identify the vertical slice boundaries, local file handling approach,
FFmpeg usage, OpenAI API touchpoints, validation strategy, error behavior, and
test coverage for each feature. Specs MUST state acceptance criteria in terms of
observable user outcomes and MUST call out transcript integrity requirements when
AI improvement is involved. Tasks MUST be organized by independently testable
user story and MUST include test tasks before implementation tasks.

Implementation reviews MUST verify that scope remains inside the MVP boundaries,
that feature code follows the selected slice structure, and that generated or
improved transcript content does not add unsupported meaning.

## Governance

This constitution supersedes conflicting repository guidance, implementation
plans, and task templates. Amendments MUST be documented in this file, include a
Sync Impact Report, update affected Spec Kit templates, and explain the semantic
version bump.

Versioning follows semantic versioning:
- MAJOR: removes or redefines a core principle or MVP boundary in a
  backward-incompatible way.
- MINOR: adds a new principle, required section, or materially expands governance.
- PATCH: clarifies wording without changing obligations.

Every plan, spec, and task list MUST pass a constitution review before
implementation begins. Any justified exception MUST be recorded in the plan's
Complexity Tracking section with the simpler alternative that was rejected.

**Version**: 1.0.0 | **Ratified**: 2026-05-25 | **Last Amended**: 2026-05-25
