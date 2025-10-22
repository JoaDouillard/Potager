using UnityEngine;

public class CloudSpawner : MonoBehaviour
{
    [Header("Prefabs de nuages")]
    public GameObject[] nuagePrefabs; // Drag cloud_1 et cloud_2 ici

    [Header("Configuration")]
    public int nombreNuages = 30;
    public Vector2 tailleZone = new Vector2(500, 500); // Taille X et Z
    public float hauteurMin = 50f;
    public float hauteurMax = 150f;

    [Header("Variation")]
    public float scaleMin = 0.7f;
    public float scaleMax = 1.5f;

    void Start()
    {
        SpawnerNuages();
    }

    void SpawnerNuages()
    {
        if (nuagePrefabs.Length == 0)
        {
            Debug.LogWarning("Aucun prefab de nuage assign� !");
            return;
        }

        for (int i = 0; i < nombreNuages; i++)
        {
            // Position al�atoire
            float x = Random.Range(-tailleZone.x / 2, tailleZone.x / 2);
            float y = Random.Range(hauteurMin, hauteurMax);
            float z = Random.Range(-tailleZone.y / 2, tailleZone.y / 2);
            Vector3 position = new Vector3(x, y, z);

            // Choisir un prefab al�atoire
            GameObject prefab = nuagePrefabs[Random.Range(0, nuagePrefabs.Length)];

            // Spawner
            GameObject nuage = Instantiate(prefab, position, Quaternion.identity, transform);

            // Rotation al�atoire
            nuage.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

            // Scale al�atoire
            float scale = Random.Range(scaleMin, scaleMax);
            nuage.transform.localScale = Vector3.one * scale;
        }

        Debug.Log($"{nombreNuages} nuages spawn�s !");
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
