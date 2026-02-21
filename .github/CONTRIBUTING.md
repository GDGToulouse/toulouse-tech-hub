# Contribuer au projet

Merci de contribuer à Toulouse Tech Hub ! Ce projet est un site Jekyll qui agrèges les événements tech de Toulouse et publie plusieurs formats (site web, JSON, Atom, iCal).

## Architecture

Le projet utilise les **collections Jekyll** pour organiser les données :

- **`_groups/`** - Définitions des communautés tech (logo, description, réseaux)
- **`_confs/`** - Conférences annuelles (DevFest, PGDay, Capitole du Libre, etc.)
- **`_events/`** - Événements individuels (auto-générés et manuels)
- **`.github/`** - Configuration GitHub (templates, workflows, guides)

### Flux de Mise à Jour des Événements

1. **Job quotidien** (9h00 et 17h00 UTC)
2. **Script C#** (`.github/workflows/update.cs`) scan les pages Meetup
3. **Générer YAML** pour chaque nouvel événement
4. **Télécharger images** dans `event-imgs/`
5. **Jekyll build** génère l'HTML et les formats (iCal, JSON, Atom)

### Formats Générés

Le site produit plusieurs formats à partir des mêmes données :

- **HTML** - Page web avec calendrier Bootstrap Cards
- **iCal** - `events.ics` (compatible Google Cal, Apple Cal, Outlook)
- **Atom/RSS** - `events.atom.xml` (agrégateurs de flux)
- **JSON** - `events.json` (API)
- **PNG** - Outil organisateurs (`orgas.html`)

## Vue d'ensemble

- Source Jekyll (pages + templates) dans ce repo.
- Les données sont organisées en collections Jekyll : `_groups/`, `_confs/`, `_events/`.
- Les données d'événements sont dans `_events/` (fichiers `.html`) et `events-job.json`.
- Les images d'événements sont dans `event-imgs/`.
- Les sorties générées sont dans `_site/`.

## Installation de Jekyll (local)

### Prerequis (Ubuntu / WSL recommande)

Docs officielles Jekyll : https://jekyllrb.com/docs/ et https://jekyllrb.com/docs/installation/

```bash
sudo apt update
sudo apt install -y ruby-full build-essential zlib1g-dev

sudo gem install jekyll bundler

ruby -v
jekyll -v
```

### Lancer le site localement

```bash
jekyll serve
```

Puis ouvrir http://localhost:4000

## Lancer une mise a jour des donnees manuellement

Le job d'update est un script C# stocke dans `.github/workflows/update.cs`. Il detecte le repo racine via `events-job.json`.

### Execution locale (sans chargement reseau)

```bash
dotnet run .github/workflows/update.cs --no-load
```

### Execution locale (avec chargement des evenements)

```bash
dotnet run .github/workflows/update.cs
```

### Execution via GitHub Actions

Le workflow `Update Data` (fichier `.github/workflows/update-data.yml`) peut etre lance manuellement depuis l'onglet Actions, ou via son cron.

## Ajouter / modifier un evenement

### ⚠️ Politique de génération des fichiers YAML

Le job Update Data génère automatiquement des fichiers YAML pour les événements Meetup et Toulouse Game Dev. **Ces fichiers générés sont écrasés à chaque exécution du job.**

Pour créer un événement manuel qui **ne sera pas écrasé** :
- Utilisez un nom de fichier qui ne suit **pas** le pattern généré : `custom-{name}.html` ou `YYYY-MM-DD-custom-{name}.html`
- Le pattern généré est : `YYYY-MM-DD-{community-slug}-{event-id}.html` (ex: `2025-03-04-agile-meetup-305839478.html`)

Alternativement, si vous devez modifier un événement généré temporairement, utilisez le mécanisme `.skip` :
```bash
touch _events/2025-03-04-agile-meetup-305839478.html.skip
```
Cela empêchera la régénération du fichier, mais il faudra mettre à jour/nettoyer manuellement au prochain cycle.

### Créer un événement manuel

Créer un fichier HTML dans `_events/` avec le format suivant :

**Front matter (YAML entre `---` delimiters):**
```yaml
---
id: unique-id
title: 'Event Title'
community: Community Name
datePublished: YYYY-MM-DD HH:MM
dateIso: YYYY-MM-DD HH:MM
dateFr: jour DD mois
timeFr: 'HH:MM'
place: Venue Name
placeAddr: Address
link: https://example.com
img: https://example.com/image.jpg
localImg: event-imgs/unique-id.webp
---
```

**Contenu (HTML après `---`):**
```html
<p>Event description in HTML format</p>
```

## Bonnes pratiques

- Les fichiers doivent etre en UTF-8 (voir `.editorconfig`).
- Verifier `jekyll serve` en local apres une grosse modification.
- Ne pas committer `_site/`.
- **Événements manuels** : nommez les fichiers en dehors du pattern généré pour éviter qu'ils soient écrasés.
- **Appels du job** : relancer manuellement `dotnet run .github/workflows/update.cs` après le `jekyll serve` pour vérifier que la génération YAML fonctionne correctement.

## Reglages d'edition

Le fichier `.editorconfig` fixe les conventions d'edition : UTF-8, fins de ligne LF, 2 espaces par defaut, 4 espaces pour C#.
