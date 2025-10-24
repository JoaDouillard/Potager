# 📋 BRIEF PROJET UNITY - POTAGER SOLARPUNK (VERSION DRONE)

## 🎮 CONTEXTE DU PROJET

**Projet en cours :** Adaptation du "Potager Solarpunk" avec un système de drone contrôlable

**État actuel :**
- ✅ Système de caméra première/troisième personne implémenté
- ✅ Contrôle du drone fonctionnel
- ✅ Génération procédurale de map (nouveau à chaque partie)
- ⏳ Reste à implémenter : Tout le gameplay et les systèmes de jeu

**Niveau :** Débutant
**Durée estimée :** 6-8 heures

---

## 🎯 OBJECTIF DU JEU

Créer un jeu de gestion de potager en 3D où le joueur pilote un **drone horticole** chargé d'entretenir un jardin. Le drone doit planter des graines, les arroser pour les faire pousser, et récolter les légumes mûrs avant qu'ils n'explosent.

---

## 🕹️ MÉCANIQUES DE JEU

### **DRONE (Contrôlé par le joueur)**
- ✅ Se déplacer dans les 4 directions (ZQSD/Flèches)
- ✅ Voler au-dessus du sol (pas de contact direct)
- ⏳ **Planter des graines** sous le drone (Input Action)
- ⏳ **Arroser** les graines/légumes sous le drone (Input Action)
- ⏳ **Récolter** les légumes mûrs en passant dessus (Trigger automatique)

### **GRAINES**
- Apparaissent quand le drone plante
- Grossissent progressivement avec le temps
- Se transforment en légumes après X secondes

### **LÉGUMES**
- Apparaissent après maturation des graines
- Continuent de grossir avec le temps
- **Explosent** si non récoltés à temps (trop mûrs)
- Donnent des points quand récoltés

---

## 🛠️ SYSTÈMES À IMPLÉMENTER

### **1. SYSTÈME DE PLANTATION**
```
- Input Action "Planter" (Button)
- Instancier un prefab "Graine" sous le drone
- Position = sous le drone (attention à la hauteur)
- Cooldown optionnel pour éviter le spam
```

### **2. SYSTÈME D'ARROSAGE**
```
- Input Action "Arroser" (Button)
- Détecter s'il y a une graine/légume sous le drone (Raycast ou Overlap)
- Instancier des particules d'eau
- Accélérer la croissance de la plante ciblée
```

### **3. SYSTÈME DE CROISSANCE**
```
- Coroutine pour transformer Graine → Légume (après X sec)
- Animation/Script pour faire grossir progressivement (Scale ou Animation)
- Changement de couleur optionnel (vert → rouge/mûr)
- Timer avant explosion si non récolté
```

### **4. SYSTÈME DE RÉCOLTE**
```
- Collider isTrigger sur les légumes (assez haut pour détecter le drone)
- OnTriggerEnter : détecter le drone
- Détruire le légume
- Ajouter des points au score
- Effet sonore + particules (optionnel)
```

### **5. SYSTÈME DE SCORE & UI**
```
- TextMeshPro affichant le score actuel
- Score += points à chaque récolte
- Bonus : différents légumes = différents points
- Écran de fin quand objectif atteint
```

### **6. SYSTÈME DE TEMPS/CYCLES**
```
- Time.deltaTime pour la croissance progressive
- Coroutines pour les transformations (graine→légume, légume→explosion)
- Timer visible optionnel pour chaque plante
```

---

## 📦 PREFABS À CRÉER

| Prefab | Composants nécessaires | Notes |
|--------|------------------------|-------|
| **Graine** | - Model 3D<br>- Collider<br>- Script "Graine" | Petit, au sol |
| **Légume** | - Model 3D<br>- Collider (isTrigger)<br>- Script "Legume" | Trigger assez haut pour le drone |
| **Particules Eau** | - Particle System | Instancié temporairement |
| **Explosion** | - Particle System<br>- Audio Source | Destruction après effet |

---

## 🎨 ANIMATIONS & EFFETS

### **Animations à créer :**
- Croissance des graines (Scale 0.1 → 1.0)
- Croissance des légumes (Scale 1.0 → 2.0)
- Explosion des légumes trop mûrs
- Nuages qui bougent en fond (décor)
- Balancement de buissons (optionnel)

### **Effets visuels :**
- Particules d'eau tombant du drone
- Particules de graines qui tombent (optionnel)
- Particules d'explosion
- Changement de couleur des légumes (maturation)

### **Audio :**
- Son de plantation
- Son d'arrosage (eau qui coule)
- Son de récolte
- Son d'explosion
- Musique de fond apaisante

---

## 🎯 ÉTAPES PRIORITAIRES (Dans l'ordre)

### ✅ **DÉJÀ FAIT**
1. Configuration de la scène
2. Caméra 1ère/3ème personne
3. Contrôles du drone
4. Génération procédurale de map

### ⏳ **À FAIRE MAINTENANT**

**Phase 1 - Système de base (PRIORITÉ MAX)**
```
5. Créer les prefabs (Graine, Légume)
6. Implémenter le système de plantation (Input + Instantiate)
7. Implémenter la croissance des graines (Coroutine + Scale)
8. Implémenter la transformation Graine → Légume
```

**Phase 2 - Interactions**
```
9. Système de récolte (OnTriggerEnter)
10. Système de score (UI + incrémentation)
11. Système d'explosion (Timer + Destroy)
```

**Phase 3 - Arrosage (OPTIONNEL mais recommandé)**
```
12. Input arrosage
13. Détection de plante sous le drone
14. Particules d'eau
15. Accélération de croissance
```

**Phase 4 - Polish**
```
16. Effets sonores
17. Particules diverses
18. Animations de décor
19. Écran titre
20. Menu pause
21. Écran de fin
```

---

## 💻 SCRIPTS PRINCIPAUX À CRÉER

### **1. DroneController.cs** (Déjà partiellement fait)
```csharp
// Ajouter :
- Input Action pour Planter
- Input Action pour Arroser
- Fonction PlanterGraine()
- Fonction Arroser()
```

### **2. Graine.cs**
```csharp
- Timer de croissance
- Scale progressif
- Coroutine pour transformation en Légume
- Instanciation du prefab Légume
- Destruction de la graine
```

### **3. Legume.cs**
```csharp
- Timer avant explosion
- Scale progressif
- Changement de couleur (optionnel)
- Fonction Exploser()
- OnTriggerEnter (détection drone)
```

### **4. ScoreManager.cs** (Singleton)
```csharp
- int score actuel
- Fonction AjouterPoints(int points)
- Mise à jour UI
```

### **5. UIManager.cs**
```csharp
- Référence TextMeshPro score
- Fonction MettreAJourScore()
- Écran de fin (optionnel)
```

---

## 🚨 POINTS D'ATTENTION IMPORTANTS

### **❗ Positionnement des objets**
- Le drone plante/arrose **EN DESSOUS** (pas devant)
- Utiliser `transform.position + Vector3.down * offset` pour instancier
- Ne pas gérer la rotation pour l'instant (trop complexe)

### **❗ Détection du drone par les légumes**
- Le Trigger des légumes doit être **assez haut** (le drone vole)
- Augmenter la hauteur du BoxCollider ou utiliser un Trigger séparé

### **❗ Gestion du temps**
- Utiliser `Time.deltaTime` pour les croissances progressives
- Utiliser `Coroutines` pour les transformations temporisées
- Attention aux fuites mémoire (StopCoroutine si objet détruit)

### **❗ Limites du terrain**
- Empêcher le drone de sortir de la map générée
- Soit avec des murs invisibles (BoxCollider)
- Soit en clampant la position dans le script

## **Information importante**
# **❗ METTRE A JOUR LE CLAUDE.MD A CHAQUE MODIFICATION POUR AVOIR UN SUIVIS DE PROJET **

## **INTERDICTION**

### Ne pas faire des commantaire dans ce type de format
///<summery>
/// Le commentaire
///<summery>

---

## 🌅 SYSTÈME SKYBOX - DayNightSkyboxManager

**Fichier :** `Assets/Scrypt/Evironnement/Sky/DayNightSkyboxManager.cs`

**Fonctionnalités :**
- 5 skyboxes : Matin / Midi / Après-midi / Soirée / Nuit
- Heures configurables par sliders
- Soleil et lune tournent automatiquement
- Étoiles (particules) la nuit
- Rotation nuages constante

**Configuration :**

1. **Soleil & Lune** : Créer 2 Directional Lights → Les assigner (ils tourneront automatiquement autour de la scène)

2. **Étoiles** : Créer Particle System
   - Position : Au centre de la scène (0, 0, 0)
   - Shape : **Sphere**
   - Radius : **200-300** (grand pour couvrir tout le ciel)
   - Start Lifetime : 999 (pour qu'elles restent affichées)
   - Start Speed : 0
   - Start Size : 0.05-0.1
   - Simulation Space : **World** (important !)
   - Max Particles : 500-1000

3. Assigner 5 skyboxes dans l'Inspector

4. Configurer Gradient "Couleur Soleil" et Courbe "Intensité Soleil"

**Heures par défaut :**
- Matin : 6h-10h
- Midi : 10h-16h
- Après-midi : 16h-17h30
- Soirée : 17h30-19h
- Nuit : 19h-6h

**Positions soleil/lune :**
- 12h (midi) : Soleil au zénith (point le plus haut)
- 0h (minuit) : Lune au zénith (point le plus haut)
- Le soleil et la lune tournent en fonction de `heureActuelle`, pas du temps réel
- Distance : 1000 unités du centre pour simuler l'infini

**Si vous ne voyez pas le soleil/lune :**
- Vérifiez que les Directional Lights ne sont PAS enfants d'un autre objet
- Le script doit pouvoir modifier leur Transform librement
- Regardez dans la Scene view en mode jeu, ils doivent se déplacer

---

## 🎁 BONUS / DÉFIS OPTIONNELS

- [ ] Power-ups (engrais, outils spéciaux)
- [ ] Nuages qui arrosent automatiquement (mais pas où on veut)
- [ ] Différents types de légumes (vitesse croissance, points différents)
- [ ] Système de saisons
- [ ] High scores sauvegardés (PlayerPrefs)
- [ ] Synergies entre plantes
- [ ] Système de rotation du drone (pour planter devant)
- [ ] Mini-map de la zone

---

## 📚 RESSOURCES FOURNIES

**Documentation Unity :**
- Prefabs : https://docs.unity3d.com/Manual/Prefabs.html
- Instantiation : https://docs.unity3d.com/Manual/instantiating-prefabs.html
- UI : https://docs.unity3d.com/Manual/UISystem.html

**Assets gratuits :**
- Food Pack : https://www.kenney.nl/assets/food-kit
- Particle Pack : https://www.kenney.nl/assets/particle-pack
- 3D Plants : https://www.fab.com/listings/fd545301-e55f-4705-a777-d62fbe39e72d
- 3D Crops : https://www.fab.com/listings/be6d6432-9386-4544-a34f-80c82f8d1e06
- Drones : https://www.fab.com/search?q=drone&asset_formats=fbx&is_free=1

---

## ✅ CHECKLIST DE VALIDATION

**Fonctionnalités obligatoires :**
- [ ] Le drone se déplace correctement
- [ ] Les graines apparaissent quand on plante
- [ ] Les graines deviennent des légumes
- [ ] Les légumes grossissent visuellement
- [ ] Les légumes peuvent être récoltés en passant dessus
- [ ] Les légumes explosent s'ils sont trop mûrs
- [ ] Le score s'affiche et augmente à la récolte
- [ ] Au moins 1 effet visuel (particules ou animation)
- [ ] Au moins 1 effet sonore

**Polish (optionnel mais recommandé) :**
- [ ] Système d'arrosage fonctionnel
- [ ] Particules d'eau
- [ ] Animations de décor
- [ ] Menu pause
- [ ] Écran de fin

---

## 🚀 PRÊT À COMMENCER ?

**Question pour l'IA d'assistance :**
> "Je travaille sur un projet Unity de drone horticole. J'ai déjà implémenté les contrôles du drone et la génération procédurale de map. Je dois maintenant créer le système de plantation de graines. Peux-tu m'aider à créer un script pour instancier un prefab de graine sous le drone quand j'appuie sur un bouton avec le nouveau Input System ?"

**Commence par la Phase 1 - Étape 5 : Création des prefabs !** 🌱