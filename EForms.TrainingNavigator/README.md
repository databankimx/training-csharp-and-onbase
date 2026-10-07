# EForms.TrainingNavigator

A minimal ASP.NET Core web application that serves the EForms training curriculum as a self-hosted local web experience. It is not a course-management system and has no user authentication - it is a static file host with one backend API endpoint bolted on.

Targets **net10.0**.

---

## What it is

The training content itself lives in `wwwroot/` as plain HTML, CSS, JavaScript, and Markdown files organised by chapter folder. `index.html` (also in `wwwroot/`) is a single-page application that:

- Renders a collapsible chapter/lesson navigation sidebar built from a hardcoded manifest in the JavaScript
- Loads each lesson's `.html` file into a sandboxed `<iframe>` for the live preview
- Extracts the lesson's inline and linked HTML, CSS, and JavaScript and displays them in syntax-highlighted tabs
- Renders the accompanying `.md` (Learn More) notes using `marked.js`
- Intercepts `console.log/warn/error` calls inside the iframe and surfaces them in a console panel

The backend (`Program.cs`) does two things:

1. Serves everything in `wwwroot/` as static files (the default ASP.NET Core static file middleware)
2. Exposes `POST /api/location/lookup` - a zip code lookup endpoint used by the GL coding form lessons that query city/state/county from the `ExternalData` SQL Server database

---

## Running it

### Prerequisites

- .NET 10 SDK
- SQL Server instance with the `ExternalData` database restored from `Resources/ExternalData.bak` (in the solution root `Resources/` folder). The database contains the `ZipCodes` table used by the location lookup endpoint. Without it the server still starts, but the coding form lessons that call the lookup will get 500 errors.

### Connection string

Edit `appsettings.json` and point `DefaultConnection` at your SQL Server instance:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=ExternalData;User Id=hsi;Password=wstinol;TrustServerCertificate=true"
}
```

The default points at `OnBaseSandboxVM`, which is DataBank's internal development VM. Change `Server=` to `localhost` (or whatever your instance name is) if you're running SQL Server locally.

### Start the server

```
dotnet run --project EForms.TrainingNavigator
```

or press **F5** in Visual Studio with `EForms.TrainingNavigator` set as the startup project.

The server listens on `http://localhost:5000`* by default (ASP.NET Core's Kestrel default for a non-HTTPS development profile). Open that URL in a browser.

> Note: \* Your solution may run on a different port if you have multiple ASP.NET Core projects in the same solution. Check the console output for the actual URL.

### Restoring ExternalData.bak

The backup file is included in the solution at `Resources/ExternalData.bak`. To restore it:

**SQL Server Management Studio:** Right-click Databases → Restore Database → Device → add the `.bak` file → restore as `ExternalData`.

**T-SQL:**
```sql
RESTORE DATABASE ExternalData
FROM DISK = 'C:\path\to\ExternalData.bak'
WITH MOVE 'ExternalData' TO 'C:\path\ExternalData.mdf',
     MOVE 'ExternalData_log' TO 'C:\path\ExternalData_log.ldf',
     REPLACE;
```

Alternatively, the server exposes `GET /api/download/externaldata-backup` which streams the `.bak` file directly from the server's `Resources/` folder, so you can download it from a running instance without needing access to the solution files.

---

## Content structure

```
wwwroot/
├── index.html                      Single-page shell and navigation
├── styles/                         Shared CSS used across lessons
├── images/                         Shared images
├── 01-fundamentals/                HTML Fundamentals chapter
│   ├── 000-basic-structure.html
│   ├── 000-basic-structure.md      Learn More notes (optional, per lesson)
│   └── ...
├── 02-forms/                       HTML Forms chapter
├── 03-onbase-forms/                OnBase Forms chapter
├── 04-scripting-and-validation/    JavaScript and jQuery
├── 05-common-mistakes/             Common JavaScript mistakes
├── 06-dynamic-forms/               Dynamic/Vue.js forms
├── 07-jquery-plugins/              Building jQuery plugins
├── 08-cookies/                     Cookies
├── 09-bootstrap-basics/            Bootstrap 3 and 5
├── 10-databank-branding/           DataBank branding reference
├── 11-just-for-fun/                Miscellaneous
├── 12-avoiding-xss/                Cross-site scripting
└── 13-accessibility/               WCAG 2.1 (Markdown-only chapter)
```

Each chapter folder contains `.html` lesson files and optionally `.md` files with the same base name for the Learn More tab. Chapter 13 (Accessibility) is a documentation-only chapter - it has no `.html` files, only `.md` files, and the navigator renders them full-width without the code/preview panes.

---

## Adding a lesson

1. Add a `.html` file to the appropriate chapter folder. The filename is the lesson identifier - use the same numeric-prefix convention as the existing files.
2. Optionally add a `.md` file with the same base name for Learn More notes.
3. Register the lesson in the `CHAPTERS` manifest at the top of the `<script>` block in `wwwroot/index.html`. The navigator builds the nav sidebar entirely from this manifest - a file that isn't listed there won't appear in the UI even if it's on disk.

### Manifest structure

```javascript
{ file: "025-basic-form-structure", title: "Basic Form Structure" }
```

A lesson with child pages:
```javascript
{
  file: "018-layout-elements", title: "Layout Elements",
  children: [
    { file: "018a-css-float-layout", title: "CSS Float Layout" },
    { file: "018b-css-flexbox-layout", title: "CSS Flexbox Layout" }
  ]
}
```

A documentation-only lesson (Markdown, no preview pane):
```javascript
{ file: "130-introduction", title: "Introduction", type: "md" }
```

### Console capture

Every HTML lesson gets `console.log/warn/error/info` intercepted automatically by the navigator shell and surfaced in the console panel. Lessons don't need to do anything special - just use `console.log` normally and the output appears below the preview iframe.

Lessons can also navigate to another lesson programmatically:
```javascript
window.navigateToLesson("018b-css-flexbox-layout");
```

---

## The zip code lookup endpoint

`POST /api/location/lookup` accepts:
```json
{ "requestId": "any-string", "zipCode": "12345" }
```

Returns:
```json
{
  "requestId": "any-string",
  "data": [{ "state": "NY", "county": "Albany", "city": "Albany", "zipCode": "12205" }],
  "errors": []
}
```

Callers can override the connection string for a specific request by including an `X-Connection-String` header. This allows the coding form lessons to point at a different database instance at runtime without changing `appsettings.json`.

The endpoint is used by the GL coding form lessons in chapter 04 (`044-coding-form-validation`).
