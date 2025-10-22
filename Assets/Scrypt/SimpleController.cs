using UnityEngine;
using UnityEngine.InputSystem;

public class SimplestController : MonoBehaviour
{
    [Header("Mouvement")]
    public float vitesse = 5f;

    [Header("Caméra")]
    public Transform camera;
    public float sensibiliteSouris = 0.2f;

    private CharacterController controller;
    private float velociteY = 0f;
    private float rotationX = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
      
        if (Mouse.current != null)
        {
            Vector2 souris = Mouse.current.delta.ReadValue();

            transform.Rotate(Vector3.up * souris.x * sensibiliteSouris);

            rotationX -= souris.y * sensibiliteSouris;
            rotationX = Mathf.Clamp(rotationX, -10f, 30f); 

            if (camera != null)
            {
                camera.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
            }
        }

        if (controller.isGrounded)
        {
            velociteY = -2f;
        }
        else
        {
            velociteY -= 9.81f * Time.deltaTime;
        }

        
        Vector2 input = Keyboard.current != null ?
            new Vector2(
                (Keyboard.current.dKey.isPressed ? 1 : 0) + (Keyboard.current.aKey.isPressed ? -1 : 0),
                (Keyboard.current.wKey.isPressed ? 1 : 0) + (Keyboard.current.sKey.isPressed ? -1 : 0)
            ) : Vector2.zero;

        Vector3 deplacement = transform.right * input.x + transform.forward * input.y;
        Vector3 mouvementFinal = deplacement * vitesse + Vector3.up * velociteY;

        controller.Move(mouvementFinal * Time.deltaTime);

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
