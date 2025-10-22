using UnityEngine;

public class DroneController : MonoBehaviour
{
    [Header("Paramètres de mouvement")]
    public float vitesseDeplacement = 2f;
    public float vitesseSprint = 4f; // Vitesse en sprint
    public float vitesseMonteeDescente = 0.5f;

    [Header("Paramètres d'altitude")]
    public float chuteLente = 0.5f;
    public float altitudeMax = 50f;
    public float altitudeMin = 0f;

    [Header("Sensibilité souris")]
    public float sensibiliteSourisX = 2f;
    public float sensibiliteSourisY = 2f;

    [Header("Caméras")]
    public Camera cameraFPS; // Caméra première personne
    public Camera cameraTPS; // Caméra troisième personne
    public float distanceTPS = 5f; // Distance de la caméra TPS
    public float hauteurTPS = 2f; // Hauteur de la caméra TPS

    private float rotationX = 0f;
    private float rotationY = 0f;
    private bool estEnFPS = true; // true = FPS, false = TPS

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Activer la caméra FPS par défaut
        ActiverCameraFPS();
    }

    void Update()
    {
        GererRotationCamera();
        GererDeplacement();
        GererAltitude();
        LimiterAltitude();
        GererSwitchCamera();

        // Si en TPS, positionner la caméra
        if (!estEnFPS)
        {
            PositionnerCameraTPS();
        }

        // Déverrouiller curseur avec Echap
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Reverrouiller avec clic
        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void GererSwitchCamera()
    {
        // Touche Entrée pour changer de caméra
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            estEnFPS = !estEnFPS;

            if (estEnFPS)
            {
                ActiverCameraFPS();
            }
            else
            {
                ActiverCameraTPS();
            }
        }
    }

    void ActiverCameraFPS()
    {
        if (cameraFPS != null) cameraFPS.enabled = true;
        if (cameraTPS != null) cameraTPS.enabled = false;
        Debug.Log("Mode: Première Personne");
    }

    void ActiverCameraTPS()
    {
        if (cameraFPS != null) cameraFPS.enabled = false;
        if (cameraTPS != null) cameraTPS.enabled = true;
        Debug.Log("Mode: Troisième Personne");
    }

    void PositionnerCameraTPS()
    {
        if (cameraTPS == null) return;

        // Position derrière le drone
        Vector3 positionCible = transform.position - transform.forward * distanceTPS + Vector3.up * hauteurTPS;

        // Déplacer la caméra en douceur (optionnel)
        cameraTPS.transform.position = Vector3.Lerp(cameraTPS.transform.position, positionCible, Time.deltaTime * 10f);

        // Faire regarder le drone
        cameraTPS.transform.LookAt(transform.position + Vector3.up * hauteurTPS * 0.5f);
    }

    void GererRotationCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensibiliteSourisX;
        float mouseY = Input.GetAxis("Mouse Y") * sensibiliteSourisY;

        rotationY += mouseX;
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);

        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0f);

        // Si en FPS, synchroniser la rotation de la caméra
        if (estEnFPS && cameraFPS != null)
        {
            cameraFPS.transform.rotation = transform.rotation;
        }
    }

    void GererDeplacement()
    {
        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(KeyCode.Z)) vertical = 1f;
        if (Input.GetKey(KeyCode.S)) vertical = -1f;
        if (Input.GetKey(KeyCode.Q)) horizontal = -1f;
        if (Input.GetKey(KeyCode.D)) horizontal = 1f;

        float vitesseActuelle = Input.GetKey(KeyCode.LeftShift) ? vitesseSprint : vitesseDeplacement;

        Vector3 direction = (transform.forward * vertical + transform.right * horizontal).normalized;
        transform.position += direction * vitesseActuelle * Time.deltaTime;
    }

    void GererAltitude()
    {
        float deplVertical = 0f;

        if (Input.GetKey(KeyCode.Space))
        {
            deplVertical = vitesseMonteeDescente * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            deplVertical = -vitesseMonteeDescente * Time.deltaTime;
        }
        else
        {
            deplVertical = -chuteLente * Time.deltaTime;
        }

        transform.position += new Vector3(0, deplVertical, 0);
    }

    void LimiterAltitude()
    {
        Vector3 pos = transform.position;

        if (pos.y > altitudeMax)
        {
            pos.y = altitudeMax;
        }

        if (pos.y < altitudeMin)
        {
            pos.y = altitudeMin;
        }

        transform.position = pos;
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 20), "Altitude: " + transform.position.y.ToString("F1") + "m");
        GUI.Label(new Rect(10, 30, 300, 20), "Caméra: " + (estEnFPS ? "Première Personne" : "Troisième Personne"));
        GUI.Label(new Rect(10, 50, 300, 20), "Appuyez sur Entrée pour changer");
    }
}
