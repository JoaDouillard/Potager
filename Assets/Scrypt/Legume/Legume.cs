using System.Collections;
using UnityEngine;


// Script pour gérer le comportement des légumes (croissance, récolte, explosion)
public class Legume : MonoBehaviour
{
    [Header("Paramètres de croissance")]
    [Tooltip("Taille initiale du légume")]
    public float scaleInitial = 1f;

    [Tooltip("Taille maximale avant explosion")]
    public float scaleMax = 2.5f;

    [Tooltip("Vitesse de croissance du légume")]
    public float vitesseCroissance = 0.3f;

    [Header("Paramètres d'explosion")]
    [Tooltip("Temps avant explosion (en secondes)")]
    public float tempsAvantExplosion = 10f;

    [Tooltip("Prefab de l'effet d'explosion")]
    public GameObject prefabExplosion;

    [Header("Paramètres de récolte")]
    [Tooltip("Points gagnés lors de la récolte")]
    public int pointsRecolte = 10;

    [Tooltip("Tag du drone pour détecter la récolte")]
    public string tagDrone = "Player";

    [Tooltip("Hauteur du trigger pour détecter le drone")]
    public float hauteurTrigger = 2f;

    [Header("Effets visuels")]
    [Tooltip("Particules de récolte")]
    public GameObject particulesRecolte;

    [Header("Effets sonores (optionnel)")]
    [Tooltip("Son de récolte")]
    public AudioClip sonRecolte;

    [Tooltip("Son d'explosion")]
    public AudioClip sonExplosion;

    [Header("Changement de couleur")]
    [Tooltip("Activer le changement de couleur progressif")]
    public bool changerCouleur = true;

    [Tooltip("Couleur initiale (jeune)")]
    public Color couleurJeune = Color.green;

    [Tooltip("Couleur finale (mûr/dangereux)")]
    public Color couleurMur = Color.red;

    [Header("Debug")]
    public bool afficherDebug = true;

    private float tempsEcoule = 0f;
    private bool estRecolte = false;
    private Renderer rendererLegume;
    private AudioSource audioSource;
    private BoxCollider triggerCollider;

    void Start()
    {
        // Initialiser la taille
        transform.localScale = Vector3.one * scaleInitial;

        // Récupérer le Renderer pour le changement de couleur
        rendererLegume = GetComponent<Renderer>();

        // Récupérer ou ajouter un AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (sonRecolte != null || sonExplosion != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Configurer le trigger pour la récolte
        ConfigurerTrigger();

        // Démarrer la coroutine d'explosion
        StartCoroutine(TimerExplosion());

        if (afficherDebug)
        {
            Debug.Log($"[Legume] Légume créé à {transform.position}. Explosion dans {tempsAvantExplosion}s");
        }
    }

    void Update()
    {
        if (!estRecolte)
        {
            // Croissance progressive
            if (transform.localScale.x < scaleMax)
            {
                float nouveauScale = transform.localScale.x + (vitesseCroissance * Time.deltaTime);
                transform.localScale = Vector3.one * Mathf.Min(nouveauScale, scaleMax);
            }

            // Changement de couleur progressif
            if (changerCouleur && rendererLegume != null)
            {
                float progression = tempsEcoule / tempsAvantExplosion;
                Color couleurActuelle = Color.Lerp(couleurJeune, couleurMur, progression);
                rendererLegume.material.color = couleurActuelle;
            }

            // Incrémenter le temps écoulé
            tempsEcoule += Time.deltaTime;
        }
    }

    // Configure le BoxCollider comme trigger pour détecter le drone
    void ConfigurerTrigger()
    {
        // Chercher un BoxCollider existant
        triggerCollider = GetComponent<BoxCollider>();

        // Si pas de BoxCollider, en ajouter un
        if (triggerCollider == null)
        {
            triggerCollider = gameObject.AddComponent<BoxCollider>();
        }

        // Configurer comme trigger
        triggerCollider.isTrigger = true;

        // Augmenter la hauteur du collider pour détecter le drone qui vole
        Vector3 size = triggerCollider.size;
        size.y = hauteurTrigger;
        triggerCollider.size = size;

        // Décaler le centre vers le haut
        Vector3 center = triggerCollider.center;
        center.y = hauteurTrigger / 2f;
        triggerCollider.center = center;
    }

 
    // Coroutine qui gère le timer d'explosion
    IEnumerator TimerExplosion()
    {
        yield return new WaitForSeconds(tempsAvantExplosion);

        // Si le légume n'a pas été récolté, il explose
        if (!estRecolte)
        {
            Exploser();
        }
    }


    // Détecte quand le drone entre dans le trigger (récolte)
    void OnTriggerEnter(Collider other)
    {
        if (estRecolte) return;

        // Vérifier si c'est le drone
        if (other.CompareTag(tagDrone))
        {
            Recolter();
        }
    }


    // Fonction appelée lors de la récolte du légume
    void Recolter()
    {
        estRecolte = true;

        if (afficherDebug)
        {
            Debug.Log($"[Legume] Récolté ! +{pointsRecolte} points");
        }

        // Ajouter des points au score
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AjouterPoints(pointsRecolte);
        }
        else
        {
            Debug.LogWarning("[Legume] ScoreManager introuvable !");
        }

        // Jouer le son de récolte
        if (audioSource != null && sonRecolte != null)
        {
            audioSource.PlayOneShot(sonRecolte);
        }

        // Instancier les particules de récolte
        if (particulesRecolte != null)
        {
            GameObject particules = Instantiate(particulesRecolte, transform.position, Quaternion.identity);
            Destroy(particules, 2f);
        }

        // Détruire le légume
        Destroy(gameObject, 0.1f); // Petit délai pour le son
    }


    // Fonction appelée lors de l'explosion du légume
    void Exploser()
    {
        if (afficherDebug)
        {
            Debug.Log($"[Legume] EXPLOSION ! Le légume était trop mûr.");
        }

        // Jouer le son d'explosion
        if (audioSource != null && sonExplosion != null)
        {
            audioSource.PlayOneShot(sonExplosion);
        }

        // Instancier l'effet d'explosion
        if (prefabExplosion != null)
        {
            GameObject explosion = Instantiate(prefabExplosion, transform.position, Quaternion.identity);
            Destroy(explosion, 3f);
        }

        // Détruire le légume
        Destroy(gameObject, 0.1f); // Petit délai pour le son
    }

    
    // Fonction pour accélérer la croissance (arrosage)
    public void Arroser(float accelerationMultiplier = 1.5f)
    {
        vitesseCroissance *= accelerationMultiplier;

        if (afficherDebug)
        {
            Debug.Log($"[Legume] Légume arrosé ! Croissance accélérée.");
        }
    }


    // Affichage visuel pour le debug
    void OnDrawGizmos()
    {
        if (afficherDebug && Application.isPlaying)
        {
            // Afficher le trigger en jaune
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position + Vector3.up * (hauteurTrigger / 2f),
                new Vector3(1f, hauteurTrigger, 1f));

            // Afficher une ligne pour le temps restant
            float pourcentageRestant = 1f - (tempsEcoule / tempsAvantExplosion);
            Gizmos.color = Color.Lerp(Color.red, Color.green, pourcentageRestant);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 3f * pourcentageRestant);
        }
    }
}
