# Contribuer au projet

Merci de contribuer a Toulouse Tech Hub ! Ce projet est un site Jekyll qui agreges les evenements tech de Toulouse et publie plusieurs formats (site web, JSON, Atom, iCal).

## Vue d'ensemble

- Source Jekyll (pages + templates) dans ce repo.
- Les donnees d'evenements sont dans `_data/events/` et `events-job.json`.
- Les images d'evenements sont dans `event-imgs/`.
- Les sorties generees sont dans `_site/`.

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

Pour un ajout manuel, creer un fichier YAML dans `_data/events/` avec le format suivant :

```yaml
id: 'unique-id'
title: 'Event Title'
community: 'Community Name'
datePublished: 'YYYY-MM-DD HH:MM'
dateIso: 'YYYY-MM-DD HH:MM'
dateFr: 'jour DD mois'
timeFr: 'HH:MM'
place: "Venue Name"
placeAddr: "Address"
link: https://example.com
img: https://example.com/image.jpg
localImg: event-imgs/unique-id.webp
description: >
  HTML description
```

## Bonnes pratiques

- Les fichiers doivent etre en UTF-8 (voir `.editorconfig`).
- Verifier `jekyll serve` en local apres une grosse modification.
- Ne pas committer `_site/`.

## Reglages d'edition

Le fichier `.editorconfig` fixe les conventions d'edition : UTF-8, fins de ligne LF, 2 espaces par defaut, 4 espaces pour C#.
