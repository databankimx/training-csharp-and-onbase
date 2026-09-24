## Principle 1: Perceivable

*Information and user interface components must be presentable to users in ways they can perceive.*

**Overview**

**Principle 1: Perceivable** ensures that **information and user interface components must be presentable to users in ways they can perceive**. This is the foundation of web accessibility: if users cannot perceive content - visually, audibly, or tactually - they cannot interact with it.

Accessibility under this principle applies to users who are **blind, have low vision, are deaf or hard of hearing**, or have cognitive conditions that affect processing of sensory input. To support these users, content must be adaptable across sensory modalities, ensuring that no essential information is conveyed solely through sight or sound.

---

**Key Concepts**

Under Principle 1, content must be made available through **text alternatives, captions, audio descriptions, proper structure, and color contrast**. Images must have meaningful alternative text (`alt`), audio-only content should have transcripts, and synchronized media like videos must include captions and possibly audio descriptions.

Content should also be **structured using semantic HTML**, which allows assistive technologies to interpret layout and meaning correctly. Headings, lists, and labels provide critical navigational and contextual cues for screen reader users. For visual clarity, sufficient **contrast between foreground and background** is vital, as is support for text resizing and content reflow for responsive layouts.

---

**Covered Guidelines**

Principle 1 includes the following WCAG guidelines:
- **1.1 Text Alternatives** - Provide text for non-text content
- **1.2 Time-based Media** - Provide alternatives for audio/video
- **1.3 Adaptable** - Create content that can be presented in different ways
- **1.4 Distinguishable** - Make it easier to see and hear content

These guidelines aim to ensure that **every user can perceive and understand the content, regardless of ability or technology used**.

---

Principle 1 is about **inclusivity at the first level** - perception. Without perceivable content, accessibility breaks down before interaction even begins.

---

**Guideline Links**

- [Guideline 1.1: Text Alternatives](#guideline-11-text-alternatives)
- [Guideline 1.2: Time-based Media](#guideline-12-time-based-media)
- [Guideline 1.3: Adaptable](#guideline-13-adaptable)
- [Guideline 1.4: Distinguishable](#guideline-14-distinguishable)

---

### Guideline 1.1: Text Alternatives
**Provide text alternatives for any non-text content.**

*Provide text alternatives for any non-text content so that it can be changed into other forms people need, such as large print, braille, speech, symbols or simpler language.*

**Overview**

**Guideline 1.1** of the Web Content Accessibility Guidelines (WCAG) 2.1 requires that all **non-text content** has a **text alternative**. This allows users who cannot perceive visual or auditory information to still access the content through assistive technologies such as screen readers, braille displays, or speech input.

This guideline supports users who are blind, have low vision, are deaf-blind, or those who use text-based output devices. It ensures that all meaningful media - including images, controls, and complex graphics - are understandable regardless of sensory ability.

It contains one success criterion:

- **1.1.1 Non-text Content (Level A)** - All non-text content that is presented to the user must have a text alternative that serves the equivalent purpose.

---

**Visual Examples**

**✓ Correct: Informative Image with `alt` Description**

```html
<img src="weather-sunny.png" alt="Sunny weather with a high of 75°F">
```

**Why it works:** The image conveys weather information, and the `alt` text communicates this meaning in words.

---

**✗ Incorrect: Missing empty `alt` for Informative Image**

```html
<img src="weather-sunny.png">
```

**Problem:** Screen reader users have no idea what this image is about.

---

**✓ Correct: Decorative Image with Empty `alt`**

```html
<img src="border-shadow.png" alt="">
```

**Why it works:** The image has no meaningful content; `alt=""` ensures screen readers skip it, avoiding noise.

---

**✗ Incorrect: Decorative Image with Misleading `alt`**

```html
<img src="border-shadow.png" alt="Border shadow image">
```

**Problem:** Screen reader users will hear a meaningless description, wasting time and causing confusion.

---

**✓ Correct: Icon Button with Functional Label**

```html
<button aria-label="Download PDF">
  <img src="pdf-icon.png" alt="">
</button>
```

**Why it works:** The image is purely functional, and the button provides an accessible name via `aria-label`.

---

**✗ Incorrect: Icon Button with No Accessible Text**

```html
<button>
  <img src="pdf-icon.png">
</button>
```

**Problem:** No label or alt text means assistive technology cannot identify the button's purpose.

---

**Summary Checklist**

| Element Type         | Required Text Alternative | Correct Implementation Example           |
|----------------------|---------------------------|------------------------------------------|
| Informative image    | Yes                       | `alt="Bar chart showing sales growth"`   |
| Decorative image     | Empty alt (`alt=""`)      | `alt=""`                                  |
| Image as a button    | Yes (use `aria-label`)    | `aria-label="Close"`                     |
| Functional icon      | Yes                       | `alt="Search"` or `aria-label="Search"`  |

---

Providing clear, meaningful, and appropriate text alternatives is foundational to digital accessibility and should be applied consistently across all interfaces and devices.

**Success Criteria Links**

- [1.1.1 Non-text Content (Level A)](#111-non-text-content-level-a)

---

#### **1.1.1 Non-text Content (Level A):**

**Intent:**  
Ensure that all non-text content presented to users has a text alternative that serves the equivalent purpose.

### Requirements
- Provide text alternatives for images, icons, charts, audio, video, and any interactive or sensory experience.
- Use appropriate semantic elements or ARIA attributes to communicate the alternative text.

**1. Controls and Input (Functional Content)**
- Provide a name using `aria-label`, `aria-labelledby`, or associated `<label>` elements.
```html
<button aria-label="Search"></button>
```

**2. Time-based Media**
- Provide alternatives like transcripts or audio descriptions.
```html
<a href="transcript.html">Transcript of video</a>
```

**3. Tests and Exercises**
- If the content is part of a test where giving a text alternative would invalidate the test, a descriptive label may still be used.
```html
<img src="note.png" alt="Musical score image used in a hearing test">
```

**4. Sensory Experience**
- If the non-text content is primarily for creating a sensory experience (e.g., artwork or symphony), provide descriptive alternatives.
```html
<img src="painting.jpg" alt="Impressionist painting of a sunrise by Monet">
```

**5. CAPTCHA (Completely Automated Public Turing test to tell Computers and Humans Apart)**
- Provide alternative forms (e.g., audio CAPTCHA).
```html
<img src="captcha.png" alt="Enter the characters displayed in the image">
<audio controls src="captcha-audio.mp3">Audio CAPTCHA</audio>
```

**6. Decorative Content**
- Mark decorative images with `alt=""` and ensure they are ignored by screen readers.
```html
<img src="spacer.gif" alt="" role="presentation">
```

**7. Logo or Branding**
- Alt text should convey the name of the company or product.
```html
<img src="company-logo.svg" alt="OpenAI logo">
```
```html
<!-- Image with descriptive alt text -->
<img src="chart.png" alt="Bar chart comparing monthly sales for 2024.">
```

**8. Image as a Link**
```html
<a href="/profile">
  <img src="user.png" alt="View user profile">
</a>
```
*Screen readers will read the alt text so the user understands the purpose of the link.*

### jQuery Example
**9. Dynamically Inserted Image with Alt Text**
```javascript
$("#container").append('<img src="icon.png" alt="Settings icon">');
```
*Ensure that dynamically created images still include meaningful alt text.*

### Vue.js Example
**10. Image Binding in Vue Template**
```html
<template>
  <img :src="user.avatar" :alt="user.name + ' avatar'">
</template>
```
*This binds both the image source and descriptive alt text dynamically.*

### .NET MVC Example
**11. Razor View Image with Alt Attribute**
```html
@Html.Raw("<img src='/images/logo.png' alt='Company logo'>")
```
*This ensures that server-rendered images include descriptive alternative text.*

### ARIA Examples for Non-Image Content

**12. Icon Font with Label**
```html
<i class="fa fa-search" aria-hidden="true"></i>
<span class="sr-only">Search</span>
```
*The visual icon is hidden from screen readers and the text label is exposed.*

**13. Chart Description**
```html
<canvas id="salesChart" aria-describedby="chartDesc"></canvas>
<div id="chartDesc" hidden>
  Line chart showing sales growth from January to June, peaking in April.
</div>
```
*Canvas element is described using a hidden text description.*

**14. Detailed Chart Description**
```html
<figure>
    <img src="./images/graph.png" alt="Graph of students' favorite colors" >
    <figcaption style="display:none">
        <strong>Graph of students' favorite colors</strong><br>
        X-Axis: Name of Color<br>
        Y-Axis: Number of Students<br>
        Data:
        <table>
            <thead>
                <tr>
                    <th>Color</th>
                    <th>Number of Students</th>
                </tr>
            </thead>
            <tbody>
                <tr>
                    <td>Red</td>
                    <td>43</td>
                </tr>
                <tr>
                    <td>Green</td>
                    <td>18</td>
                </tr>
                <tr>
                    <td>Blue</td>
                    <td>54</td>
                </tr>
                <tr>
                    <td>Yellow</td>
                    <td>48</td>
                </tr>
                <tr>
                    <td>Orange</td>
                    <td>35</td>
                </tr>
            </tbody>
        </table>
    </figcaption>
</figure>
```
*Provides a detailed description of all content in the figure.*

### Best Practices
- Use meaningful, concise alt text for informative content.
- Use empty alt text for purely decorative content.
- Supplement complex content like graphs or charts with textual summaries.
- Test content with a screen reader to verify proper output.

### Accessibility Benefits
- Makes content accessible to people with visual, cognitive, and learning disabilities.
- Helps search engines understand content structure and relevance.

---

### Guideline 1.2: Time-based Media

*Provide alternatives for time-based media.*

**Overview**

**Guideline 1.2** addresses **time-based media**, which includes audio-only, video-only, synchronized media (like video with audio), and interactive media such as live streaming. Its goal is to ensure that **all users - including those who are deaf, hard of hearing, blind, or have cognitive disabilities - can access multimedia content**.

The guideline requires various types of alternatives depending on the nature of the media:
- **Text transcripts** for audio-only content
- **Audio descriptions** for video-only or video with audio
- **Captions** for video with sound
- **Sign language interpretation** for full accessibility

---

**Success Criteria Summary**

| Success Criterion | Description |
|------------------|-------------|
| **1.2.1 Audio-only and Video-only (Prerecorded)** | Provide transcript or description |
| **1.2.2 Captions (Prerecorded)** | Provide synchronized captions |
| **1.2.3 Audio Description or Media Alternative (Prerecorded)** | Provide either an audio description or full media alternative |
| **1.2.4 Captions (Live)** | Provide captions for live audio content |
| **1.2.5 Audio Description (Prerecorded)** | Provide audio description for video |
| **1.2.6 Sign Language (Prerecorded)** | Provide sign language interpretation |
| **1.2.7 Extended Audio Description** | Provide pausable content with extended audio description |
| **1.2.8 Media Alternative (Prerecorded)** | Provide full text alternative including descriptions |
| **1.2.9 Audio-only (Live)** | Provide real-time transcription for live-only audio |

---

**Visual Examples**

**✓ 1.2.1 Audio-only with Transcript**

```html
<audio controls src="podcast.mp3"></audio>
<p><strong>Transcript:</strong> Welcome to our podcast on accessibility best practices...</p>
```

---

**✓ 1.2.2 Video with Captions**

```html
<video controls>
  <source src="intro.mp4" type="video/mp4">
  <track src="captions.vtt" kind="subtitles" srclang="en" label="English">
</video>
```

---

**✓ 1.2.3 Audio Description Option**

```html
<video controls>
  <source src="product-demo.mp4" type="video/mp4">
  <track src="descriptions.vtt" kind="descriptions" srclang="en" label="Audio Description">
</video>
```

---

**✓ 1.2.6 Sign Language Overlay (AAA)**

```html
<video controls>
  <source src="presentation.mp4" type="video/mp4">
</video>
<video controls class="sign-language-overlay">
  <source src="presentation-asl.mp4" type="video/mp4">
</video>
```

---

**✓ 1.2.9 Live Audio with Real-time Text (AAA)**

For example, a live radio stream supported by a third-party **CART** (Communication Access Realtime Translation) service embedding real-time text output on the same page.

---

**Checklist**

| Requirement                         | Example Provided | Meets Standard |
|------------------------------------|------------------|----------------|
| Captions for prerecorded video     | ✓ Yes            | ✓             |
| Transcript for audio-only          | ✓ Yes            | ✓             |
| Audio descriptions for video       | ✓ Yes            | ✓             |
| Sign language or media alternative (AAA) | ✓ Yes            | ✓             |
| Captions for live content          | ✓ Yes (example)  | ✓             |

---

By providing appropriate alternatives for **audio and video media**, Guideline 1.2 ensures that multimedia content is accessible to a wide range of users, no matter how they interact with technology.


**Success Criteria Links**

- [1.2.1 Audio-only and Video-only (Prerecorded) (Level A)](#121-audio-only-and-video-only-prerecorded-level-a)
- [1.2.2 Captions (Prerecorded) (Level A)](#122-captions-prerecorded-level-a)
- [1.2.3 Audio Description or Media Alternative (Prerecorded) (Level A)](#123-audio-description-or-media-alternative-prerecorded-level-a)
- [1.2.4 Captions (Live) (Level AA)](#124-captions-live-level-aa)
- [1.2.5 Audio Description (Prerecorded) (Level AA)](#125-audio-description-prerecorded-level-aa)
- <span class="aaa">1.2.6 Sign Language (Prerecorded) (Level AAA)</span>
- <span class="aaa">1.2.7 Extended Audio Description (Prerecorded) (Level AAA)</span>
- <span class="aaa">1.2.8 Media Alternative (Prerecorded) (Level AAA)</span>
- <span class="aaa">1.2.9 Audio-only (Live) (Level AAA)</span>

---

#### **1.2.1 Audio-only and Video-only (Prerecorded) (Level A)**

**Intent:**  
The purpose of this success criterion is to ensure that people who cannot see or hear are provided with alternatives to audio-only or video-only content, so they can understand the purpose and meaning of the media.

**Requirements**

Provide:

- A transcript (for audio-only content)
- A full description of the video (for video-only content)

**Examples**

**1. Audio-only Content (e.g., podcast)**  
A transcript of the podcast is provided on the same page.

```html
<audio controls>
  <source src="podcast.mp3" type="audio/mpeg">
</audio>
<p><a href="podcast-transcript.html">Read the transcript</a></p>
```

**2. Video-only Content (no audio)**  
A descriptive text of the visual information is included.

```html
<video autoplay loop muted>
  <source src="silent-tour.mp4" type="video/mp4">
</video>
<p>This video shows a visual tour of the building's interior including the lobby, offices, and lounge areas.</p>
```

**Best Practices**

- Link transcripts clearly near the media
- Ensure that transcripts are synchronized and complete
- If media includes important sounds (like alarms), describe them

**Accessibility Benefits**

- Blind users can read transcripts of audio-only content using a screen reader
- Deaf users can understand video content through descriptions

---

#### **1.2.2 Captions (Prerecorded) (Level A)**

**Intent:**  
To ensure that people who are deaf or hard of hearing can access the audio content of prerecorded synchronized media (video with audio), this success criterion requires captions.

**Requirements**

- Captions must include dialogue and important non-speech information (e.g., sound effects, speaker ID).
- The captions should be synchronized with the audio.

**Examples**

**1. Captioned Video Using HTML5**
```html
<video controls>
  <source src="presentation.mp4" type="video/mp4">
  <track src="captions.vtt" kind="captions" srclang="en" label="English">
</video>
```

**2. Caption Format Example (WebVTT file)**  
File: `captions.vtt`
```
WEBVTT

00:00:00.000 --> 00:00:04.000
[Music playing]

00:00:04.000 --> 00:00:08.000
John: Welcome to our accessibility training session.
```

**Best Practices**

- Ensure all spoken content is included accurately.
- Include meaningful sounds (e.g., "[applause]", "[door slams]").
- Place captions in a readable position and use good contrast.
- Use closed captions rather than open captions when possible (closed can be toggled).

**Accessibility Benefits**

- People who are deaf or hard of hearing can access multimedia content.
- Also helpful in noisy environments or for users with auditory processing issues.

This criterion applies only to **prerecorded synchronized media**. Live media captions are covered in SC 1.2.4.

---

#### **1.2.3 Audio Description or Media Alternative (Prerecorded) (Level A)**

**Intent:**  
This success criterion ensures that people who are blind or visually impaired can understand important visual content in prerecorded synchronized media by providing either:

- An audio description of the video content, or
- A full media alternative (like a descriptive transcript)

**Requirements**

- Provide an **audio description** of the essential visual content, or
- Offer a **text-based media alternative** that includes both visual and audio descriptions

**Examples**

**1. Audio Description Track in HTML5 Video**
```html
<video controls>
  <source src="interview.mp4" type="video/mp4">
  <track src="descriptions.vtt" kind="descriptions" srclang="en" label="Audio Description">
</video>
```

**2. Descriptive Transcript**
```html
<a href="descriptive-transcript.html">Descriptive Transcript of the video</a>
```

**3. Embedded Narration in Video**
A video of a slide presentation includes a voiceover that describes not only the speaker's words but also any key visuals shown on screen.

**Best Practices**

- Audio description should not overlap important dialogue.
- Media alternatives should include all meaningful visual and auditory elements.
- Clearly label links to transcripts or description files.

**Accessibility Benefits**

- Users who cannot see visual information can still understand what is happening through descriptive audio.
- Text alternatives allow screen reader users to consume rich media content.

This criterion applies specifically to **prerecorded synchronized media**.

---

#### **1.2.4 Captions (Live) (Level AA)**

**Intent:**  
Ensure that people who are deaf or hard of hearing have access to the audio portion of live synchronized media through captions.

**Requirements**

- Captions must be provided for **live** audio content in **synchronized media**.
- Captions must include speech and important non-speech sounds (like laughter or alarms).

**Examples**

**1. Live Captioning for Broadcasts**  
A news broadcast includes captions generated in real time by a stenographer using speech-to-text technology.

**2. Captioned Webinar with Live Audio**  
A webinar tool includes a live captioning feature that displays subtitles as participants speak.

**Technologies That Can Help**

- CART (Communication Access Realtime Translation)
- Automated speech recognition systems with human oversight

**Code Example**

*Live Captioning Integration Example Using JavaScript (hypothetical):*
```html
<div id="captions" aria-live="polite" role="log"></div>
<script>
  // Simulated real-time captioning data stream
  const captions = [
    "Welcome everyone.",
    "Today we'll cover accessible design.",
    "Let's begin with an overview..."
  ];
  let index = 0;
  setInterval(() => {
    if (index < captions.length) {
      document.getElementById("captions").innerText += `\n` + captions[index++];
    }
  }, 3000);
</script>
```
This is a simple simulated example. Production-grade implementations typically integrate with CART providers or captioning services.

**Accessibility Benefits**

- Deaf and hard-of-hearing users can participate in live events
- Helpful in noisy environments or for users with limited language comprehension

**Best Practices**

- Use professional captioners when possible
- Place captions clearly and ensure good contrast
- Indicate speaker changes

This criterion ensures accessibility for live media and complements 1.2.2, which addresses prerecorded media.

---

#### **1.2.5 Audio Description (Prerecorded) (Level AA)**

**Intent:**  
Provide visually impaired users with access to important visual information in synchronized media through audio description.

**Requirements**
- Include audio descriptions of important visual details in the video's natural pauses, or
- Provide an alternative version of the video that contains audio descriptions.

**Examples**

**1. Secondary Audio Track in HTML5 Video**
```html
<video controls>
  <source src="demo.mp4" type="video/mp4">
  <track src="audio-desc.vtt" kind="descriptions" srclang="en" label="English Descriptions">
</video>
```

**2. Voiceover Included in the Video Timeline**  
A documentary includes narration describing visual scenes, inserted during moments when there's no dialogue.

**Code Example**

*ARIA Live Region with Visual Description Prompt*
```html
<div role="complementary" aria-live="polite" id="descBox">
  The screen shows a group of hikers crossing a misty forest trail.
</div>
```

**Accessibility Benefits**
- Visually impaired users receive critical visual context (e.g., actions, settings, expressions).
- Assists users who cannot view the screen (e.g., multitasking or in hands-free situations).

**Best Practices**
- Ensure descriptions are meaningful and concise.
- Avoid overlapping with spoken dialogue.
- Use professional narration if possible.

This criterion supports equal access to multimedia for users who are blind or have low vision.

---

<span class="aaa">

... The following criteria (level AAA) are omitted from this guide ...

- **1.2.6 Sign Language (Prerecorded)**
    - Sign language interpretation is provided for all prerecorded audio content in synchronized media.
- **1.2.7 Extended Audio Description (Prerecorded)**
    - Where pauses in foreground audio are insufficient to allow audio descriptions to convey the sense of the video, extended audio description is provided for all prerecorded video content in synchronized media.
- **1.2.8 Media Alternative (Prerecorded)**
    - An alternative for time-based media is provided for all prerecorded synchronized media and for all prerecorded video-only media.
- **1.2.9 Audio-only (Live)**
    - An alternative for time-based media that presents equivalent information for live audio-only content is provided.

</span>

---

### Guideline 1.3: Adaptable

*Create content that can be presented in different ways without losing meaning.*

**Overview**

**Guideline 1.3** ensures that web content is **adaptable to different presentation styles and devices** without losing meaning or functionality. This allows users to personalize their experience through assistive technologies like screen readers, magnifiers, braille displays, or custom stylesheets.

The key idea is to **separate content from presentation** by using semantic HTML and proper structure. It helps ensure that content maintains its logical order and relationships when presented in alternative formats.

---

**Success Criteria Summary**

| Success Criterion | Description |
|------------------|-------------|
| **1.3.1 Info and Relationships** | Use semantic elements to convey structure |
| **1.3.2 Meaningful Sequence** | Ensure content order is preserved logically |
| **1.3.3 Sensory Characteristics** | Avoid relying on shape, size, color, or position alone |
| **1.3.4 Orientation** | Content must not be restricted to a specific screen orientation |
| **1.3.5 Identify Input Purpose** | Input fields must expose their meaning to assistive technologies |
| **1.3.6 Identify Purpose** | Use markup to support personalization (AAA) |

---

**Visual Examples**

**✓ 1.3.1 Semantic Structure with Headings**

```html
<h1>Welcome to Our Store</h1>
<h2>Featured Products</h2>
<ul>
  <li>Wireless Headphones</li>
  <li>Smart Thermostat</li>
</ul>
```

**Why it works:** Uses heading tags and lists to convey hierarchy and relationships.

---

**✓ 1.3.2 Logical Reading Order**

```html
<div>
  <p>Step 1: Sign up.</p>
  <p>Step 2: Verify your email.</p>
  <p>Step 3: Get started!</p>
</div>
```

**Why it works:** Order in the DOM reflects the visual and logical order of steps.

---

**✗ Failure - Relying on Visual Placement Only**

```html
<p>Click the red button at the bottom right to continue.</p>
```

**Issue:** Users who cannot see the screen or have it customized may not understand where to go.

---

**✓ 1.3.3 Sensory-neutral Instruction**

```html
<p>Click the "Continue" button to move forward.</p>
```

**Why it works:** Avoids reliance on visual cues like color or position.

---

**✓ 1.3.5 Input Purpose (Autocomplete)**

```html
<label for="email">Email</label>
<input id="email" name="email" type="email" autocomplete="email">
```

**Why it works:** The `autocomplete` attribute helps browsers and assistive tech identify the input's purpose.

---

**Checklist**

| Requirement                                  | Example Provided | Meets Standard |
|---------------------------------------------|------------------|----------------|
| Semantic structure for headings, lists, etc. | ✓ Yes            | ✓             |
| Content maintains logical sequence           | ✓ Yes            | ✓             |
| Instructions don't rely on visual cues only  | ✓ Yes            | ✓             |
| Inputs identify user-specific purposes       | ✓ Yes            | ✓             |

---

Guideline 1.3 lays the foundation for **accessible structure and personalization**, ensuring that content adapts gracefully to any user's technology or preference.

**Success Criteria Links**

- [1.3.1 Info and Relationships (Level A)](#131-info-and-relationships-level-a)
- [1.3.2 Meaningful Sequence (Level A)](#132-meaningful-sequence-level-a)
- [1.3.3 Sensory Characteristics (Level A)](#133-sensory-characteristics-level-a)
- [1.3.4 Orientation (Level AA)](#134-orientation-level-aa)
- [1.3.5 Identify Input Purpose (Level AA)](#135-identify-input-purpose-level-aa)
- <span class="aaa">1.3.6 Identify Purpose (Level AAA)</span>

---

#### **1.3.1 Info and Relationships (Level A)**

**Intent:**  
Ensure that the structure and relationships within content are programmatically determinable or available in text. This helps users who rely on assistive technologies like screen readers understand the content's organization.

**Requirements**
- Use semantic HTML to convey structure (e.g., headings, lists, tables).
- Provide labels, roles, and relationships that can be interpreted by assistive tech.

**Examples**

**1. Table with Semantic Markup**
```html
<table>
  <caption>Quarterly Revenue</caption>
  <thead>
    <tr><th>Quarter</th><th>Revenue</th></tr>
  </thead>
  <tbody>
    <tr><td>Q1</td><td>$1M</td></tr>
    <tr><td>Q2</td><td>$1.5M</td></tr>
  </tbody>
</table>
```

**2. Form with Associated Labels**
```html
<form>
  <label for="email">Email:</label>
  <input type="email" id="email" name="email">
</form>
```

**3. ARIA Roles to Express Hierarchy**
```html
<ul role="tree">
  <li role="treeitem" aria-expanded="true">Documents
    <ul role="group">
      <li role="treeitem">2023 Report</li>
    </ul>
  </li>
</ul>
```

**4. Semantic Tags Instead of `<div>`s**
```html
<header>
  <h1>Company Newsletter</h1>
</header>
<main>
  <article>
    <h2>Team Updates</h2>
    <p>Our team reached a major milestone...</p>
  </article>
</main>
<footer>
  <p>Contact us at support@example.com</p>
</footer>
```

**Accessibility Benefits**
- Screen reader users can perceive the structure and purpose of content.
- Users navigating by keyboard can better understand form layouts and data groupings.

**Best Practices**
- Use native HTML elements whenever possible.
- Avoid using visual layout alone to imply relationships.
- Test with screen readers to confirm accurate interpretation.

---

#### **1.3.2 Meaningful Sequence (Level A)**

**Intent:**  
Ensure that the reading and navigation order of content preserves its meaning when presented in a linear fashion by assistive technologies.

**Requirements**
- Use markup that reflects the intended sequence of information.
- Ensure that the visual order matches the logical order in the source code.

**Examples**

**1. Ordered Steps Using `<ol>`**
```html
<ol>
  <li>Install the software</li>
  <li>Restart your computer</li>
</ol>
```

**2. Correct Tab Order for Form Fields**
```html
<form>
  <label for="name">Name:</label>
  <input type="text" id="name">

  <label for="email">Email:</label>
  <input type="email" id="email">

  <button type="submit">Submit</button>
</form>
```

**3. Avoiding Broken Reading Sequences**  
Avoid using tables for layout that create illogical reading order:
```html
<!-- ✗ Poor example -->
<table>
  <tr><td>First Name</td><td>Last Name</td></tr>
  <tr><td>John</td><td>Smith</td></tr>
</table>
```

**4. Semantic Structure Using Headings**
```html
<h1>Chapter Title</h1>
<p>Introduction paragraph.</p>
<h2>Subsection</h2>
<p>Details of the subsection.</p>
```

**Accessibility Benefits**
- Screen readers present content in a meaningful order.
- Users relying on keyboard navigation access content in a logical sequence.

**Best Practices**
- Ensure DOM order matches visual order.
- Use CSS for visual positioning, not HTML tricks like nested tables or absolute positioning.
- Test with a screen reader to confirm the flow of information is logical.

---

#### **1.3.3 Sensory Characteristics (Level A)**

**Intent:**  
Ensure that instructions provided for understanding or operating content do not rely solely on sensory characteristics such as shape, size, visual location, orientation, or sound.

**Requirements**
- Provide additional cues besides shape, color, or position.
- Avoid saying "click the red button" without also labeling it textually.

**Examples**

**1. Button with Textual Label**
```html
<p>Click the button labeled <strong>Submit</strong> to finish.</p>
<button>Submit</button>
```

**2. Form Field Reference by Label, Not Position**
Avoid: "Enter your email in the box to the right."  
Use: "Enter your email in the field labeled 'Email'."

**3. Instructional Diagram with Shape and Text**
```html
<svg aria-labelledby="shape-desc" role="img" width="100" height="100">
  <title id="shape-desc">Red triangle labeled 'Warning'</title>
  <polygon points="50,15 90,85 10,85" style="fill:red;stroke:black;stroke-width:2"/>
</svg>
```

**Accessibility Benefits**
- Users with visual, auditory, or cognitive disabilities can understand instructions.
- Screen reader users aren't confused by instructions relying solely on visual cues.

**Best Practices**
- Combine visual indicators with labels or textual descriptions.
- Use clear, descriptive labels for controls and icons.
- Test your content with a screen reader to ensure clarity of instructions.

> Note: Refer to guideline 1.4 for requirements relating to color

---

#### **1.3.4 Orientation (Level AA)**

**Intent:**  
Prevent locking content to a specific display orientation (portrait or landscape) so users can view content in the orientation they need.

**Requirements**
- Content must not restrict its view and operation to a single display orientation, unless a specific orientation is essential (e.g., piano keyboard app).

**Examples**

**1. Responsive Web App**
```html
<meta name="viewport" content="width=device-width, initial-scale=1.0">
```

**CSS Example:**
```css
body {
  display: flex;
  flex-direction: column;
}
```

**2. Essential Orientation Exception**  
A banking check deposit app requires landscape mode to properly frame the check during scanning - this is an essential orientation.

**Accessibility Benefits**
- Users who mount devices to mobility aids or use screen magnifiers can interact with content in their preferred orientation.

**Best Practices**
- Use responsive design to adapt to both orientations.
- Avoid locking orientation via CSS/JavaScript unless absolutely required.
- Clearly document any exception cases.

> Note: Examples where a particular display orientation may be essential are a bank check, a piano application, slides for a projector or television, or virtual reality content where content is not necessarily restricted to landscape or portrait display orientation.

---

#### **1.3.5 Identify Input Purpose (Level AA)**

**Intent:**  
Support personalization and assistive technologies by ensuring that form input fields can be programmatically identified with their intended purpose.

**Requirements**
- Use the HTML `autocomplete` attribute, with values from the WHATWG HTML specification, to describe the input's purpose.
- Applies only to inputs that collect information about the user (name, email, address, etc.).

**Examples**

**1. Email Input Field with Autocomplete**
```html
<label for="userEmail">Email address:</label>
<input type="email" id="userEmail" name="email" autocomplete="email">
```

**2. Name and Address Fields**
```html
<form>
  <label for="fname">First Name:</label>
  <input id="fname" name="given-name" autocomplete="given-name">

  <label for="lname">Last Name:</label>
  <input id="lname" name="family-name" autocomplete="family-name">

  <label for="street">Street Address:</label>
  <input id="street" name="address-line1" autocomplete="address-line1">
</form>
```

**Accessibility Benefits**
- Screen readers can inform users about the input's expected purpose.
- Supports browser autofill, reducing user input effort.

**Best Practices**
- Use correct `autocomplete` values from the WHATWG HTML spec.
- Only apply this for fields collecting user-identifying information.

---

<span class="aaa">

... The following criteria (level AAA) are omitted from this guide ...

- **1.3.6 Identify Purpose**
    - In content implemented using markup languages, the purpose of user interface components, icons, and regions can be programmatically determined.

</span>

---

### Guideline 1.4: Distinguishable

*Make it easier for users to see and hear content.*

**Overview**

**Guideline 1.4** ensures that users can **visually perceive content**, including text, images, and interface elements, by enhancing their **contrast, visibility, and separation** from the background. This is crucial for people with **low vision, color blindness, or cognitive challenges** who may struggle with overlapping content, low contrast, or reliance on color alone.

This guideline focuses on making it easier for users to **see and hear content**, emphasizing clarity and accessibility across different environments and devices.

---

**Success Criteria Summary**

| Success Criterion | Description |
|------------------|-------------|
| **1.4.1 Use of Color** | Do not use color as the only means of conveying information |
| **1.4.2 Audio Control** | Provide a way to stop or adjust audio that plays automatically |
| **1.4.3 Contrast (Minimum)** | Ensure sufficient contrast between text and background (AA) |
| **1.4.4 Resize Text** | Allow users to resize text up to 200% without loss of content/function |
| **1.4.5 Images of Text** | Avoid using images to present text (use real text instead) |
| **1.4.6 Contrast (Enhanced)** | Provide stronger contrast for Level AAA compliance |
| **1.4.7 Low or No Background Audio** | Ensure background audio doesn't obscure speech |
| **1.4.8 Visual Presentation** | Allow customization of text spacing, line length, and justification |
| **1.4.9 Images of Text (No Exception)** | No exceptions allowed for text in images |
| **1.4.10 Reflow** | Content must reflow without scrolling in two dimensions |
| **1.4.11 Non-text Contrast** | UI components and graphics must meet contrast requirements |
| **1.4.12 Text Spacing** | Maintain readability with increased spacing between characters/lines |
| **1.4.13 Content on Hover or Focus** | Make hover/focus content dismissible and persistent when needed |

---

**Visual Examples**

**✓ 1.4.1 Use of Color with Redundancy**

```html
<p><strong>Status:</strong> <span style="color: red;">Error</span> <span aria-hidden="true">✗</span></p>
```

**Why it works:** Uses color and an icon/label to reinforce meaning.

---

**✓ 1.4.3 Minimum Contrast (AA)**

```html
<p style="color: #000; background: #fff;">This text has a contrast ratio of 21:1.</p>
```

**Why it works:** Easily readable for users with visual impairments.

---

**✗ Failure - Low Contrast Text**

```html
<p style="color: #999; background: #fff;">This light gray text may be hard to read.</p>
```

**Issue:** Fails contrast ratio of 4.5:1 required for normal text.

---

**✓ 1.4.10 Reflow (Responsive Design)**

```css
body {
  max-width: 100%;
  word-wrap: break-word;
}
```

**Why it works:** Ensures content doesn't overflow horizontally on small screens.

---

**✓ 1.4.13 Hover Content with Persistent Display**

```html
<button aria-describedby="tooltip">More Info</button>
<div role="tooltip" id="tooltip" style="display:none;">Detailed explanation appears here.</div>
```

**Why it works:** Tooltip appears on demand and is controllable by the user.

---

**Checklist**

| Requirement                          | Example Provided | Meets Standard |
|-------------------------------------|------------------|----------------|
| Color not used alone                | ✓ Yes            | ✓             |
| Text contrast minimum (4.5:1)       | ✓ Yes            | ✓             |
| Text resizable up to 200%           | ✓ Yes            | ✓             |
| UI controls have sufficient contrast| ✓ Yes            | ✓             |
| Responsive, reflowable layout       | ✓ Yes            | ✓             |

---

Guideline 1.4 helps ensure that **all users, regardless of visual or auditory ability**, can distinguish and interact with content. It emphasizes clarity, flexibility, and visual accessibility in every environment.

**Success Criteria Links**

- [1.4.1 Use of Color (Level A)](#141-use-of-color-level-a)
- [1.4.2 Audio Control (Level A)](#142-audio-control-level-a)
- [1.4.3 Contrast (Minimum) (Level AA)](#143-contrast-minimum-level-aa)
- [1.4.4 Resize Text (Level AA)](#144-resize-text-level-aa)
- [1.4.5 Images of Text (Level AA)](#145-images-of-text-level-aa)
- <span class="aaa">1.4.6 Contrast (Enhanced) (Level AAA)</span>
- <span class="aaa">1.4.7 Low or No Background Audio (Level AAA)</span>
- <span class="aaa">1.4.8 Visual Presentation (Level AAA)</span>
- <span class="aaa">1.4.9 Images of Text (No Exception) (Level AAA)</span>
- [1.4.10 Reflow (Level AA)](#1410-reflow-level-aa)
- [1.4.11 Non-text Contrast (Level AA)](#1411-non-text-contrast-level-aa)
- [1.4.12 Text Spacing (Level AA)](#1412-text-spacing-level-aa)
- [1.4.13 Content on Hover or Focus (Level AA)](#1413-content-on-hover-or-focus-level-aa)

---

#### **1.4.1 Use of Color (Level A)**

**Intent:**  
Ensure that color is not the sole means of conveying information, indicating an action, prompting a response, or distinguishing a visual element.

**Requirements**
- Information conveyed with color must also be available in text or through another visual means.
- Applies to charts, form validation, instructions, indicators, etc.

**Examples**

**1. Required Fields Using Text and Symbols**
```html
<p><strong>Required fields are marked with an asterisk (*).</strong></p>
<label for="name">Name *</label>
<input id="name" name="name" required>
```

**2. Form Validation Using Text, Not Just Color**
```html
<p style="color:red;">Please enter a valid email address.</p>
<input type="email" aria-invalid="true">
```

**3. Accessible Chart with Patterns and Labels**
```html
<svg width="300" height="200" role="img" aria-labelledby="chart-title chart-desc">
  <title id="chart-title">Monthly Sales Comparison</title>
  <desc id="chart-desc">Blue bars represent online sales, striped bars represent in-store sales.</desc>
  <!-- Bars with color + texture/pattern for distinction -->
</svg>
```

**Accessibility Benefits**
- Users who are color blind or using monochrome displays can still access content meaningfully.
- Ensures usability for those with visual impairments or cognitive challenges.

**Best Practices**
- Use text, patterns, labels, icons, or positioning to supplement color.
- Test interfaces using color-blind simulators or grayscale settings.

> Note: This success criterion addresses color perception specifically. Other forms of perception are covered in Guideline 1.3 including programmatic access to color and other visual presentation coding.

---

#### **1.4.2 Audio Control (Level A)**

**Intent:**  
Ensure users can control audio that plays automatically, preventing interference with screen readers or other assistive tools.

**Requirements**
- If audio plays automatically for more than 3 seconds, provide a mechanism to pause, stop, or control the volume independently.

**Examples**

**1. Audio Player With Controls**
```html
<audio controls>
  <source src="intro.mp3" type="audio/mpeg">
  Your browser does not support the audio element.
</audio>
```

**2. Stop Button for Background Audio**
```html
<button onclick="document.getElementById('bg-music').pause()">Stop Music</button>
<audio id="bg-music" autoplay>
  <source src="music.mp3" type="audio/mpeg">
</audio>
```

**Accessibility Benefits**
- Prevents automatic audio from interfering with screen reader output.
- Gives users control over audio playback.

**Best Practices**
- Avoid autoplay audio unless necessary.
- Always provide accessible controls near the audio.
- Test that screen readers can still operate without interruption.

> Note: Since any content that does not meet this success criterion can interfere with a user's ability to use the whole page, all content on the Web page (whether or not it is used to meet other success criteria) must meet this success criterion.

---

#### **1.4.3 Contrast (Minimum) (Level AA)**

**Intent:**  
Ensure that text is readable by maintaining a sufficient contrast ratio between text (or images of text) and background.

**Requirements**
- Text must have a **contrast ratio of at least 4.5:1** for normal text, and **3:1** for large text (18pt or 14pt bold).
- Applies to text, images of text, and interactive elements.

**How to Determine Compliance**
Contrast ratio is calculated based on the relative luminance of the text color and background color. You can use tools such as:
- [WebAIM Contrast Checker](https://webaim.org/resources/contrastchecker/)
- [Accessible Colors](https://accessible-colors.com/)
- Browser dev tools or accessibility extensions

**Formula:**

<span class="big">

$\frac{(L1 + 0.05)}{(L2 + 0.05)}$

</span>

Where:
- $L1$ is the relative luminance of the lighter color
- $L2$ is the relative luminance of the darker color

**Examples**

**1. Sufficient Contrast on Button Text**
```css
.button {
  background-color: #005fcc;
  color: white; /* Contrast ratio ~12.6:1 */
}
```

**2. Insufficient Contrast (✗)**
```css
.bad-button {
  background-color: #cccccc;
  color: #eeeeee; /* Contrast ratio too low */
}
```

**3. Using a Contrast Checker Tool**
Provide a color picker to test combinations:
```html
<input type="color" id="fgColor"> Text Color
<input type="color" id="bgColor"> Background Color
```

**Accessibility Benefits**
- Helps users with low vision or color blindness read text easily.
- Improves readability in various lighting conditions.

**Best Practices**
- Test color combinations for all UI states (hover, focus, disabled).
- Don't rely on contrast alone - use larger text or bold styling for additional clarity.
- Document and verify contrast ratios during design and QA.

---

#### **1.4.4 Resize Text (Level AA)**

**Intent:**  
Ensure that users can increase text size up to 200% without loss of content or functionality.

**Requirements**
- Text must remain readable and usable when resized up to 200%.
- Content should not require horizontal scrolling (except for content requiring two-dimensional layout).

**Examples**

**1. Responsive Text Sizing Using Relative Units**
```css
body {
  font-size: 100%;
}
```

**2. Zoom-Compatible Layout Using Flexbox**
```css
.container {
  display: flex;
  flex-direction: column;
}
```

**3. Accessible Zoom on Web Pages**
```html
<meta name="viewport" content="width=device-width, initial-scale=1">
```

**4. Optionally Provide a Text Zoom Control**

*HTML*
```html
<label for="fontSize75">Small</label>
<input type="radio" name="fontSize" id="fontSize75" value="75%">
<label for="fontSize100">Normal</label>
<input type="radio" name="fontSize" id="fontSize100" value="100%" checked>
<label for="fontSize150">Large</label>
<input type="radio" name="fontSize" id="fontSize150" value="150%">
```

*JavaScript*
```javascript
const fontSizeInputs = document.querySelectorAll('input[name="fontSize"]');
fontSizeInputs.forEach(input => {
    input.addEventListener('change', () => {
        if (input.checked)
            document.body.style.fontSize = input.value;
    });
});
```

**Accessibility Benefits**
- Supports users with low vision who need to increase text size.
- Prevents overlap or content loss due to fixed layouts.

**Best Practices**
- Use relative units like `em`, `rem`, `%` instead of `px`.
- Test your design at 200% zoom.
- Avoid absolute positioning that can break when scaled.

---

#### **1.4.5 Images of Text (Level AA)**

**Intent:**  
Ensure that text content is provided using actual text rather than images of text so that it can be resized, styled, and interpreted by assistive technologies.

**Requirements**
- Use real text instead of images of text to convey information.
- Exceptions: when image of text is essential (e.g. a logo) or can be customized by the user.

**Examples**

**1. Proper Use of Real Text**
```html
<h1>Welcome to AccessSite</h1>
```

**2. Decorative Logo (Allowed Exception)**
```html
<img src="logo.png" alt="AccessSite logo">
```

**3. Avoid Image-Only Headings (✗)**
```html
<!-- ✗ Poor example: heading as image only -->
<img src="heading.png" alt="Welcome to AccessSite">
```

**Accessibility Benefits**
- Text can be resized and restyled using user stylesheets or browser zoom.
- Content is more responsive and accessible for screen readers.

**Best Practices**
- Use CSS for custom fonts and visual styling.
- If using image of text for design reasons, ensure there's a text alternative or allow customization.
- Consider user control preferences for themes and contrast.

> Note: Logotypes (text that is part of a logo or brand name) are considered essential.

---

<span class="aaa">

... The following criteria (level AAA) are omitted from this guide ...

- **1.4.6 Contrast (Enhanced)**
    - The visual presentation of text and images of text has a contrast ratio of at least 7:1, except for ...
- **1.4.7 Low or No Background Audio**
    - For prerecorded audio-only content that (1) contains primarily speech in the foreground, (2) is not an audio CAPTCHA or audio logo, and (3) is not vocalization intended to be primarily musical expression such as singing or rapping, at least one of the following is true...
- **1.4.8 Visual Presentation**
    - For the visual presentation of blocks of text, a mechanism is available to achieve the following...
- **1.4.9 Images of Text (No Exception)**
    - Images of text are only used for pure decoration or where a particular presentation of text is essential to the information being conveyed.

</span>

---

#### **1.4.10 Reflow (Level AA)**

**Intent:**  
Ensure that content can be presented without loss of information or functionality, and without requiring scrolling in two dimensions, especially on small screens or when zoomed.

**Requirements**
- Content must reflow to fit within a window that is 320 CSS pixels wide without requiring horizontal scrolling.
- Applies to vertical scrolling content only; exceptions exist for components requiring two-dimensional layout (e.g., data tables, maps).

**Examples**

**1. Responsive Layout with Flexbox or Grid**
```css
.container {
  display: flex;
  flex-wrap: wrap;
}
```

**2. Media Queries to Adjust Layout**
```css
@media screen and (max-width: 320px) {
  .nav {
    flex-direction: column;
  }
}
```

**3. Text Content That Adapts**
```html
<meta name="viewport" content="width=device-width, initial-scale=1">
```

**Accessibility Benefits**
- Improves usability for people with low vision using magnification.
- Ensures content is readable and operable on mobile devices.

**Best Practices**
- Use relative sizing and fluid layouts.
- Avoid fixed pixel widths and absolute positioning.
- Test layouts at 320px wide to verify functionality.

**Notes**

> 320 CSS pixels is equivalent to a starting viewport width of 1280 CSS pixels wide at 400% zoom. For web content which is designed to scroll horizontally (e.g., with vertical text), 256 CSS pixels is equivalent to a starting viewport height of 1024 CSS pixels at 400% zoom.

> Examples of content which requires two-dimensional layout are images required for understanding (such as maps and diagrams), video, games, presentations, data tables (not individual cells), and interfaces where it is necessary to keep toolbars in view while manipulating content. It is acceptable to provide two-dimensional scrolling for such parts of the content.

---

#### **1.4.11 Non-text Contrast (Level AA)**

**Intent:**  
Ensure that all important visual information presented through graphical components or interface elements is perceivable by users with visual impairments by providing sufficient contrast.

**Requirements**
- User interface components (like buttons, sliders, inputs) and graphical objects that convey information must have a contrast ratio of **at least 3:1** against adjacent colors.
- Applies to visual states such as focus, hover, and active, not just static components.

**Examples**

**1. Sufficient Contrast for UI Components**
```css
.button {
  background-color: #005fcc;
  color: white; /* Contrast > 3:1 */
  border: 2px solid #003399;
}
```

**2. Slider Handle with Contrast**
```css
input[type=range]::-webkit-slider-thumb {
  background: #444;
  border: 1px solid #ccc; /* Ensure thumb contrast exceeds 3:1 */
}
```

**3. Graphical Objects (e.g., Icons)**
```html
<svg role="img" aria-label="Warning">
  <circle cx="20" cy="20" r="18" fill="#f00" stroke="#000" stroke-width="2"/>
</svg>
```

**Accessibility Benefits**
- Users with low vision or color vision deficiencies can perceive controls and visual cues.
- Ensures interaction elements and states are clearly visible.

**Best Practices**
- Evaluate components in all visual states.
- Use contrast testing tools like axe DevTools, Chrome Lighthouse, or WebAIM Contrast Checker.
- Avoid relying on color alone to convey meaning.

---

#### **1.4.12 Text Spacing (Level AA)**

**Intent:**  
Ensure that text remains readable and functional when users override spacing styles for better readability.

**Requirements**
- No loss of content or functionality should occur when all of the following are applied:
  - Line height (line spacing) to at least 1.5 times the font size
  - Spacing following paragraphs to at least 2 times the font size
  - Letter spacing (tracking) to at least 0.12 times the font size
  - Word spacing to at least 0.16 times the font size

**Examples**

**1. Sample CSS for Text Spacing**
```css
.text-content {
  line-height: 1.5;
  margin-bottom: 2em;
  letter-spacing: 0.12em;
  word-spacing: 0.16em;
}
```

**2. CSS Reset Tolerant to User Overrides**
```css
body {
  overflow-wrap: break-word;
  max-width: 100%;
}
```

**Accessibility Benefits**
- Enhances readability for users with dyslexia, low vision, or cognitive disabilities.
- Supports user stylesheets and assistive tech settings.

**Best Practices**
- Avoid fixed heights or overflow that might cut off resized text.
- Test layout behavior with common text spacing adjustments.
- Ensure text containers expand flexibly to accommodate spacing.

---

#### **1.4.13 Content on Hover or Focus (Level AA)**

**Intent:**  
Ensure that additional content triggered by hover or focus is dismissible, hoverable, and persistent so it doesn't interfere with users' ability to interact with content.

**Requirements**
- Dismissible: Can be dismissed without moving pointer or keyboard focus.
- Hoverable: Pointer can move to the new content without it disappearing.
- Persistent: Remains visible until user removes hover/focus, dismisses it, or the action is completed.

**Examples**

**1. Tooltip With Persistent Hover Using ARIA**
```html
<button aria-describedby="help1">More Info</button>
<div id="help1" role="tooltip" style="display: none;">Explains additional context for this option.</div>
```

**2. Custom Tooltip With Hover and Keyboard Accessibility**
```javascript
button.onfocus = button.onmouseover = () => tooltip.style.display = 'block';
button.onblur = button.onmouseout = () => tooltip.style.display = 'none';
```

**Accessibility Benefits**
- Users with low vision or motor impairments can reach and interact with hover/focus content.
- Prevents content from vanishing too quickly for screen reader or keyboard-only users.

**Best Practices**
- Use scripts to manage visibility and interactivity of tooltips/popups.
- Ensure the additional content has accessible roles and labels.
- Test with both pointer and keyboard input for reliability.

**Notes:**

> Examples of additional content controlled by the user agent include browser tooltips created through use of the HTML title attribute.

> Custom tooltips, sub-menus, and other non-modal popups that display on hover and focus are examples of additional content covered by this criterion.
