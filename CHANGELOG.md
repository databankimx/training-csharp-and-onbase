# Changelog

All notable changes to the DataBank IMX Developer Training Solution are documented here.

This file follows the [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) format.
Versions follow [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

### What gets a changelog entry

- New files, chapters, or solution projects
- Structural or organizational changes to the repository
- Build, CI, or tooling changes
- Major policy changes (e.g. new or substantially revised governance documents)

### What does not

- Prose edits, typo fixes, and minor wording improvements within existing documents
- Formatting-only changes

---

## [Unreleased]

---

## [2.0.1] - 2026-10-09

### Added

- `CSharp.Supplemental.AiAssistedDevelopment` chapter
  - 11 lesson steps covering vibe coding, tool selection, prompt engineering, markdown pre-instruction, OnBase API context documents, working with generated code, code review obligations, NuGet license hygiene, runtime eval risks, agentic workflows, and knowing when to stop
  - `Program.cs` with three runnable demos: `DemonstratePromptAnatomy`, `DemonstrateDependencyVetting`, `DemonstrateCodeReviewChecklist`
  - `Lesson.md` with full narrative coverage of all 11 steps, inline code samples, and the "never do this" runtime eval exhibit
  - `Resources/AgentContext-DataBankStandards.md` - agent-ready condensed DataBank coding standards context document
  - `Resources/AgentContext-OnBaseUnityAPI.md` - agent-ready Hyland Unity API context document derived from DataBank reference files
  - `Resources/DataBank-AI-Acceptable-Use-Policy.md` - markdown rendering of the DataBank AI Acceptable Use Policy v1.7
  - `IsExternalInit.cs` shim to support `sealed record` types on net48
- `development-rules-of-engagement.md` at the repository root - full policy for quoting, approving, developing, reviewing, scanning, storing, and delivering custom code
- README "DataBank Responsibilities for Custom Code" section linking to the rules of engagement and summarising key developer obligations
- README supplementary lessons table entry for `CSharp.Supplemental.AiAssistedDevelopment`
- This changelog

[Unreleased]: https://databankimx.ghe.com/Development/developer-training/compare/v2.0.1...HEAD
[2.0.1]: https://databankimx.ghe.com/Development/developer-training/releases/tag/v2.0.1
