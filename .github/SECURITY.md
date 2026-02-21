# Politique de Sécurité

## Signaler une Vulnérabilité

La sécurité de Toulouse Tech Hub est importante pour nous. Si tu as découvert une vulnérabilité de sécurité, nous apprécierions que tu nous le fasses savoir de manière responsable.

### Comment Signaler

**NE PAS** créer d'issue GitHub publique pour les vulnérabilités de sécurité !

À la place :

1. **Envoie un email** à l'équipe du projet avec les détails (voir les contacts du projet)
2. **Ou utilise GitHub Security Advisory** :
   - Va sur : https://github.com/GDGToulouse/toulouse-tech-hub/security/advisories
   - Clique sur "Report a vulnerability"
   - Remplis le formulaire avec les détails

### Informations à Inclure

Aide-nous à comprendre la vulnérabilité en fournissant :

- **Description** : Qu'est-ce que la vulnérabilité ?
- **Étapes de reproduction** : Comment la reproduire ?
- **Composant affecté** : Quel fichier ou fonctionnalité est concernée ?
- **Sévérité** : Critique, haute, moyenne, basse ?
- **Preuve de concept** : Code ou configuration de test si possible
- **Correctifs suggérés** : Avez-vous une idée de solution ?

### Délai de Réponse

- **Accusé de réception** : Dans les 24-48 heures
- **Évaluation initiale** : Dans la semaine
- **Correction et divulgation coordonnée** : Selon la sévérité

## Points de Sécurité Connus

### À Rester Vigilant

Bien que ce soit un projet statique Jekyll simple, restez attentif à :

- **Dépendances** : Mises à jour de Jekyll et plugins
- **Données** : Les fichiers YAML contiennent des URLs externes
- **GitHub Actions** : Les workflows qui téléchargent des données externes
- **Secrets** : Ne commitez jamais de clés, tokens ou credentials

### Pratiques Recommandées

- Utiliser HTTPS pour tous les liens externes
- Valider les URLs avant de les ajouter aux données
- Garder les versions de Jekyll à jour
- Utiliser SSH keys pour les commits

## Signalement Responsable

Nous pratiquons la **divulgation coordonnée** :

1. Tu nous signales la vulnérabilité en privé
2. Nous travaillons sur un correctif
3. Nous testons le correctif
4. Nous publions une version de sécurité
5. Après la publication, la vulnérabilité peut être divulguée publiquement

Nous te remercierons pour ta contribution en te crédits dans le security advisory.

## Questions

Pour toute question sur cette politique de sécurité, ouvre une discussion GitHub ou contacte l'équipe du projet.
