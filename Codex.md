# KarmaGame AI README

This file is the single source of truth for AI-assisted work in this repo.

If the user says:
- `Execute the request in Codex.md`
- `Run Codex.md`
- `Do the inbox item from Codex.md`

Then the AI should:
1. Read this file.
2. Execute the top `Open` item in `Request Inbox`.
3. Move that item to `In Progress` while working.
4. Move it to `Completed Log` when done, with date and outcome.

## AI Operating Instructions
Follow this exact order when using this file:
1. Read `AI Operating Instructions`.
2. Read `Project Context` and `Technical Notes`.
3. Read `Request Inbox` and pick the first unchecked item in `Open`.
4. Execute the request end-to-end (code changes, validation, and summary).
5. Update `Request Inbox` and `Completed Log` before finishing.

Execution rules:
- Treat this file as authoritative for task selection and workflow.
- Prefer small, targeted edits over broad refactors unless requested.
- Do not revert unrelated existing changes in the repo.
- If request details conflict with code reality, follow code reality and record the assumption in the completion note.
- If blocked, add a short blocker note under `In Progress` with the exact missing info.

Validation checklist for each completed request:
- Build affected project(s) when practical.
- Run relevant tests/checks when available.
- Confirm acceptance criteria from the request item.
- Summarize changed files and behavior impact.

Request formatting rules (for humans editing this file):
- Put new requests at the top of `### Open`.
- Use unique IDs: `REQ-XXXX`.
- Include:
  - Objective
  - Constraints
  - Acceptance criteria
  - Optional file hints

Request item template:
- [ ] `REQ-XXXX` Short title.
  - Objective: what should be implemented/changed.
  - Constraints: boundaries, non-goals, or technical limits.
  - Acceptance: measurable outcome(s).
  - Hints (optional): relevant files, classes, or docs.

## Project Context
- Repo: `KarmaGame`
- Primary runtime target: `Sandbox`
- Architecture anchor: `SandboxGame` is the composition root.
- Scene orchestration: `Sandbox/Sandbox/Game/Scene/SandboxScene.cs`
- Core gameplay nodes: `MapNode`, `PlayerNode`, `CameraNode`

## Technical Notes
- Y-sort helpers:
  - `Engine/Engine/Core/IYSortDrawable.cs`
  - `Engine/Engine/Core/YSortRenderer.cs`
- Collision map behavior:
  - Collision checks use `Collision` layer with masked raw GID logic in `TiledMapRuntime`.
  - `Collision` currently renders in background tiled layers through `TiledMapAuthoringProfile`.

## Request Inbox (Edit This Section)
Use this section to define what you want done next.

### Open
- [ ] `REQ-0002` Implement/refine sandbox day/night cycle behavior.
  - Clock increments by 15 in-game minutes every 5 real seconds.
  - Full-screen tint for day/night transitions.
  - Start at 6:00 AM, with day ending at 2:00 AM.
  - Acceptance: feature runs in Sandbox and is configurable via settings if practical.

### In Progress
- [ ] (none)

## Completed Log
- 2026-02-24 `REQ-0001`: Refactored `Codex.md` into an AI README with executable request workflow and dedicated inbox sections.
