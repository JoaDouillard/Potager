using UnityEngine;

public class StoneSpawner : MonoBehaviour
{
    [Header("Prefabs de pierres")]
    public GameObject[] stonePrefabs;

    [Header("Configuration")]
    public int nombrePierres = 200;
    public Vector2 tailleZone = new Vector2(350, 350);
    public float hauteurSol = 2.6f;

    [Header("Éviter les obstacles")]
    public float distanceMinObstacle = 10f;
    public LayerMask layerObstacle;
    public float distanceMinCreate = 5f;
    public LayerMask layerCreate;

    [Header("Variation")]
    public float scaleMin = 0.5f;
    public float scaleMax = 2f;

    void Start()
    {
        SpawnerPierres();
    }

    void SpawnerPierres()
    {
        if (stonePrefabs.Length == 0)
        {
            Debug.LogWarning("Aucun prefab de pierre assigné !");
            return;
        }

        int spawned = 0;
        int tentatives = 0;
        int maxTentatives = nombrePierres * 20;

        while (spawned < nombrePierres && tentatives < maxTentatives)
        {
            tentatives++;

            // Position aléatoire
            float x = Random.Range(-tailleZone.x / 2, tailleZone.x / 2);
            float z = Random.Range(-tailleZone.y / 2, tailleZone.y / 2);
            Vector3 position = new Vector3(x, hauteurSol + 100f, z);

            // VÉRIFIER obstacles fixes
            if (Physics.CheckSphere(position, distanceMinObstacle, layerObstacle))
            {
                continue;
            }

            // VÉRIFIER autres objets spawnés
            if (Physics.CheckSphere(position, distanceMinCreate, layerCreate))
            {
                continue;
            }

            // Raycast pour trouver le sol
            RaycastHit hit;
            if (Physics.Raycast(position, Vector3.down, out hit, 200f))
            {
                position.y = hit.point.y;
            }
            else
            {
                position.y = hauteurSol;
            }

            // Choisir un prefab aléatoire
            GameObject prefab = stonePrefabs[Random.Range(0, stonePrefabs.Length)];

            // Spawner
            GameObject pierre = Instantiate(prefab, position, Quaternion.identity, transform);
            pierre.layer = LayerMask.NameToLayer("Create");

            // Rotation aléatoire
            pierre.transform.rotation = Quaternion.Euler(
                Random.Range(0f, 20f),
                Random.Range(0f, 360f),
                Random.Range(0f, 20f)
            );

            // Scale aléatoire
            float scale = Random.Range(scaleMin, scaleMax);
            pierre.transform.localScale = Vector3.one * scale;

            spawned++;
        }

        Debug.Log($"🪨 {spawned}/{nombrePierres} pierres spawnées ({tentatives} tentatives)");
    }

    void OnApplicationQuit()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}
