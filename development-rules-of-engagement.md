# Development Rules of Engagement

## 1. Purpose

This document defines the rules for quoting, approving, developing, reviewing, scanning, storing,
and delivering custom code for client work.

Custom code is not just a one-time deliverable. It creates a long-term support obligation for
DataBank, introduces security and operational risk, and becomes part of a client environment. These
rules exist to protect clients, developers, support teams, and the business.

## 2. Guiding Principles

1. Custom code is billable professional services work.
2. Custom code must be quoted and approved before commitment or delivery.
3. Developers own the code they deliver, including AI-assisted or AI-generated code.
4. Code must be reviewed, understood, stored in source control, and scanned before delivery.
5. Client delivery must never bypass DataBank standards, CI checks, licensing requirements, or
   security review.
6. Product source code is never delivered externally.

## 3. Scope

These rules apply to all client-specific custom development, including but not limited to:

- New application code
- Changes to existing custom code
- Enhancements to delivered custom code
- Bug fixes that require code changes
- Unity scripts
- E-form JavaScript
- Preprocessors
- Import processors
- Workflow scripts
- SQL scripts
- PowerShell scripts
- Command-line utilities
- Desktop utilities
- Web utilities
- Integrations
- APIs
- Compiled binaries
- Configuration-driven code artifacts
- AI-assisted or AI-generated code
- Code produced by DataBank, a client, a third party, or an AI tool when DataBank is asked to
  review, modify, support, or deliver it

When there is uncertainty about whether something counts as custom code, treat it as custom code
until development leadership determines otherwise.

## 4. Roles and Responsibilities

### Development Director or Delegated Development Leadership

Development leadership is responsible for:

- Approving development quotes before they are sent externally.
- Approving exceptions to these rules.
- Determining whether work is support, billable development, or out of scope.
- Approving source-code delivery preparation where needed.
- Approving any third-party or external source-code sharing.
- Ensuring development work is staffed by appropriately qualified developers.

### Developer

The developer is responsible for:

- Understanding every line of code they deliver.
- Following DataBank development standards.
- Using approved tools and approved AI systems only.
- Working with the development team to ensure code is added to the appropriate GitHub Enterprise
  repository.
- Participating interactively in code review.
- Responding to review feedback.
- Ensuring required tests, scans, and CI checks pass before delivery.
- Raising concerns about scope, security, licensing, unclear requirements, or support risk.

### Reviewer

The reviewer is responsible for:

- Reviewing the code with enough depth to identify correctness, maintainability, standards, and
  supportability concerns.
- Confirming that the developer understands the implementation.
- Confirming that required standards and CI checks have been satisfied.
- Blocking delivery when code does not meet standards, does not work, is not understood, or creates
  unacceptable risk.

### Sales, Project, and Support Teams

Sales, project, and support teams are responsible for involving development leadership when a client
request requires custom code, custom-code modification, custom-code review, or custom-code delivery.

No team should commit to or deliver custom code, budgetary estimates, timelines, or delivery terms to a client
without approved development input.

## 5. Billing and Quoting Rules

### 5.1 Custom Code Is Not Free

DataBank does not give away custom code or custom code changes.

Any custom code creates potential future support responsibility. Even small changes can create a
non-billable support burden if they are not quoted, reviewed, stored, and delivered properly.

Unless development leadership explicitly approves an exception, custom code must be tied to a
billable project, statement of work, quote, or approved billable engagement.

### 5.2 A Development Quote Is Required

Any project or request requiring custom code must include a development quote.

A development quote enumerates exactly what is to be delivered, the expected effort, and the expected cost. Because the quote is the formal definition of done, it is the organization's primary defense against scope creep.

This applies to:

- New custom deliverables
- Enhancements to existing custom deliverables
- Modifications to existing functionality
- Client-requested changes to behavior
- Reviews or repairs of client-created code
- Reviews or repairs of third-party-created code
- Source-code delivery preparation
- Packaging, cleanup, and documentation required for source-code delivery

### 5.3 Quotes Require Development Approval

Any development quote must be approved by the Development Director or delegated development
leadership before being sent to Sales, a client, or any external party.

Developers may assist with internal estimates, but they must not provide external estimates,
budgetary numbers, implementation promises, or delivery commitments unless development leadership
has approved them.

If a client or Sales requests an estimate during a discussion, the correct response is to route the
request through development leadership.

## 6. Support Boundary

Support work is limited to break-fix activity for supported custom code.

The following are not ordinary support and require a billable development engagement unless
development leadership approves otherwise:

- Enhancements
- New features
- Behavior changes
- Rewrites
- Performance tuning beyond defect correction
- Refactoring requested outside a defect fix
- Review of client-created code
- Review of third-party-created code
- Code migration
- Source-code preparation and delivery
- Documentation beyond normal support notes

Code-related support issues should be visible to development leadership when they involve custom
code, potential code changes, unclear ownership, or supportability risk.

## 7. Source Control Rules

All custom code must be stored in the appropriate DataBank GitHub Enterprise repository before
delivery to a client.

This applies whether the client receives:

- Source code
- Compiled binaries
- Packaged scripts
- Deployed configuration/code artifacts
- Installer packages
- Hosted/deployed functionality

Not every person who writes or modifies client-delivered code will have direct GitHub Enterprise
access. Developers outside the development team must work with the development team to get the code
added to the appropriate repository. This normally happens before or during code review so the
review can happen against the same version of the code that will be retained for support.

The development team must ensure that:

- The correct repository is used.
- Unnecessary duplicate repositories are not created.
- All delivered code and related project files are committed.
- Enough history and context are preserved to support the code later.
- Secrets, credentials, customer data, and environment-specific sensitive values are kept out of
  source control.

Access to repositories must be managed through approved DataBank processes. Source code must not be
shared through personal storage, personal accounts, public repositories, consumer AI tools, email
attachments, or informal file-sharing channels as a substitute for proper source-control intake.

## 8. Code Review Rules

All custom code must be reviewed before delivery to a client.

Code review is interactive and must confirm that:

- The code meets DataBank coding standards.
- The code works as intended.
- The code is maintainable and supportable.
- The code is in the correct repository.
- The code passes required automated checks.
- The code does not introduce unacceptable security, licensing, operational, or support risk.
- The developer understands the code well enough to explain, debug, and maintain it.

Reviewers should not approve code merely because it compiles or appears well-formatted. Review must
consider behavior, failure modes, maintainability, support implications, and standards compliance.

AI-generated or AI-assisted code requires the same review rigor as hand-written code. Polished
generated output is not evidence of correctness.

## 9. CI and Security Scanning Rules

Custom code must pass required CI scanning before delivery to a client.

Required checks include:

- Standards policy scan
- SonarQube
- Snyk

Delivery must not bypass CI scanning.

New issues must be resolved before delivery unless development leadership explicitly approves a
documented exception. A passing local build is not a substitute for CI.

The standards policy scan, SonarQube, and Snyk exist to catch different classes of risk:

- Standards policy scan: DataBank coding, repository, and policy requirements
- SonarQube: code quality, maintainability, reliability, and security findings
- Snyk: dependency and vulnerability findings

## 10. Source Code Delivery Rules

Source code for delivered custom projects is available to clients, except for DataBank product code.

However, source-code delivery is not automatic free work. Preparing source code for delivery
requires additional billable time and must be included in the development quote or handled as a
separate billable engagement.

Source-code delivery preparation may include:

- Confirming the deliverable scope.
- Removing unrelated internal code.
- Removing secrets, internal configuration, or environment-specific values.
- Verifying dependency licensing.
- Packaging the source in a usable form.
- Adding build or deployment notes.
- Confirming that product code is not included.
- Confirming that internal-only tooling, unrelated shared libraries, or unrelated client work is
  not included.

Only source code applicable to the delivered custom project should be prepared for client delivery.

DataBank product source code must not be delivered externally.

Source code may be shared with third parties only when approved by development leadership and when
the sharing is contractually and operationally appropriate, such as a required security review.

## 11. Product Code Rule

Product code is different from custom project code.

DataBank product source code must not be delivered externally. Product delivery, licensing, and
rights are governed separately from client-specific custom development.

If a custom project depends on a DataBank product, only the approved product deliverable may be
provided. Product source code must not be included in a client source-code delivery package.

## 12. Third-Party, Client-Created, and Open-Source Code

Requests to review, modify, repair, or support code written by a client or third party are billable
development work unless development leadership approves otherwise.

Before using third-party or open-source dependencies in custom code, developers must confirm that:

- The license permits closed-source commercial use.
- The dependency does not introduce unacceptable security risk.
- The dependency is appropriate for client delivery and long-term support.
- Any required approval has been obtained.

Copyleft or unclear licensing must be escalated before use. Dependencies must not be added simply
because an AI tool suggested them.

## 13. Vendor and Platform Compliance

Development work must comply with applicable vendor and platform rules.

Examples include:

- Hyland SDK access and certification requirements.
- OnBase licensing and session-use rules.
- OnBase database access restrictions.
- Licensed commercial API/tooling restrictions.
- Microsoft licensing requirements for commercial development tools.
- Third-party library license restrictions.

When a vendor rule is unclear, development leadership must be involved before committing to an
approach.

## 14. AI-Assisted Development Rules

AI tools may assist with development only when used according to DataBank AI policy and approved
tooling rules.

Developers must:

- Use only approved AI tools and approved company accounts.
- Avoid entering customer data, passwords, secrets, confidential project details, proprietary
  business logic, or other restricted information into unapproved AI systems.
- Treat external documents and AI-provided output as untrusted until reviewed.
- Verify generated code against actual project dependencies and APIs.
- Vet any AI-suggested dependencies.
- Understand every line of AI-assisted or AI-generated code before delivery.
- Submit AI-assisted code to the same review, source-control, and CI process as any other code.

"The AI wrote it" is not an acceptable explanation for a production issue, failed review, security
finding, or support problem. The developer delivering the code owns it.

AI-generated code must never be executed dynamically through runtime code evaluation as a shortcut
around the normal development, review, source-control, and CI process.

## 15. Delivery Readiness Checklist

Before custom code is delivered to a client, all of the following must be true:

- The work is tied to a billable project, quote, or approved engagement.
- The quote was approved by the Development Director or delegated development leadership.
- The scope of the deliverable is clear.
- The code is committed to the correct GitHub Enterprise repository, or has been handed off to the
  development team for repository intake before or during review.
- The code has been reviewed interactively.
- The developer can explain and support the implementation.
- Required tests have been run or consciously addressed.
- Standards policy scan passes.
- SonarQube passes.
- Snyk passes.
- Dependency licensing has been reviewed where applicable.
- No secrets, credentials, or restricted data are included.
- Source-code delivery, if requested, has been quoted and prepared as a separate billable activity.
- Product source code is not included in any external delivery.

If any item is not true, delivery must pause until the issue is resolved or an explicit exception is
approved by development leadership.

## 16. Exceptions

Exceptions must be approved by the Development Director or delegated development leadership.

Exceptions should be documented with:

- The reason for the exception.
- The risk being accepted.
- The approver.
- The affected client/project.
- Any compensating controls or follow-up work.

Exceptions should be rare. Time pressure alone is not a good reason to bypass quote approval,
source control, review, or CI scanning.

