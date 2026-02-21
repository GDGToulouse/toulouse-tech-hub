# Toulouse Tech Hub - Copilot Instructions

## Project Overview

This is a **Jekyll-based static site** that aggregates tech events and communities in Toulouse, France. The site automatically scrapes Meetup pages and community websites to maintain a unified calendar.

**Live site:** https://toulouse-tech-hub.fr

## Architecture

### Static Site Generation
- **Jekyll** builds the site using Liquid templates
- **GitHub Actions** deploys automatically on push to `main` and daily at 4am UTC (via cron)
- No build tools, test runners, or package managers - pure Jekyll

### Data Model
All data lives as Jekyll collections (markdown files with YAML front matter):

- **`_groups/` collection** – Community definitions  
  - One `.md` file per community (e.g., `gdg.md`, `jug.md`, `python.md`)
  - Front matter fields: `id`, `name`, `url`, `description` (HTML), `social[]` array
  - Images in `groups-imgs/` named `{group-id}.{ext}` (e.g., `jug.jpg`)

- **`_confs/` collection** – Annual conferences
  - One `.md` file per conference (e.g., `cloud-toulouse.md`, `pgday.md`)
  - Front matter fields: `id`, `name`, `date` (optional ISO format), `url`, `image`, `social[]` array
  - Images in `confs-imgs/` named `{conf-id}.{ext}` (e.g., `devfest.jpg`)

- **`_events/` collection** – Events (auto-generated from scraper)
  - One `.html` file per event, named by date and source (e.g., `2025-03-04-agile-meetup-305839478.html`)
  - Front matter fields (YAML): `eventId`, `groupId`, `title`, `community`, `datePublished`, `dateIso`, `dateFr`, `timeFr`, `link`, `img`, `place` (optional), `placeAddr` (optional), `dateIsoEnd` (optional)
  - Content (HTML): Event description fetched directly from Meetup (after `---` separator)
  - Images in `event-imgs/` computed from filename: `event-imgs/{date}-{groupId}-{eventId}.webp`
  - **Note:** No JSON cache - events are loaded directly from YAML front matter

### Output Formats
The site generates multiple consumable formats from the same event data:

1. **HTML** (`index.md` → `/`) - Main calendar view with Bootstrap cards
2. **iCal** (`events.ics`) - Calendar subscription format
3. **Atom/RSS** (`events.atom.xml`) - Feed format
4. **JSON** (`events.json`) - API format with event details
5. **Organizer tool** (`orgas.html`) - Interactive event selector with PNG export using html2canvas

All formats filter future events only using Liquid's date comparison: `{%- if event_time < now_time -%}{%- continue -%}{%- endif -%}`

### Layout Structure
- **`_layouts/default.html`** - Single layout with Bootstrap 5.0.2 + Bootstrap Icons
- **`index.md`** - Main page with three sections: Agenda (events), Conférences (annual conferences), Communautés (groups)

## Key Conventions

### Date Handling
- `dateIso`: ISO 8601 format (`YYYY-MM-DD HH:MM`) used for sorting and time math
- `dateFr`: French display format (`"jeudi 12 février"`)
- `timeFr`: Time display (`"18:45"`)
- Jekyll's `site.time` is compared as Unix timestamps: `| date: "%s" | plus: 0`

### Scraper Control
- To suppress a scraper-generated event, add a `.skip` file next to the markdown file (example: `_events/2025-03-04-agile-meetup-305839478.md.skip`).

### Event IDs
- Meetup events use `meetup-<numericId>` (example: `meetup-305479083`).
- Toulouse Game Dev events use `tgd-YYYY-MM-DD` based on the event date.

### Image Management
- Event images are stored in `event-imgs/` with filename matching the event ID
- Conference images are in `confs-imgs/`
- Images should be WebP format when possible
- Cards use `aspect-ratio: 16/9` CSS for consistent display

### File Encoding
- Files should be UTF-8; see `.editorconfig`

### Event Lifecycle
1. **Automated**: Update Data workflow runs at 9:00 and 17:00 UTC, executes `.github/workflows/update.cs`, downloads images into `event-imgs/`, and generates `_events/*.html`
2. **Manual**: Create HTML file in `_events/` following the convention, submit PR
3. **Build**: Jekyll generates all output formats on every build
4. **Display**: Only future events appear (past events are filtered by Liquid templates)

### Data Update Job
- Script: `.github/workflows/update.cs`
- Workflow: `.github/workflows/update-data.yml`
- Schedule: runs at 9:00 and 17:00 UTC, plus manual dispatch
- The script loads events directly from YAML front matter (no JSON cache)
- Image paths are computed from event metadata: `{date}-{groupId}-{eventId}.webp`

### Adding a New Event ManuallygroupId}-{eventId}.html`:

**Front matter (YAML between `---` delimiters):**
```yaml
---
eventId: "meetup-12345678"
groupId: "agile"
title: "Event Title"
community: "Community Name"
dateIso: "2025-03-15 18:30"
datePublished: "2025-03-01 10:00"
dateFr: "vendredi 15 mars"
timeFr: "18:30"
place: "Venue Name"
placeAddr: "123 Avenue Example, Toulouse"
link: https://example.com/event/12345678
img: https://example.com/image.jpg
---
```

**Content (HTML after `---`):**
```html
<p>Event description in HTML format</p>
```

**Image file:** Place image at `event-imgs/2025-03-15-agile-meetup-12345678.webp` (computed from filename)

**Important Notes:**
- Use `eventId` not `id` (Jekyll reserves `.id` for collection URLs)
- Always use double quotes for string values (handles special chars like `:`)
- `groupId` is used to compute image path and group association
- Image path is automatically computed: no need for `localImg` field

### Adding a New Community
Create a markdown file in `_groups/` named `{community-slug}.md`:
```yaml
---
name: Full Name
link: https://website.com
description: |
  <p>HTML description</p>
social:
  - icon: bi-globe
    url: https://website.com
    title: "Site Web"
  - icon: bi-twitter-x
    url: https://x.com/handle
    title: "Page X / Twitter"
---
```

**Note:** `link` property is for the main website. `social` array uses `icon`, `url` (for social profiles), and `title`. No `id` field (slug is derived from filename).

### Adding a New Conference
Create a markdown file in `_confs/` named `{conference-slug}.md`:
```yaml
---
id: slug
name: Conference Name
date: YYYY-MM-DD
link: https://example.com
image: confs-imgs/slug.jpg
social:
  - icon: bi-globe
    url: https://example.com
    title: Official Site
---
```

**Note:** `link` property is for the main website. `social` array uses `icon`, `url` (for social profiles), and `title`.

## Working with Jekyll Locally

No build commands - Jekyll runs via GitHub Actions only. To test locally:

### Windows Developers (Recommended)
- Use Ubuntu via WSL for the development environment.
- Install Jekyll inside the Ubuntu (WSL) distro.
- Use VS Code with the WSL extension to work in the Linux filesystem.

### WSL Prerequisites (Ubuntu)
```bash
sudo apt update
sudo apt install -y ruby-full build-essential zlib1g-dev

gem install jekyll bundler

ruby -v
jekyll -v
```

### Local Testing (All Platforms)
```bash
# Install Jekyll (if needed)
gem install jekyll bundler

# Serve locally
jekyll serve

# View at http://localhost:4000
```

## Browser Testing with Playwright

### When to Use What

**playwright-cli (PREFERRED for Visual Testing)**
- ✅ **Use for:** Visual regression, screenshot comparison, UI debugging, exploring site structure
- ✅ **Benefits:** Token-efficient, persistent sessions, generates Playwright code, detailed snapshots
- ✅ **Best for:** Iterative development, visual verification, manual exploration

**MCP Playwright Tools**
- ✅ **Use for:** Automated test scripts, integration in agent workflows, programmatic validation
- ✅ **Benefits:** Direct integration, no terminal management, returns structured data
- ✅ **Best for:** Automated checks during development, CI/CD integration

### playwright-cli Installation (WSL)

**CRITICAL: Install via npm in WSL, NOT Windows**

```bash
# Install globally in WSL (required for proper operation)
npm install -g @playwright/cli

# Verify installation
which playwright-cli  # Should show: /home/user/.nvm/.../bin/playwright-cli
playwright-cli --version  # Should show: @playwright/cli@0.x.x

# Install workspace skills (one-time per project)
playwright-cli install --skills
```

**Common Issue:** If `which playwright-cli` returns a Windows path (`/mnt/c/Program Files/...`), uninstall and reinstall via npm in WSL:
```bash
npm uninstall -g @playwright/cli
npm install -g @playwright/cli
```

### Essential Commands

**Session Management (Use Named Sessions)**
```bash
# Open browser in background session
playwright-cli -s=dev open http://localhost:4000 &

# List all active sessions
playwright-cli list

# Close specific session
playwright-cli -s=dev close

# Close all sessions
playwright-cli close-all
```

**Capture & Inspection**
```bash
# Take screenshot (save in .playwright-cli/ to auto-ignore)
playwright-cli -s=dev screenshot --filename=.playwright-cli/homepage.png

# Capture page structure (YAML with element references)
playwright-cli -s=dev snapshot --filename=.playwright-cli/page-structure.yml

# Evaluate JavaScript
playwright-cli -s=dev eval "document.querySelectorAll('.card').length"
```

**Interaction (Use refs from snapshots)**
```bash
# Click element (ref from snapshot)
playwright-cli -s=dev click e10

# Fill input
playwright-cli -s=dev fill e35 "text"

# Navigate
playwright-cli -s=dev goto https://example.com
playwright-cli -s=dev go-back
playwright-cli -s=dev go-forward
```

**DevTools & Debugging**
```bash
# View console logs
playwright-cli -s=dev console

# Network requests
playwright-cli -s=dev network

# List cookies
playwright-cli -s=dev cookie-list
```

### Complete Testing Workflow

**1. Start Jekyll and Open Browser Session**
```bash
# Terminal 1: Start Jekyll
jekyll serve --incremental

# Terminal 2: Open browser session in background
playwright-cli -s=test open http://localhost:4000 &
```

**2. Capture Initial State**
```bash
# Take snapshot to get element references (save in .playwright-cli/)
playwright-cli -s=test snapshot --filename=.playwright-cli/initial-state.yml

# Screenshot for visual comparison (save in .playwright-cli/)
playwright-cli -s=test screenshot --filename=.playwright-cli/before-changes.png
```

**3. Test Interactions**
```bash
# Example: Click on "Conférences" link (ref from snapshot)
playwright-cli -s=test click e10

# Verify URL changed
playwright-cli -s=test eval "window.location.href"
# Result: http://localhost:4000/#conferences

# Count elements
playwright-cli -s=test eval "document.querySelectorAll('.card').length"
# Result: 29
```

**4. Make Code Changes**
- Edit files (Jekyll auto-regenerates)
- Refresh browser: `playwright-cli -s=test reload`
- Take new screenshot: `playwright-cli -s=test screenshot --filename=.playwright-cli/after-changes.png`

**5. Cleanup**
```bash
playwright-cli -s=test close
```

### Generated Artifacts

**Best Practice:** Save all captures in `.playwright-cli/` directory (already in `.gitignore`):
```bash
playwright-cli -s=test screenshot --filename=.playwright-cli/my-test.png
playwright-cli -s=test snapshot --filename=.playwright-cli/my-snapshot.yml
```

All outputs are automatically saved:
- **Screenshots:** Use `--filename=.playwright-cli/name.png` (or current directory if not specified)
- **Snapshots (auto):** `.playwright-cli/page-YYYY-MM-DDTHH-MM-SS.yml`
- **Console logs:** `.playwright-cli/console-*.log`
- **Network logs:** `.playwright-cli/network-*.log`

The `.playwright-cli/` directory is already in `.gitignore`, so all files inside are automatically ignored.

### Key Features

**Snapshots Provide Element References**
Snapshot YAML contains element refs for interactions:
```yaml
- link "📢 Conférences" [ref=e10] [cursor=pointer]:
  - /link: "#conferences"
```
Use `ref=e10` in commands: `playwright-cli click e10`

**Generates Playwright Code**
Each command shows equivalent Playwright code:
```bash
$ playwright-cli click e10
### Ran Playwright code
await page.getByRole('link', { name: '📢 Conférences' }).click();
```
Copy this code for automated tests.

**Persistent Sessions**
Sessions maintain state between commands:
- Cookies preserved
- LocalStorage intact
- Navigation history available
- Use `--persistent` flag to save profile to disk

### Monitoring Dashboard

Visual dashboard to observe all sessions (requires X11):
```bash
playwright-cli show
```
- Live previews of all sessions
- Click to zoom and interact
- Remote control any browser
- Useful when running multiple tests

### Advanced Usage

**Run Custom Playwright Code**
```bash
playwright-cli -s=test run-code "
  await page.goto('http://localhost:4000');
  const cards = await page.localeAll('.card');
  return cards.length;
"
```

**Mock Network Requests**
```bash
playwright-cli -s=test route "**/*.jpg" --abort
playwright-cli -s=test route-list
```

**Video Recording**
```bash
playwright-cli -s=test video-start
# ... perform actions ...
playwright-cli -s=test video-stop recording.webm
```

**Tracing**
```bash
playwright-cli -s=test tracing-start
# ... perform actions ...
playwright-cli -s=test tracing-stop trace.zip
```

### Troubleshooting

**Issue: Command hangs/blocks**
- Use `&` to run sessions in background: `playwright-cli -s=name open URL &`
- Or use named sessions: `playwright-cli -s=name <command>`

**Issue: "Browser not found"**
- Browser downloads automatically on first use
- If fails: `playwright-cli install-browser`

**Issue: Screenshots empty/wrong page**
- Verify session is open: `playwright-cli list`
- Check page loaded: `playwright-cli -s=name snapshot`
- Ensure Jekyll is running: `curl http://localhost:4000`

**Issue: Multiple playwright-cli versions**
- Check which version: `which playwright-cli`
- Should be in WSL: `/home/user/.nvm/.../bin/`
- NOT Windows: `/mnt/c/Program Files/...`

### Reference Documentation

- Official repo: https://github.com/microsoft/playwright-cli
- Installed skills: `.claude/skills/playwright-cli/SKILL.md`
- Config file: `.playwright/cli.config.json`

## Deployment

Automatic via `.github/workflows/jekyll-gh-pages.yml`:
- Triggers on push to `main`
- Runs daily at 4am UTC to refresh event list
- Builds with `actions/jekyll-build-pages@v1`
- Deploys to GitHub Pages

## Special Features

### Organizer Tool (`orgas.html`)
- Interactive event card selector
- Removes unwanted events with × button
- Captures visible cards as PNG using html2canvas library
- Designed for meetup organizers to create promotional slides
## Development Notes & Lessons Learned

### Liquid Template Gotchas
When working with Liquid filters in loops and ranges:

1. **Filters in range loops don't evaluate inline**
   - ❌ `{% for i in (0..first_day | minus: 1) %}` - the filter isn't evaluated before creating the range
   - ✅ `{% assign empty_cells = first_day | minus: 1 %}` then `{% for i in (0..empty_cells) %}`
   - This affects loops spawning empty calendar cells and similar constructs

2. **Array access with filters requires variable assignment**
   - ❌ `{{ event_counts[i | minus: 1] }}` - returns nil, filter not evaluated in bracket notation
   - ✅ `{% assign idx = i | minus: 1 %}` then `{{ event_counts[idx] }}`
   - Always assign computed indices to a variable before array access

**Impact**: Calendar and event loops must pre-compute loop variables to avoid silent failures

### Data-Driven Architecture Pattern
Recent refactors established this pattern for dynamic content:

- **`_data/confs.yml`** - Conferences now use YAML instead of hardcoded HTML cards
- **`_includes/conferences.html`** - Renders conferences from YAML data
- **`_includes/calendar.html`** - Generates month grids dynamically with event counting
- Same pattern should be applied to other static/semi-static content (groups cards already do this)

**Benefits**: Easy to add/update content without touching templates, single source of truth per component

### Bootstrap Icons in YAML
Simplified social links structure:

- ❌ Old: `name: "globe"` → if/elif in template to map names to `bi-globe` classes
- ✅ New: `icon: "bi-globe"` → directly render `<i class="bi {{ social.icon }}"></i>`

This reduces template logic and makes icon codes visible in YAML data

### Calendar Implementation Notes
- Week starts Monday (France convention) - Sunday is column 7 (Dim)
- Event counting filters future events using `site.time` as Unix timestamp
- Badge colors: green (1), orange (2-3), red (4-6), dark red (7+)
- Dates use `{{ conf.date | date: "%d %B %Y" }}` format for display
- Date-range support: if `end` field exists, display as "01 Jan - 02 Jan"

## Documentation Structure

### README.md vs CONTRIBUTING.md

The project separates documentation by audience:

**README.md** (~70 lines) - For community organizers and event participants
- What the project is and what it does
- List of followed communities
- How to contribute via GitHub issue templates (no technical details)
- Tech stack overview (names only)
- Table of contents for quick navigation

**CONTRIBUTING.md** (.github/CONTRIBUTING.md) - For developers wanting to modify the code
- Complete architecture explanation (collections, workflows)
- Event update flow and generated formats
- Jekyll installation and local setup
- How to run the update job manually
- Detailed contribution guidelines for developers

**Result:**
- Users and organizers read **README** → understand what the site does → create issues to contribute
- Developers read **CONTRIBUTING** → understand internals → deploy changes via PR

## GitHub Issue Templates

The project uses GitHub Issue Forms (YAML templates) to streamline contributions. Each template has an associated Copilot workflow guide for automated processing.

### Available Templates

**1. Add Community** (`.github/ISSUE_TEMPLATE/add-community.yml`)
- **Label:** 🧑‍💻 communauté
- **Purpose:** Add a new tech community to the site
- **Workflow Guide:** `.github/COPILOT_COMMUNITY_WORKFLOW.md`
- **Auto-detection:** If community URL is a Meetup page, automatically extract slug and add to `update.cs` for event sync
- **Files created:** `_groups/{slug}.md`, `groups-imgs/{slug}.jpg` (optional)

**2. Add Conference** (`.github/ISSUE_TEMPLATE/add-conference.yml`)
- **Label:** 📢 conférence
- **Purpose:** Add an annual/recurring tech conference
- **Workflow Guide:** `.github/COPILOT_CONFERENCE_WORKFLOW.md`
- **Examples:** DevFest, PGDay, Capitole du Libre, Agile Tour
- **Files created:** `_confs/{slug}.md`, `confs-imgs/{slug}.jpg` (optional)

**3. Add Event** (`.github/ISSUE_TEMPLATE/add-event.yml`)
- **Label:** 📅 évènement
- **Purpose:** Add a one-time event (not auto-synced from Meetup)
- **Workflow Guide:** `.github/COPILOT_EVENT_WORKFLOW.md`
- **EventId format:** `manual-{unix-timestamp}`
- **Files created:** `_events/{date}-{slug}-{eventId}.html`, `event-imgs/` (optional)

**4. Bug Report** (`.github/ISSUE_TEMPLATE/bug-report.yml`)
- **Label:** 🐞 erreur
- **Purpose:** Report errors (missing event, broken link, incorrect info)
- **Fields:** Description (required), URL (optional)
- **Simplified:** 2 fields only for accessibility

**5. Feature Request** (`.github/ISSUE_TEMPLATE/feature-request.yml`)
- **Label:** 💡 suggestion
- **Purpose:** Suggest new features or improvements
- **Fields:** Description (required), Contribution checkbox (optional)
- **Simplified:** 1 main field for accessibility

### Template Design Principles

- **Simplified forms:** 1-3 required fields to reduce friction (user feedback: "ne pas rendre le process trop complexe")
- **Casual tone:** Friendly French with emojis ("Décrivez simplement ce qui ne va pas, on s'occupe du reste ! 👍")
- **Auto-detection:** Extract structured data from URLs (e.g., Meetup slug from URL)
- **Validation checkboxes:** Ensure data quality (e.g., "J'ai vérifié qu'elle n'est pas déjà listée")
- **GitHub labels:** Use existing labels with emojis (🧑‍💻, 📢, 📅, 🐞)

### Processing Workflow

**Community Addition:**
1. Extract fields from issue
2. Create `_groups/{slug}.md` with front matter
3. If Meetup URL detected, add to `update.cs`
4. Download and convert logo to JPG
5. Create PR with template

**Conference Addition:**
1. Extract fields from issue
2. Create `_confs/{slug}.md` with front matter
3. Download and convert logo to JPG
4. Validate date format (YYYY-MM-DD)
5. Create PR with template

**Event Addition:**
1. Extract fields from issue
2. Map community name to slug (20 communities table)
3. Generate `manual-{timestamp}` eventId
4. Format dates: dateIso, dateFr, timeFr
5. Create `_events/{date}-{slug}-{eventId}.html`
6. Create PR with template

## Git Workflow

### Development Process
1. **Make changes**: Edit files, create new files, modify configuration
2. **Build & validate**: Run `jekyll build` to check for errors, verify output
3. **Review**: Test visually with browser or MCP if needed
4. **Stop and wait**: Do NOT commit automatically - wait for user validation
5. **User approval**: User reviews changes and indicates approval
6. **Commit only then**: Run `git add`, `git commit`, `git push` only after explicit approval

### Key Rules
- ⛔ **Never auto-commit** - Always wait for explicit user approval
- ⛔ **Never git push** until user validates the changes
- ✅ **Do test locally** - Run `jekyll build` to verify no errors before stopping
- ✅ **Do show the work** - Report what was changed and where
- ✅ **Do suggest commit message** - Describe changes in a clear message when ready to commit

### Branch Strategy
- Current development branch: `copilot` (for experimental features)
- Target for merging: `main` (production)
- Always push to the feature/current branch, never directly to `main`
