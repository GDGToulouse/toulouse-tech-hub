# Instructions pour traiter une issue "Ajouter un événement"

## Contexte

Quand quelqu'un crée une issue via le template `add-event.yml`, cette procédure permet de créer le fichier d'événement correspondant.

## Données à extraire de l'issue

L'issue contient les champs suivants :
- **event-title** : Titre de l'événement
- **community** : Nom de la communauté organisatrice (dropdown)
- **event-date** : Date et heure au format `YYYY-MM-DD HH:MM`
- **event-link** : URL d'inscription à l'événement
- **event-description** : Description de l'événement
- **event-location** : Nom du lieu (optionnel)
- **event-address** : Adresse complète (optionnel)
- **event-image** : URL d'une image (optionnel)

## Étapes pour créer l'événement

### 1. Trouver le slug de la communauté

Mapper le nom de communauté vers son slug en consultant `_groups/` :

| Nom (dropdown) | Slug |
|----------------|------|
| Agile Toulouse | agile |
| AI-ficionados | aificionados |
| Artilect FabLab | artilect |
| AWS User Group Toulouse | aws |
| C++ Toulouse | cpp |
| Devops & Cloud Toulouse | devops-cloud |
| Embedded Meetup Toulouse | embedded |
| GDG Toulouse | gdg |
| JS & Co Toulouse | js-and-co |
| JUG Toulouse | jug |
| La "Toul Box" du Cloud Natif | toulbox |
| MTG:Toulouse | mtg |
| Postgres Toulouse | postgres |
| Python Toulouse | python |
| Rust Toulouse | rust |
| Swift Toulouse | swift |
| Tech a Break | tech-a-break |
| Tech Speak'her | tech-speak-her |
| Toulouse Data Science | tds |
| Toulouse Data-Viz | dataviz |
| Toulouse DevOps | devops |
| Toulouse Game Dev | tgd |
| Toulouse Ruby and Friends | ruby |

### 2. Générer l'eventId

Format : `manual-{timestamp}` où timestamp = Unix timestamp en secondes

Exemple : `manual-1709740200` (généré au moment de la création)

Alternative : utiliser un UUID court si préféré

### 3. Formater la date

À partir de `event-date` (ex: `2026-03-15 18:30`) :
- **dateIso** : `2026-03-15 18:30` (tel quel)
- **dateFr** : Convertir en français (ex: `"dimanche 15 mars"`)
  - Jours : lundi, mardi, mercredi, jeudi, vendredi, samedi, dimanche
  - Mois : janvier, février, mars, avril, mai, juin, juillet, août, septembre, octobre, novembre, décembre
- **timeFr** : `'18:30'` (entre quotes simples pour le YAML)
- **datePublished** : Date/heure actuelle au format `YYYY-MM-DD HH:MM`

### 4. Créer le fichier `_events/{date}-{slug}-{eventId}.html`

**Nom du fichier :** `_events/2026-03-15-{community-slug}-{eventId}.html`

**Contenu :**

```yaml
---
eventId: {eventId}
groupId: {community-slug}
title: "{event-title}"
community: "{community-name}"
datePublished: {current-datetime}
dateIso: {event-date}
dateFr: {date-in-french}
timeFr: '{time}'
{place: "{event-location}" (si fourni)}
{placeAddr: "{event-address}" (si fourni)}
link: {event-link}
{img: {event-image} (si fourni)}
---
{description-as-html}
```

**Notes importantes :**
- Entourer le titre de guillemets doubles si contient `:` ou caractères spéciaux
- `timeFr` doit être entre quotes simples (`'18:30'`)
- Convertir la description en HTML (paragraphes → `<p>`, listes → `<ul>/<li>`, etc.)
- Si pas d'image fournie, omettre le champ `img:`

### 5. Formater la description en HTML

Convertir le texte brut/markdown en HTML basique :
- Paragraphes vides → séparation en `<p>` tags
- Listes à puces (`- item`) → `<ul><li>item</li></ul>`
- Liens → `<a href="URL">texte</a>`
- Gras (`**text**`) → `<strong>text</strong>` (si markdown détecté)
- Italique (`*text*`) → `<em>text</em>` (si markdown détecté)

Classe recommandée : `<p class="mb-4">` pour l'espacement

### 6. Créer la Pull Request

**Titre :** `Add event: {event-title}`

**Description :**
```markdown
Adds manual event "{event-title}" to the calendar.

**Event details:**
- Community: {community-name}
- Date: {dateFr} à {timeFr}
- Location: {event-location} ({event-address})
- Registration: {event-link}

**Changes:**
- ✅ Created `_events/{filename}.html`

Closes #{issue-number}
```

**Labels :** `event`

## Validation

Avant de créer la PR, vérifier :
- [ ] La date est dans le futur (events passés ne s'affichent pas)
- [ ] Le slug de communauté existe dans `_groups/`
- [ ] Le format de date est correct (`YYYY-MM-DD HH:MM`)
- [ ] Le fichier YAML front matter est valide (quotes, indentation)
- [ ] La description HTML est bien formée (tags fermés)
- [ ] Jekyll build réussit : `jekyll build`
- [ ] L'événement apparaît sur `http://localhost:4000`

## Commandes utiles

```bash
# Générer un Unix timestamp
date +%s

# Tester le build Jekyll
jekyll build

# Vérifier qu'un événement apparaît
grep "{event-title}" _site/index.html

# Formater une date en français (avec date command Linux)
date -d "2026-03-15" "+%A %d %B" | sed 's/Monday/lundi/; s/Tuesday/mardi/; ...'
```

## Exemple complet

**Issue contenant :**
- event-title: "Workshop React avancé"
- community: "JS & Co Toulouse"
- event-date: "2026-03-15 14:00"
- event-link: "https://www.eventbrite.com/e/workshop-react"
- event-description: "Venez apprendre React ! Au programme : hooks, context, suspense."
- event-location: "La Cantine Toulouse"
- event-address: "27 Rue d'Aubuisson, 31000 Toulouse"
- event-image: "https://example.com/workshop.jpg"

**Résultat :**

**Fichier :** `_events/2026-03-15-js-and-co-manual-1709740200.html`

```yaml
---
eventId: manual-1709740200
groupId: js-and-co
title: "Workshop React avancé"
community: "JS & Co Toulouse"
datePublished: 2026-02-21 21:30
dateIso: 2026-03-15 14:00
dateFr: dimanche 15 mars
timeFr: '14:00'
place: "La Cantine Toulouse"
placeAddr: "27 Rue d'Aubuisson, 31000 Toulouse"
link: https://www.eventbrite.com/e/workshop-react
img: https://example.com/workshop.jpg
---
<p class="mb-4">Venez apprendre React ! Au programme : hooks, context, suspense.</p>
```

**PR Title:** `Add event: Workshop React avancé`

**PR Body:**
```markdown
Adds manual event "Workshop React avancé" to the calendar.

**Event details:**
- Community: JS & Co Toulouse
- Date: dimanche 15 mars à 14:00
- Location: La Cantine Toulouse (27 Rue d'Aubuisson, 31000 Toulouse)
- Registration: https://www.eventbrite.com/e/workshop-react

**Changes:**
- ✅ Created `_events/2026-03-15-js-and-co-manual-1709740200.html`

Closes #123
```

## Notes importantes

### Événements manuels vs automatiques

- **Événements Meetup** : Ne PAS créer d'issue, ils sont synchronisés automatiquement par `.github/workflows/update.cs`
- **Événements ponctuels** : Utiliser cette procédure
- **Événements récurrents non-Meetup** : Envisager d'ajouter un scraper dans `update.cs` plutôt que de créer manuellement

### Gestion des images

- Si `event-image` est fourni, utiliser tel quel (pas de téléchargement/conversion nécessaire)
- Les images sont chargées depuis leur URL d'origine
- Pas de stockage local dans `event-imgs/` pour les événements manuels

### Format de l'eventId

Utiliser `manual-{timestamp}` pour garantir l'unicité et distinguer des événements Meetup (`meetup-12345678`) et TGD (`tgd-YYYY-MM-DD`).
