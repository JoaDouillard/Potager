using UnityEngine;
using System.Collections;

public class ZonePlantation : MonoBehaviour
{
    public bool estOccupee = false;
    public GameObject planteCourante = null;
    public ChampPlantation champParent;

    [Header("Prefab germe generique")]
    public GameObject prefabGerme;

    [Header("Croissance")]
    public float tempsGerme = 3f;
    public float tailleGermeMin = 0.1f;
    public float tailleGermeMax = 0.5f;

    private BoxCollider zoneCollider;
    private TypeGraine typeGraineActuelle;
    private InfoGraine infoGraineActuelle;

    void Start()
    {
        zoneCollider = GetComponent<BoxCollider>();
        if (zoneCollider == null)
        {
            zoneCollider = gameObject.AddComponent<BoxCollider>();
        }

        zoneCollider.isTrigger = true;

        if (prefabGerme == null)
        {
            prefabGerme = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            prefabGerme.transform.localScale = Vector3.one * 0.1f;
            prefabGerme.GetComponent<Renderer>().material.color = Color.green;
            Destroy(prefabGerme.GetComponent<Collider>());
            prefabGerme.SetActive(false);
        }
    }

    public bool PeutPlanter()
    {
        return !estOccupee && planteCourante == null;
    }

    public void PlanterGraine(TypeGraine typeGraine, InfoGraine infoGraine)
    {
        if (!PeutPlanter())
        {
            Debug.LogWarning("[ZonePlantation] Zone deja occupee !");
            return;
        }

        typeGraineActuelle = typeGraine;
        infoGraineActuelle = infoGraine;

        Vector3 positionPlantation = transform.position;

        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 10f, Vector3.down, out hit, 20f))
        {
            positionPlantation = hit.point;
        }

        GameObject germe;
        if (prefabGerme != null && prefabGerme.scene.rootCount == 0)
        {
            germe = Instantiate(prefabGerme, positionPlantation, Quaternion.identity, transform);
        }
        else
        {
            germe = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            germe.transform.position = positionPlantation;
            germe.transform.SetParent(transform);
            germe.transform.localScale = Vector3.one * tailleGermeMin;

            Renderer rend = germe.GetComponent<Renderer>();
            if (rend != null && infoGraine != null)
            {
                rend.material.color = infoGraine.couleur;
            }

            Destroy(germe.GetComponent<Collider>());
        }

        germe.name = $"Germe_{typeGraine}";
        planteCourante = germe;
        estOccupee = true;

        StartCoroutine(CroissanceGerme(germe));

        Debug.Log($"[ZonePlantation] {typeGraine} plante dans zone {name}");
    }

    IEnumerator CroissanceGerme(GameObject germe)
    {
        float tempsEcoule = 0f;
        Vector3 scaleInitial = germe.transform.localScale;
        Vector3 scaleFinal = Vector3.one * tailleGermeMax;

        while (tempsEcoule < tempsGerme)
        {
            tempsEcoule += Time.deltaTime;
            float progression = tempsEcoule / tempsGerme;

            germe.transform.localScale = Vector3.Lerp(scaleInitial, scaleFinal, progression);

            yield return null;
        }

        TransformerEnLegume(germe);
    }

    void TransformerEnLegume(GameObject germe)
    {
        if (infoGraineActuelle == null || infoGraineActuelle.prefabLegume == null)
        {
            Debug.LogWarning($"[ZonePlantation] Pas de prefab legume pour {typeGraineActuelle}");
            Destroy(germe);
            LibererZone();
            return;
        }

        Vector3 position = germe.transform.position;
        Destroy(germe);

        GameObject legume = Instantiate(infoGraineActuelle.prefabLegume, position, Quaternion.identity, transform);
        legume.name = $"Legume_{typeGraineActuelle}";
        planteCourante = legume;

        StartCoroutine(CroissanceLegume(legume));

        Debug.Log($"[ZonePlantation] Germe transforme en {typeGraineActuelle}");
    }

    IEnumerator CroissanceLegume(GameObject legume)
    {
        float tempsEcoule = 0f;
        float tempsCroissance = infoGraineActuelle != null ? infoGraineActuelle.tempsCroissance : 10f;
        float tailleMin = infoGraineActuelle != null ? infoGraineActuelle.tailleMin : 0.5f;
        float tailleMax = infoGraineActuelle != null ? infoGraineActuelle.tailleMax : 2f;

        Vector3 scaleInitial = Vector3.one * tailleMin;
        Vector3 scaleFinal = Vector3.one * tailleMax;

        legume.transform.localScale = scaleInitial;

        while (tempsEcoule < tempsCroissance)
        {
            tempsEcoule += Time.deltaTime;
            float progression = tempsEcoule / tempsCroissance;

            legume.transform.localScale = Vector3.Lerp(scaleInitial, scaleFinal, progression);

            yield return null;
        }

        Debug.Log($"[ZonePlantation] {typeGraineActuelle} a atteint sa taille maximale");

        yield return new WaitForSeconds(5f);

        Destroy(legume);
        LibererZone();
        Debug.Log($"[ZonePlantation] {typeGraineActuelle} a disparu");
    }

    public void LibererZone()
    {
        estOccupee = false;
        planteCourante = null;
        typeGraineActuelle = 0;
        infoGraineActuelle = null;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = estOccupee ? Color.red : Color.green;
        Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.3f);

        if (GetComponent<BoxCollider>() != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(Vector3.zero, GetComponent<BoxCollider>().size);
        }
    }
}
