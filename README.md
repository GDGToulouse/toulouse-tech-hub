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

Le projet utilise des **templates d'issues GitHub** pour simplifier les contributions. Consultez [CONTRIBUTING.md](.github/CONTRIBUTING.md) pour les détails.

- **Ajouter une communauté** : [Créer une issue](https://github.com/GDGToulouse/toulouse-tech-hub/issues/new?template=add-community.yml) - Les événements Meetup se synchronisent automatiquement !
- **Ajouter une conférence** : [Créer une issue](https://github.com/GDGToulouse/toulouse-tech-hub/issues/new?template=add-conference.yml)
- **Ajouter un événement** : [Créer une issue](https://github.com/GDGToulouse/toulouse-tech-hub/issues/new?template=add-event.yml) - Ou directement via PR
- **Signaler un bug** : [Créer une issue](https://github.com/GDGToulouse/toulouse-tech-hub/issues/new?template=bug-report.yml)
- **Proposer une amélioration** : [Créer une issue](https://github.com/GDGToulouse/toulouse-tech-hub/issues/new?template=feature-request.yml)

**Note :** Les fichiers `_groups/`, `_confs/`, et `_events/` sont de simples fichiers markdown/HTML - vous pouvez aussi proposer directement une PR !

## 🛠️ Tech Stack

- **[Jekyll 4.4](https://jekyllrb.com/)** - Static site generator
- **[Liquid](https://shopify.github.io/liquid/)** - Templating engine
- **[Bootstrap 5](https://getbootstrap.com/)** - UI framework
- **[GitHub Actions](https://github.com/features/actions)** - CI/CD automation
- **[GitHub Pages](https://pages.github.com/)** - Hosting

## 📁 Architecture

Le projet utilise les **collections Jekyll** pour organiser les données :

- **`_groups/`** - Définitions des communautés tech (logo, description, réseaux)
- **`_confs/`** - Conférences annuelles (DevFest, PGDay, Capitole du Libre, etc.)
- **`_events/`** - Événements individuels (auto-générés et manuels)
- **`.github/`** - Configuration GitHub (templates, workflows, guides)

Pour plus de détails sur l'architecture, l'update workflow et les formats générés, consulte [CONTRIBUTING.md](.github/CONTRIBUTING.md).

## 🚀 Développement local

Vois [CONTRIBUTING.md](.github/CONTRIBUTING.md) pour les instructions d'installation de Jekyll et de lancement local.
