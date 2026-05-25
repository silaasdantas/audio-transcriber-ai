# Data Model: Audio Transcription

## TranscriptionJob

Represents one uploaded audio file and its processing lifecycle.

Fields:
- `id`: Stable unique identifier used in API paths.
- `originalFileName`: Sanitized original file name for display.
- `storedFileName`: Stored local file name.
- `format`: `mp3`, `wav`, or `m4a`.
- `sizeBytes`: Uploaded file size.
- `status`: `Pending`, `Processing`, `Completed`, or `Failed`.
- `createdAtUtc`: Upload creation timestamp.
- `updatedAtUtc`: Last metadata update timestamp.
- `rawTranscriptPath`: Local path to raw transcript artifact when available.
- `improvedTranscriptPath`: Local path to improved transcript artifact when available.
- `failureCode`: Safe internal failure category when status is `Failed`.
- `failureMessage`: User-facing failure message when status is `Failed`.

Validation:
- `id` is required and unique.
- `format` must be one of `mp3`, `wav`, `m4a`.
- `sizeBytes` must be greater than zero and less than or equal to configured
  maximum upload size.
- `failureMessage` must not contain API keys, transcript content, uploaded audio
  content, or sensitive local paths.

State transitions:
- `Pending` -> `Processing`
- `Processing` -> `Completed`
- `Processing` -> `Failed`
- `Pending` -> `Failed`

## AudioUpload

Represents the uploaded original audio file.

Fields:
- `jobId`: Owning transcription job id.
- `originalFileName`: Sanitized client file name.
- `contentType`: Client-provided content type, if available.
- `extension`: Normalized extension.
- `sizeBytes`: Uploaded byte length.
- `storedPath`: Local path under the job directory.

Validation:
- File is required.
- File must not be empty.
- Extension must be `.mp3`, `.wav`, or `.m4a`, case-insensitive.
- Size must be within configured limit before saving.

## TranscriptArtifact

Represents a generated transcript file.

Fields:
- `jobId`: Owning transcription job id.
- `kind`: `raw` or `improved`.
- `path`: Local transcript artifact path.
- `createdAtUtc`: Artifact creation timestamp.
- `lengthCharacters`: Character count.

Validation:
- Raw transcript may be empty only if the provider explicitly returns no speech;
  otherwise empty provider output is treated as a processing failure.
- Improved transcript must preserve raw transcript meaning and mark uncertainty
  as `[unclear]`.

## DownloadArtifact

Represents a user-downloadable view of a transcript.

Fields:
- `jobId`: Owning transcription job id.
- `type`: `raw` or `improved`.
- `format`: `md`.
- `fileName`: Download file name.
- `contentType`: Response content type.
- `content`: Generated download text.

Validation:
- `type` must be `raw` or `improved`.
- `format` must be `md`.
- Requested transcript must exist before download is generated.

## Metadata File Layout

Each job stores metadata at:

```text
data/transcriptions/{id}/metadata.json
```

Artifact paths are stored relative to the job directory where possible to avoid
leaking absolute local paths through API responses.
