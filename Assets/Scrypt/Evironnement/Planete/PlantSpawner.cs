using UnityEngine;

public class PlantSpawner : MonoBehaviour
{
    [Header("Prefabs de plantes")]
    public GameObject[] plantePrefabs;

    [Header("Configuration")]
    public int nombrePlantes = 400;
    public Vector2 tailleZone = new Vector2(350, 350);
    public float hauteurSol = 2.6f;

    [Header("Éviter les obstacles")]
    public float distanceMinObstacle = 10f;
    public LayerMask layerObstacle;
    public float distanceMinCreate = 5f;
    public LayerMask layerCreate;

    [Header("Variation")]
    public float scaleMin = 0.8f;
    public float scaleMax = 1.2f;

    void Start()
    {
        SpawnerPlantes();
    }

    void SpawnerPlantes()
    {
        if (plantePrefabs.Length == 0)
        {
            Debug.LogWarning("Aucun prefab de plante assigné !");
            return;
        }

        int spawned = 0;
        int tentatives = 0;
        int maxTentatives = nombrePlantes * 20;

        while (spawned < nombrePlantes && tentatives < maxTentatives)
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
            GameObject prefab = plantePrefabs[Random.Range(0, plantePrefabs.Length)];

            // Spawner
            GameObject plante = Instantiate(prefab, position, Quaternion.identity, transform);
            plante.layer = LayerMask.NameToLayer("Create");

            // Rotation aléatoire
            plante.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

            // Scale aléatoire
            float scale = Random.Range(scaleMin, scaleMax);
            plante.transform.localScale = Vector3.one * scale;

            spawned++;
        }

        Debug.Log($"🌱 {spawned}/{nombrePlantes} plantes spawnées ({tentatives} tentatives)");
    }

    void OnApplicationQuit()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}
