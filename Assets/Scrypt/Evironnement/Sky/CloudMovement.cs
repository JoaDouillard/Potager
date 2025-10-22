using UnityEngine;


// Script pour animer les nuages avec mouvement, courbure et effet de perspective
// A attacher sur chaque prefab de nuage
public class CloudMovement : MonoBehaviour
{
    [Header("Mouvement")]
    [Tooltip("Vitesse de deplacement du nuage")]
    public float vitesseDeplacement = 2f;

    [Tooltip("Direction de deplacement (normalise automatiquement)")]
    public Vector3 directionDeplacement = Vector3.right;

    [Tooltip("Variation aleatoire de vitesse")]
    public float variationVitesse = 0.5f;

    [Header("Oscillation (mouvement vertical)")]
    [Tooltip("Activer l'oscillation verticale")]
    public bool activerOscillation = true;

    [Tooltip("Amplitude de l'oscillation verticale")]
    public float amplitudeOscillation = 2f;

    [Tooltip("Vitesse de l'oscillation")]
    public float vitesseOscillation = 0.5f;

    [Header("Rotation")]
    [Tooltip("Activer la rotation lente du nuage")]
    public bool activerRotation = true;

    [Tooltip("Vitesse de rotation sur l'axe Y (degres/seconde)")]
    public float vitesseRotation = 5f;

    [Header("Courbure et Perspective")]
    [Tooltip("Rayon de la planete pour la courbure")]
    public float rayonPlanete = 500f;

    [Tooltip("Centre de la planete (habituellement le joueur)")]
    public Transform centrePlanete;

    [Tooltip("Activer l'effet de courbure")]
    public bool activerCourbure = true;

    [Tooltip("Activer l'effet de perspective (plus gros quand proche)")]
    public bool activerPerspective = true;

    [Tooltip("Distance minimale pour le scale")]
    public float distanceMin = 50f;

    [Tooltip("Distance maximale pour le scale")]
    public float distanceMax = 300f;

    [Tooltip("Scale minimum")]
    public float scaleMin = 0.5f;

    [Tooltip("Scale maximum")]
    public float scaleMax = 2f;

    [Header("Limites et Respawn")]
    [Tooltip("Distance maximale avant respawn")]
    public float distanceRespawn = 600f;

    [Tooltip("Zone de spawn X")]
    public Vector2 zoneSpawnX = new Vector2(-500f, 500f);

    [Tooltip("Zone de spawn Z")]
    public Vector2 zoneSpawnZ = new Vector2(-500f, 500f);

    [Tooltip("Zone de spawn Y")]
    public Vector2 zoneSpawnY = new Vector2(50f, 150f);

    [Header("Transparence selon distance")]
    [Tooltip("Activer le fade en fonction de la distance")]
    public bool activerFade = true;

    [Tooltip("Distance de debut du fade")]
    public float distanceFadeDebut = 400f;

    private Vector3 positionInitiale;
    private float offsetOscillation;
    private float vitesseReelle;
    private Vector3 scaleInitial;
    private Renderer[] renderers;
    private MaterialPropertyBlock propBlock;

    void Start()
    {
        positionInitiale = transform.position;
        scaleInitial = transform.localScale;

        // Variation aleatoire de la vitesse
        vitesseReelle = vitesseDeplacement + Random.Range(-variationVitesse, variationVitesse);

        // Offset aleatoire pour l'oscillation (pour que les nuages ne soient pas synchronises)
        offsetOscillation = Random.Range(0f, Mathf.PI * 2f);

        // Normaliser la direction
        directionDeplacement = directionDeplacement.normalized;

        // Si pas de centre planete defini, chercher le joueur
        if (centrePlanete == null)
        {
            GameObject joueur = GameObject.FindGameObjectWithTag("Player");
            if (joueur != null)
            {
                centrePlanete = joueur.transform;
            }
        }

        // Recuperer les renderers pour le fade
        renderers = GetComponentsInChildren<Renderer>();
        propBlock = new MaterialPropertyBlock();
    }

    void Update()
    {
        // === DEPLACEMENT ===
        Vector3 deplacement = directionDeplacement * vitesseReelle * Time.deltaTime;
        transform.position += deplacement;

        // === OSCILLATION VERTICALE ===
        if (activerOscillation)
        {
            float oscillation = Mathf.Sin((Time.time * vitesseOscillation) + offsetOscillation) * amplitudeOscillation;
            Vector3 pos = transform.position;
            pos.y = positionInitiale.y + oscillation;
            transform.position = pos;
        }

        // === ROTATION ===
        if (activerRotation)
        {
            transform.Rotate(Vector3.up, vitesseRotation * Time.deltaTime);
        }

        // === COURBURE (suivre la surface de la planete) ===
        if (activerCourbure && centrePlanete != null)
        {
            Vector3 directionVersCentre = (centrePlanete.position - transform.position).normalized;
            Vector3 positionSurSphere = centrePlanete.position + (-directionVersCentre * rayonPlanete);

            // Interpoler vers la position sur la sphere
            transform.position = Vector3.Lerp(transform.position, positionSurSphere, Time.deltaTime * 0.5f);
        }

        // === PERSPECTIVE (scale selon la distance) ===
        if (activerPerspective && centrePlanete != null)
        {
            float distance = Vector3.Distance(transform.position, centrePlanete.position);
            float pourcentage = Mathf.InverseLerp(distanceMin, distanceMax, distance);
            float scaleFacteur = Mathf.Lerp(scaleMax, scaleMin, pourcentage);

            transform.localScale = scaleInitial * scaleFacteur;
        }

        // === FADE SELON DISTANCE ===
        if (activerFade && centrePlanete != null)
        {
            float distance = Vector3.Distance(transform.position, centrePlanete.position);

            if (distance > distanceFadeDebut)
            {
                float alpha = 1f - Mathf.InverseLerp(distanceFadeDebut, distanceRespawn, distance);
                AppliquerAlpha(alpha);
            }
            else
            {
                AppliquerAlpha(1f);
            }
        }

        // === RESPAWN SI TROP LOIN ===
        if (centrePlanete != null)
        {
            float distance = Vector3.Distance(transform.position, centrePlanete.position);
            if (distance > distanceRespawn)
            {
                RespawnNuage();
            }
        }
    }


    // Respawn le nuage a une position aleatoire
    void RespawnNuage()
    {
        float x = Random.Range(zoneSpawnX.x, zoneSpawnX.y);
        float y = Random.Range(zoneSpawnY.x, zoneSpawnY.y);
        float z = Random.Range(zoneSpawnZ.x, zoneSpawnZ.y);

        Vector3 nouvellePosition = new Vector3(x, y, z);

        // Si on a un centre planete, spawner du cote oppose
        if (centrePlanete != null)
        {
            Vector3 direction = (transform.position - centrePlanete.position).normalized;
            nouvellePosition = centrePlanete.position - (direction * (distanceRespawn * 0.8f));
            nouvellePosition.y = y; // Garder l'altitude aleatoire
        }

        transform.position = nouvellePosition;
        positionInitiale = nouvellePosition;

        // Nouvelle vitesse aleatoire
        vitesseReelle = vitesseDeplacement + Random.Range(-variationVitesse, variationVitesse);

        // Nouveau offset d'oscillation
        offsetOscillation = Random.Range(0f, Mathf.PI * 2f);
    }

 
    // Appliquer l'alpha a tous les renderers
    void AppliquerAlpha(float alpha)
    {
        foreach (Renderer rend in renderers)
        {
            if (rend != null)
            {
                rend.GetPropertyBlock(propBlock);
                Color couleur = rend.sharedMaterial.color;
                couleur.a = alpha;
                propBlock.SetColor("_Color", couleur);
                rend.SetPropertyBlock(propBlock);
            }
        }
    }

    // Visualisation du rayon de courbure dans l'editeur
    void OnDrawGizmosSelected()
    {
        if (centrePlanete != null && activerCourbure)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(centrePlanete.position, rayonPlanete);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, centrePlanete.position);
        }

        // Distance de respawn
        if (centrePlanete != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(centrePlanete.position, distanceRespawn);
        }
    }
}
