# Instructions pour traiter une issue "Ajouter une communauté"

## Contexte

Quand quelqu'un crée une issue via le template `add-community.yml`, GitHub Copilot peut automatiser la création d'une PR pour ajouter cette communauté au site.

## Données à extraire de l'issue

L'issue contient les champs suivants :
- **community-name** : Nom complet de la communauté
- **community-slug** : Identifiant court (ex: `python`, `js-and-co`)
- **community-url** : URL du site principal (peut être une page Meetup)
- **registration-url** : URL d'inscription aux événements (optionnel, si différent de community-url)
- **description** : Description de la communauté
- **logo-url** : URL du logo (optionnel)
- **social-links** : Liste de liens sociaux (optionnel)

## Détection automatique de Meetup

Si `community-url` contient `meetup.com`, c'est une page Meetup :
1. Extraire le slug Meetup de l'URL (ex: `https://www.meetup.com/python-toulouse/` → `python-toulouse`)
2. Ajouter automatiquement dans `.github/workflows/update.cs`
3. Les événements seront synchronisés automatiquement

Exemples de détection :
- `https://www.meetup.com/python-toulouse/` → Meetup (slug: `python-toulouse`)
- `https://www.meetup.com/fr-FR/rust-community-toulouse/` → Meetup (slug: `rust-community-toulouse`)
- `https://toulousegamedev.fr/` → Site custom (pas de synchronisation auto)
- `https://www.agiletoulouse.fr/` → Site custom (pas de synchronisation auto)

## Étapes pour créer la PR

### 1. Créer le fichier `_groups/{slug}.md`

```yaml
---
name: {community-name}
link: {community-url}
img: none  # ou supprimer si un logo est fourni
---

{description formatée en HTML si nécessaire}
```

**Notes :**
- Si un logo est fourni (`logo-url`), télécharger l'image, la traiter avec ImageMagick, et la placer dans `groups-imgs/{slug}.jpg`
- Si un logo existe, supprimer la ligne `img: none`
- Convertir la description en HTML si elle contient du Markdown

**Traitement du logo avec ImageMagick (fond blanc, format 16:9, marges) :**

```bash
# Télécharger l'image originale
wget -O /tmp/logo-original.png "{logo-url}"

# Convertir en JPG avec fond blanc, ratio 16:9 (1200×675), 15% de padding
convert /tmp/logo-original.png \
  -background white \
  -gravity center \
  -resize 1020x573 \
  -extent 1200x675 \
  groups-imgs/{slug}.jpg
```

Cette commande garantit :
- Fond blanc (important pour les logos transparents)
- Dimensions 1200×675 (ratio 16:9 requis pour les cartes)
- 15% de marge autour du logo pour une meilleure présentation

### 2. Ajouter les liens sociaux (si fournis)

Si `social-links` est renseigné, ajouter au front matter :

```yaml
social:
  - icon: bi-{type}
    url: {url}
    title: "{Title}"
```

Mapping des types sociaux (icon Bootstrap et titre) :
- `website` → `bi-globe` (title: "Site Web")
- `twitter` → `bi-twitter-x` (title: "Page X / Twitter")
- `linkedin` → `bi-linkedin` (title: "Page LinkedIn")
- `github` → `bi-github` (title: "GitHub")
- `youtube` → `bi-youtube` (title: "YouTube")
- `discord` → `bi-discord` (title: "Discord")
- `slack` → `bi-slack` (title: "Slack")
- `mastodon` → `bi-mastodon` (title: "Mastodon")

### 3. Ajouter dans `.github/workflows/update.cs` (si Meetup)

**Détection automatique :** Si `community-url` contient `meetup.com` :

1. Extraire le slug Meetup de l'URL
   - Regex : `meetup\.com/(?:fr-FR/)?([^/]+)`
   - Exemples :
     - `https://www.meetup.com/python-toulouse/` → `python-toulouse`
     - `https://www.meetup.com/fr-FR/rust-community-toulouse/` → `rust-community-toulouse`

2. Ajouter dans le tableau `groups` (ligne ~37) :

```csharp
new MeetupGroup("{meetup-slug}", "{community-slug}"),
```

Maintenir l'ordre alphabétique par `community-slug`.

### 4. Mettre à jour le template des issues et instructions workflow

> ⚠️ **Ordre alphabétique obligatoire :** Toutes les listes ci-dessous doivent être maintenues dans l'ordre alphabétique. Vérifier la position correcte avant d'insérer.

1. Mettre à jour le fichier `.github/ISSUE_TEMPLATE/add-event.yml` pour ajouter le nom de la
   communauté dans les choix possibles des communautés organisatrices **(ordre alphabétique)**

2. Mettre à jour le tableau de la section "Trouver le slug de la communauté" des instructions
   `.github/COPILOT_EVENT_WORKFLOW.md` en ajoutant le slug et le nom de la communauté renseigné au point 1 **(ordre alphabétique)**

### 5. Mettre à jour `README.md`

Ajouter la communauté dans la liste **(maintenir l'ordre alphabétique)** :

```markdown
- [{community-name}]({community-url})
```

### 6. Créer la Pull Request

**Titre :** `Add {community-name} community`

**Description :**
```markdown
Adds {community-name} to the Toulouse Tech Hub.

**Community details:**
- Name: {community-name}
- Website: {community-url}
{- Registration: {registration-url} (si différent)}
{- Meetup sync: Yes (slug: {meetup-slug}) (si détecté)}

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
# Télécharger et traiter un logo (fond blanc, 16:9, 15% de padding)
wget -O /tmp/logo-original.png "{logo-url}"
convert /tmp/logo-original.png \
  -background white \
  -gravity center \
  -resize 1020x573 \
  -extent 1200x675 \
  groups-imgs/{slug}.jpg

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
- [ ] Le logo (si fourni) a été traité avec fond blanc et dimensions 1200×675 (16:9)
- [ ] Le fichier YAML est valide (front matter bien formé)
- [ ] Jekyll build réussit sans erreur
- [ ] `.github/ISSUE_TEMPLATE/add-event.yml` mis à jour **(ordre alphabétique respecté)**
- [ ] `.github/COPILOT_EVENT_WORKFLOW.md` tableau de mapping mis à jour **(ordre alphabétique respecté)**
- [ ] `README.md` mis à jour **(ordre alphabétique respecté)**
- [ ] Si Meetup : `.github/workflows/update.cs` mis à jour **(ordre alphabétique par community-slug respecté)**
- [ ] Si Meetup : le slug Meetup est bien extrait de l'URL

## Exemple complet

Issue contenant :
- community-name: "Rust Toulouse"
- community-slug: "rust"
- community-link: "https://www.meetup.com/fr-FR/rust-community-toulouse/"
- registration-link: (vide, même URL que community-url)
- description: "Communauté des développeurs Rust à Toulouse..."
- logo-link: "https://example.com/rust-logo.png"
- social-links: 
  ```
  twitter: https://x.com/rust_tlse
  github: https://github.com/rust-toulouse
  ```

**Détection automatique :** URL contient `meetup.com` → slug Meetup extrait = `rust-community-toulouse`

Résultat :

**`_groups/rust.md`**
```yaml
---
name: Rust Toulouse
link: https://www.meetup.com/fr-FR/rust-community-toulouse/
social:
  - icon: bi-twitter-x
    link: https://x.com/rust_tlse
    title: Page X / Twitter
  - icon: bi-github
    link: https://github.com/rust-toulouse
    title: GitHub
---

Communauté des développeurs Rust à Toulouse...
```

**`groups-imgs/rust.jpg`** (logo traité : fond blanc, 1200×675, 15% de padding)

```bash
wget -O /tmp/logo-original.png "https://example.com/rust-logo.png"
convert /tmp/logo-original.png \
  -background white \
  -gravity center \
  -resize 1020x573 \
  -extent 1200x675 \
  groups-imgs/rust.jpg
```

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
- Meetup sync: Yes (slug: rust-community-toulouse)

**Changes:**
- ✅ Created `_groups/rust.md`
- ✅ Added logo `groups-imgs/rust.jpg`
- ✅ Updated `.github/workflows/update.cs`
- ✅ Updated `.github/ISSUE_TEMPLATE/add-event.yml`
- ✅ Updated `.github/COPILOT_EVENT_WORKFLOW.md`
- ✅ Updated `README.md`

Closes #42
```
