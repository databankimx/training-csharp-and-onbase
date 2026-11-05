# WCAG 2.1 A & AA Development Compliance Guide

## Introduction

This guide clarifies DataBank's internal standard for accessibility on web-based projects delivered by the custom development team. It targets the Web Content Accessibility Guidelines (WCAG) 2.1, Levels A and AA.

### Purpose

At DataBank IMX, accessibility is not just a design consideration - it is a development standard. This guide defines the expectations, responsibilities, and resources for all web developers building digital experiences under our brand. Our mandate is simple:

- **WCAG 2.1 Level A** compliance is strictly required.
- **Level AA** must be met in all cases unless a documented, reviewed, and justified exception is approved.
- **Level AAA** is optional, recommended where feasible, and will be prioritized in future accessibility iterations.

By adopting these standards, we help ensure that our applications are inclusive, future-proof, and legally compliant.

### Why Accessibility Matters

**Equal access for all users.** Accessibility ensures that our content can be used by everyone, including individuals with disabilities such as blindness, color blindness, hearing loss, mobility impairments, and cognitive conditions. Good accessibility enhances usability for all users, not just those with impairments.

**Legal exposure in the United States.** The legal landscape here has real, current deadlines worth knowing precisely rather than treating as vague risk:

- **ADA Title II (state and local government).** In April 2024, the Department of Justice finalized a rule under Title II of the Americans with Disabilities Act requiring state and local government entities to make their web content and mobile applications conform to WCAG 2.1 Level AA. In April 2026, DOJ issued an interim rule extending the original compliance dates by one year. As of this writing, the deadlines are **April 26, 2027** for public entities serving a population of 50,000 or more, and **April 26, 2028** for smaller entities and all special district governments. The technical standard itself (WCAG 2.1 AA) was not changed by the extension, only the timing. This is directly relevant to us: any government customer using a DataBank-built or DataBank-hosted system is a covered entity under this rule, on this timeline.
- **ADA Title III (private businesses).** There is no finalized DOJ rule yet specifically applying WCAG to private-sector "places of public accommodation," and courts remain split on whether a website alone qualifies as one. In practice, this legal uncertainty has not slowed enforcement: private plaintiffs and advocacy groups continue to file ADA Title III lawsuits against businesses over inaccessible websites, and WCAG 2.1 AA is the de facto standard courts and settlement agreements point to. Treat Title III risk as real even without a finalized rule.
- **Section 508 (federal agencies)** requires federal agency web content to be accessible, and has long referenced WCAG success criteria.
- **Section 504 of the Rehabilitation Act** was updated by the Department of Health and Human Services to also mandate WCAG 2.1 AA compliance for covered health-related entities, with its own phased deadlines.

Because compliance deadlines and the specifics of these rules can and do change, verify current dates before communicating them externally (to a customer or in a contract) rather than relying on this guide alone.

Failing to meet accessibility standards can lead to:
- ADA-related lawsuits, settlements, or DOJ enforcement actions
- Loss of business opportunities with government or health-sector institutions
- Negative press and reputational harm

**Professional and ethical obligation.** As developers, we have a professional responsibility to build products that do not exclude or disadvantage others. Accessibility is a fundamental aspect of quality, just like performance, security, and responsive design.

### Recommended Accessibility Testing Tools

| Tool | Use Case | Link |
|---|---|---|
| WAVE by WebAIM | Visual browser-based accessibility checker | [wave.webaim.org](https://wave.webaim.org) |
| axe DevTools | Chrome/Firefox extension for automated testing | [deque.com/axe/devtools](https://www.deque.com/axe/devtools/) |
| axe DevTools (Edge) | Edge extension for automated testing | Available from the Edge Add-ons store |
| axe Accessibility Linter | Visual Studio Code extension | Available from the VS Code Marketplace |
| Lighthouse | Accessibility audit and performance report | Built into Chrome DevTools |
| NVDA | Screen reader testing on Windows | [nvaccess.org](https://www.nvaccess.org) |
| VoiceOver | Native screen reader for Apple devices | Included with all Apple products |

### Final Word

This guide sets the tone for building accessible, inclusive, and legally compliant digital solutions at DataBank IMX. Every line of code, form field, and UI element should be built with these principles in mind.

Accessibility is not an add-on. It's built in by design, from the start.

> This document is a living reference and is subject to change as WCAG guidance and the legal landscape evolve.

### References

- [WCAG 2.1 Specification (W3C)](https://www.w3.org/TR/WCAG21/)
- [WAI Introduction to WCAG](https://www.w3.org/WAI/standards-guidelines/wcag/)
- [W3C Markup Validation Service](https://validator.w3.org/)

### How This Guide Is Organized

WCAG 2.1 is built around four core principles, each with its own file in this chapter:

- **[131-principle-1-perceivable](131-principle-1-perceivable.md)** - information and interface components must be presentable to users in ways they can perceive.
- **[132-principle-2-operable](132-principle-2-operable.md)** - interface components and navigation must be operable.
- **[133-principle-3-understandable](133-principle-3-understandable.md)** - information and the operation of the interface must be understandable.
- **[134-principle-4-robust](134-principle-4-robust.md)** - content must be robust enough to be interpreted reliably by a wide range of user agents, including assistive technologies.

Each principle file covers its guidelines and every applicable Level A/AA success criterion, with an intent statement, requirements, and correct/incorrect code examples for each.

Beyond the four principles:

- **[135-testing-process](135-testing-process.md)** - the step-by-step process for auditing a page or feature for compliance.
- **[136-glossary](136-glossary.md)** - definitions for accessibility terms used throughout this guide.
- **[137-checklist](137-checklist.md)** - every Level A/AA success criterion in one table, for tracking compliance on a project.

Level AAA criteria are not covered in this guide, as DataBank is not currently adopting Level AAA as a requirement. They're noted where relevant so you know they exist, should a future project need them.
