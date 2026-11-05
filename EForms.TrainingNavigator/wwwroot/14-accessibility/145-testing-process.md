# Accessibility Compliance Testing Process

**(WCAG 2.1 - Level A required, Level AA unless justified, AAA optional)**

---

**Step 1: Planning & Scope Definition**

Define what to test:

- Identify **critical pages**, workflows, and UI components.
- Specify **target browsers**, assistive technologies, and devices.
- Document **any Level AA criteria** to be excluded with justification and approval.

*Tip*: Prioritize forms, modals, authentication flows, and dynamic content.

---

**Step 2: Automated Testing (Baseline)**

Run automated tools to catch common accessibility failures.

**Recommended Tools**

| Tool | Purpose | Link |
|------|---------|------|
| **WAVE** | Visual accessibility overview | [https://wave.webaim.org](https://wave.webaim.org) |
| **axe DevTools** | Developer-friendly issue detection | [https://www.deque.com/axe/devtools/](https://www.deque.com/axe/devtools/) |
| **Lighthouse** | Chrome-integrated accessibility auditing | Built into Chrome |
| **tota11y** | Overlay tool for visual inspection | [https://khan.github.io/tota11y](https://khan.github.io/tota11y) |

**Check for:**
- Missing `alt` text
- Color contrast (AA-level)
- Improper heading structure
- Missing form labels
- ARIA misuse
- Missing focus styles

Fix **Level A** violations immediately. Flag **Level AA** unless justifiably exempt.

---

**Step 3: Manual Testing - Keyboard & Screen Reader**

**Keyboard Testing**
- Tabbing must follow a **logical order**
- **No keyboard traps**
- Focus must be **visibly indicated**
- All UI elements must be operable via keyboard

**Screen Reader Testing**
Use tools such as:
- **NVDA** (Windows)
- **VoiceOver** (macOS or iOS)

Test for:
- Proper reading of **labels**, **roles**, and **status messages**
- Descriptive link and button text
- Announced validation errors
- Correct landmark structure and heading levels

Level A failures are blockers. Level AA must be addressed unless documented otherwise.

---

**Step 4: Forms & Error Handling**

Verify:
- Labels and format hints
- Error messages associated via `aria-describedby`
- Helpful suggestions for correction
- Confirmation steps before critical data is submitted

Applies to criteria 3.3.1 - 3.3.4.

---

**Step 5: Optional AAA Evaluation**

Check only where applicable:

- 1.4.6 Enhanced contrast
- 3.1.5 Reading level assessment
- 3.3.5 Context-sensitive help
- 2.5.5 Large target sizes
- 1.2.6 Sign language (for high-accessibility sectors)

AAA compliance is **encouraged but not required**.

---

**Step 6: Documentation**

Each audit must include:

- Page or component name
- Tools/methods used
- Categorized list of issues (A, AA, AAA)
- Screenshots or video for clarity
- Notes on any granted exceptions
- Remediation status

Archive reports for **legal and QA review**.

---

**Step 7: Retesting**

After fixes:
- Re-run automated tests
- Repeat manual flows
- Confirm regressions are resolved
- Update report version

---

**Optional: CI/CD Integration**

Automate ongoing compliance with:

- [axe-core CLI](https://www.deque.com/axe/core-documentation/api-documentation/)
- [Pa11y](https://pa11y.org/)
- Lighthouse CI

Trigger accessibility checks during PRs or deploys.

---

**Accessibility is not a one-time task. It's a quality standard woven into every release.**
