# Toulouse Tech Hub

> Le calendrier unifié de tous les événements et communautés tech toulousaines 🚀

[![Site web](https://img.shields.io/badge/Site-toulouse--tech--hub.fr-blue)](https://toulouse-tech-hub.fr)
[![GitHub Issues](https://img.shields.io/github/issues/GDGToulouse/toulouse-tech-hub)](https://github.com/GDGToulouse/toulouse-tech-hub/issues)
[![Contributions bienvenues](https://img.shields.io/badge/contributions-bienvenues-brightgreen.svg)](https://github.com/GDGToulouse/toulouse-tech-hub/issues/new/choose)

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
