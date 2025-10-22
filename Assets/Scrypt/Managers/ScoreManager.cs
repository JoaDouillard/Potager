using UnityEngine;
using TMPro;


// Singleton qui gère le score du joueur
public class ScoreManager : MonoBehaviour
{
    // Singleton instance
    public static ScoreManager Instance { get; private set; }

    [Header("Score")]
    [Tooltip("Score actuel du joueur")]
    public int scoreActuel = 0;

    [Tooltip("Score à atteindre pour gagner (optionnel, 0 = infini)")]
    public int scoreObjectif = 0;

    [Header("UI")]
    [Tooltip("TextMeshPro pour afficher le score")]
    public TextMeshProUGUI texteScore;

    [Tooltip("Format d'affichage du score")]
    public string formatAffichage = "Score: {0}";

    [Header("Events (optionnel)")]
    [Tooltip("Déclenché quand l'objectif est atteint")]
    public UnityEngine.Events.UnityEvent onObjectifAtteint;

    [Header("Debug")]
    public bool afficherDebug = true;

    private bool objectifAtteint = false;

    void Awake()
    {
        // Pattern Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            Debug.LogWarning("[ScoreManager] Une instance existe déjà, destruction de ce doublon.");
            return;
        }

        Instance = this;

        // Optionnel : persister entre les scènes
        // DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Initialiser l'affichage du score
        MettreAJourAffichage();

        if (afficherDebug)
        {
            Debug.Log($"[ScoreManager] Initialisé. Objectif: {(scoreObjectif > 0 ? scoreObjectif.ToString() : "Aucun")}");
        }
    }


    // Ajoute des points au score
    public void AjouterPoints(int points)
    {
        scoreActuel += points;

        if (afficherDebug)
        {
            Debug.Log($"[ScoreManager] +{points} points ! Score total: {scoreActuel}");
        }

        // Mettre à jour l'affichage
        MettreAJourAffichage();

        // Vérifier si l'objectif est atteint
        VerifierObjectif();
    }

    // Retire des points au score (utile pour les pénalités)
    public void RetirerPoints(int points)
    {
        scoreActuel = Mathf.Max(0, scoreActuel - points);

        if (afficherDebug)
        {
            Debug.Log($"[ScoreManager] -{points} points. Score total: {scoreActuel}");
        }

        MettreAJourAffichage();
    }

    public void ReinitialiserScore()
    {
        scoreActuel = 0;
        objectifAtteint = false;

        if (afficherDebug)
        {
            Debug.Log("[ScoreManager] Score réinitialisé.");
        }

        MettreAJourAffichage();
    }


    // Met à jour l'affichage du score dans l'UI
    void MettreAJourAffichage()
    {
        if (texteScore != null)
        {
            texteScore.text = string.Format(formatAffichage, scoreActuel);
        }
    }


    // Vérifie si l'objectif de score est atteint
    void VerifierObjectif()
    {
        if (scoreObjectif > 0 && !objectifAtteint && scoreActuel >= scoreObjectif)
        {
            objectifAtteint = true;

            if (afficherDebug)
            {
                Debug.Log($"[ScoreManager] OBJECTIF ATTEINT ! Score: {scoreActuel}/{scoreObjectif}");
            }

            // Déclencher l'événement
            onObjectifAtteint?.Invoke();
        }
    }

    // Retourne le score actuel
    public int ObtenirScore()
    {
        return scoreActuel;
    }


    // Vérifie si l'objectif a été atteint
    public bool EstObjectifAtteint()
    {
        return objectifAtteint;
    }

    // Affiche les informations du ScoreManager dans l'inspecteur
    void OnGUI()
    {
        if (afficherDebug && texteScore == null)
        {
            // Affichage de secours si pas d'UI assignée
            GUI.Label(new Rect(10, 70, 300, 20), $"Score: {scoreActuel}");

            if (scoreObjectif > 0)
            {
                GUI.Label(new Rect(10, 90, 300, 20), $"Objectif: {scoreObjectif}");
            }
        }
    }
}
