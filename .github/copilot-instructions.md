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
All data lives in `_data/` as YAML files:

- **`_data/events/*.yml`** - Individual event files (one per event)
  - Naming convention: `YYYY-MM-DD-{community}-{identifier}.yml`
  - Required fields: `id`, `title`, `community`, `dateIso`, `dateFr`, `timeFr`, `link`, `img`, `localImg`
  - Optional fields: `place`, `placeAddr`, `description`, `dateIsoEnd`
  
- **`_data/groups.yml`** - Community definitions
  - Each entry can have: `id`, `name`, `url`, `registration`, `description`, `social[]`

- **`_data/confs.yml`** - Major annual conferences (currently unused, hardcoded in index.md)

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

### Image Management
- Event images are stored in `event-imgs/` with filename matching the event ID
- Conference images are in `confs-imgs/`
- Images should be WebP format when possible
- Cards use `aspect-ratio: 16/9` CSS for consistent display

### Event Lifecycle
1. **Automated**: Script scrapes Meetup/community sites and creates YAML files in `_data/events/`
2. **Manual**: Create YAML file following the convention, submit PR
3. **Build**: Jekyll generates all output formats on every build
4. **Display**: Only future events appear (past events are filtered by Liquid templates)

### Adding a New Event Manually
Create `_data/events/YYYY-MM-DD-{community}-{title}.yml`:
```yaml
id: 'unique-id'
title: 'Event Title'
community: 'Community Name'
dateIso: 'YYYY-MM-DD HH:MM'
dateFr: 'jour DD mois'
timeFr: 'HH:MM'
place: "Venue Name"
placeAddr: "Address"
link: https://example.com
img: https://example.com/image.jpg
localImg: event-imgs/unique-id.webp
description: >
  HTML description (can be multi-line)
```

### Adding a New Community
Add to `_data/groups.yml`:
```yaml
- id: slug
  name: Full Name
  url: https://website.com
  description: |
    <p>HTML description</p>
  social:
  - name: x
    url: https://x.com/handle
```

## Working with Jekyll Locally

No build commands - Jekyll runs via GitHub Actions only. To test locally:
```bash
# Install Jekyll (if needed)
gem install jekyll bundler

# Serve locally
jekyll serve

# View at http://localhost:4000
```

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
