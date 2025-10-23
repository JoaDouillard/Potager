using UnityEngine;

public class DroneController : MonoBehaviour
{
    [Header("Mouvement")]
    public float vitesseDeplacement = 5f;
    public float vitesseSprint = 10f;
    public float vitesseMonteeDescente = 3f;
    public float chuteLente = 0.5f;

    [Header("Altitude")]
    public float altitudeMin = 0.5f;
    public float altitudeMax = 50f;

    [Header("Inclinaison du drone")]
    public float angleInclinaisonMax = 3f;
    public float vitesseInclinaison = 5f;

    [Header("Camera")]
    public Camera cameraJoueur;
    public float sensibiliteSourisX = 2f;
    public float sensibiliteSourisY = 2f;
    public Vector3 offsetCameraFPS = new Vector3(0f, 0.5f, 0f);
    public Vector3 offsetCameraTPS = new Vector3(0f, 3f, -8f);
    public float vitesseTransitionCamera = 25f;
    public KeyCode toucheSwitchCamera = KeyCode.V;
    public float limiteInclinaisonCameraHaut = 30f;
    public float limiteInclinaisonCameraBas = 10f;

    [Header("Plantation")]
    public InfoGraine[] typesGrainesDisponibles;
    public int indexGraineCourante = 0;
    public KeyCode touchePlantation = KeyCode.E;
    public KeyCode toucheChangerGraine = KeyCode.R;
    public float rayonDetectionZone = 3f;

    private float rotationCameraX = 0f;
    private float rotationCameraY = 0f;
    private bool estEnFPS = true;
    private Vector3 offsetCameraCible;
    private Vector3 velociteCamera;
    private ZonePlantation zoneProcheActuelle;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraJoueur == null)
        {
            cameraJoueur = Camera.main;
        }

        offsetCameraCible = offsetCameraFPS;

        if (cameraJoueur != null)
        {
            cameraJoueur.transform.SetParent(null);
        }
    }

    void Update()
    {
        GererRotationCamera();
        GererDeplacement();
        GererAltitude();
        LimiterAltitude();
        GererInclinaisonDrone();
        GererSwitchCamera();
        DetecterZonePlantation();
        GererChangementGraine();
        GererPlantation();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void LateUpdate()
    {
        PositionnerCamera();
    }

    void GererRotationCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensibiliteSourisX;
        float mouseY = Input.GetAxis("Mouse Y") * sensibiliteSourisY;

        rotationCameraY += mouseX;
        rotationCameraX -= mouseY;
        rotationCameraX = Mathf.Clamp(rotationCameraX, limiteInclinaisonCameraBas, limiteInclinaisonCameraHaut);
    }

    void PositionnerCamera()
    {
        if (cameraJoueur == null) return;

        offsetCameraCible = estEnFPS ? offsetCameraFPS : offsetCameraTPS;

        Vector3 positionCible = transform.position + transform.TransformDirection(offsetCameraCible);

        cameraJoueur.transform.position = Vector3.SmoothDamp(
            cameraJoueur.transform.position,
            positionCible,
            ref velociteCamera,
            1f / vitesseTransitionCamera
        );

        cameraJoueur.transform.rotation = Quaternion.Euler(rotationCameraX, rotationCameraY, 0f);
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

        Vector3 directionCamera = Quaternion.Euler(0f, rotationCameraY, 0f) * Vector3.forward;
        Vector3 droiteCamera = Quaternion.Euler(0f, rotationCameraY, 0f) * Vector3.right;

        Vector3 direction = (directionCamera * vertical + droiteCamera * horizontal).normalized;

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

    void GererInclinaisonDrone()
    {
        float vertical = 0f;
        float horizontal = 0f;

        if (Input.GetKey(KeyCode.Z)) vertical = -1f;
        if (Input.GetKey(KeyCode.S)) vertical = 1f;
        if (Input.GetKey(KeyCode.Q)) horizontal = 1f;
        if (Input.GetKey(KeyCode.D)) horizontal = -1f;

        float inclinaisonAvant = -vertical * angleInclinaisonMax;
        float inclinaisonCote = horizontal * angleInclinaisonMax;

        Quaternion rotationBase = Quaternion.Euler(0, rotationCameraY, 0f);
        Quaternion inclinaison = Quaternion.Euler(inclinaisonAvant, 0f, inclinaisonCote);
        Quaternion rotationCible = rotationBase * inclinaison;

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            rotationCible,
            Time.deltaTime * vitesseInclinaison
        );
    }

    void GererSwitchCamera()
    {
        if (Input.GetKeyDown(toucheSwitchCamera))
        {
            estEnFPS = !estEnFPS;
            Debug.Log(estEnFPS ? "Mode: Premiere Personne" : "Mode: Troisieme Personne");
        }
    }

    void DetecterZonePlantation()
    {
        Collider[] zonesProches = Physics.OverlapSphere(transform.position, rayonDetectionZone);

        zoneProcheActuelle = null;

        foreach (Collider col in zonesProches)
        {
            ZonePlantation zone = col.GetComponent<ZonePlantation>();
            if (zone != null && zone.PeutPlanter())
            {
                zoneProcheActuelle = zone;
                break;
            }
        }
    }

    void GererChangementGraine()
    {
        if (Input.GetKeyDown(toucheChangerGraine))
        {
            if (typesGrainesDisponibles == null || typesGrainesDisponibles.Length == 0)
            {
                Debug.LogWarning("[DroneController] Aucune graine disponible !");
                return;
            }

            indexGraineCourante = (indexGraineCourante + 1) % typesGrainesDisponibles.Length;
            Debug.Log($"[DroneController] Graine selectionnee : {typesGrainesDisponibles[indexGraineCourante].type}");
        }
    }

    void GererPlantation()
    {
        if (Input.GetKeyDown(touchePlantation))
        {
            PlanterGraine();
        }
    }

    void PlanterGraine()
    {
        if (typesGrainesDisponibles == null || typesGrainesDisponibles.Length == 0)
        {
            Debug.LogWarning("[DroneController] Aucune graine disponible !");
            return;
        }

        if (zoneProcheActuelle == null)
        {
            Debug.LogWarning("[DroneController] Aucune zone de plantation proche !");
            return;
        }

        if (!zoneProcheActuelle.PeutPlanter())
        {
            Debug.LogWarning("[DroneController] Zone deja occupee !");
            return;
        }

        InfoGraine graineInfo = typesGrainesDisponibles[indexGraineCourante];

        zoneProcheActuelle.PlanterGraine(graineInfo.type, graineInfo);

        Debug.Log($"[DroneController] {graineInfo.type} plantee dans zone {zoneProcheActuelle.name}");
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 20), "Altitude: " + transform.position.y.ToString("F1") + "m");
        GUI.Label(new Rect(10, 30, 300, 20), "Camera: " + (estEnFPS ? "Premiere Personne (FPS)" : "Troisieme Personne (TPS)"));
        GUI.Label(new Rect(10, 50, 300, 20), "Appuyez sur " + toucheSwitchCamera.ToString() + " pour changer");

        if (typesGrainesDisponibles != null && typesGrainesDisponibles.Length > 0)
        {
            string graineCourante = typesGrainesDisponibles[indexGraineCourante].type.ToString();
            GUI.Label(new Rect(10, 70, 300, 20), "Graine: " + graineCourante + " (R pour changer)");
        }

        if (zoneProcheActuelle != null && zoneProcheActuelle.PeutPlanter())
        {
            GUI.Label(new Rect(10, 90, 300, 20), "Appuyez sur " + touchePlantation.ToString() + " pour planter");
        }
        else if (zoneProcheActuelle != null)
        {
            GUI.Label(new Rect(10, 90, 300, 20), "Zone deja occupee");
        }
    }
}
