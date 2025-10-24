using UnityEngine;

/// Gère le cycle jour/nuit avec skyboxes, soleil, lune et étoiles
public class DayNightSkyboxManager : MonoBehaviour
{
    #region === REFERENCES ===
    [Header("=== REFERENCES ===")]
    [SerializeField] private Light soleil;
    [SerializeField] private Light lune;
    [SerializeField] private ParticleSystem particlesEtoiles;
    #endregion

    #region === SKYBOXES ===
    [Header("=== SKYBOXES ===")]
    [SerializeField] private Material skyboxMatin;
    [SerializeField] private Material skyboxMidi;
    [SerializeField] private Material skyboxApresMidi;
    [SerializeField] private Material skyboxSoiree;
    [SerializeField] private Material skyboxNuit;

    [Header("Heures de changement")]
    [Range(0f, 24f)] public float heureDebutMatin = 6f;
    [Range(0f, 24f)] public float heureDebutMidi = 10f;
    [Range(0f, 24f)] public float heureDebutApresMidi = 16f;
    [Range(0f, 24f)] public float heureDebutSoiree = 17.5f;
    [Range(0f, 24f)] public float heureDebutNuit = 19f;
    #endregion

    #region === ROTATION DES NUAGES ===
    [Header("=== ROTATION NUAGES ===")]
    [SerializeField] private float vitesseRotationNuages = 1f;
    #endregion

    #region === CYCLE JOUR/NUIT ===
    [Header("=== CYCLE JOUR/NUIT ===")]
    [SerializeField] private float dureeJourEnSecondes = 180f;
    [SerializeField] private bool cycleActif = true;
    [SerializeField][Range(0f, 24f)] private float heureActuelle = 12f;
    [SerializeField] private float vitesseTemps = 1f;
    #endregion

    #region === LUMINOSITE ===
    [Header("=== LUMINOSITE ===")]
    [SerializeField] private Gradient couleurSoleil;
    [SerializeField] private AnimationCurve intensiteSoleil;
    [SerializeField] private float intensiteMaxSoleil = 1.5f;

    [Header("Lune")]
    [SerializeField] private Color couleurLune = new Color(0.5f, 0.6f, 0.8f, 1f);
    [SerializeField] private float intensiteLune = 0.3f;

    [Header("Ambiante")]
    [SerializeField] private float intensiteAmbianteJour = 1f;
    [SerializeField] private float intensiteAmbianteNuit = 0.2f;
    #endregion

    #region === ETOILES ===
    [Header("=== ETOILES ===")]
    [SerializeField] private bool activerEtoiles = true;
    [SerializeField] private float nombreMaxEtoiles = 100f;
    #endregion

    #region === VARIABLES PRIVEES ===
    private float rotationNuagesAccumulee = 0f;
    private static readonly int RotationPropertyID = Shader.PropertyToID("_Rotation");
    private string skyboxActuelle = "";
    #endregion

    void Start()
    {
        MettreAJourCielComplet();
    }

    void Update()
    {
        if (!cycleActif) return;

        heureActuelle += (Time.deltaTime / dureeJourEnSecondes) * 24f * vitesseTemps;

        if (heureActuelle >= 24f)
        {
            heureActuelle -= 24f;
        }

        MettreAJourCielComplet();
    }

    void MettreAJourCielComplet()
    {
        MettreAJourSoleil();
        MettreAJourLune();
        MettreAJourEtoiles();
        MettreAJourSkybox();
        MettreAJourRotationNuages();
        MettreAJourLumiereAmbiante();
    }

    void MettreAJourSoleil()
    {
        if (soleil == null) return;

        float pourcentageJour = heureActuelle / 24f;

        // CALCUL BASÉ SUR L'HEURE :
        // 6h (matin) = soleil à l'horizon est (angle = -90°)
        // 12h (midi) = soleil au zénith (angle = 0°)
        // 18h (soir) = soleil à l'horizon ouest (angle = 90°)

        // Mapper l'heure sur un angle de rotation
        // À 12h (midi), le soleil est en haut (Y positif)
        float angleRotation = ((heureActuelle - 12f) / 24f) * 360f;

        // Calcul de la position du soleil sur un cercle vertical
        Vector3 direction = new Vector3(
            Mathf.Cos(angleRotation * Mathf.Deg2Rad), // X : se déplace d'est en ouest
            Mathf.Sin(angleRotation * Mathf.Deg2Rad), // Y : monte et descend
            0f
        );

        // Position du soleil dans le ciel (assez loin pour simuler l'infini)
        soleil.transform.position = direction * 1000f;

        // Le soleil regarde toujours vers le centre de la scène
        soleil.transform.LookAt(Vector3.zero);

        // Couleur
        if (couleurSoleil != null)
        {
            soleil.color = couleurSoleil.Evaluate(pourcentageJour);
        }

        // Intensité
        if (intensiteSoleil != null)
        {
            float intensiteNormalisee = intensiteSoleil.Evaluate(pourcentageJour);
            soleil.intensity = intensiteNormalisee * intensiteMaxSoleil;
        }

        // Activer/désactiver
        bool estNuit = heureActuelle < heureDebutMatin || heureActuelle >= heureDebutNuit;
        soleil.enabled = !estNuit;
    }

    void MettreAJourLune()
    {
        if (lune == null) return;

        // La lune est à l'opposé du soleil (décalage de 12h)
        // À minuit (0h), la lune est au zénith
        float heureDecalee = heureActuelle + 12f;
        if (heureDecalee >= 24f) heureDecalee -= 24f;

        float angleRotation = ((heureDecalee - 12f) / 24f) * 360f;

        Vector3 direction = new Vector3(
            Mathf.Cos(angleRotation * Mathf.Deg2Rad),
            Mathf.Sin(angleRotation * Mathf.Deg2Rad),
            0f
        );

        lune.transform.position = direction * 1000f;
        lune.transform.LookAt(Vector3.zero);

        lune.color = couleurLune;

        // Activer la lune la nuit
        bool estNuit = heureActuelle < heureDebutMatin || heureActuelle >= heureDebutNuit;
        lune.intensity = estNuit ? intensiteLune : 0f;
        lune.enabled = estNuit;
    }

    void MettreAJourLumiereAmbiante()
    {
        bool estNuit = heureActuelle < heureDebutMatin || heureActuelle >= heureDebutNuit;
        float intensiteCible = estNuit ? intensiteAmbianteNuit : intensiteAmbianteJour;
        RenderSettings.ambientIntensity = Mathf.Lerp(
            RenderSettings.ambientIntensity,
            intensiteCible,
            Time.deltaTime * 2f
        );
    }

    void MettreAJourEtoiles()
    {
        if (!activerEtoiles || particlesEtoiles == null) return;

        // CORRECTION : Les étoiles sont visibles la nuit seulement
        bool estNuit = heureActuelle < heureDebutMatin || heureActuelle >= heureDebutNuit;

        var emission = particlesEtoiles.emission;

        if (estNuit)
        {
            // Activer les étoiles la nuit
            emission.rateOverTime = nombreMaxEtoiles;

            if (!particlesEtoiles.isPlaying)
            {
                particlesEtoiles.Play();
            }
        }
        else
        {
            // Désactiver les étoiles le jour
            emission.rateOverTime = 0f;

            if (particlesEtoiles.isPlaying)
            {
                particlesEtoiles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }

    void MettreAJourSkybox()
    {
        string nouvelleSkybox = "";
        Material skyboxMaterial = null;

        // Déterminer quelle skybox utiliser selon l'heure
        if (heureActuelle >= heureDebutMatin && heureActuelle < heureDebutMidi)
        {
            skyboxMaterial = skyboxMatin;
            nouvelleSkybox = "Matin";
        }
        else if (heureActuelle >= heureDebutMidi && heureActuelle < heureDebutApresMidi)
        {
            skyboxMaterial = skyboxMidi;
            nouvelleSkybox = "Midi";
        }
        else if (heureActuelle >= heureDebutApresMidi && heureActuelle < heureDebutSoiree)
        {
            skyboxMaterial = skyboxApresMidi;
            nouvelleSkybox = "Apres-Midi";
        }
        else if (heureActuelle >= heureDebutSoiree && heureActuelle < heureDebutNuit)
        {
            skyboxMaterial = skyboxSoiree;
            nouvelleSkybox = "Soiree";
        }
        else
        {
            skyboxMaterial = skyboxNuit;
            nouvelleSkybox = "Nuit";
        }

        // Changer la skybox si nécessaire
        if (skyboxActuelle != nouvelleSkybox && skyboxMaterial != null)
        {
            RenderSettings.skybox = skyboxMaterial;
            DynamicGI.UpdateEnvironment();
            skyboxActuelle = nouvelleSkybox;
        }
    }

    void MettreAJourRotationNuages()
    {
        if (RenderSettings.skybox == null) return;

        rotationNuagesAccumulee += vitesseRotationNuages * Time.deltaTime;

        if (rotationNuagesAccumulee >= 360f)
        {
            rotationNuagesAccumulee -= 360f;
        }

        if (RenderSettings.skybox.HasProperty(RotationPropertyID))
        {
            RenderSettings.skybox.SetFloat(RotationPropertyID, rotationNuagesAccumulee);
        }
    }
}