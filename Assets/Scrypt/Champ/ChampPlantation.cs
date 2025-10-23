using UnityEngine;

public class ChampPlantation : MonoBehaviour
{
    [Header("Configuration du champ")]
    public int nombreLignes = 5;
    public int nombreColonnes = 5;
    public float espacementX = 2f;
    public float espacementZ = 2f;
    public Vector3 tailleZone = new Vector3(1.5f, 2f, 1.5f);

    [Header("Croissance")]
    public float tempsGerme = 3f;
    public float tailleGermeMin = 0.1f;
    public float tailleGermeMax = 0.5f;

    private ZonePlantation[] zones;

    void Start()
    {
        GenererZones();
    }

    void GenererZones()
    {
        zones = new ZonePlantation[nombreLignes * nombreColonnes];

        float offsetX = -(nombreColonnes - 1) * espacementX / 2f;
        float offsetZ = -(nombreLignes - 1) * espacementZ / 2f;

        int index = 0;
        for (int ligne = 0; ligne < nombreLignes; ligne++)
        {
            for (int colonne = 0; colonne < nombreColonnes; colonne++)
            {
                float x = offsetX + colonne * espacementX;
                float z = offsetZ + ligne * espacementZ;

                Vector3 position = transform.position + new Vector3(x, 0f, z);

                RaycastHit hit;
                if (Physics.Raycast(position + Vector3.up * 10f, Vector3.down, out hit, 20f))
                {
                    position = hit.point + Vector3.up * (tailleZone.y / 2f);
                }

                GameObject zoneObj = new GameObject($"Zone_{ligne}_{colonne}");
                zoneObj.transform.position = position;
                zoneObj.transform.SetParent(transform);

                BoxCollider box = zoneObj.AddComponent<BoxCollider>();
                box.isTrigger = true;
                box.size = tailleZone;

                ZonePlantation zone = zoneObj.AddComponent<ZonePlantation>();
                zone.champParent = this;
                zone.tempsGerme = tempsGerme;
                zone.tailleGermeMin = tailleGermeMin;
                zone.tailleGermeMax = tailleGermeMax;

                zones[index] = zone;
                index++;
            }
        }

        Debug.Log($"[ChampPlantation] {zones.Length} zones de plantation generees");
    }

    public ZonePlantation ObtenirZoneProche(Vector3 position, float distanceMax = 2f)
    {
        ZonePlantation zonePlusProche = null;
        float distanceMin = float.MaxValue;

        foreach (ZonePlantation zone in zones)
        {
            if (zone == null) continue;

            float distance = Vector3.Distance(position, zone.transform.position);
            if (distance < distanceMin && distance <= distanceMax)
            {
                distanceMin = distance;
                zonePlusProche = zone;
            }
        }

        return zonePlusProche;
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            float offsetX = -(nombreColonnes - 1) * espacementX / 2f;
            float offsetZ = -(nombreLignes - 1) * espacementZ / 2f;

            for (int ligne = 0; ligne < nombreLignes; ligne++)
            {
                for (int colonne = 0; colonne < nombreColonnes; colonne++)
                {
                    float x = offsetX + colonne * espacementX;
                    float z = offsetZ + ligne * espacementZ;

                    Vector3 position = transform.position + new Vector3(x, 0f, z);
                    Gizmos.DrawWireCube(position, tailleZone);
                }
            }
        }
    }
}
