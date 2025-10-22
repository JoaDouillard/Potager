using UnityEngine;

// Spawner ameliore de nuages avec support pour les nuages animes
// Integre avec le systeme de courbure et perspective
public class CloudSpawner : MonoBehaviour
{
    [Header("Prefabs de nuages")]
    [Tooltip("Liste des prefabs de nuages a spawner")]
    public GameObject[] nuagePrefabs;

    [Header("Configuration du Spawn")]
    [Tooltip("Nombre de nuages a generer")]
    public int nombreNuages = 50;

    [Tooltip("Taille de la zone de spawn (X et Z)")]
    public Vector2 tailleZone = new Vector2(800, 800);

    [Tooltip("Altitude minimale des nuages")]
    public float hauteurMin = 80f;

    [Tooltip("Altitude maximale des nuages")]
    public float hauteurMax = 200f;

    [Header("Variation de taille")]
    [Tooltip("Scale minimum des nuages")]
    public float scaleMin = 1f;

    [Tooltip("Scale maximum des nuages")]
    public float scaleMax = 3f;

    [Tooltip("Appliquer variation selon la distance (plus gros quand proche)")]
    public bool variationSelonDistance = true;

    [Header("Mouvement des nuages")]
    [Tooltip("Activer le mouvement automatique des nuages")]
    public bool activerMouvement = true;

    [Tooltip("Vitesse de deplacement globale")]
    public float vitesseGlobale = 2f;

    [Tooltip("Variation de vitesse entre nuages")]
    public float variationVitesse = 1f;

    [Tooltip("Direction principale du vent")]
    public Vector3 directionVent = Vector3.right;

    [Header("Effets avances")]
    [Tooltip("Activer la courbure (suivre la surface de la planete)")]
    public bool activerCourbure = true;

    [Tooltip("Rayon de la planete pour la courbure")]
    public float rayonPlanete = 500f;

    [Tooltip("Centre de la planete (habituellement le joueur)")]
    public Transform centrePlanete;

    [Tooltip("Activer l'effet de perspective (scale selon distance)")]
    public bool activerPerspective = true;

    [Header("Respawn automatique")]
    [Tooltip("Distance maximale avant respawn")]
    public float distanceRespawn = 600f;

    [Header("Integration DayNight")]
    [Tooltip("Materiel partage des nuages (pour changer la couleur selon l'heure)")]
    public Material materielNuages;

    [Tooltip("Assigner automatiquement le materiel au DayNightManager")]
    public bool assignerAuDayNightManager = true;

    [Header("Debug")]
    [Tooltip("Afficher les informations de debug")]
    public bool afficherDebug = true;

    private GameObject[] nuagesSpawnes;

    void Start()
    {
        // Trouver le centre planete si non assigne
        if (centrePlanete == null)
        {
            GameObject joueur = GameObject.FindGameObjectWithTag("Player");
            if (joueur != null)
            {
                centrePlanete = joueur.transform;
            }
            else
            {
                // Si pas de joueur, utiliser le centre du monde
                centrePlanete = transform;
            }
        }

        // Assigner le materiel au DayNightManager
        if (assignerAuDayNightManager && materielNuages != null)
        {
            DayNightCycle dayNight = FindObjectOfType<DayNightCycle>();
            if (dayNight != null)
            {
                // Note: Necessite que DayNightManager ait un champ public materielNuages
                if (afficherDebug)
                {
                    Debug.Log("[CloudSpawner] Materiel des nuages assigne au DayNightManager");
                }
            }
        }

        // Spawner les nuages
        SpawnerNuages();

        if (afficherDebug)
        {
            Debug.Log($"[CloudSpawner] {nombreNuages} nuages spawnes avec succes !");
        }
    }


    // Generer tous les nuages
    void SpawnerNuages()
    {
        if (nuagePrefabs == null || nuagePrefabs.Length == 0)
        {
            Debug.LogWarning("[CloudSpawner] Aucun prefab de nuage assigne !");
            return;
        }

        nuagesSpawnes = new GameObject[nombreNuages];

        for (int i = 0; i < nombreNuages; i++)
        {
            nuagesSpawnes[i] = SpawnerUnNuage(i);
        }
    }


    // Spawner un seul nuage avec tous ses parametres
    GameObject SpawnerUnNuage(int index)
    {
        // Position aleatoire dans la zone
        float x = Random.Range(-tailleZone.x / 2, tailleZone.x / 2);
        float y = Random.Range(hauteurMin, hauteurMax);
        float z = Random.Range(-tailleZone.y / 2, tailleZone.y / 2);
        Vector3 position = new Vector3(x, y, z);

        // Si on a un centre planete, ajuster la position
        if (centrePlanete != null && centrePlanete != transform)
        {
            position += centrePlanete.position;
        }

        // Choisir un prefab aleatoire
        GameObject prefab = nuagePrefabs[Random.Range(0, nuagePrefabs.Length)];

        // Instancier le nuage
        GameObject nuage = Instantiate(prefab, position, Quaternion.identity, transform);
        nuage.name = $"Nuage_{index}";

        // Rotation aleatoire sur l'axe Y
        nuage.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

        // Scale aleatoire (ou selon distance si active)
        float scale;
        if (variationSelonDistance && centrePlanete != null)
        {
            float distance = Vector3.Distance(position, centrePlanete.position);
            float pourcentage = Mathf.InverseLerp(100f, 500f, distance);
            scale = Mathf.Lerp(scaleMax, scaleMin, pourcentage);
        }
        else
        {
            scale = Random.Range(scaleMin, scaleMax);
        }
        nuage.transform.localScale = Vector3.one * scale;

        // Appliquer le materiel partage si present
        if (materielNuages != null)
        {
            Renderer[] renderers = nuage.GetComponentsInChildren<Renderer>();
            foreach (Renderer rend in renderers)
            {
                rend.sharedMaterial = materielNuages;
            }
        }

        // Ajouter le script CloudMovement si le mouvement est active
        if (activerMouvement)
        {
            CloudMovement movement = nuage.GetComponent<CloudMovement>();
            if (movement == null)
            {
                movement = nuage.AddComponent<CloudMovement>();
            }

            // Configurer le mouvement
            movement.vitesseDeplacement = vitesseGlobale;
            movement.variationVitesse = variationVitesse;
            movement.directionDeplacement = directionVent.normalized;
            movement.activerCourbure = activerCourbure;
            movement.rayonPlanete = rayonPlanete;
            movement.centrePlanete = centrePlanete;
            movement.activerPerspective = activerPerspective;
            movement.distanceRespawn = distanceRespawn;

            // Parametres de la zone de respawn
            movement.zoneSpawnX = new Vector2(-tailleZone.x / 2, tailleZone.x / 2);
            movement.zoneSpawnZ = new Vector2(-tailleZone.y / 2, tailleZone.y / 2);
            movement.zoneSpawnY = new Vector2(hauteurMin, hauteurMax);
        }

        return nuage;
    }

    // Regenerer tous les nuages (utile pour tester)
    public void RegenerNuages()
    {
        // Detruire les nuages existants
        if (nuagesSpawnes != null)
        {
            foreach (GameObject nuage in nuagesSpawnes)
            {
                if (nuage != null)
                {
                    Destroy(nuage);
                }
            }
        }

        // Respawner
        SpawnerNuages();

        if (afficherDebug)
        {
            Debug.Log("[CloudSpawner] Nuages regeneres !");
        }
    }


    // Changer la direction du vent
    public void ChangerDirectionVent(Vector3 nouvelleDirection)
    {
        directionVent = nouvelleDirection.normalized;

        // Mettre a jour tous les nuages existants
        if (nuagesSpawnes != null)
        {
            foreach (GameObject nuage in nuagesSpawnes)
            {
                if (nuage != null)
                {
                    CloudMovement movement = nuage.GetComponent<CloudMovement>();
                    if (movement != null)
                    {
                        movement.directionDeplacement = directionVent;
                    }
                }
            }
        }

        if (afficherDebug)
        {
            Debug.Log($"[CloudSpawner] Direction du vent changee : {directionVent}");
        }
    }


    // Changer la vitesse du vent
    public void ChangerVitesseVent(float nouvelleVitesse)
    {
        vitesseGlobale = nouvelleVitesse;

        // Mettre a jour tous les nuages existants
        if (nuagesSpawnes != null)
        {
            foreach (GameObject nuage in nuagesSpawnes)
            {
                if (nuage != null)
                {
                    CloudMovement movement = nuage.GetComponent<CloudMovement>();
                    if (movement != null)
                    {
                        movement.vitesseDeplacement = vitesseGlobale;
                    }
                }
            }
        }

        if (afficherDebug)
        {
            Debug.Log($"[CloudSpawner] Vitesse du vent changee : {vitesseGlobale}");
        }
    }


    // Obtenir le nombre de nuages actifs
    public int ObtenirNombreNuagesActifs()
    {
        if (nuagesSpawnes == null) return 0;

        int count = 0;
        foreach (GameObject nuage in nuagesSpawnes)
        {
            if (nuage != null) count++;
        }
        return count;
    }


    // Visualisation dans l'editeur
    void OnDrawGizmosSelected()
    {
        // Zone de spawn
        Gizmos.color = Color.cyan;
        Vector3 centre = centrePlanete != null ? centrePlanete.position : transform.position;
        Gizmos.DrawWireCube(centre + Vector3.up * ((hauteurMin + hauteurMax) / 2f),
            new Vector3(tailleZone.x, hauteurMax - hauteurMin, tailleZone.y));

        // Rayon de courbure si active
        if (activerCourbure)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(centre, rayonPlanete);
        }

        // Distance de respawn
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(centre, distanceRespawn);

        // Direction du vent
        Gizmos.color = Color.green;
        Gizmos.DrawRay(centre + Vector3.up * hauteurMax, directionVent.normalized * 50f);
    }

    void OnApplicationQuit()
    {
        // Nettoyer tous les enfants
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}
