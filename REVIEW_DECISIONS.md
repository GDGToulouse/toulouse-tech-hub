# Copilot Review - Sujets Métier à Analyser

**Date** : 21/02/2026  
**Source** : Copilot PR Review #9 "Move data update job into repo"  
**Statut** : En attente de décision

---

## 1. Politique d'écrasement des fichiers YAML

**Issue identifiée**
- Le script **écrase** (overwrite) les fichiers YAML existants lors de chaque run, **ne supprime jamais** les fichiers.
- Si un événement généré (`YYYY-MM-DD-{GroupId}-{Id}.yml`) est modifié manuellement (ajout de contenu), ces modifications seront perdues au prochain run.
- Conflictuel avec la capacité décrite dans la doc de créer/modifier des événements manuellement.

**Contexte actuel**
- Fichiers générés: `YYYY-MM-DD-{GroupId}-{Id}.yml` (écrasés à chaque run, contenus manuels perdus)
- Fichiers manuels: tout fichier YAML sortant du pattern de génération sera conservé (jamais supprimé)
- Mécanisme existant `.skip` permet d'ignorer un événement spécifique, pas de préserver les modifications manuelles.
- Aucun marquage pour distinguer "généré automatiquement" vs "créé manuellement".

**Options**
- [ ] **A** : Accepter que tous les fichiers soient générés/régénérés (événements manuels non supportés)
- [ ] **B** : Créer un répertoire séparé pour les événements manuels (`_data/events-custom/`)
- [ ] **C** : Ajouter un champ de métadonnées (ex: `source: manual|automated`) pour détecter les fichiers manuels
- [ ] **D** : Documenter que les événements manuels doivent être regénérés après chaque run du job

**Recommandation**  
À discuter avec toi pour décider la stratégie d'édition des événements.

---

## 2. Convention de nommage YAML : `{community}` = quoi exactement ?

**Issue identifiée**
- La doc dit : `YYYY-MM-DD-{community}-{identifier}.yml`
- Le code génère : `YYYY-MM-DD-{evt.GroupId}-{evt.Id}.yml` (ex: `2025-03-04-agile-meetup-305839478.yml`)
- `GroupId` = slug court ("agile", "jug", "dataviz"), pas le nom lisible ("Agile Toulouse", "Toulouse Java User Group", etc.)

**Contexte actuel**
- Nommage actuel (slug) est plus compact et lisible en tant que chemin de fichier.
- Documentation insuffisante/ambiguë sur le terme `{community}`.

**Options**
- [x] **A** : Clarifier la doc : `{community}` = slug court (GroupId) — ✅ **DÉCIDÉ**
- [ ] **B** : Changer le code pour utiliser le nom complet (implique renomer tous les fichiers existants)

**Décision**  
**Option A** est retenue pour éviter confusion continuelle entre "community" (terme métier) et "group" (terme code). La doc sera clarifiée pour indiquer que `{community}` dans le pattern de nommage = slug court du groupe (ex: "agile", "jug", "dataviz").

---

## 3. Fréquence du job : 9h/17h UTC vs cadence plus élevée

**Issue identifiée**
- Job ancien : "run every 4 hours" (hypothèse basée sur la doc initiale)
- Job actuel : 9h et 17h UTC seulement (2 runs/jour)
- Demande : aligner sur le réel besoin opérationnel

**Contexte actuel**
```yaml
schedule:
  - cron: "0 9,17 * * *"  # 9am and 5pm UTC
```

**Questions**
- [ ] Fréquence actuelle (2x/jour) est-elle suffisante pour les événements Meetup ?
- [ ] Besoin d'une cadence plus élevée (ex: 4h, 6h) ?
- [ ] Faut-il s'aligner sur le cron de build Jekyll (4am UTC) ?

**Recommandation**  
À discuter avec l'équipe pour valider le besoin en fraîcheur des données d'événements.

---

## 4. Stratégie d'alertes pour les HACKs de parsing

**Issue identifiée**
- Code contient 2 workarounds pour Toulouse Game Dev :
  - `Replace("Mars2025", "Mars 2025")` : correction d'espaces manquants
  - `Replace("2024", "2025")` : correction d'années incorrectes
- Ces hacks masquent les problèmes de qualité des données sources.
- Pas de logging/alerting quand ces hacks s'appliquent.

**Contexte actuel**
```csharp
// HACK: Fix source data formatting issues in Toulouse Game Dev website
text = text.Replace("Mars2025", "Mars 2025");

// HACK: Fix incorrect year in source website
datePart = datePart.Replace("2024", "2025");
```

**Options**
- [ ] **A** : Ajouter des logs/warnings quand les hacks s'appliquent pour tracker la fréquence
- [ ] **B** : Créer des issues GitHub pour signaler au ToulouseGameDev de corriger leurs données
- [ ] **C** : Prendre manuellement contact avec l'équipe Toulouse Game Dev
- [ ] **D** : Ignorer (les hacks fonctionnent, pas besoin d'alertes)

**Recommandation**  
Option A + B : logger quand ça se déclenche, créer des issues pour pousser à la correction source.

---

## 5. Gestion des mises à jour d'images

**Issue identifiée**
- Le code télécharge les images uniquement si le fichier local n'existe pas.
- Si une image Meetup change côté source, la version locale ne sera jamais mise à jour.
- Mode "write-once" des images : pas de refresh automatique.

**Contexte actuel**
```csharp
if (!File.Exists(imgPath) && (evt.FullImgSrc != null || evt.ImgSrc != null))
{
    // Télécharge seulement si n'existe pas
    Directory.CreateDirectory(Path.GetDirectoryName(imgPath)!);
    // ...
}
```

**Options**
- [ ] **A** : Garder le mode "write-once" (images téléchargées une seule fois)
- [ ] **B** : Vérifier le "last-modified" et télécharger si la version source a changé
- [ ] **C** : Réinitialiser périodiquement le cache d'images (ex: 1x/mois)
- [ ] **D** : Ajouter un drapeau `--refresh-images` pour forcer la rédownload

**Recommandation**  
À décider : besoin réel de rafraîchir les images ou write-once suffisant ?

---

## 6. Clarification : Nommage du job dans le workflow

**Status** : ✅ CORRIGÉ dans PR #9  
Le job `build` a été renommé en `update-data` pour clarté.

---

## 7. Clarification : Gestion des erreurs git commit/push

**Status** : ✅ CORRIGÉ dans PR #9  
Les commandes `git commit` et `git push` affichent maintenant des messages d'erreur explicites en cas d'échec.

---

## Validation des décisions

**À faire**
1. Revue de cette liste avec l'équipe
2. Trancher sur chaque point
3. Implémenter les éventuels changements

**Priorité suggérée**
1. **Haute** : #2 (clarifier nommage doc) - simple ✓
2. **Haute** : #1 (écrasement YAML) - impact élevé
3. **Moyenne** : #4 (alertes hacks) - observabilité
4. **Basse** : #3 (fréquence job) - validation besoins
5. **Basse** : #5 (images) - cas d'usage rare

---

*Fin de liste - à compléter après décisions*
