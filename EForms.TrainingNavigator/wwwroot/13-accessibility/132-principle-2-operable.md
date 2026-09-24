## Principle 2: Operable

*User interface components and navigation must be operable.*

**Overview**

**Principle 2: Operable** ensures that **users can interact with and control interface components** regardless of how they navigate or the assistive technologies they use. It focuses on usability from the perspective of motor access, alternative inputs, and cognitive control.

A user should be able to operate all content using **a keyboard, screen reader, voice command, switch device**, or other input method. To support this, interfaces must be designed to **respond predictably** and allow for flexibility in how users interact with elements like buttons, forms, and menus.

---

**Key Concepts**

An operable interface must not require a specific input method like a mouse or touchscreen gesture alone. For example, **keyboard users must be able to tab through links and controls** in a logical order, with visible focus indicators that show their current position.

The interface should avoid **keyboard traps**, where users can navigate into an element but not out of it. Users must also be allowed **enough time** to read and respond to content, and be protected from actions or stimuli that may cause seizures or physical reactions - such as flashing content or unexpected movements.

Another important aspect is **navigability**: providing clear headings, consistent navigation, and descriptive link text enables users to find and understand their current location within the site or app.

---

**Covered Guidelines**

Principle 2 includes the following WCAG guidelines:
- **2.1 Keyboard Accessible** - Functionality available via keyboard
- **2.2 Enough Time** - Give users time to read and act
- **2.3 Seizures and Physical Reactions** - Avoid triggering content
- **2.4 Navigable** - Help users find and maintain orientation
- **2.5 Input Modalities** - Support all types of input including touch, speech, and gestures

---

**Operability is essential** for ensuring that users can move through and interact with content safely, comfortably, and without unnecessary barriers. It turns static pages into interactive, usable experiences for everyone.

---

**Guideline Links**

- [Guideline 2.1: Keyboard Accessible](#guideline-21-keyboard-accessible)
- [Guideline 2.2: Enough Time](#guideline-22-enough-time)
- [Guideline 2.3: Seizures and Physical Reactions](#guideline-23-seizures-and-physical-reactions)
- [Guideline 2.4: Navigable](#guideline-24-navigable)
- [Guideline 2.5: Input Modalities](#guideline-25-input-modalities)

---

### Guideline 2.1: Keyboard Accessible

*Make all functionality available from a keyboard.*

**Overview**

**Guideline 2.1** ensures that **all functionality is accessible by keyboard**, without requiring a mouse or complex gestures. This is essential for users with **mobility impairments, screen reader users, and others who rely on keyboard navigation** or alternative input devices like switches or speech commands.

This guideline emphasizes building interfaces that are fully operable through simple, sequential keyboard commands such as `Tab`, `Enter`, `Arrow` keys, and shortcuts.

---

**Success Criteria Summary**

| Success Criterion | Description |
|------------------|-------------|
| **2.1.1 Keyboard** | All functions must be operable via keyboard alone |
| **2.1.2 No Keyboard Trap** | Users must be able to move focus away from all components using a keyboard |
| **2.1.3 Keyboard (No Exception)** | *[AAA]* Absolutely no functionality should require a mouse |
| **2.1.4 Character Key Shortcuts** | Provide options to turn off or remap single-key shortcuts |

---

**Visual Examples**

**✓ 2.1.1 Keyboard-Operable Form**

```html
<form>
  <label for="name">Name:</label>
  <input type="text" id="name">
  <button type="submit">Submit</button>
</form>
```

**Why it works:** All inputs and buttons are focusable and operable via keyboard.

---

**✗ Failure - Mouse-only Event**

```html
<div onclick="openMenu()">Menu</div>
```

**Issue:** Cannot be activated by keyboard (`Enter` or `Space`), and lacks a semantic role.

---

**✓ 2.1.2 Focus Can Move In and Out**

```html
<div tabindex="0" onkeydown="if(event.key === 'Escape'){this.blur();}">
  <p>Focusable modal box. Press Esc to exit.</p>
</div>
```

**Why it works:** Allows the user to exit the element with the `Escape` key.

---

**✓ 2.1.4 Disable Character Key Shortcut**

```html
<button accesskey="S" aria-keyshortcuts="Alt+S">Save</button>
```

**Note:** For single-key commands, ensure they are not active unless focus is in a controlled input region, or allow remapping through settings.

---

**Checklist**

| Requirement                                | Example Provided | Meets Standard |
|-------------------------------------------|------------------|----------------|
| All functionality available via keyboard  | ✓ Yes            | ✓             |
| No elements trap keyboard focus           | ✓ Yes            | ✓             |
| Single-key shortcuts manageable or remappable | ✓ Yes        | ✓             |
| Keyboard support works across browsers    | ✓ Yes            | ✓             |

---

Guideline 2.1 is foundational for accessibility, ensuring that **every user, regardless of device or mobility**, can interact fully with the content and controls.

**Success Criteria Links**

- [2.1.1 Keyboard (Level A)](#211-keyboard-level-a)
- [2.1.2 No Keyboard Trap (Level A)](#212-no-keyboard-trap-level-a)
- <span class="aaa">2.1.3 Keyboard (No Exception) (Level AAA)</span>
- [2.1.4 Character Key Shortcuts (Level A)](#214-character-key-shortcuts-level-a)

---

#### **2.1.1 Keyboard (Level A)**

**Intent:**  
Ensure that all functionality on a website is accessible using a keyboard alone, without requiring a mouse or pointer device.

**Requirements**
- All interactive elements must be operable via standard keyboard input (e.g., Tab, Enter, Arrow keys).
- No keyboard-only user should encounter a situation where they are "trapped" or unable to progress through the interface.

**Examples**

**1. Button Accessible with Tab and Enter**
```html
<button onclick="openMenu()" tabindex="0">Menu</button>
```

**2. Custom Widget with Keyboard Support**
```javascript
element.addEventListener('keydown', function(e) {
  if (e.key === 'Enter' || e.key === ' ') {
    performAction();
  }
});
```

**3. Accessible Modal Dialog**
```html
<div role="dialog" aria-modal="true">
  <button autofocus>Close</button>
</div>
```

**Accessibility Benefits**
- Supports users who rely on switch controls, screen readers, or alternative input devices.
- Helps users with motor impairments who cannot use a mouse.

**Best Practices**
- Use semantic HTML elements (e.g., `<button>`, `<a>`) which are natively keyboard accessible.
- Ensure focus styles are visible and logical.
- Use `tabindex="0"` to make all non-focusable elements keyboard accessible.

**Notes:**

> This exception relates to the underlying function, not the input technique. For example, if using handwriting to enter text, the input technique (handwriting) requires path-dependent input but the underlying function (text input) does not.

> This does not forbid and should not discourage providing mouse input or other input methods in addition to keyboard operation.

---

#### **2.1.2 No Keyboard Trap (Level A)**

**Intent:**  
Ensure that keyboard-only users are not trapped within any element and can navigate away using standard keyboard controls.

**Requirements**
- If a keyboard trap is possible (e.g., custom widgets or modals), provide a clear method to exit using the keyboard.
- Focus should be able to move both into and out of interactive components.

**Examples**

**1. Modal Dialog with Close Button**
```html
<div role="dialog" aria-modal="true">
  <button autofocus>Close</button>
  <p>This is a modal dialog. Press Esc to close.</p>
</div>
```

**2. JavaScript Trap Escape for Custom Component**
```javascript
const widget = document.querySelector('#widget');
widget.addEventListener('keydown', (e) => {
  if (e.key === 'Escape') widget.blur();
});
```

**3. Bad Example (✗): No Exit Mechanism**
```html
<div tabindex="0" onkeydown="event.preventDefault();">Custom widget trap</div>
```

**Accessibility Benefits**
- Prevents users from becoming stuck in interactive components.
- Supports smooth navigation with screen readers and switch devices.

**Best Practices**
- Avoid overriding standard keyboard behavior unless necessary.
- Always provide an exit or movement option like Esc or Tab.
- Test your UI with only the keyboard to verify all pathways in and out.

> Note: Since any content that does not meet this success criterion can interfere with a user's ability to use the whole page, all content on the Web page (whether it is used to meet other success criteria or not) must meet this success criterion.

---

<span class="aaa">

... The following criteria (level AAA) are omitted from this guide ...

- **2.1.3 Keyboard (No Exception)**
    - All functionality of the content is operable through a keyboard interface without requiring specific timings for individual keystrokes.

</span>

---

#### **2.1.4 Character Key Shortcuts (Level A)**

**Intent:**  
Ensure that single-key shortcuts (e.g., letter or number keys) do not interfere with users relying on speech input or assistive technologies.

**Requirements**
If a keyboard shortcut is implemented using only letter (A-Z), punctuation, number (0-9), or symbol characters, at least one of the following must be true:
- The shortcut can be turned off.
- The shortcut can be remapped.
- The shortcut is only active when the component has focus.

**Examples**

**1. Toggle Shortcut Enabled Only on Focus**
```html
<button id="quickToggle" accesskey="t">Toggle Setting</button>
```

**2. Disable Shortcut Outside of Focus**
```javascript
let shortcutEnabled = false;
document.getElementById('targetInput').addEventListener('focus', ()
    => shortcutEnabled = true);
document.getElementById('targetInput').addEventListener('blur', ()
    => shortcutEnabled = false);
document.addEventListener('keydown', e => {
    if (shortcutEnabled && e.key === 't') toggleSomething();
});
```

**Accessibility Benefits**
- Prevents accidental activation of features by speech input users or screen readers.
- Gives users control over when keyboard shortcuts are active.

**Best Practices**
- Use modifier keys (Ctrl, Alt, etc.) rather than single characters.
- Provide settings to enable/disable or reassign shortcuts.
- Document any shortcuts and their conditions clearly for all users.

---

### Guideline 2.2: Enough Time

*Provide users enough time to read and use content.*

**Overview**

**Guideline 2.2** ensures that users are given **enough time to read and interact with content**. It is especially important for people with **cognitive limitations, reading disabilities, physical disabilities**, or users who require assistive technologies that may slow navigation.

The guideline requires that time limits be avoidable or adjustable, animations be controllable, and interruptions be manageable - ensuring that users are not unfairly rushed or disoriented by sudden changes.

---

**Success Criteria Summary**

| Success Criterion | Description |
|------------------|-------------|
| **2.2.1 Timing Adjustable** | Allow users to turn off, adjust, or extend time limits |
| **2.2.2 Pause, Stop, Hide** | Provide user controls to pause, stop, or hide moving content |
| **2.2.3 No Timing** | *[AAA]* Content does not rely on time limits |
| **2.2.4 Interruptions** | *[AAA]* Allow postponing or suppressing interruptions |
| **2.2.5 Re-authenticating** | *[AAA]* Re-authentication preserves user data |
| **2.2.6 Timeouts** | *[AAA]* Warn users of timeouts and the effects of inactivity |

---

**Visual Examples**

**✓ 2.2.1 Adjustable Timeout Prompt**

```html
<p>Your session will expire in 60 seconds.</p>
<button onclick="extendSession()">Extend Session</button>
```

**Why it works:** Users can extend their session to avoid forced timeouts.

---

**✓ 2.2.2 Pause Moving Carousel**

```html
<div id="carousel" aria-live="off">
  <button onclick="pauseCarousel()">Pause</button>
  <div class="slide">Slide 1</div>
</div>
```

**Why it works:** User is given control over auto-advancing animations.

---

**✗ Failure - Auto-Redirect Without Warning**

```html
<meta http-equiv="refresh" content="10; url=/next-page">
```

**Issue:** Page changes after 10 seconds without user control.

---

**✓ 2.2.6 Timeout Warning for Inactivity (AAA)**

```html
<script>
  setTimeout(() => {
    alert("You've been inactive for 4 minutes. Click OK to remain logged in.");
  }, 240000);
</script>
```

**Why it works:** Notifies user before session expires, allowing them to take action.

---

**Checklist**

| Requirement                                     | Example Provided | Meets Standard |
|------------------------------------------------|------------------|----------------|
| Users can extend or disable time limits        | ✓ Yes            | ✓             |
| Users can pause or stop animations             | ✓ Yes            | ✓             |
| No unexpected auto-refresh or redirection      | ✓ Yes            | ✓             |
| Session timeout warnings provided (AAA)         | ✓ Yes            | ✓             |

---

By following Guideline 2.2, developers ensure that users have **enough time to understand, respond, and act on content** - eliminating barriers caused by restrictive or inaccessible time-based interactions.

**Success Criteria Links**

- [2.2.1 Timing Adjustable (Level A)](#221-timing-adjustable-level-a)
- [2.2.2 Pause, Stop, Hide (Level A)](#222-pause-stop-hide-level-a)
- <span class="aaa">2.2.3 No Timing (Level AAA)</span>
- <span class="aaa">2.2.4 Interruptions (Level AAA)</span>
- <span class="aaa">2.2.5 Re-authenticating (Level AAA)</span>
- <span class="aaa">2.2.6 Timeouts (Level AAA)</span>

---

#### **2.2.1 Timing Adjustable (Level A)**

**Intent:**  
Ensure users have sufficient time to read and interact with content. If there are time limits, users should be able to adjust or disable them.

**Requirements**
If a time limit is set, one or more of the following must be true:
- The user can turn off the time limit.
- The user can adjust the time limit to at least 10 times the default.
- The user is warned before time expires and given at least 20 seconds to extend it.

**Exceptions**
- Real-time events (e.g., auctions) where timing is essential.
- Time limits essential for valid activity (e.g., online exams).
- 20-hour or longer time limits.

**Examples**

**1. Extend Session Warning**
```html
<p>Your session will expire in <span id="timer">60</span> seconds. <button onclick="extendSession()">Extend</button></p>
```

**2. Adjusting Timeout Duration**
```html
<label for="timeout">Select timeout duration:</label>
<select id="timeout">
  <option value="5">5 minutes</option>
  <option value="15">15 minutes</option>
  <option value="30">30 minutes</option>
</select>
```

**Accessibility Benefits**
- Helps users with cognitive or physical disabilities who need more time.
- Prevents sudden loss of data or progress for slower users.

**Best Practices**
- Inform users of any time limits at the beginning.
- Provide clear, accessible mechanisms to extend or disable time limits.
- Avoid using timeouts unless strictly necessary.

> Note: This success criterion helps ensure that users can complete tasks without unexpected changes in content or context that are a result of a time limit. This success criterion should be considered in conjunction with Success Criterion 3.2.1, which puts limits on changes of content or context as a result of user action.

---

#### **2.2.2 Pause, Stop, Hide (Level A)**

**Intent:**  
Allow users to pause, stop, or hide moving, blinking, or scrolling content to avoid distractions and maintain control over their experience.

**Requirements**
If content moves, blinks, scrolls, or auto-updates and:
- Starts automatically,
- Lasts more than 5 seconds,
- And is presented in parallel with other content,

...then users must be able to pause, stop, or hide it.

**Exceptions:**
- Essential animations (e.g., loading spinner).
- Content necessary for functionality that would be invalid if stopped (e.g., stock ticker in a financial trading tool).

**Examples**

**1. Pause Button for Auto-Scrolling Carousel**
```html
<button onclick="pauseCarousel()">Pause</button>
```

**2. CSS Animation with Reduced Motion Option**
```css
@media (prefers-reduced-motion: reduce) {
  .animated {
    animation: none;
  }
}
```

**3. JavaScript Toggle for Blinking Notice**
```javascript
let blinking = true;
function toggleBlink() {
  blinking = !blinking;
  document.getElementById('alert').style.visibility = blinking ? 'visible' : 'hidden';
}
```

**Accessibility Benefits**
- Helps users with attention deficits or cognitive impairments who are easily distracted.
- Supports individuals with vestibular disorders affected by motion.

**Best Practices**
- Use animations only when necessary.
- Provide clear, visible controls to pause or hide content.
- Respect user OS/browser-level motion preferences.

**Notes:**

> For requirements related to flickering or flashing content, refer to Guideline 2.3.

> Since any content that does not meet this success criterion can interfere with a user's ability to use the whole page, all content on the Web page (whether it is used to meet other success criteria or not) must meet this success criterion.

> Content that is updated periodically by software or that is streamed to the user agent is not required to preserve or present information that is generated or received between the initiation of the pause and resuming presentation, as this may not be technically possible, and in many situations could be misleading to do so.

> An animation that occurs as part of a preload phase or similar situation can be considered essential if interaction cannot occur during that phase for all users and if not indicating progress could confuse users or cause them to think that content was frozen or broken.

---

<span class="aaa">

... The following criteria (level AAA) are omitted from this guide ...

- **2.2.3 No Timing**
    - Timing is not an essential part of the event or activity presented by the content, except for non-interactive synchronized media and real-time events.
- **2.2.4 Interruptions**
    - Interruptions can be postponed or suppressed by the user, except interruptions involving an emergency.
- **2.2.5 Re-authenticating**
    - When an authenticated session expires, the user can continue the activity without loss of data after re-authenticating.
- **2.2.6 Timeouts**
    - Users are warned of the duration of any user inactivity that could cause data loss, unless the data is preserved for more than 20 hours when the user does not take any actions.

</span>

---

### Guideline 2.3: Seizures and Physical Reactions

*Do not design content in a way that is known to cause seizures or physical reactions.*

**Overview**

**Guideline 2.3** aims to prevent **seizures and physical reactions** caused by flashing or strobing content. This is vital for users with **photosensitive epilepsy**, vestibular disorders, or other neurological conditions triggered by visual stimuli.

This guideline restricts flashing content to safe thresholds and promotes the use of alternatives when animation is involved.

---

**Success Criteria Summary**

| Success Criterion | Description |
|------------------|-------------|
| **2.3.1 Three Flashes or Below Threshold** | Avoid flashing more than 3 times per second unless within safe limits |
| **2.3.2 Three Flashes** | *[AAA]* No flashing above threshold at all |
| **2.3.3 Animation from Interactions** | *[AAA]* Allow disabling animations triggered by user interaction |

---

**Visual Examples**

**✓ 2.3.1 Controlled Flashing Animation**

```html
<!-- Simulates a safe blinking element with delay beyond 1/3 second -->
<div style="animation: flash 2s infinite;">Safe Flashing Box</div>

<style>
@keyframes flash {
  0%, 100% { background: white; }
  50% { background: red; }
}
</style>
```

**Why it works:** Animation flashes fewer than 3 times per second.

---

**✗ Failure - Unsafe Flashing Banner**

```html
<marquee behavior="alternate" scrollamount="50">Emergency Alert </marquee>
```

**Issue:** Rapid movement and flashing could trigger seizures in sensitive users.

---

**✓ 2.3.3 Interaction-triggered Animation with Toggle (AAA)**

```html
<button onclick="document.body.classList.toggle('animated')">Toggle Animation</button>
<div class="box">Expanding Box</div>

<style>
.animated .box {
  transition: transform 0.5s ease-in-out;
  transform: scale(1.2);
}
</style>
```

**Why it works:** Animation is user-triggered and can be turned off.

---

**Checklist**

| Requirement                                                    | Example Provided | Meets Standard |
|----------------------------------------------------------------|------------------|----------------|
| No flashing more than 3x per second above threshold            | ✓ Yes            | ✓             |
| Users can disable triggered animations (AAA)                   | ✓ Yes            | ✓             |
| Dangerous strobing or visual patterns are avoided              | ✓ Yes            | ✓             |

---

By adhering to Guideline 2.3, content creators protect users from **harmful visual triggers** and offer safer, more inclusive experiences for everyone - including those with sensitive neurological conditions.

**Success Criteria Links**

- [2.3.1 Three Flashes or Below Threshold (Level A)](#231-three-flashes-or-below-threshold-level-a)
- <span class="aaa">2.3.2 Three Flashes (Level AAA)</span>
- <span class="aaa">2.3.3 Animation from Interactions (Level AAA)</span>

---

#### **2.3.1 Three Flashes or Below Threshold (Level A)**

**Intent:**  
Prevent seizures and physical reactions in users with photosensitive epilepsy by limiting flashing content.

**Requirements**
- Content must not flash more than **three times in any one-second period**, unless the flashing is below the general flash and red flash thresholds.
- Applies to full screen or smaller flashing areas.

**Determining Compliance**
The flash must:
- Not exceed 3 flashes per second.
- Not have red flashes covering more than 25% of the screen at one time.

Use tools such as:
- PEAT (Photosensitive Epilepsy Analysis Tool)
- WCAG 2.1 Flash Threshold Definition from W3C

**Examples**

**1. Safe Flashing Using CSS Animation**
```css
@keyframes safePulse {
  0%, 100% { background: white; }
  50% { background: gray; }
}
```

**2. JavaScript Timer Flash Control (Compliant)**
```javascript
let flashes = 0;
let startTime = Date.now();
const interval = setInterval(() => {
  const now = Date.now();
  if (now - startTime < 1000) {
    if (flashes < 3) {
      triggerFlash();
      flashes++;
    }
  } else {
    flashes = 0;
    startTime = now;
  }
}, 200);
```

**Accessibility Benefits**
- Prevents triggering seizures or physical responses in users with photosensitive epilepsy.

**Best Practices**
- Avoid flashing content altogether unless necessary.
- Use transitions, fades, or other safer motion alternatives.
- Always test flashing content with compliance tools.

---

<span class="aaa">

... The following criteria (level AAA) are omitted from this guide ...

- **2.3.2 Three Flashes**
    - Web pages do not contain anything that flashes more than three times in any one second period.
- **2.3.3 Animation from Interactions**
    - Motion animation triggered by interaction can be disabled, unless the animation is essential to the functionality or the information being conveyed.

</span>

---

### Guideline 2.4: Navigable

*Provide ways to help users navigate, find content, and determine where they are.*

**Overview**

**Guideline 2.4** ensures that **users can navigate, find content, and determine where they are** within a website or application. This is especially important for users with **visual impairments, screen reader users, and keyboard-only users** who rely on predictable and well-structured interfaces.

The guideline covers headings, focus indicators, link purpose, keyboard navigation, and consistent navigation across pages.

---

**Success Criteria Summary**

| Success Criterion | Description |
|------------------|-------------|
| **2.4.1 Bypass Blocks** | Provide a way to skip repeated content (e.g., skip to main) |
| **2.4.2 Page Titled** | Each page must have a descriptive `<title>` |
| **2.4.3 Focus Order** | Interactive elements must receive focus in a logical sequence |
| **2.4.4 Link Purpose (In Context)** | Links must make sense from context |
| **2.4.5 Multiple Ways** | *[AA]* Provide more than one way to locate pages (e.g., search + sitemap) |
| **2.4.6 Headings and Labels** | *[AA]* Use descriptive headings and form labels |
| **2.4.7 Focus Visible** | *[AA]* Keyboard focus must be clearly visible |
| **2.4.8 Location** | *[AAA]* Indicate where users are within a set of pages |
| **2.4.9 Link Purpose (Link Only)** | *[AAA]* Each link is clearly identifiable on its own |
| **2.4.10 Section Headings** | *[AAA]* Use section headings to organize content meaningfully |

---

**Visual Examples**

**✓ 2.4.1 Skip to Main Content Link**

```html
<a href="#mainContent" class="skip-link">Skip to main content</a>
<main id="mainContent">
  <h1>Home Page</h1>
</main>
```

**Why it works:** Enables keyboard users to bypass navigation menus quickly.

---

**✓ 2.4.2 Page Title**

```html
<title>Product Details - WidgetPro</title>
```

**Why it works:** Descriptive title informs users and helps with browser/tab navigation.

---

**✓ 2.4.3 Logical Focus Order**

```html
<form>
  <input type="text" placeholder="First name">
  <input type="text" placeholder="Last name">
  <button type="submit">Submit</button>
</form>
```

**Why it works:** The focus moves in a top-to-bottom, left-to-right order matching visual layout.

---

**✓ 2.4.4 Link Purpose**

```html
<ul>
  <li><a href="/report.pdf">Download the Annual Report (PDF)</a></li>
  <li><a href="/contact">Contact Customer Support</a></li>
</ul>
```

**Why it works:** Users understand where each link leads even when isolated from the surrounding content.

---

**✓ 2.4.7 Visible Focus Style**

```css
button:focus {
  outline: 3px solid #005fcc;
}
```

**Why it works:** Users navigating with a keyboard can see which element is focused.

---

**Checklist**

| Requirement                            | Example Provided | Meets Standard |
|---------------------------------------|------------------|----------------|
| Skip link to bypass navigation        | ✓ Yes            | ✓             |
| Clear and unique page titles          | ✓ Yes            | ✓             |
| Logical tab/focus order               | ✓ Yes            | ✓             |
| Descriptive links, headings, and labels | ✓ Yes          | ✓             |
| Visible focus indicators              | ✓ Yes            | ✓             |

---

By implementing Guideline 2.4, designers and developers create interfaces that are **predictable, efficient, and accessible for keyboard and assistive technology users**, enhancing overall usability for everyone.

**Success Criteria Links**

- [2.4.1 Bypass Blocks (Level A)](#241-bypass-blocks-level-a)
- [2.4.2 Page Titled (Level A)](#242-page-titled-level-a)
- [2.4.3 Focus Order (Level A)](#243-focus-order-level-a)
- [2.4.4 Link Purpose (In Context) (Level A)](#244-link-purpose-in-context-level-a)
- [2.4.5 Multiple Ways (Level AA)](#245-multiple-ways-level-aa)
- [2.4.6 Headings and Labels (Level AA)](#246-headings-and-labels-level-aa)
- [2.4.7 Focus Visible (Level AA)](#247-focus-visible-level-aa)
- <span class="aaa">2.4.8 Location (Level AAA)</span>
- <span class="aaa">2.4.9 Link Purpose (Link Only) (Level AAA)</span>
- <span class="aaa">2.4.10 Section Headings (Level AAA)</span>

---

#### **2.4.1 Bypass Blocks (Level A)**

**Intent:**  
Allow users to bypass repeated blocks of content (like headers and navigation menus) to access the main content more efficiently using keyboard navigation or assistive technologies.

**Requirements**
- Provide a mechanism to skip past repeated content.
- Typically implemented as a "skip to main content" link that appears when focused.

**Examples**

**1. Skip Link Using Anchor**
```html
<a href="#main" class="skip-link">Skip to main content</a>
```

**2. Visible on Focus Only**
```css
.skip-link {
  position: absolute;
  left: -999px;
  top: auto;
  width: 1px;
  height: 1px;
  overflow: hidden;
}
.skip-link:focus {
  position: static;
  width: auto;
  height: auto;
}
```

**3. Defining the Target Region**
```html
<main id="main">
  <h1>Welcome to Our Site</h1>
  <p>Content starts here.</p>
</main>
```

**Accessibility Benefits**
- Helps keyboard-only users avoid repetitive navigation.
- Speeds up access to core page content.

**Best Practices**
- Ensure the skip link is the first focusable element on the page.
- Make it visible on keyboard focus.
- Use `main` or `role="main"` to clearly identify the main content block.

---

#### **2.4.2 Page Titled (Level A)**

**Intent:**  
Ensure users can quickly determine the topic or purpose of a web page by providing a clear, descriptive title.

**Requirements**
- Each web page must have a descriptive and specific title defined in the `<title>` element in the HTML `<head>`.

**Examples**

**1. Simple Descriptive Title**
```html
<head>
  <title>About Us - Acme Corporation</title>
</head>
```

**2. Dynamic Title on Single Page Apps (SPAs)**
```javascript
document.title = "Shopping Cart - Acme Store";
```

**Accessibility Benefits**
- Helps screen reader users understand where they are.
- Supports browser tab identification and bookmarking.

**Best Practices**
- Make the title specific to the page's purpose or function.
- Place key information first (e.g., "Settings - MyApp").
- Keep titles concise but meaningful.

---

#### **2.4.3 Focus Order (Level A)**

**Intent:**  
Ensure that the order of keyboard focus follows a meaningful sequence that preserves relationships and meaning.

**Requirements**
- Navigation order must match the visual and logical reading order.
- Use the default DOM order or explicitly manage focus using scripting if necessary.

**Examples**

**1. Semantic Navigation Menu**
```html
<nav>
  <a href="/">Home</a>
  <a href="/services">Services</a>
  <a href="/contact">Contact</a>
</nav>
```

**2. Logical Focus with Tabindex**
```html
<div tabindex="1">Step 1</div>
<div tabindex="2">Step 2</div>
<div tabindex="3">Step 3</div>
```

**3. Incorrect Focus Order (✗)**
```html
<div>
  <input type="text" id="second" />
  <input type="text" id="first" />
</div>
<!-- visually first is #first, but it receives focus second -->
```

**Accessibility Benefits**
- Maintains usability for keyboard users, including those using switch controls or screen readers.
- Preserves meaning and relationships between elements (e.g., labels and form inputs).

**Best Practices**
- Avoid disrupting natural DOM order with CSS alone.
- Test your interface using only the keyboard.
- Clearly associate labels, buttons, and dynamic content with their targets.

---

#### **2.4.4 Link Purpose (In Context) (Level A)**

**Intent:**  
Ensure that the purpose of each link can be determined from the link text alone or from the link text together with its context.

**Requirements**
- The link text (or its surrounding context) must clearly indicate the link's purpose.
- Avoid vague links like "click here" or "more" when taken out of context.

**Examples**

**1. Clear Standalone Link Text**
```html
<a href="services.html">Explore Our Services</a>
```

**2. Link With Context in Sentence**
```html
<p>To learn more, visit our <a href="history.html">Company History</a> page.</p>
```

**3. Link List With Context Provided by List Heading**
```html
<h2>Product Information</h2>
<ul>
  <li><a href="specs.html">Specifications</a></li>
  <li><a href="manual.html">User Manual</a></li>
</ul>
```

**4. Bad Example (✗) Vague Text**
```html
<ul>
  <li><a href="product.html">Details</a></li>
</ul>
```

**Accessibility Benefits**
- Helps screen reader users understand link destinations when scanning links out of context.
- Improves usability and clarity for all users.

**Best Practices**
- Use meaningful link text that is descriptive even when read independently.
- Avoid generic phrases unless context makes the link's purpose obvious.
- Use headings or labels to provide additional context when necessary.

---

#### **2.4.5 Multiple Ways (Level AA)**

**Intent:**  
Provide users with more than one method to locate content within a website to accommodate diverse navigation needs and preferences.

**Requirements**
- Pages within a set of web pages must be reachable in at least two ways (e.g., a menu, search box, site map, or list of related links).
- Does not apply to pages that are the result of, or a step in, a process (e.g., checkout page).

**Examples**

**1. Main Navigation and Site Map**
```html
<nav>
  <a href="/services">Services</a>
  <a href="/about">About Us</a>
  <a href="/contact">Contact</a>
</nav>
<a href="/site-map">Site Map</a>
```

**2. Search Functionality**
```html
<form action="/search">
  <label for="search">Search the site:</label>
  <input id="search" name="q" type="text">
  <button type="submit">Search</button>
</form>
```

**3. Breadcrumb Navigation**
```html
<nav aria-label="Breadcrumb">
  <ol>
    <li><a href="/">Home</a></li>
    <li><a href="/products">Products</a></li>
    <li>Details</li>
  </ol>
</nav>
```

**Accessibility Benefits**
- Supports users with cognitive and memory impairments.
- Provides flexibility for screen reader and keyboard-only users.

**Best Practices**
- Combine search, navigation menus, and breadcrumb trails.
- Ensure each method is accessible and usable independently.
- Clearly identify page location using headings and landmarks.

---

#### **2.4.6 Headings and Labels (Level AA)**

**Intent:**  
Help users understand the purpose of content and controls by providing clear, descriptive headings and labels.

**Requirements**
- Headings and labels must describe the topic or purpose of the section or control they reference.
- Avoid vague or generic labels that don't inform the user.

**Examples**

**1. Descriptive Headings**
```html
<h2>Frequently Asked Questions</h2>
<h3>Account Settings</h3>
```

**2. Clear Form Labels**
```html
<label for="email">Email Address</label>
<input id="email" name="email" type="email">
```

**3. Vague Example (✗)**
```html
<label for="text1">Click Here</label>
```

**Accessibility Benefits**
- Helps screen reader users quickly understand the page structure.
- Assists all users in navigating and interacting with content efficiently.

**Best Practices**
- Keep headings short but descriptive.
- Use proper heading hierarchy (`<h1>` to `<h6>`) for structure.
- Ensure form fields have associated `<label>` elements.
- Use consistent terminology across similar functions or sections.

---

#### **2.4.7 Focus Visible (Level AA)**

**Intent:**  
Ensure keyboard users can visually track where they are on the page by requiring a visible indicator for focus.

**Requirements**
- Any user interface component that can receive keyboard focus must have a visible focus indicator.
- Do not disable or remove default focus styling without providing a visible replacement.

**Examples**

**1. Default Focus Styles (Recommended)**
```css
button:focus {
  outline: 2px solid blue;
}
```

**2. Custom Focus Styles**
```css
.custom-button:focus {
  box-shadow: 0 0 0 3px #ffa500;
  outline: none;
}
```

**3. Poor Example (✗)**
```css
button:focus {
  outline: none;
}
```

**Accessibility Benefits**
- Helps users who rely on keyboard navigation (e.g., users with mobility impairments).
- Reduces confusion about which element has focus.

**Best Practices**
- Use high contrast and visible styles for focused elements.
- Test with only a keyboard (Tab, Shift+Tab, Enter, Space).
- Ensure all custom components provide focus feedback.

---

<span class="aaa">

... The following criteria (level AAA) are omitted from this guide ...

- **2.4.8 Location**
    - Information about the user's location within a set of Web pages is available.
- **2.4.9 Link Purpose (Link Only)**
    - A mechanism is available to allow the purpose of each link to be identified from link text alone, except where the purpose of the link would be ambiguous to users in general.
- **2.4.10 Section Headings**
    - Section headings are used to organize the content.

</span>

---

### Guideline 2.5: Input Modalities

*Make it easier for users to operate functionality through various inputs.*

**Overview**

**Guideline 2.5** ensures that **all input methods - including touch, mouse, voice, and assistive tech - can be used effectively and without accidental activation**. This promotes accessibility for users with mobility impairments, those who rely on alternative input methods, and users with tremors or limited dexterity.

It focuses on making interface components operable across diverse devices and adaptable to user preferences or needs.

---

**Success Criteria Summary**

| Success Criterion | Description |
|------------------|-------------|
| **2.5.1 Pointer Gestures** | Use simple gestures; provide alternatives for complex multi-point gestures |
| **2.5.2 Pointer Cancellation** | Ensure actions triggered by pointer down can be aborted or confirmed |
| **2.5.3 Label in Name** | Visible text labels must match programmatic name (for speech input users) |
| **2.5.4 Motion Actuation** | Provide alternative to motion-triggered actions (e.g., device shake) |
| **2.5.5 Target Size** | *[AAA]* Touch targets must be at least 44x44 CSS pixels |
| **2.5.6 Concurrent Input Mechanisms** | *[AAA]* Do not limit input to just one method (e.g., touch-only) |

---

**Visual Examples**

**✓ 2.5.1 Simple Tap Instead of Gesture**

```html
<button onclick="zoomIn()">Zoom In</button>
```

**Why it works:** Users don't need to perform pinch-zoom or multi-finger gestures.

---

**✓ 2.5.2 Confirm Click on Pointer Down**

```html
<button onmousedown="previewAction()" onclick="confirmAction()">Submit</button>
```

**Why it works:** Activation is finalized on `click`, not `mousedown`, allowing cancellation.

---

**✓ 2.5.3 Label in Name**

```html
<label for="search">Search</label>
<input id="search" type="text" aria-label="Search">
```

**Why it works:** Programmatic name ("Search") matches the visible label for voice control compatibility.

---

**✓ 2.5.4 Motion Alternatives**

```html
<input type="checkbox" id="shakeToSend" checked>
<label for="shakeToSend">Enable shake-to-send</label>
```

**Why it works:** Feature is optional, and users can activate the same function via button or toggle.

---

**✓ 2.5.5 Touch Target Size (AAA)**

```css
button {
  min-width: 44px;
  min-height: 44px;
}
```

**Why it works:** Provides a comfortably large target for users with motor impairments.

---

**Checklist**

| Requirement                                             | Example Provided | Meets Standard |
|--------------------------------------------------------|------------------|----------------|
| Single-point alternatives to gestures                  | ✓ Yes            | ✓             |
| Pointer down actions can be canceled                   | ✓ Yes            | ✓             |
| Speech input compatible labels                         | ✓ Yes            | ✓             |
| Motion-activated controls have alternatives            | ✓ Yes            | ✓             |
| Touch targets are large enough (44x44) (AAA)           | ✓ Yes            | ✓             |

---

Guideline 2.5 ensures that **all users - regardless of device or physical ability - can interact confidently and accurately** with web content, using whatever input methods best suit their needs.

**Success Criteria Links**

- [2.5.1 Pointer Gestures (Level A)](#251-pointer-gestures-level-a)
- [2.5.2 Pointer Cancellation (Level A)](#252-pointer-cancellation-level-a)
- [2.5.3 Label in Name (Level A)](#253-label-in-name-level-a)
- [2.5.4 Motion Actuation (Level A)](#254-motion-actuation-level-a)
- <span class="aaa">2.5.5 Target Size (Level AAA)</span>
- <span class="aaa">2.5.6 Concurrent Input Mechanisms (Level AAA)</span>

---

#### **2.5.1 Pointer Gestures (Level A)**

**Intent:**  
Ensure that functionality relying on complex pointer gestures (such as swiping, pinching, or dragging) is also accessible via simpler, single-pointer actions.

**Requirements**
- All functionality that uses multipoint or path-based gestures must be usable with a single-pointer alternative.
- This includes drag-and-drop, swipe to delete, pinch to zoom, etc.

**Examples**

**1. Drag-and-Drop with Button Alternative**
```html
<button onclick="moveItemUp()">Move Up</button>
<button onclick="moveItemDown()">Move Down</button>
```

**2. Pinch to Zoom with Controls**
```html
<button onclick="zoomIn()">+</button>
<button onclick="zoomOut()">-</button>
```

**3. Swipe Gesture with Alternative**
```html
<button onclick="deleteItem()">Delete</button>
```

**Accessibility Benefits**
- Enables users who cannot perform complex gestures (e.g., users with motor impairments or assistive tech users) to operate interfaces.

**Best Practices**
- Avoid requiring gestures that are difficult to perform.
- Offer visible, device-independent alternatives.
- Test using a keyboard and single-pointer input to ensure full functionality.

> Note: This requirement applies to web content that interprets pointer actions (i.e. this does not apply to actions that are required to operate the user agent or assistive technology).

---

#### **2.5.2 Pointer Cancellation (Level A)**

**Intent:**  
Prevent accidental activation of functionality due to unintended pointer input by allowing users to cancel or undo pointer actions.

**Requirements**
At least one of the following must be true for functionality triggered by pointer input:
- The action is not triggered on the down-event of the pointer.
    - Unless a function on the down event is essential.
- The user is provided with a mechanism to abort or undo the action.
- The up-event action is reversible.

**Examples**

**1. Click Triggered on Mouse Up**
```javascript
element.addEventListener('mouseup', () => doSomething());
```

**2. Swipe to Delete with Undo Option**
```html
<button onclick="deleteItem()">Delete</button>
<div role="alert">Item deleted. <button onclick="undoDelete()">Undo</button></div>
```

**3. Drag Cancel Mechanism**
```javascript
document.addEventListener('dragend', (e) => {
    if (!e.dropEffect || e.dropEffect === 'none') cancelDrag();
});
```

**Accessibility Benefits**
- Reduces the likelihood of unintended actions for users with tremors or difficulty with fine motor control.
- Provides an opportunity to recover from mistakes.

**Best Practices**
- Avoid using `mousedown` or `touchstart` to immediately trigger irreversible actions.
- Provide feedback and cancellation for all gestures and actions.
- Test with users who navigate using different types of input devices.

**Notes**

> Functions that emulate a keyboard or numeric keypad key press are considered essential.

> This requirement applies to web content that interprets pointer actions (i.e. this does not apply to actions that are required to operate the user agent or assistive technology).

---

#### **2.5.3 Label in Name (Level A)**

**Intent:**  
Ensure that visual labels for UI components are included in their accessible names, so users who rely on speech input can activate them more easily.

**Requirements**
- For user interface components with a visible text label, the accessible name must include the visible label text.
- This ensures compatibility with voice recognition software.

**Examples**

**1. Button With Matching Label and Accessible Name**
```html
<button aria-label="Search">Search</button>
```

**2. Icon Button With Visible Label and aria-label**
```html
<button aria-label="Save">
  <img src="save-icon.svg" alt="">
  Save
</button>
```

**3. Mismatch Example (✗)**
```html
<!-- visible label is "Help" but aria-label is "Support" -->
<button aria-label="Support">Help</button>
```

**Accessibility Benefits**
- Enables accurate operation of controls using speech input.
- Reduces confusion between visual and programmatic labels.

**Best Practices**
- Include the visible text in the `aria-label` when overriding default labeling.
- Avoid mismatches between the UI's visible text and accessible name.
- Use native HTML elements and labeling mechanisms when possible.

> Note: A best practice is to have the text of the label at the start of the name.

---

#### **2.5.4 Motion Actuation (Level A)**

**Intent:**  
Ensure that users can operate device motion-activated functionality (such as shaking or tilting) through standard controls, and that such functionality can be disabled.

**Requirements**
- Functionality triggered by device motion or user movement must also be operable through standard user interface components.
- The motion-based functionality must be able to be disabled to prevent accidental activation.

**Exceptions**
- The motion is essential for the function.
- The motion is used for accessibility (e.g., switch control head gestures).

**Examples**

**1. Shake to Undo with Button Alternative**
```html
<button onclick="undoAction()">Undo</button>
```

**2. Disable Motion Trigger via Settings**
```html
<label for="motion-toggle">Enable motion control</label>
<input type="checkbox" id="motion-toggle" checked>
```

**3. JavaScript to Detect Device Motion and Provide Fallback**
```javascript
if (window.DeviceMotionEvent) {
    window.addEventListener('devicemotion', function(event) {
        if (isShake(event)) {
            if (document.getElementById('motion-toggle').checked) {
                undoAction();
            }
        }
    });
}
```

**Accessibility Benefits**
- Users with motor impairments or those in environments where motion-based input is impractical can still operate the interface.
- Prevents unintended actions caused by unintentional device movement.

**Best Practices**
- Always provide a UI-based alternative for motion-triggered actions.
- Allow users to enable or disable motion interactions in settings.
- Use motion-based interactions only as enhancements, not as primary controls.

---

<span class="aaa">

... The following criteria (level AAA) are omitted from this guide ...

- **2.5.5 Target Size**
    - The size of the target for pointer inputs is at least 44 by 44 CSS pixels except when...
- **2.5.6 Concurrent Input Mechanisms**
    - Web content does not restrict use of input modalities available on a platform except where the restriction is essential, required to ensure the security of the content, or required to respect user settings.

</span>

---
