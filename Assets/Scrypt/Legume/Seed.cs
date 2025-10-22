using System.Collections;
using UnityEngine;


// Script pour gérer la croissance d'une graine et sa transformation en légume
public class Seed : MonoBehaviour
{
    [Header("Paramètres de croissance")]
    [Tooltip("Temps en secondes avant de devenir un légume")]
    public float tempsCroissance = 5f;

    [Tooltip("Taille initiale de la graine")]
    public float scaleInitial = 0.1f;

    [Tooltip("Taille finale avant transformation")]
    public float scaleFinal = 1f;

    [Tooltip("Vitesse de croissance visuelle")]
    public float vitesseCroissance = 0.5f;

    [Header("Transformation")]
    [Tooltip("Prefab du légume à instancier")]
    public GameObject prefabLegume;

    [Header("Effets visuels (optionnel)")]
    [Tooltip("Particules lors de la transformation")]
    public GameObject particulesTransformation;

    [Header("Debug")]
    public bool afficherDebug = true;

    private float tempsEcoule = 0f;
    private bool enCroissance = true;
    private Vector3 scaleTarget;

    void Start()
    {
        // Initialiser la taille de la graine
        transform.localScale = Vector3.one * scaleInitial;
        scaleTarget = Vector3.one * scaleFinal;

        // Démarrer la coroutine de transformation
        StartCoroutine(CroissanceEtTransformation());

        if (afficherDebug)
        {
            Debug.Log($"[Seed] Graine créée à la position {transform.position}");
        }
    }

    void Update()
    {
        // Croissance progressive visuelle
        if (enCroissance && transform.localScale.x < scaleFinal)
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                scaleTarget,
                Time.deltaTime * vitesseCroissance
            );
        }
    }


    // Coroutine qui gère la croissance et la transformation en légume
    IEnumerator CroissanceEtTransformation()
    {
        // Attendre le temps de croissance
        while (tempsEcoule < tempsCroissance)
        {
            tempsEcoule += Time.deltaTime;
            yield return null;
        }

        // Transformation en légume
        TransformerEnLegume();
    }

    
    // Transforme la graine en légume
    void TransformerEnLegume()
    {
        if (prefabLegume == null)
        {
            Debug.LogError("[Seed] Aucun prefab de légume assigné !");
            Destroy(gameObject);
            return;
        }

        if (afficherDebug)
        {
            Debug.Log($"[Seed] Transformation en légume à la position {transform.position}");
        }

        // Instancier le légume à la même position
        GameObject legume = Instantiate(prefabLegume, transform.position, transform.rotation);

        // Optionnel : effet de particules
        if (particulesTransformation != null)
        {
            GameObject particules = Instantiate(particulesTransformation, transform.position, Quaternion.identity);
            Destroy(particules, 2f); // Détruire les particules après 2 secondes
        }

        // Détruire la graine
        Destroy(gameObject);
    }


    // Fonction pour accélérer la croissance (appelée lors de l'arrosage)
    public void Arroser(float accelerationMultiplier = 2f)
    {
        vitesseCroissance *= accelerationMultiplier;
        tempsCroissance /= accelerationMultiplier;

        if (afficherDebug)
        {
            Debug.Log($"[Seed] Graine arrosée ! Croissance accélérée.");
        }
    }


    // Affichage visuel pour le debug
    void OnDrawGizmos()
    {
        if (afficherDebug && Application.isPlaying)
        {
            // Afficher une sphère pour indiquer la zone de la graine
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}
