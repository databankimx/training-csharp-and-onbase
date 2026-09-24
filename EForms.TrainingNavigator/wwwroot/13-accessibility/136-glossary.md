# Glossary

| Term | Definition |
|------|------------|
| **WCAG (Web Content Accessibility Guidelines)** | The W3C standard this guide is based on, defining testable success criteria for web accessibility across three conformance levels (A, AA, AAA). |
| **POUR** | The four core principles WCAG is organized around: Perceivable, Operable, Understandable, and Robust. |
| **Success Criterion** | A single, testable requirement within WCAG (e.g., "1.1.1 Non-text Content"), each assigned a conformance level. |
| **Conformance Level (A, AA, AAA)** | The three tiers of WCAG compliance. Level A is the minimum; Level AA is the commonly required target (and this guide's default requirement); Level AAA is the strictest and is optional here. |
| **Assistive Technology (AT)** | Hardware or software used to increase, maintain, or improve the functional capabilities of individuals with disabilities. |
| **Programmatically Determined** | Information that can be extracted by assistive technologies using the underlying code (HTML, ARIA, etc.). |
| **Non-text Content** | Any content presented in a non-text format (e.g., images, video, sound). |
| **Name, Role, Value** | Key accessibility attributes of UI components that must be exposed to assistive tech (e.g., a button's label, its role as a button, and its state). |
| **Screen Reader** | A software application that converts text and other elements on the screen into synthesized speech or Braille. |
| **Keyboard Navigation** | Interaction with a web interface using only keyboard commands (e.g., tab, enter, arrow keys). |
| **ARIA (Accessible Rich Internet Applications)** | A set of attributes used to make dynamic content and custom UI elements accessible. |
| **Landmark Regions** | Sections of a page defined by roles (e.g., main, navigation, banner) to help assistive technologies navigate more efficiently. |
| **Focus** | The point of interaction currently active on a page, such as a text box or button that receives input from the user. |
| **Live Region** | An area of a page that is updated dynamically and conveys important status messages or changes without receiving user focus. |
| **Parsing** | The process of analyzing code structure to ensure it conforms to language rules. Historically important for assistive tech compatibility; see the note under 4.1.1 Parsing in the Robust principle for why this criterion was later removed from WCAG. |
| **Placeholder** | A short hint displayed inside a form field before the user enters a value. Not a substitute for a label. |
| **Autocomplete** | A browser feature that suggests or auto-fills values based on recognized purpose or user history. |
| **Undo** | A user-initiated action that reverses a previous operation, often critical in preventing irreversible changes. |
| **Validation** | The process of checking user input for errors and providing corrective guidance. |
| **Context Change** | An unexpected navigation or interface shift triggered by user input (e.g., new window, automatic form submission). |
