## Principle 4: Robust

*Content must be robust enough that it can be interpreted by a wide variety of user agents, including assistive technologies.*

**Overview**

**Principle 4: Robust** ensures that **content must be robust enough to be interpreted reliably by a wide variety of user agents, including assistive technologies**. This means websites and applications should use clean, well-structured code and follow established web standards.

Robust content remains accessible across current and future technologies, providing **long-term compatibility** and improving reliability for users who depend on screen readers, voice navigation tools, and other assistive devices.

---

**Key Concepts**

Robust design begins with **valid, semantic HTML**. Markup must be well-formed, with correct nesting of elements, unique IDs, and complete tag closure. This allows browsers and assistive technologies to parse and interpret the content without confusion or error.

Developers should also ensure that interactive controls convey their **name, role, and value** programmatically using native HTML or ARIA (Accessible Rich Internet Applications) attributes. This enables users of assistive tech to understand what a control does and what state it is in (e.g., a button labeled "Play" with `aria-pressed="true"`).

Finally, **status messages** - such as form submission confirmations or alerts - must be communicated without requiring focus shifts, using live regions (`aria-live`) so screen readers can detect and announce them in real time.

---

**Covered Guidelines**

Principle 4 includes the following WCAG guidelines:
- **4.1 Compatible** - Maximize compatibility with current and future user agents
  - **4.1.1 Parsing** - Use valid HTML/XML with no syntax errors
  - **4.1.2 Name, Role, Value** - Ensure UI elements expose semantics
  - **4.1.3 Status Messages** - Announce dynamic content via ARIA without requiring focus

---

By following Principle 4, developers ensure that their content remains **functional, dependable, and accessible** across a wide spectrum of platforms and assistive tools - now and into the future.

---

**Guideline Links**

- [Guideline 4.1: Compatible](#guideline-41-compatible)

---

### Guideline 4.1: Compatible

*Maximize compatibility with current and future user agents, including assistive technologies.*

**Overview**

**Guideline 4.1** ensures that content is **compatible with current and future assistive technologies**. It promotes robust HTML and accurate metadata so screen readers, voice control tools, and other assistive software can reliably interpret and interact with content.

This guideline is essential for technical interoperability, focusing on **clean code, proper semantics, and up-to-date ARIA practices**.

---

**Success Criteria Summary**

| Success Criterion | Description |
|------------------|-------------|
| **4.1.1 Parsing** | Use valid markup with no duplicate IDs, unclosed tags, or improper nesting |
| **4.1.2 Name, Role, Value** | UI components expose correct semantics (name, role, value) |
| **4.1.3 Status Messages** | Status updates are programmatically announced without focus change |

---

**Visual Examples**

**✓ 4.1.1 Valid HTML with Unique IDs**

```html
<label for="email">Email:</label>
<input type="email" id="email">
```

**Why it works:** HTML is properly nested and each ID is unique.

---

**✗ Failure - Duplicate IDs**

```html
<input id="user" type="text">
<input id="user" type="email">
```

**Issue:** Reusing IDs leads to confusion for assistive tech.

---

**✓ 4.1.2 ARIA Attributes for Custom Control**

```html
<div role="button" tabindex="0" aria-pressed="false" aria-label="Play Video">▶</div>
```

**Why it works:** Exposes semantic information (role, name, state) to screen readers.

---

**✗ Failure - Unlabeled Custom Control**

```html
<div onclick="submitForm()">Go</div>
```

**Issue:** No role, no label - screen readers can't determine what this does.

---

**✓ 4.1.3 Status Message with `aria-live`**

```html
<div aria-live="polite" id="status">Form submitted successfully.</div>
```

**Why it works:** Screen readers are alerted when the content changes without stealing focus.

---

**Checklist**

| Requirement                                                   | Example Provided | Meets Standard |
|--------------------------------------------------------------|------------------|----------------|
| HTML is valid, well-formed, and uses unique IDs              | ✓ Yes            | ✓             |
| Interactive elements have roles, names, and values           | ✓ Yes            | ✓             |
| Status messages are announced via ARIA live regions          | ✓ Yes            | ✓             |

---

Guideline 4.1 helps ensure that content is **robust and forward-compatible**, providing the necessary foundation for **assistive technologies to operate effectively across platforms and devices**.

**Success Criteria Links**

- [4.1.1 Parsing (Level A)](#411-parsing-level-a)
- [4.1.2 Name, Role, Value (Level A)](#412-name-role-value-level-a)
- [4.1.3 Status Messages (Level AA)](#413-status-messages-level-aa)

---

#### **4.1.1 Parsing (Level A)**

**Intent:**  
Ensure that web content can be reliably interpreted by assistive technologies by requiring valid and well-structured markup.

**Requirements**
- In content implemented using markup languages (like HTML), elements must:
  1. Have complete start and end tags.
  2. Be nested according to their specifications.
  3. Not contain duplicate attributes.
  4. Have unique IDs within a document.
- Markup must conform to the formal grammar of the language used.

**Examples**

**1. Valid HTML Example**
```html
<ul>
  <li>Apples</li>
  <li>Bananas</li>
</ul>
```

**2. Invalid Nesting Example (✗)**
```html
<p>This is <em>emphasized <strong>and strong</em> text</strong>.</p>
```

**3. Duplicate Attribute Error (✗)**
```html
<input type="text" name="username" name="user">
```

**4. Non-unique ID Example (✗)**
```html
<div id="section">Content A</div>
<div id="section">Content B</div>
```

**Accessibility Benefits**
- Improves interoperability with assistive technologies.
- Ensures consistent interpretation of content structure and semantics.

**Best Practices**
- Use a validator like the W3C Markup Validation Service to check for syntax errors.
- Avoid dynamically generating invalid markup.
- Test your code for compliance with language specifications.
- Ensure JavaScript and templating systems maintain correct structure and ID uniqueness.

> **Current status:** WCAG 2.2 formally removed 4.1.1 Parsing as a success criterion entirely, on the grounds that modern browsers handle malformed markup gracefully and assistive technology reads the parsed DOM rather than raw HTML, not the source directly - so the original problem this criterion targeted no longer really exists. The W3C's own editorial errata for WCAG 2.0 and 2.1 go further: for conformance to those versions, 4.1.1 is now considered automatically satisfied for any content using markup that is properly exposed to assistive technology. Clean, valid markup is still good practice for its own sake (and genuine structural problems will still be caught by other criteria like 1.3.1 and 4.1.2), but this specific criterion no longer needs to be actively tested for, even when formally targeting WCAG 2.1.

---

#### **4.1.2 Name, Role, Value (Level A)**

**Intent:**  
Ensure that custom UI components are accessible by exposing their name, role, and value to assistive technologies.

**Requirements**
For all user interface components:
- Name and role must be programmatically determinable.
- States, properties, and values that can be set by the user must be programmatically settable.
- Changes to these must be programmatically available to assistive technologies.

**Examples**

**1. Accessible Button With ARIA Role and Label**
```html
<div role="button" tabindex="0" aria-label="Submit Form">Submit</div>
```

**2. Custom Slider With ARIA Attributes**
```html
<div role="slider" tabindex="0" aria-valuemin="0" aria-valuemax="100" aria-valuenow="50" aria-label="Volume"></div>
```

**3. Checkbox Using ARIA**
```html
<div role="checkbox" aria-checked="false" tabindex="0" aria-label="Subscribe to newsletter"></div>
```

**4. Inaccessible Custom Element (✗)**
```html
<!-- No role or label for assistive technologies -->
<div onclick="submitForm()">Submit</div>
```

**Accessibility Benefits**
- Enables screen readers and other assistive tools to interact with custom widgets.
- Makes interactive components usable for users relying on keyboard and screen readers.

**Best Practices**
- Use native HTML elements where possible (e.g., `<button>`, `<input>`).
- When using custom components, supplement with ARIA roles, states, and properties.
- Test components with screen readers and keyboard-only navigation.
- Keep ARIA roles up to date with the component's actual behavior.

---

#### **4.1.3 Status Messages (Level AA)**

**Intent:**  
Ensure that users of assistive technologies are informed of important changes in content that do not receive focus, such as status messages or dynamic updates.

**Requirements**
- Status messages must be programmatically determinable through role or properties such that assistive technologies can present them to the user without receiving focus.
- Applies to messages like: form validation results, confirmation messages, loading indicators, etc.

**Examples**

**1. ARIA Live Region for Form Feedback**
```html
<div aria-live="polite" id="statusMessage"></div>
<script>
  function showStatus(msg) {
    document.getElementById("statusMessage").textContent = msg;
  }
</script>
```

**2. ARIA Role "status" Example**
```html
<div role="status">Your settings have been saved successfully.</div>
```

**3. ARIA Alert for Important Errors**
```html
<div role="alert">Payment failed. Please try again.</div>
```

**Accessibility Benefits**
- Keeps users informed of key actions or results, even when content is updated without focus movement.
- Prevents users from missing critical information due to screen reader silence.

**Best Practices**
- Use `aria-live="polite"` for low-priority updates, `aria-live="assertive"` or `role="alert"` for urgent content.
- Avoid updating live regions too frequently.
- Ensure content in live regions is meaningful and concise.
- Test with various assistive technologies to ensure announcements are delivered properly.

---
