# Copilot Workflow: Add Conference

This guide explains how to process conference addition issues created via the GitHub issue template `add-conference.yml`.

## 📋 Issue Template Fields

When a user submits a conference addition, the issue contains these fields:

- **conference-name**: Full conference name (e.g., "DevFest Toulouse")
- **conference-slug**: Short identifier in lowercase without spaces (e.g., "devfest")
- **conference-url**: Official website URL (optional)
- **conference-date**: Next edition date in YYYY-MM-DD format (optional)
- **conference-end-date**: End date if multi-day event in YYYY-MM-DD format (optional)
- **description**: Brief description of the conference
- **logo-url**: Conference logo URL (optional)
- **social-links**: Social media and additional links (free-form text, optional)

## 🎯 Workflow Steps

### Step 1: Extract Information from Issue

Parse the issue body to extract all field values. The fields are marked with HTML comments like `<!-- conference-name -->`.

### Step 2: Create Conference File

Create a new markdown file in `_confs/` named `{slug}.md`:

**File path:** `_confs/{conference-slug}.md`

**Front matter structure:**

```yaml
---
id: {conference-slug}
name: {conference-name}
date: {conference-date}  # Optional: YYYY-MM-DD format
end: {conference-end-date}  # Optional: YYYY-MM-DD format if multi-day
url: {conference-url}  # Optional
image: confs-imgs/{conference-slug}.jpg  # Or .png/.webp depending on logo format
social:
  - icon: bi-globe
    url: {website-url}
    title: Site Officiel
  - icon: bi-linkedin
    url: {linkedin-url}
    title: Page LinkedIn
  # ... more social links
---
```

**Important notes:**
- `id` field uses the slug (no front matter ID needed, use `.slug` in templates)
- `date` field is optional - only include if next edition date is known
- `end` field is optional - only include for multi-day conferences
- `url` field is optional - only include if provided
- `image` path uses convention: `confs-imgs/{slug}.{ext}`
- Social links array uses Bootstrap Icons class names

### Step 3: Social Links Mapping

Parse the free-form social-links field and convert to structured YAML array.

**Bootstrap Icons mapping** (common social platforms):

| Platform | Bootstrap Icon Class | Title Template |
|----------|---------------------|----------------|
| Site web / Website | `bi-globe` | "Site Officiel" |
| LinkedIn | `bi-linkedin` | "Page LinkedIn" |
| X / Twitter | `bi-twitter-x` | "Page X / Twitter" |
| YouTube | `bi-youtube` | "Chaîne YouTube" or "Playlist YouTube" |
| Bluesky | `bi-bluesky` | "Page Bluesky" |
| Facebook | `bi-facebook` | "Page Facebook" |
| Instagram | `bi-instagram` | "Page Instagram" |
| Mastodon | `bi-mastodon` | "Compte Mastodon" |

**Example conversion:**

Input (from issue):
```
Site web: https://devfesttoulouse.fr/
LinkedIn: https://www.linkedin.com/company/devfesttoulous
X/Twitter: https://twitter.com/devfesttoulouse
YouTube: https://www.youtube.com/channel/abc123
```

Output (in YAML):
```yaml
social:
  - icon: bi-globe
    url: https://devfesttoulouse.fr/
    title: Site Officiel
  - icon: bi-linkedin
    url: https://www.linkedin.com/company/devfesttoulous
    title: Page LinkedIn
  - icon: bi-twitter-x
    url: https://twitter.com/devfesttoulouse
    title: Page X / Twitter
  - icon: bi-youtube
    url: https://www.youtube.com/channel/abc123
    title: Chaîne YouTube
```

### Step 4: Download and Process Logo

If a logo URL is provided:

1. **Download the logo:**
   ```bash
   wget -O confs-imgs/{slug}.{ext} "{logo-url}"
   ```

2. **Convert to JPG if needed (recommended for consistency):**
   ```bash
   # If downloaded as PNG/WebP/etc., convert to JPG
   convert confs-imgs/{slug}.png confs-imgs/{slug}.jpg
   rm confs-imgs/{slug}.png
   ```

3. **Update image path in YAML:**
   ```yaml
   image: confs-imgs/{slug}.jpg
   ```

**Note:** If no logo is provided, you can:
- Search online for the conference official logo
- Ask the issue author to provide one
- Create a placeholder or skip the image field

### Step 5: Validate Conference File

Before creating a PR, validate:

1. **Unique slug:** Check no other file exists in `_confs/` with same name
   ```bash
   ls _confs/{slug}.md
   # Should return: No such file or directory
   ```

2. **YAML syntax:** Verify front matter is valid
   ```bash
   jekyll build 2>&1 | grep -i error
   # Should return empty if no errors
   ```

3. **Image exists:** If image path specified, verify file exists
   ```bash
   ls confs-imgs/{slug}.jpg
   ```

4. **Dates format:** If dates provided, verify YYYY-MM-DD format
   ```bash
   # Valid examples:
   date: 2026-10-15
   end: 2026-10-16
   
   # Invalid:
   date: 15/10/2026  # Wrong format
   date: October 15  # Missing year and wrong format
   ```

### Step 6: Create Pull Request

Create a PR with the following template:

**Title:** `feat: Add {conference-name} conference`

**Body:**
```markdown
Adds {conference-name} to the conferences list.

**Conference details:**
- Name: {conference-name}
- Website: {conference-url}
- Date: {date-display}
- Description: {brief-description}

Closes #{issue-number}
```

**Files changed:**
- `_confs/{slug}.md` (new file)
- `confs-imgs/{slug}.jpg` (new file, if applicable)

## 📝 Complete Example

### Example Issue Content

```
conference-name: PG Day Toulouse
conference-slug: pgday
conference-url: https://pgday.fr/
conference-date: 2026-06-03
conference-end-date: 2026-06-04
description: Conférence PostgreSQL francophone. Deux jours de conférences et ateliers autour de PostgreSQL et des bases de données relationnelles.
logo-url: https://pgday.fr/static/logo-pgday.png
social-links:
Site web: https://pgday.fr/
LinkedIn: https://www.linkedin.com/company/postgresqlfr
YouTube: https://www.youtube.com/channel/UCR7skKC85Zn6p7fJ-lW7G8g
```

### Step-by-Step Processing

1. **Create file `_confs/pgday.md`:**

```yaml
---
id: pgday
name: PGDay Toulouse
date: 2026-06-03
end: 2026-06-04
url: https://pgday.fr/
image: confs-imgs/pgday.jpg
social:
  - icon: bi-globe
    url: https://pgday.fr/
    title: Site Officiel
  - icon: bi-linkedin
    url: https://www.linkedin.com/company/postgresqlfr
    title: Page LinkedIn
  - icon: bi-youtube
    url: https://www.youtube.com/channel/UCR7skKC85Zn6p7fJ-lW7G8g
    title: Chaîne YouTube
---
```

2. **Download and convert logo:**

```bash
wget -O confs-imgs/pgday.png "https://pgday.fr/static/logo-pgday.png"
convert confs-imgs/pgday.png confs-imgs/pgday.jpg
rm confs-imgs/pgday.png
```

3. **Validate:**

```bash
# Check no duplicate
ls _confs/pgday.md  # Should show the new file

# Test Jekyll build
jekyll build 2>&1 | grep -i error  # Should be empty

# Verify image
ls confs-imgs/pgday.jpg  # Should exist
```

4. **Create PR:**

Title: `feat: Add PGDay Toulouse conference`

Body:
```markdown
Adds PGDay Toulouse to the conferences list.

**Conference details:**
- Name: PGDay Toulouse
- Website: https://pgday.fr/
- Date: June 3-4, 2026
- Description: Conférence PostgreSQL francophone. Deux jours de conférences et ateliers autour de PostgreSQL.

Closes #42
```

## ✅ Validation Checklist

Before merging the PR, verify:

- [ ] Conference file created in `_confs/{slug}.md`
- [ ] YAML front matter is valid (no syntax errors)
- [ ] Conference slug is unique (no duplicate)
- [ ] Date format is YYYY-MM-DD (if provided)
- [ ] End date is after start date (if multi-day event)
- [ ] Logo downloaded in `confs-imgs/` (if applicable)
- [ ] Image path matches actual file
- [ ] Social links use correct Bootstrap Icons classes
- [ ] Jekyll builds without errors
- [ ] Conference displays correctly on /mentions.html
- [ ] Issue is closed with PR link

## 🛠️ Useful Commands

```bash
# Download logo with wget
wget -O confs-imgs/{slug}.png "{logo-url}"

# Convert image to JPG
convert confs-imgs/{slug}.png confs-imgs/{slug}.jpg

# Test Jekyll build
jekyll build

# Check for errors
jekyll build 2>&1 | grep -i error

# List all conferences
ls _confs/

# View conference in browser (local)
jekyll serve
# Then open http://localhost:4000/mentions.html
```

## 📚 Related Files

- **Template:** `.github/ISSUE_TEMPLATE/add-conference.yml`
- **Conferences collection:** `_confs/*.md`
- **Conference images:** `confs-imgs/`
- **Display template:** `_includes/conferences.html`
- **Main page:** `mentions.html` (conferences section)

## 🔍 Notes

- Conferences appear on the `/mentions.html` page (not on the main calendar)
- Conferences are recurring annual events, not one-time events
- For one-time events, use the event template instead
- The `date` field is optional - some conferences might not have their next edition date yet
- Images should be in landscape format (16:9 ratio preferred)
- Bootstrap Icons are used for social links (class names like `bi-globe`, `bi-linkedin`)
