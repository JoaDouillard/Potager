using UnityEngine;

public enum TypeGraine
{
    Tomate,
    Carotte,
    Salade,
    Potiron,
    Radis
}

[System.Serializable]
public class InfoGraine
{
    public TypeGraine type;
    public GameObject prefabGraine;
    public GameObject prefabLegume;
    public float tempsCroissance = 10f;
    public float tailleMin = 0.5f;
    public float tailleMax = 2f;
    public int prixVente = 10;
    public Color couleur = Color.green;
}
