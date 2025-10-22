using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light soleil; // Directional Light principale (Soleil)
    [SerializeField] private Light lune; // Directional Light secondaire pour la lune (optionnel)
    [SerializeField] private Material skyboxMaterial; // Materiau Skybox

    [Header("Systeme d'etoiles")]
    [SerializeField] private ParticleSystem particlesEtoiles; // Systeme de particules pour les etoiles
    [SerializeField] private bool activerEtoiles = true;
    [SerializeField] private AnimationCurve visibiliteEtoiles; // Visibilite des etoiles selon l'heure

    [Header("Parametres du Cycle")]
    [SerializeField] private float dureeJourEnSecondes = 180f; // 3 minutes = 1 jour complet
    [SerializeField] private bool cycleActif = true;
    [SerializeField][Range(0f, 24f)] private float heureActuelle = 12f; // Commence a midi
    [SerializeField] private float vitesseTemps = 1f; // Multiplicateur de vitesse

    [Header("Couleurs du Soleil")]
    [SerializeField] private Gradient couleurSoleil; // Couleur de la lumiere du soleil
    [SerializeField] private AnimationCurve intensiteSoleil; // Intensite du soleil (0-2)

    [Header("Couleurs de la Lune")]
    [SerializeField] private Color couleurLune = new Color(0.5f, 0.6f, 0.8f, 1f); // Bleu pale
    [SerializeField] private float intensiteLune = 0.2f;

    [Header("Couleurs du Ciel")]
    [SerializeField] private Gradient couleurCiel; // Couleur principale du ciel
    [SerializeField] private Gradient couleurHorizon; // Couleur de l'horizon
    [SerializeField] private Gradient couleurAmbiant; // Lumiere ambiante

    [Header("Brouillard et Atmosphere")]
    [SerializeField] private bool activerBrouillard = true;
    [SerializeField] private Gradient couleurBrouillard; // Couleur du fog
    [SerializeField] private AnimationCurve densiteBrouillard; // Densite selon l'heure (0-0.1)
    [SerializeField] private float brouillardDistanceMax = 500f;

    [Header("Nuages (integration)")]
    [SerializeField] private Material materielNuages; // Materiel des nuages pour changer la couleur
    [SerializeField] private Gradient couleurNuages; // Couleur des nuages selon l'heure

    [Header("Debug")]
    [SerializeField] private bool afficherDebug = true;
    [SerializeField] private bool afficherHeure = true;

    private float vitesseRotation;
    private ParticleSystem.EmissionModule emissionEtoiles;

    void Start()
    {
        // Calculer la vitesse de rotation (360 degres par jour)
        vitesseRotation = 360f / dureeJourEnSecondes;

        // Configurer les gradients et courbes par defaut
        ConfigurerParametresParDefaut();

        // Configurer les etoiles
        if (particlesEtoiles != null)
        {
            emissionEtoiles = particlesEtoiles.emission;
        }

        // Configurer le brouillard
        if (activerBrouillard)
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
        }

        // Appliquer l'heure de depart
        MettreAJourCiel();

        if (afficherDebug)
        {
            Debug.Log("[DayNightCycle] Initialise - Duree du jour: " + dureeJourEnSecondes + "s");
        }
    }

    void Update()
    {
        if (!cycleActif) return;

        // Avancer le temps
        heureActuelle += (Time.deltaTime / dureeJourEnSecondes) * 24f * vitesseTemps;

        // Boucler sur 24h
        if (heureActuelle >= 24f)
            heureActuelle = 0f;

        MettreAJourCiel();
    }

    void MettreAJourCiel()
    {
        // Calculer le pourcentage de la journee (0 = minuit, 0.5 = midi)
        float pourcentageJour = heureActuelle / 24f;

        // === SOLEIL ===
        // Rotation du soleil (lever a l'est, coucher a l'ouest)
        float angleRotationSoleil = (heureActuelle - 6f) * 15f; // 6h = lever du soleil a 0 degres
        soleil.transform.rotation = Quaternion.Euler(angleRotationSoleil - 90f, 170f, 0f);

        // Couleur et intensite du soleil
        soleil.color = couleurSoleil.Evaluate(pourcentageJour);
        soleil.intensity = intensiteSoleil.Evaluate(pourcentageJour);

        // Determiner si c'est le jour ou la nuit
        bool estNuit = heureActuelle < 6f || heureActuelle > 18f;

        // Desactiver le soleil la nuit pour plus de realisme
        soleil.enabled = !estNuit;

        // === LUNE ===
        if (lune != null)
        {
            // La lune est opposee au soleil
            float angleRotationLune = angleRotationSoleil + 180f;
            lune.transform.rotation = Quaternion.Euler(angleRotationLune - 90f, 170f, 0f);
            lune.color = couleurLune;
            lune.intensity = estNuit ? intensiteLune : 0f;
            lune.enabled = estNuit;
        }

        // === CIEL (SKYBOX) ===
        if (skyboxMaterial != null)
        {
            Color couleurCielActuelle = couleurCiel.Evaluate(pourcentageJour);
            skyboxMaterial.SetColor("_SkyTint", couleurCielActuelle);

            // Si le skybox supporte une couleur d'horizon
            if (couleurHorizon != null)
            {
                Color couleurHorizonActuelle = couleurHorizon.Evaluate(pourcentageJour);
                skyboxMaterial.SetColor("_GroundColor", couleurHorizonActuelle);
            }

            // Exposition du skybox (plus sombre la nuit)
            float exposition = estNuit ? 0.3f : 1.2f;
            skyboxMaterial.SetFloat("_Exposure", exposition);
        }

        // === LUMIERE AMBIANTE ===
        Color couleurAmbiantActuelle = couleurAmbiant.Evaluate(pourcentageJour);
        RenderSettings.ambientLight = couleurAmbiantActuelle;

        // Intensite ambiante plus faible la nuit
        RenderSettings.ambientIntensity = estNuit ? 0.3f : 1f;

        // === BROUILLARD ===
        if (activerBrouillard)
        {
            RenderSettings.fogColor = couleurBrouillard.Evaluate(pourcentageJour);
            RenderSettings.fogDensity = densiteBrouillard.Evaluate(pourcentageJour);
        }

        // === ETOILES ===
        if (activerEtoiles && particlesEtoiles != null)
        {
            float visibilite = visibiliteEtoiles.Evaluate(pourcentageJour);
            var emission = particlesEtoiles.emission;
            emission.rateOverTime = visibilite * 100f; // Plus d'etoiles la nuit

            // Activer/desactiver les particules selon la visibilite
            if (visibilite > 0.1f && !particlesEtoiles.isPlaying)
            {
                particlesEtoiles.Play();
            }
            else if (visibilite <= 0.1f && particlesEtoiles.isPlaying)
            {
                particlesEtoiles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        // === NUAGES ===
        if (materielNuages != null && couleurNuages != null)
        {
            Color couleurNuagesActuelle = couleurNuages.Evaluate(pourcentageJour);
            materielNuages.color = couleurNuagesActuelle;
        }
    }

    void ConfigurerParametresParDefaut()
    {
        // === GRADIENT COULEUR SOLEIL ===
        if (couleurSoleil == null || couleurSoleil.colorKeys.Length == 0)
        {
            couleurSoleil = new Gradient();
            GradientColorKey[] couleursLumiere = new GradientColorKey[6];
            GradientAlphaKey[] alphas = new GradientAlphaKey[2];

            // Minuit (presque noir avec teinte bleue)
            couleursLumiere[0] = new GradientColorKey(new Color(0.05f, 0.08f, 0.15f), 0f);
            // Aube (orange rose)
            couleursLumiere[1] = new GradientColorKey(new Color(1f, 0.6f, 0.4f), 0.23f);
            // Matin (jaune dore)
            couleursLumiere[2] = new GradientColorKey(new Color(1f, 0.95f, 0.8f), 0.35f);
            // Midi (blanc leger)
            couleursLumiere[3] = new GradientColorKey(new Color(1f, 1f, 0.95f), 0.5f);
            // Crepuscule (orange chaud)
            couleursLumiere[4] = new GradientColorKey(new Color(1f, 0.5f, 0.3f), 0.77f);
            // Minuit
            couleursLumiere[5] = new GradientColorKey(new Color(0.05f, 0.08f, 0.15f), 1f);

            alphas[0] = new GradientAlphaKey(1f, 0f);
            alphas[1] = new GradientAlphaKey(1f, 1f);

            couleurSoleil.SetKeys(couleursLumiere, alphas);
        }

        // === GRADIENT COULEUR CIEL ===
        if (couleurCiel == null || couleurCiel.colorKeys.Length == 0)
        {
            couleurCiel = new Gradient();
            GradientColorKey[] couleursCiel = new GradientColorKey[6];
            GradientAlphaKey[] alphas = new GradientAlphaKey[2];

            // Nuit profonde (noir bleu)
            couleursCiel[0] = new GradientColorKey(new Color(0.02f, 0.04f, 0.1f), 0f);
            // Aube (rose orange)
            couleursCiel[1] = new GradientColorKey(new Color(1f, 0.6f, 0.5f), 0.23f);
            // Matin (bleu clair)
            couleursCiel[2] = new GradientColorKey(new Color(0.4f, 0.7f, 0.95f), 0.35f);
            // Jour (bleu ciel vif)
            couleursCiel[3] = new GradientColorKey(new Color(0.4f, 0.75f, 1f), 0.5f);
            // Crepuscule (violet orange)
            couleursCiel[4] = new GradientColorKey(new Color(0.9f, 0.4f, 0.6f), 0.77f);
            // Nuit profonde
            couleursCiel[5] = new GradientColorKey(new Color(0.02f, 0.04f, 0.1f), 1f);

            alphas[0] = new GradientAlphaKey(1f, 0f);
            alphas[1] = new GradientAlphaKey(1f, 1f);

            couleurCiel.SetKeys(couleursCiel, alphas);
        }

        // === GRADIENT COULEUR HORIZON ===
        if (couleurHorizon == null || couleurHorizon.colorKeys.Length == 0)
        {
            couleurHorizon = new Gradient();
            GradientColorKey[] couleursHorizon = new GradientColorKey[4];
            GradientAlphaKey[] alphas = new GradientAlphaKey[2];

            couleursHorizon[0] = new GradientColorKey(new Color(0.1f, 0.1f, 0.2f), 0f); // Nuit
            couleursHorizon[1] = new GradientColorKey(new Color(1f, 0.8f, 0.6f), 0.25f); // Aube
            couleursHorizon[2] = new GradientColorKey(new Color(0.8f, 0.9f, 1f), 0.5f); // Jour
            couleursHorizon[3] = new GradientColorKey(new Color(0.1f, 0.1f, 0.2f), 1f); // Nuit

            alphas[0] = new GradientAlphaKey(1f, 0f);
            alphas[1] = new GradientAlphaKey(1f, 1f);

            couleurHorizon.SetKeys(couleursHorizon, alphas);
        }

        // === GRADIENT LUMIERE AMBIANTE ===
        if (couleurAmbiant == null || couleurAmbiant.colorKeys.Length == 0)
        {
            couleurAmbiant = new Gradient();
            GradientColorKey[] couleursAmbiant = new GradientColorKey[4];
            GradientAlphaKey[] alphas = new GradientAlphaKey[2];

            couleursAmbiant[0] = new GradientColorKey(new Color(0.1f, 0.15f, 0.25f), 0f); // Nuit (tres sombre)
            couleursAmbiant[1] = new GradientColorKey(new Color(0.8f, 0.7f, 0.6f), 0.25f); // Aube
            couleursAmbiant[2] = new GradientColorKey(new Color(0.7f, 0.8f, 0.9f), 0.5f); // Jour
            couleursAmbiant[3] = new GradientColorKey(new Color(0.1f, 0.15f, 0.25f), 1f); // Nuit

            alphas[0] = new GradientAlphaKey(1f, 0f);
            alphas[1] = new GradientAlphaKey(1f, 1f);

            couleurAmbiant.SetKeys(couleursAmbiant, alphas);
        }

        // === GRADIENT COULEUR BROUILLARD ===
        if (couleurBrouillard == null || couleurBrouillard.colorKeys.Length == 0)
        {
            couleurBrouillard = new Gradient();
            GradientColorKey[] couleursFog = new GradientColorKey[4];
            GradientAlphaKey[] alphas = new GradientAlphaKey[2];

            couleursFog[0] = new GradientColorKey(new Color(0.05f, 0.08f, 0.15f), 0f); // Nuit
            couleursFog[1] = new GradientColorKey(new Color(0.9f, 0.7f, 0.6f), 0.25f); // Aube
            couleursFog[2] = new GradientColorKey(new Color(0.7f, 0.85f, 1f), 0.5f); // Jour
            couleursFog[3] = new GradientColorKey(new Color(0.05f, 0.08f, 0.15f), 1f); // Nuit

            alphas[0] = new GradientAlphaKey(1f, 0f);
            alphas[1] = new GradientAlphaKey(1f, 1f);

            couleurBrouillard.SetKeys(couleursFog, alphas);
        }

        // === GRADIENT COULEUR NUAGES ===
        if (couleurNuages == null || couleurNuages.colorKeys.Length == 0)
        {
            couleurNuages = new Gradient();
            GradientColorKey[] couleursNuages = new GradientColorKey[4];
            GradientAlphaKey[] alphas = new GradientAlphaKey[2];

            couleursNuages[0] = new GradientColorKey(new Color(0.15f, 0.15f, 0.25f), 0f); // Nuit (gris fonce)
            couleursNuages[1] = new GradientColorKey(new Color(1f, 0.8f, 0.7f), 0.25f); // Aube (rose)
            couleursNuages[2] = new GradientColorKey(new Color(1f, 1f, 1f), 0.5f); // Jour (blanc)
            couleursNuages[3] = new GradientColorKey(new Color(0.15f, 0.15f, 0.25f), 1f); // Nuit

            alphas[0] = new GradientAlphaKey(1f, 0f);
            alphas[1] = new GradientAlphaKey(1f, 1f);

            couleurNuages.SetKeys(couleursNuages, alphas);
        }

        // === COURBE INTENSITE SOLEIL ===
        if (intensiteSoleil == null || intensiteSoleil.length == 0)
        {
            intensiteSoleil = new AnimationCurve();
            intensiteSoleil.AddKey(0f, 0f);      // Minuit - eteint
            intensiteSoleil.AddKey(0.23f, 0.6f); // Aube
            intensiteSoleil.AddKey(0.5f, 1.5f);  // Midi - maximum
            intensiteSoleil.AddKey(0.77f, 0.6f); // Crepuscule
            intensiteSoleil.AddKey(1f, 0f);      // Minuit
        }

        // === COURBE DENSITE BROUILLARD ===
        if (densiteBrouillard == null || densiteBrouillard.length == 0)
        {
            densiteBrouillard = new AnimationCurve();
            densiteBrouillard.AddKey(0f, 0.01f);   // Nuit - leger
            densiteBrouillard.AddKey(0.23f, 0.03f); // Aube - plus dense
            densiteBrouillard.AddKey(0.5f, 0.005f); // Midi - clair
            densiteBrouillard.AddKey(0.77f, 0.03f); // Crepuscule - dense
            densiteBrouillard.AddKey(1f, 0.01f);   // Nuit
        }

        // === COURBE VISIBILITE ETOILES ===
        if (visibiliteEtoiles == null || visibiliteEtoiles.length == 0)
        {
            visibiliteEtoiles = new AnimationCurve();
            visibiliteEtoiles.AddKey(0f, 1f);    // Minuit - pleinement visible
            visibiliteEtoiles.AddKey(0.2f, 0.5f); // Aube - commence a disparaitre
            visibiliteEtoiles.AddKey(0.3f, 0f);   // Jour - invisible
            visibiliteEtoiles.AddKey(0.7f, 0f);   // Jour
            visibiliteEtoiles.AddKey(0.8f, 0.5f); // Crepuscule - reapparait
            visibiliteEtoiles.AddKey(1f, 1f);    // Minuit
        }
    }

    // === FONCTIONS UTILITAIRES PUBLIQUES ===

    /// <summary>
    /// Definir l'heure manuellement
    /// </summary>
    public void DefinirHeure(float heure)
    {
        heureActuelle = Mathf.Clamp(heure, 0f, 24f);
        MettreAJourCiel();
    }

    /// <summary>
    /// Activer/desactiver le cycle automatique
    /// </summary>
    public void ActiverCycle(bool actif)
    {
        cycleActif = actif;
    }

    /// <summary>
    /// Changer la vitesse du temps
    /// </summary>
    public void ChangerVitesseTemps(float vitesse)
    {
        vitesseTemps = Mathf.Max(0f, vitesse);
    }

    /// <summary>
    /// Obtenir l'heure actuelle
    /// </summary>
    public float ObtenirHeure()
    {
        return heureActuelle;
    }

    /// <summary>
    /// Verifier si c'est la nuit
    /// </summary>
    public bool EstNuit()
    {
        return heureActuelle < 6f || heureActuelle > 18f;
    }

    // === DEBUG ===
    void OnGUI()
    {
        if (afficherHeure)
        {
            int heures = Mathf.FloorToInt(heureActuelle);
            int minutes = Mathf.FloorToInt((heureActuelle - heures) * 60f);
            string heureFormatee = string.Format("{0:00}:{1:00}", heures, minutes);

            GUI.Label(new Rect(10, 110, 300, 20), "Heure: " + heureFormatee);
            GUI.Label(new Rect(10, 130, 300, 20), "Phase: " + (EstNuit() ? "NUIT" : "JOUR"));
        }
    }
}
