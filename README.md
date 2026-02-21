# Toulouse Tech Hub

> Le calendrier unifié de tous les événements et communautés tech toulousaines 🚀

[![Site web](https://img.shields.io/badge/Site-toulouse--tech--hub.fr-blue)](https://toulouse-tech-hub.fr)
[![GitHub Issues](https://img.shields.io/github/issues/GDGToulouse/toulouse-tech-hub)](https://github.com/GDGToulouse/toulouse-tech-hub/issues)
[![Contributions bienvenues](https://img.shields.io/badge/contributions-bienvenues-brightgreen.svg)](https://github.com/GDGToulouse/toulouse-tech-hub/issues/new/choose)

## 📚 Table des matières

- [À propos](#-à-propos)
- [Communautés suivies](#-communautés-suivies)
- [Comment contribuer ?](#-comment-contribuer-)
- [Tech Stack](#-tech-stack)
- [Architecture](#-architecture)
- [Développement local](#-développement-local)

## 📅 À propos

Ce site liste tous les événements tech à venir organisés par les communautés toulousaines, en un seul endroit.

**Comment ça marche ?** Les événements Meetup sont automatiquement synchronisés plusieurs fois par jour. Les événements sur d'autres plateformes peuvent être ajoutés manuellement via une simple issue GitHub.

**Pour les organisateurs :** Une [page dédiée](https://toulouse-tech-hub.fr/orgas.html) permet de générer facilement une image avec les prochains événements, parfaite pour vos slides de présentation !

## 🎯 Communautés suivies


- [Agile Toulouse](https://www.agiletoulouse.fr/)
- [Artilect FabLab](https://www.meetup.com/artilect-fablab/)
- [AWS User Group Toulouse](https://www.meetup.com/toulouse-amazon-web-services/)
- [C++ Toulouse](https://www.meetup.com/ateliers-cpp-toulouse/)
- [Devops & Cloud Toulouse](https://www.meetup.com/devops-cloud-toulouse/)
- [GDG Toulouse](https://www.gdgtoulouse.fr/)
- [JS & Co Toulouse](https://www.meetup.com/javascript-and-co/)
- [JUG Toulouse](https://www.meetup.com/toulouse-java-user-group/)
- [La "Toul Box" du Cloud Natif](https://www.meetup.com/latoulboxducloudnatif/)
- [MTG:Toulouse](https://www.meetup.com/mtg-toulouse/)
- [Postgres Toulouse](https://www.meetup.com/postgres-toulouse)
- [Python Toulouse](https://www.meetup.com/python-toulouse/)
- [Rust Toulouse](https://www.meetup.com/fr-FR/rust-community-toulouse/)
- [Swift Toulouse](https://www.meetup.com/swift-toulouse/)
- [Tech a Break](https://www.meetup.com/tech-a-break/)
- [Toulouse Data Science](https://www.meetup.com/tlse-data-science/)
- [Toulouse Data-Viz](https://www.meetup.com/meetup-visualisation-des-donnees-toulouse/)
- [Toulouse DevOps](https://www.meetup.com/toulouse-devops/)
- [Toulouse Game Dev](https://toulousegamedev.fr/)
- [Toulouse Ruby and Friends](https://www.meetup.com/toulouse-ruby-friends/)

Il existe une page pour les organisateurs, permettant de gérer une image à inclure dans vos diapos de meetup pour faire la pub pour les prochains évènements : <https://toulouse-tech-hub.fr/orgas.html>

## 🤝 Comment contribuer ?

### Ajouter une communauté

Votre communauté tech toulousaine n'est pas encore listée ? 

👉 [Créez une issue "Ajouter une communauté"](https://github.com/GDGToulouse/toulouse-tech-hub/issues/new?template=add-community.yml)

Remplissez simplement le formulaire avec les informations de votre communauté (nom, site web, description). Si vous avez une page Meetup, les événements seront automatiquement synchronisés !

### Ajouter une conférence

Vous organisez une conférence tech annuelle ou régulière à Toulouse ?

👉 [Créez une issue "Ajouter une conférence"](https://github.com/GDGToulouse/toulouse-tech-hub/issues/new?template=add-conference.yml)

Les conférences apparaissent dans la section spéciale des grands événements annuels (DevFest, PGDay, Capitole du Libre, etc.).

### Ajouter un événement ponctuel

Vous organisez un événement tech qui n'est pas sur Meetup ou qui nécessite une annonce spéciale ?

👉 [Créez une issue "Ajouter un événement"](https://github.com/GDGToulouse/toulouse-tech-hub/issues/new?template=add-event.yml)

*Note : Les événements Meetup des communautés listées sont déjà synchronisés automatiquement, pas besoin de les ajouter manuellement.*

### Signaler un problème

Vous avez remarqué une erreur (événement manquant, lien cassé, information incorrecte) ?

👉 [Créez une issue "Signaler un bug"](https://github.com/GDGToulouse/toulouse-tech-hub/issues/new?template=bug-report.yml)

### Proposer une amélioration

Vous avez une idée pour améliorer le site ?

👉 [Créez une issue "Suggestion d'amélioration"](https://github.com/GDGToulouse/toulouse-tech-hub/issues/new?template=feature-request.yml)

### Contribuer directement au code

Les événements et communautés sont de simples fichiers dans les dossiers `_events/` et `_groups/`. Vous pouvez également proposer vos modifications via pull request directement !

## 🛠️ Développement local

Ce site est généré avec Jekyll. Pour le tester localement :

```bash
# Installer Jekyll (si nécessaire)
gem install jekyll bundler

# Lancer le serveur local
jekyll serve

# Le site est accessible sur http://localhost:4000
```

## 🛠️ Tech Stack

- **[Jekyll 4.4](https://jekyllrb.com/)** - Static site generator
- **[Liquid](https://shopify.github.io/liquid/)** - Templating engine
- **[Bootstrap 5](https://getbootstrap.com/)** - UI framework
- **[Bootstrap Icons](https://icons.getbootstrap.com/)** - Icon library
- **[GitHub Actions](https://github.com/features/actions)** - CI/CD automation
- **[GitHub Pages](https://pages.github.com/)** - Hosting

## 📁 Architecture

Le projet utilise les **collections Jekyll** pour organiser les données :

- **`_groups/`** (20 fichiers) - Définitions des communautés tech
  - Un fichier `.md` par communauté avec logo, description et réseaux sociaux
  - Images dans `groups-imgs/{slug}.jpg`

- **`_confs/`** (5 fichiers) - Conférences annuelles (DevFest, PGDay, etc.)
  - Un fichier `.md` par conférence avec dates et liens
  - Images dans `confs-imgs/{slug}.jpg`

- **`_events/`** (136+ fichiers) - Événements individuels
  - Fichiers `.html` générés automatiquement par le job d'update
  - Nommage : `YYYY-MM-DD-{community-slug}-{event-id}.html`
  - Images dans `event-imgs/`

- **`.github/`** - Configuration GitHub
  - `ISSUE_TEMPLATE/` - Templates d'issues pour les contributions
  - `COPILOT_*.md` - Guides utilisables par Copilot pour traiter les issues
  - `workflows/` - Workflows GitHub Actions

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
