using System.Collections;
using UnityEngine;

public class Seed : MonoBehaviour
{
    [Header("Parametres de croissance")]
    public float tempsCroissance = 5f;
    public float scaleInitial = 0.1f;
    public float scaleFinal = 1f;
    public float vitesseCroissance = 0.5f;

    [Header("Transformation")]
    public GameObject prefabLegume;

    [Header("Effets visuels")]
    public GameObject particulesTransformation;

    [Header("Zone de plantation")]
    public ZonePlantation zonePlantation;

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

    void TransformerEnLegume()
    {
        if (prefabLegume == null)
        {
            Debug.LogError("[Seed] Aucun prefab de legume assigne !");
            Destroy(gameObject);
            return;
        }

        if (afficherDebug)
        {
            Debug.Log($"[Seed] Transformation en legume a la position {transform.position}");
        }

        GameObject legume = Instantiate(prefabLegume, transform.position, transform.rotation);

        if (zonePlantation != null)
        {
            legume.transform.SetParent(zonePlantation.transform);
            zonePlantation.planteCourante = legume;
        }

        if (particulesTransformation != null)
        {
            GameObject particules = Instantiate(particulesTransformation, transform.position, Quaternion.identity);
            Destroy(particules, 2f);
        }

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
