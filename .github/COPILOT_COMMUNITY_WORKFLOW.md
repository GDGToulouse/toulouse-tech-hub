# Instructions pour traiter une issue "Ajouter une communauté"

## Contexte

Quand quelqu'un crée une issue via le template `add-community.yml`, GitHub Copilot peut automatiser la création d'une PR pour ajouter cette communauté au site.

## Données à extraire de l'issue

L'issue contient les champs suivants :
- **community-name** : Nom complet de la communauté
- **community-slug** : Identifiant court (ex: `python`, `js-and-co`)
- **community-url** : URL du site principal
- **event-source** : Type de source (Meetup.com, Site web custom, Manuel)
- **meetup-url** : URL Meetup (si applicable)
- **description** : Description de la communauté
- **logo-url** : URL du logo (optionnel)
- **social-links** : Liste de liens sociaux (optionnel)

## Étapes pour créer la PR

### 1. Créer le fichier `_groups/{slug}.md`

```yaml
---
name: {community-name}
url: {community-url}
img: none  # ou supprimer si un logo est fourni
---

{description formatée en HTML si nécessaire}
```

**Notes :**
- Si un logo est fourni (`logo-url`), télécharger l'image, la convertir en JPG avec ImageMagick, et la placer dans `groups-imgs/{slug}.jpg`
- Si un logo existe, supprimer la ligne `img: none`
- Convertir la description en HTML si elle contient du Markdown

### 2. Ajouter les liens sociaux (si fournis)

Si `social-links` est renseigné, ajouter au front matter :

```yaml
social:
  - icon: bi-{type}
    url: {url}
    title: {Title}
```

Mapping des types :
- `website` → `bi-globe` (title: "Site web")
- `twitter` → `bi-twitter-x` (title: "Page X / Twitter")
- `linkedin` → `bi-linkedin` (title: "LinkedIn")
- `github` → `bi-github` (title: "GitHub")
- `youtube` → `bi-youtube` (title: "YouTube")
- `discord` → `bi-discord` (title: "Discord")
- `slack` → `bi-slack` (title: "Slack")

### 3. Ajouter dans `.github/workflows/update.cs` (si Meetup)

Si `event-source` == "Meetup.com" et `meetup-url` est fourni :

Extraire le slug Meetup de l'URL (ex: `https://www.meetup.com/python-toulouse/` → `python-toulouse`)

Ajouter dans le tableau `groups` (ligne ~37) :

```csharp
new MeetupGroup("{meetup-slug}", "{community-slug}"),
```

Maintenir l'ordre alphabétique par `community-slug`.

### 4. Mettre à jour `README.md`

Ajouter la communauté dans la liste (maintenir l'ordre alphabétique) :

```markdown
- [{community-name}]({community-url})
```

### 5. Créer la Pull Request

**Titre :** `Add {community-name} community`

**Description :**
```markdown
Adds {community-name} to the Toulouse Tech Hub.

**Community details:**
- Name: {community-name}
- Website: {community-url}
- Event source: {event-source}
{- Meetup: {meetup-url} (si applicable)}

**Changes:**
- ✅ Created `_groups/{slug}.md`
{- ✅ Added logo `groups-imgs/{slug}.jpg` (si applicable)}
{- ✅ Updated `.github/workflows/update.cs` (si Meetup)}
- ✅ Updated `README.md`

Closes #{issue-number}
```

**Labels :** `community`, `enhancement`

**Reviewers :** Assigner un mainteneur (@tbolon ou autre)

## Commandes utiles

```bash
# Télécharger et convertir un logo
wget -O /tmp/logo.ext "{logo-url}"
convert /tmp/logo.ext groups-imgs/{slug}.jpg

# Vérifier qu'aucun doublon n'existe
ls _groups/{slug}.md  # ne doit pas exister
grep -r "{community-name}" _groups/  # vérifier les noms similaires

# Tester le build Jekyll
jekyll build

# Vérifier la syntaxe C#
dotnet script .github/workflows/update.cs --help
```

## Checklist de validation

Avant de créer la PR, vérifier :
- [ ] Le slug est unique (pas de fichier existant avec ce nom)
- [ ] Le slug ne contient que des caractères alphanumériques et tirets
- [ ] Le nom de la communauté n'existe pas déjà
- [ ] L'URL est valide et accessible
- [ ] Le logo (si fourni) a été converti en JPG
- [ ] Le fichier YAML est valid (front matter bien formé)
- [ ] Jekyll build réussit sans erreur
- [ ] Le README est à jour et trié alphabétiquement
- [ ] Si Meetup : le slug Meetup est bien extrait de l'URL

## Exemple complet

Issue contenant :
- community-name: "Rust Toulouse"
- community-slug: "rust"
- community-url: "https://www.meetup.com/fr-FR/rust-community-toulouse/"
- event-source: "Meetup.com"
- meetup-url: "https://www.meetup.com/rust-community-toulouse/"
- description: "Communauté des développeurs Rust à Toulouse..."
- logo-url: "https://example.com/rust-logo.png"
- social-links: 
  ```
  twitter: https://x.com/rust_tlse
  github: https://github.com/rust-toulouse
  ```

Résultat :

**`_groups/rust.md`**
```yaml
---
name: Rust Toulouse
url: https://www.meetup.com/fr-FR/rust-community-toulouse/
social:
  - icon: bi-twitter-x
    url: https://x.com/rust_tlse
    title: Page X / Twitter
  - icon: bi-github
    url: https://github.com/rust-toulouse
    title: GitHub
---

Communauté des développeurs Rust à Toulouse...
```

**`groups-imgs/rust.jpg`** (logo converti)

**`.github/workflows/update.cs`** (ajout ligne ~50)
```csharp
new MeetupGroup("rust-community-toulouse", "rust"),
```

**`README.md`** (ajout dans la liste)
```markdown
- [Rust Toulouse](https://www.meetup.com/fr-FR/rust-community-toulouse/)
```

**PR Title:** `Add Rust Toulouse community`

**PR Body:**
```markdown
Adds Rust Toulouse to the Toulouse Tech Hub.

**Community details:**
- Name: Rust Toulouse
- Website: https://www.meetup.com/fr-FR/rust-community-toulouse/
- Event source: Meetup.com
- Meetup: https://www.meetup.com/rust-community-toulouse/

**Changes:**
- ✅ Created `_groups/rust.md`
- ✅ Added logo `groups-imgs/rust.jpg`
- ✅ Updated `.github/workflows/update.cs`
- ✅ Updated `README.md`

Closes #42
```
