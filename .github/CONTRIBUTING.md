# Contribuer au projet

Merci de contribuer à Toulouse Tech Hub. Ce projet est un site Jekyll qui agrège les événements tech de Toulouse et publie plusieurs formats (site web, JSON, Atom, iCal).

## Environnement de développement sous Windows

Sous Windows, le workflow recommandé est **WSL (Ubuntu)**.  
**Jekyll doit être installé et exécuté dans WSL, pas dans PowerShell Windows.**

Pourquoi : beaucoup d'outils d'agents (dont Copilot CLI en session locale Windows) lancent des commandes côté Windows par défaut. Si Jekyll n'est installé que dans WSL, `jekyll serve` échoue côté Windows avec "commande introuvable".

Si vous travaillez avec un agent, demandez explicitement d'exécuter les commandes Jekyll via WSL, par exemple :

```powershell
wsl bash -lc "cd /mnt/d/code/perso/GDGToulouse/toulouse-tech-hub && jekyll build"
wsl bash -lc "cd /mnt/d/code/perso/GDGToulouse/toulouse-tech-hub && jekyll serve --livereload"
```

## Installation locale (WSL Ubuntu)

Docs officielles Jekyll : https://jekyllrb.com/docs/ et https://jekyllrb.com/docs/installation/

```bash
sudo apt update
sudo apt install -y ruby-full build-essential zlib1g-dev

gem install jekyll bundler

ruby -v
jekyll -v
```

## Lancer le site en local

Depuis un terminal WSL dans le repo :

```bash
jekyll serve
```

Puis ouvrir http://localhost:4000

## Architecture

Le projet utilise les collections Jekyll :

- **`_groups/`** - Définitions des communautés
- **`_confs/`** - Conférences annuelles
- **`_events/`** - Événements individuels (auto-générés et manuels)
- **`.github/`** - Templates, workflows et guides

### Flux de mise à jour des événements

1. Job planifié (9h00 et 17h00 UTC)
2. Script C# (`.github/workflows/update.cs`) qui lit les sources d'événements
3. Génération/maj des fichiers `_events/`
4. Téléchargement des images dans `event-imgs/`
5. Génération Jekyll des sorties (HTML, iCal, Atom, JSON)

### Formats générés

- **HTML** : page principale
- **iCal** : `events.ics`
- **Atom/RSS** : `events.atom.xml`
- **JSON** : `events.json`
- **PNG** : outil organisateurs (`orgas.html`)

## Lancer la mise à jour des données manuellement

Le job d'update est le script C# `.github/workflows/update.cs`.

```bash
# Sans chargement réseau
dotnet run .github/workflows/update.cs --no-load

# Avec chargement des événements
dotnet run .github/workflows/update.cs
```

Le workflow GitHub correspondant est `.github/workflows/update-data.yml`.

## Ajouter ou modifier un événement

### Politique de génération

Le job Update Data génère automatiquement des événements (notamment Meetup et Toulouse Game Dev).  
Ces fichiers peuvent être écrasés à la prochaine exécution.

Pour désactiver la régénération d'un événement auto-généré, ajoutez un fichier `.skip` adjacent :

```bash
touch _events/2025-03-04-agile-meetup-305839478.html.skip
# ou
touch _events/2025-03-04-agile-meetup-305839478.md.skip
```

### Créer un événement manuel

Nom de fichier recommandé :

`_events/{YYYY-MM-DD}-{groupId}-{eventId}.html` (ou `.md`)

Exemple de front matter :

```yaml
---
eventId: "manual-1709740200"
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

Puis le contenu HTML après le front matter :

```html
<p>Event description in HTML format</p>
```

Important :

- Utiliser `eventId` (pas `id`)
- Le segment `eventId` du nom de fichier doit correspondre exactement au front matter
- `place` et `placeAddr` doivent apparaître ensemble (ou être tous les deux vides)
- L'image locale est requise pour les feeds : `event-imgs/{YYYY-MM-DD}-{groupId}-{eventId}.webp`

### Données structurées SEO des évènements

La page d'accueil publie aussi des données structurées `schema.org/Event` au format JSON-LD pour les évènements à venir.

- Le balisage est injecté directement dans le HTML via `<script type="application/ld+json">`.
- Il réutilise le même filtrage que l'agenda : évènements futurs uniquement, avec prise en compte des fichiers `.skip`.
- Les champs `title`, `link`, `dateIso`, `dateIsoEnd`, `community`, `place`, `placeAddr` et le contenu HTML de l'évènement servent de source au JSON-LD.
- Le champ `image` du JSON-LD pointe vers l'image locale `event-imgs/{nom-du-fichier-evenement}.webp`, comme les autres sorties du site.
- Si `dateIsoEnd` est absent, une heure de fin par défaut est dérivée comme pour le flux iCal.
- La description publiée dans le JSON-LD est un extrait tronqué du contenu HTML de l'évènement, pour limiter le poids de la page.
- Si `place` est vide, l'évènement est publié comme évènement en ligne avec une `VirtualLocation`.

## Bonnes pratiques

- Fichiers en UTF-8 (voir `.editorconfig`)
- Vérifier localement les changements Jekyll
- Ne pas committer `_site/`

## Réglages d'édition

`.editorconfig` fixe les conventions : UTF-8, fins de ligne LF, 2 espaces par défaut, 4 espaces pour C#.
