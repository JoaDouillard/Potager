using UnityEngine;
using System.IO;

[System.Serializable]
public class DonneesPosition
{
    public float x;
    public float y;
    public float z;
}

public class Save : MonoBehaviour
{
    private string cheminFichier;

    void Awake()
    {
        cheminFichier = Application.persistentDataPath + "/save/position.json";
    }

    void Start()
    {
        ChargerPosition();
    }

    void OnApplicationQuit()
    {
        SauvegarderPosition();
    }

    void SauvegarderPosition()
    {
        DonneesPosition donnees = new DonneesPosition
        {
            x = transform.position.x,
            y = transform.position.y,
            z = transform.position.z
        };

        string json = JsonUtility.ToJson(donnees, true);
        File.WriteAllText(cheminFichier, json);
        Debug.Log("Position sauvegardée dans : " + cheminFichier);
    }

    void ChargerPosition()
    {
        if (File.Exists(cheminFichier))
        {
            string json = File.ReadAllText(cheminFichier);
            DonneesPosition donnees = JsonUtility.FromJson<DonneesPosition>(json);
            transform.position = new Vector3(donnees.x, donnees.y, donnees.z);
            Debug.Log("Position chargée !");
        }
        else
        {
            Debug.Log("Aucune sauvegarde trouvée.");
        }
    }

    // Optionnel : Pour sauvegarder manuellement (avec un bouton par exemple)
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            SauvegarderPosition();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            ChargerPosition();
        }
    }
}
