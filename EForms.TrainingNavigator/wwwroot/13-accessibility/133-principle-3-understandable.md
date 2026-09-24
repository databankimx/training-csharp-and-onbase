## Principle 3: Understandable

*Information and the operation of the user interface must be understandable.*

**Overview**

**Principle 3: Understandable** ensures that **content and interface behavior must be clear and predictable**. This principle supports users who may have **cognitive impairments, reading difficulties, or limited familiarity with digital interfaces** by emphasizing consistency, clarity, and helpful feedback.

Users should be able to comprehend the information presented to them and understand how to interact with various controls, forms, and navigational structures.

---

**Key Concepts**

Understandability is achieved through **simple language, consistent navigation, descriptive labels, and helpful error recovery mechanisms**. For example, every form should clearly label its fields, provide instructions where necessary, and explain how to fix input errors in a user-friendly way.

Consistency also plays a major role. Buttons that perform the same action across pages should use the same label. Navigation menus should remain in the same location and order. These practices help users build confidence and avoid confusion.

In addition, language tagging of content ensures that screen readers and translation tools can correctly interpret the material, particularly in multilingual contexts.

---

**Covered Guidelines**

Principle 3 includes the following WCAG guidelines:
- **3.1 Readable** - Make text content understandable by defining language and simplifying language use
- **3.2 Predictable** - Ensure consistent behavior across user interfaces
- **3.3 Input Assistance** - Help users avoid and correct mistakes when entering data

---

When digital experiences follow Principle 3, users are empowered to **navigate confidently, understand content, and recover from mistakes** - fostering independence, trust, and ease of use across all abilities.

---

**Guideline Links**

- [Guideline 3.1: Readable](#guideline-31-readable)
- [Guideline 3.2: Predictable](#guideline-32-predictable)
- [Guideline 3.3: Input Assistance](#guideline-33-input-assistance)

---

### Guideline 3.1: Readable

*Make text readable and understandable.*

**Overview**

**Guideline 3.1** ensures that **text content is readable and understandable**, especially for users with **cognitive disabilities, language barriers, or screen reader reliance**. It focuses on identifying the **language of content** and minimizing complexity where necessary.

This guideline improves accessibility by enabling assistive technologies to properly interpret and pronounce text, and by providing help for unusual words or abbreviations.

---

**Success Criteria Summary**

| Success Criterion | Description |
|------------------|-------------|
| **3.1.1 Language of Page** | Define the primary language of the document |
| **3.1.2 Language of Parts** | Mark passages in a different language |
| **3.1.3 Unusual Words** | *[AAA]* Provide explanations for jargon or idioms |
| **3.1.4 Abbreviations** | *[AAA]* Provide expansions for abbreviations |
| **3.1.5 Reading Level** | *[AAA]* Ensure content is below lower secondary level or offer summaries |
| **3.1.6 Pronunciation** | *[AAA]* Provide pronunciation help if needed for meaning |

---

**Visual Examples**

**✓ 3.1.1 Set Page Language**

```html
<html lang="en">
```

**Why it works:** Screen readers will read text using English pronunciation rules.

---

**✓ 3.1.2 Language of Inline Span**

```html
<p>He greeted me with <span lang="fr">bonjour</span>.</p>
```

**Why it works:** French word is announced correctly by screen readers.

---

**✓ 3.1.4 Expand Abbreviation (AAA)**

```html
<p><abbr title="World Health Organization">WHO</abbr> is a global health agency.</p>
```

**Why it works:** Users can access the full term, improving clarity.

---

**✓ 3.1.3 Explain Unusual Words or Idioms (AAA)**

```html
<p>He was given the **green light** <span class="definition">(permission to proceed)</span>.</p>
```

**Why it works:** Provides explanation for a figurative phrase.

---

**✓ 3.1.5 Offer Simpler Summary (AAA)**

```html
<p><strong>Summary:</strong> This article explains how plants grow from seeds using water, sunlight, and nutrients.</p>
```

**Why it works:** Simplifies complex content for users at lower reading levels.

---

**Checklist**

| Requirement                                      | Example Provided | Meets Standard |
|-------------------------------------------------|------------------|----------------|
| Primary document language is defined            | ✓ Yes            | ✓             |
| Foreign phrases are tagged with correct language| ✓ Yes            | ✓             |
| Unusual or idiomatic words are explained (AAA)  | ✓ Yes            | ✓             |
| Abbreviations are expanded or defined (AAA)     | ✓ Yes            | ✓             |
| Content can be summarized in simple language (AAA) | ✓ Yes         | ✓             |

---

Guideline 3.1 improves access to web content for **all users by increasing clarity, predictability, and correct interpretation** - especially for those using assistive tools or learning in a second language.

**Success Criteria Links**

- [3.1.1 Language of Page (Level A)](#311-language-of-page-level-a)
- [3.1.2 Language of Parts (Level AA)](#312-language-of-parts-level-aa)
- <span class="aaa">3.1.3 Unusual Words (Level AAA)</span>
- <span class="aaa">3.1.4 Abbreviations (Level AAA)</span>
- <span class="aaa">3.1.5 Reading Level (Level AAA)</span>
- <span class="aaa">3.1.6 Pronunciation (Level AAA)</span>

---

#### **3.1.1 Language of Page (Level A)**

**Intent:**  
Ensure that user agents (browsers, screen readers, etc.) can correctly present content by identifying the primary human language of each web page.

**Requirements**
- The default language of the document must be set using the `lang` attribute in the HTML element.
- The language code should conform to BCP 47 (e.g., `en`, `fr`, `es-ES`).

**Examples**

**1. Setting Default Language to English**
```html
<html lang="en">
<head>
  <title>My Website</title>
</head>
<body>
  <h1>Welcome!</h1>
</body>
</html>
```

**2. Setting Default Language to French**
```html
<html lang="fr">
<head>
  <title>Mon Site Web</title>
</head>
<body>
  <h1>Bienvenue!</h1>
</body>
</html>
```

**Accessibility Benefits**
- Enables screen readers to apply correct pronunciation and rules for the language.
- Helps users who rely on translation tools or multilingual support features.

**Best Practices**
- Always set the `lang` attribute on the root HTML element.
- Use accurate and complete language tags (e.g., `en-US`, `pt-BR`) where possible.
- If content changes language within the page, use `lang` on individual elements as needed (see SC 3.1.2).
- Validate the HTML to ensure the language attribute is applied correctly.

---

#### **3.1.2 Language of Parts (Level AA)**

**Intent:**  
Ensure that assistive technologies correctly interpret and present sections of content that differ in language from the default page language.

**Requirements**
- When a section of the content is in a different language than the primary language of the page, it must be identified with a `lang` attribute.
- The language tag must conform to BCP 47.

**Examples**

**1. Inline French Phrase in an English Page**
```html
<p>She said, <span lang="fr">Je t'aime</span>, which means "I love you."</p>
```

**2. Paragraph in Spanish in an English Page**
```html
<p lang="es">Este documento está escrito en español.</p>
```

**3. German Heading Within English Content**
```html
<h2 lang="de">Willkommen</h2>
```

**Accessibility Benefits**
- Screen readers switch pronunciation rules to match the correct language.
- Improves understanding for multilingual users.
- Helps translation and language-based tools function properly.

**Best Practices**
- Use `lang` on any element containing content in a different language.
- Avoid overuse of `lang` where not needed (e.g., common foreign terms users will already recognize).
- Always verify language changes through testing with assistive technologies.

---

<span class="aaa">

... The following criteria (level AAA) are omitted from this guide ...

- **3.1.3 Unusual Words**
    - A mechanism is available for identifying specific definitions of words or phrases used in an unusual or restricted way, including idioms and jargon.
- **3.1.4 Abbreviations**
    - A mechanism for identifying the expanded form or meaning of abbreviations is available.
- **3.1.5 Reading Level**
    - When text requires reading ability more advanced than the lower secondary education level after removal of proper names and titles, supplemental content, or a version that does not require reading ability more advanced than the lower secondary education level, is available.
- **3.1.6 Pronunciation**
    - A mechanism is available for identifying specific pronunciation of words where meaning of the words, in context, is ambiguous without knowing the pronunciation.

</span>

---

### Guideline 3.2: Predictable

*Make Web pages appear and operate in predictable ways.*

**Overview**

**Guideline 3.2** ensures that web pages and components behave in **predictable ways**. This helps users - especially those with **cognitive disabilities, motor impairments, or anxiety** - to feel in control of the interface and avoid confusion from unexpected changes.

This guideline focuses on keeping the user experience consistent, intuitive, and free from surprises during interaction.

---

**Success Criteria Summary**

| Success Criterion | Description |
|------------------|-------------|
| **3.2.1 On Focus** | Elements do not change context automatically when they receive focus |
| **3.2.2 On Input** | Changes in context only occur after user is informed or confirms |
| **3.2.3 Consistent Navigation** | Navigation components appear in the same relative order |
| **3.2.4 Consistent Identification** | Identical elements have the same label and role |
| **3.2.5 Change on Request** | *[AAA]* No change of context unless initiated by user |

---

**Visual Examples**

**✗ Failure - 3.2.1 Change on Focus**

```html
<select onfocus="location.href=this.value">
  <option value="/about">About Us</option>
  <option value="/contact">Contact</option>
</select>
```

**Issue:** Just focusing the dropdown redirects the user, without interaction.

---

**✓ 3.2.2 Change on Input *with Confirmation***

```html
<form>
  <label for="lang">Language:</label>
  <select id="lang" name="lang">
    <option value="en">English</option>
    <option value="fr">French</option>
  </select>
  <button type="submit">Apply</button>
</form>
```

**Why it works:** Changes only occur after user submits.

---

**✓ 3.2.3 Consistent Navigation Example**

```html
<nav>
  <ul>
    <li><a href="/home">Home</a></li>
    <li><a href="/services">Services</a></li>
    <li><a href="/contact">Contact</a></li>
  </ul>
</nav>
```

**Why it works:** Navigation is the same across pages.

---

**✓ 3.2.4 Consistent Labeling**

```html
<!-- Button on Page A -->
<button aria-label="Submit Order">Submit Order</button>

<!-- Button on Page B -->
<button aria-label="Submit Order">Submit Order</button>
```

**Why it works:** Same label for same function across pages.

---

**Checklist**

| Requirement                                      | Example Provided | Meets Standard |
|-------------------------------------------------|------------------|----------------|
| Focus does not trigger unexpected changes       | ✓ Yes            | ✓             |
| Input changes are confirmed before context shift| ✓ Yes            | ✓             |
| Navigation and labels are consistent            | ✓ Yes            | ✓             |
| User controls changes in context (AAA)          | ✓ Yes            | ✓             |

---

Guideline 3.2 helps users develop **trust and fluency** with your interface by ensuring it responds consistently and predictably to their actions.

**Success Criteria Links**

- [3.2.1 On Focus (Level A)](#321-on-focus-level-a)
- [3.2.2 On Input (Level A)](#322-on-input-level-a)
- [3.2.3 Consistent Navigation (Level AA)](#323-consistent-navigation-level-aa)
- [3.2.4 Consistent Identification (Level AA)](#324-consistent-identification-level-aa)
- <span class="aaa">3.2.5 Change on Request (Level AAA)</span>

---

#### **3.2.1 On Focus (Level A)**

**Intent:**  
Prevent unexpected changes in context when an element receives focus, ensuring that users remain in control of their navigation and interaction.

**Requirements**
- When an element receives focus, it must not initiate a change of context.
- "Change of context" includes:
  - Opening a new window
  - Moving focus to another element
  - Submitting a form
  - Changing the content of the page significantly

**Examples**

**1. Good Example - No Action on Focus**
```html
<input type="text" name="username" placeholder="Enter username">
```

**2. Bad Example (✗) - Form Submits on Focus**
```html
<select name="pages" onchange="this.form.submit()">
  <option value="home">Home</option>
  <option value="about">About</option>
</select>
```

**3. Acceptable Use - Submit on Change but Not on Focus**
```html
<select name="pages" onchange="navigateToPage(this.value)">
  <option value="">Select a page</option>
  <option value="home">Home</option>
  <option value="contact">Contact</option>
</select>
```

**Accessibility Benefits**
- Allows users, especially those using keyboards or assistive technologies, to explore content without triggering unwanted actions.
- Reduces confusion and loss of control, especially for users with cognitive or motor disabilities.

**Best Practices**
- Use `onchange` instead of `onfocus` to trigger actions only when a user makes a deliberate selection.
- Ensure focus behavior is consistent and predictable.
- Test forms and menus with keyboard navigation to confirm no actions are triggered on focus.

---

#### **3.2.2 On Input (Level A)**

**Intent:**  
Ensure that changes to form controls or user interface components do not automatically trigger significant changes in context, helping users maintain orientation and control.

**Requirements**
- A change of setting or data must not automatically cause a change of context.
- If a change in context *is* triggered, the user must be advised beforehand.

**What qualifies as a change of context?**
- Submitting a form automatically.
- Opening a new window or tab.
- Moving keyboard focus.
- Updating significant page content unexpectedly.

**Examples**

**1. Good Example - Controlled Form Behavior**
```html
<select name="size" id="shirt-size">
  <option value="s">Small</option>
  <option value="m">Medium</option>
  <option value="l">Large</option>
</select>
```

**2. Bad Example (✗) - Form Auto-Submits on Selection**
```html
<select name="country" onchange="this.form.submit()">
  <option value="us">USA</option>
  <option value="ca">Canada</option>
</select>
```

**3. Acceptable if Explained**
```html
<p>Changing your country will reload the page with local tax information.</p>
<select name="country" onchange="location.reload()">
  <option value="us">USA</option>
  <option value="uk">UK</option>
</select>
```

**Accessibility Benefits**
- Reduces surprise and confusion for keyboard-only and screen reader users.
- Enables users to confidently interact with form elements without unexpected navigation or content shifts.

**Best Practices**
- Inform users before initiating any automatic changes.
- Use explicit submit buttons or confirmation steps for significant context changes.
- Test all form interactions for keyboard and assistive technology users.

---

#### **3.2.3 Consistent Navigation (Level AA)**

**Intent:**  
Help users develop familiarity and predictability across a website or application by presenting repeated navigation elements in the same relative order each time they appear.

**Requirements**
- Navigational mechanisms that are repeated across web pages must occur in the same relative order, unless a change is initiated by the user.

**Examples**

**1. Consistent Header Navigation**
```html
<header>
  <nav>
    <ul>
      <li><a href="/home">Home</a></li>
      <li><a href="/about">About</a></li>
      <li><a href="/contact">Contact</a></li>
    </ul>
  </nav>
</header>
```

**2. Side Menu Appears in the Same Position**
```html
<aside>
  <ul>
    <li><a href="/dashboard">Dashboard</a></li>
    <li><a href="/settings">Settings</a></li>
  </ul>
</aside>
```

**3. Bad Example (✗) - Reordered Navigation**
```html
<!-- On homepage -->
<nav>
  <a href="/about">About</a> | <a href="/home">Home</a>
</nav>
<!-- On contact page -->
<nav>
  <a href="/home">Home</a> | <a href="/about">About</a>
</nav>
```

**Accessibility Benefits**
- Supports users with memory impairments or cognitive limitations.
- Enhances efficiency and predictability for screen reader and keyboard users.

**Best Practices**
- Define a standard layout and navigation structure.
- Use templating systems or reusable components to enforce consistency.
- Avoid dynamically reordering or omitting core navigation items unless clearly user-driven.

---

#### **3.2.4 Consistent Identification (Level AA)**

**Intent:**  
Ensure that components with the same functionality are identified consistently across a website or application to reduce user confusion.

**Requirements**
- Components that have the same functionality within a set of web pages must be identified consistently.
- This includes consistent labeling, icon usage, alt text, and accessible names.

**Examples**

**1. Consistent Button Labeling Across Pages**
```html
<!-- On homepage -->
<button>Search</button>

<!-- On contact page -->
<button>Search</button>
```

**2. Consistent ARIA Labels**
```html
<!-- Navigation landmarks on multiple pages -->
<nav aria-label="Main Navigation">
  <!-- ... -->
</nav>
```

**3. Inconsistent Example (✗)**
```html
<!-- On one page -->
<img src="cart.png" alt="Shopping Cart">

<!-- On another page -->
<img src="cart.png" alt="Basket">
```

**Accessibility Benefits**
- Helps users with cognitive disabilities understand and predict interface behavior.
- Supports users of screen readers by avoiding inconsistent terminology or labels.

**Best Practices**
- Use consistent alt text, labels, and roles for repeated UI elements.
- Standardize button labels and menu item names across your site.
- Avoid synonyms for key functions (e.g., "basket" vs. "cart").
- Verify that icon meanings are clear and consistently represented with text equivalents.

---

<span class="aaa">

... The following criteria (level AAA) are omitted from this guide ...

- **3.2.5 Change on Request**
    -  Changes of context are initiated only by user request or a mechanism is available to turn off such changes.

</span>

---

### Guideline 3.3: Input Assistance

*Help users avoid and correct mistakes.*

**Overview**

**Guideline 3.3** helps users **avoid and correct mistakes** when entering information in forms or other input fields. This is especially critical for users with **cognitive disabilities, learning differences, low vision**, or those using assistive technologies.

The guideline emphasizes the use of clear labels, instructions, error identification, and helpful suggestions to reduce frustration and improve task success.

---

**Success Criteria Summary**

| Success Criterion | Description |
|------------------|-------------|
| **3.3.1 Error Identification** | Inform users when errors are detected in inputs |
| **3.3.2 Labels or Instructions** | Provide clear labels and/or instructions |
| **3.3.3 Error Suggestion** | Offer suggestions to fix identified errors |
| **3.3.4 Error Prevention (Legal, Financial, Data)** | Confirm or allow reversal before committing sensitive info |
| **3.3.5 Help** | *[AAA]* Provide context-sensitive help for input fields |
| **3.3.6 Error Prevention (All)** | *[AAA]* Use at least one method to prevent all input errors |

---

**Visual Examples**

**✓ 3.3.1 Error Message with Explanation**

```html
<input type="email" id="userEmail" required>
<div id="emailError" style="color: red;">Please enter a valid email address.</div>
```

**Why it works:** Identifies the input with an error and describes what's wrong.

---

**✓ 3.3.2 Label and Format Help**

```html
<label for="dob">Date of Birth</label>
<input type="text" id="dob" aria-describedby="dobFormat">
<span id="dobFormat">Format: MM/DD/YYYY</span>
```

**Why it works:** Combines a label with visible instructions.

---

**✓ 3.3.3 Error Suggestion**

```html
<div class="error">Password too short. Use at least 8 characters.</div>
```

**Why it works:** Offers a fix for the error to guide the user.

---

**✓ 3.3.4 Confirmation Step**

```html
<h3>Review your order</h3>
<p>Product: Widget</p>
<p>Total: $99.99</p>
<button type="submit">Confirm Purchase</button>
```

**Why it works:** Prevents immediate submission of legal/financial data without review.

---

**✓ 3.3.5 Help Icon for Complex Fields (AAA)**

```html
<label for="taxId">Tax ID <span title="Enter your 9-digit government-issued number.">?</span></label>
<input type="text" id="taxId">
```

**Why it works:** Offers optional help near the input.

---

**Checklist**

| Requirement                                             | Example Provided | Meets Standard |
|--------------------------------------------------------|------------------|----------------|
| Input errors identified and explained                  | ✓ Yes            | ✓             |
| Clear labels and formatting instructions provided      | ✓ Yes            | ✓             |
| Suggestions given to fix errors                        | ✓ Yes            | ✓             |
| User can review or reverse critical transactions       | ✓ Yes            | ✓             |
| Optional help available for complex fields (AAA)       | ✓ Yes            | ✓             |

---

Guideline 3.3 empowers users to complete tasks accurately by providing the **guidance, feedback, and safety nets** necessary to minimize mistakes and confusion.

**Success Criteria Links**

- [3.3.1 Error Identification (Level A)](#331-error-identification-level-a)
- [3.3.2 Labels or Instructions (Level A)](#332-labels-or-instructions-level-a)
- [3.3.3 Error Suggestion (Level AA)](#333-error-suggestion-level-aa)
- [3.3.4 Error Prevention (Legal, Financial, Data) (Level AA)](#334-error-prevention-legal-financial-data-level-aa)
- <span class="aaa">3.3.5 Help (Level AAA)</span>
- <span class="aaa">3.3.6 Error Prevention (All) (Level AAA)</span>

---

#### **3.3.1 Error Identification (Level A)**

**Intent:**  
Ensure that users are informed when an input error is detected so they can correct it.

**Requirements**
- If an input error is automatically detected, the item must be identified and described to the user in text.
- Applies to forms and other inputs where correct data entry is expected.

**Examples**

**1. Text-Based Error Message**
```html
<label for="email">Email:</label>
<input id="email" name="email" type="email" required>
<span class="error" id="emailError">Please enter a valid email address.</span>
```

**2. Accessible ARIA Error Association**
```html
<input id="username" aria-describedby="usernameError">
<span id="usernameError">Username is required.</span>
```

**3. Incorrect Example (✗) - Visual Cues Only**
```html
<!-- Using color change alone to indicate error -->
<input style="border: 1px solid red">
```

**Accessibility Benefits**
- Helps users understand what needs to be corrected.
- Enables screen readers to convey error messages to blind or low vision users.

**Best Practices**
- Always provide text-based error descriptions.
- Link errors to their related inputs using `aria-describedby` or appropriate HTML structure.
- Avoid relying solely on visual cues like color or icons.
- Clearly mark required fields and validate inputs on both client and server sides.

---

#### **3.3.2 Labels or Instructions (Level A)**

**Intent:**  
Help users understand what input is expected by providing clear labels or instructions for each form control or interactive element.

**Requirements**
- Labels or instructions must be provided when user input is required.
- The guidance must be programmatically associated with the input when possible.

**Examples**

**1. Visible Text Label**
```html
<label for="fullname">Full Name:</label>
<input type="text" id="fullname" name="fullname">
```

**2. Inline Instruction Example**
```html
<label for="password">Password (min 8 characters):</label>
<input type="password" id="password" name="password">
```

**3. ARIA-Labeled Form Field**
```html
<input type="text" id="promo" aria-label="Enter promotional code">
```

**4. Placeholder Is Not a Label (✗)**
```html
<input type="email" placeholder="Email">
<!-- Placeholder alone is not sufficient for labeling -->
```

**Accessibility Benefits**
- Ensures all users, including those using screen readers, understand what is required.
- Reduces the likelihood of input errors.

**Best Practices**
- Use the `<label>` element wherever possible to associate text with form inputs.
- Supplement labels with inline or adjacent instructions if needed.
- Avoid using only placeholders as labels - they disappear when users type.
- Use `aria-label` or `aria-labelledby` for non-standard controls.

---

#### **3.3.3 Error Suggestion (Level AA)**

**Intent:**  
Assist users in correcting input errors by providing clear, constructive suggestions for fixing the issues.

**Requirements**
- If an input error is automatically detected and suggestions for correction are known, they must be provided to the user.

**Examples**

**1. Invalid Email With Suggestion**
```html
<label for="email">Email:</label>
<input type="email" id="email" name="email" required>
<span class="error">Please enter a valid email address (e.g., user@example.com).</span>
```

**2. Password Requirement Violation**
```html
<label for="password">Password:</label>
<input type="password" id="password" name="password">
<span class="error">Password must be at least 8 characters long and contain a number.</span>
```

**3. Missing Required Field With Prompt**
```html
<label for="city">City:</label>
<input id="city" name="city" required>
<span class="error">Please enter your city.</span>
```

**Accessibility Benefits**
- Provides actionable feedback that helps all users complete forms successfully.
- Especially beneficial to users with cognitive or learning disabilities.

**Best Practices**
- Use plain language and be specific in your suggestions.
- Display suggestions adjacent to the input field.
- Avoid ambiguous or generic error messages.
- Combine this with visual cues and screen reader-friendly alerts.

---

#### **3.3.4 Error Prevention (Legal, Financial, Data) (Level AA)**

**Intent:**  
Minimize serious consequences by requiring user confirmation or allowing them to review and correct input before finalizing submissions that involve legal, financial, or personal data.

**Requirements**
For web pages that require user input and where submitting that input:
- causes a legal commitment,
- results in financial transactions,
- or modifies/deletes user-controllable data,

**At least one of the following must be true:**
1. Submissions are reversible.
2. Data is checked for errors, and users are given a chance to correct them.
3. Users can review and confirm the information before finalizing.

**Examples**

**1. Confirmation Page for Online Purchase**
```html
<h2>Review Your Order</h2>
<p>Product: Wireless Headphones</p>
<p>Price: $79.99</p>
<button>Confirm Purchase</button>
<button>Edit Order</button>
```

**2. Data Validation With Inline Errors**
```html
<label for="account">Account Number:</label>
<input type="text" id="account" name="account">
<span class="error">Account number must be 10 digits.</span>
```

**3. Undo Feature for Deletion**
```html
<p>Your document has been deleted.</p>
<button onclick="undoDelete()">Undo</button>
```

**Accessibility Benefits**
- Reduces the chance of costly or irreversible mistakes.
- Supports users with cognitive impairments who may need extra review time.

**Best Practices**
- Use confirmation dialogs or review steps before submitting important data.
- Provide clear explanations of actions and their consequences.
- Allow users to undo or modify submissions where possible.

---

<span class="aaa">

... The following criteria (level AAA) are omitted from this guide ...

- **3.3.5 Help**
    - Context-sensitive help is available.
- **3.3.6 Error Prevention (All)**
    - For Web pages that require the user to submit information, at least one of the following is true...

</span>

---
