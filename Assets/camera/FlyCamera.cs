using UnityEngine;

public class FlyCamera : MonoBehaviour
{
    [Header("Camera")]
    public float sensitivity = 90f;
    public float climbSpeed = 4f;
    public float normalMoveSpeed = 20f;
    public float slowMoveSpeed = 0.5f;
    public float fastMoveSpeed = 2f;
    public Vector2 rotationLimitsX;
    public Vector2 rotationLimitsY;
    public bool limitXRotation = false;
    public bool limitYRotation = false;

    private Vector2 cameraRotation;

    // Variable pour contrôler si la caméra peut bouger
    public bool canMove = true;

    // Update is called once per frame
    private void LateUpdate()
    {
        // Si la caméra ne peut pas bouger, on arrête l'exécution ici
        if (!canMove) return;

        cameraRotation.x += Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        cameraRotation.y += Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        if (limitXRotation)
        {
            cameraRotation.x = Mathf.Clamp(cameraRotation.x, rotationLimitsX.x, rotationLimitsX.y);
        }
        if (limitYRotation)
        {
            cameraRotation.y = Mathf.Clamp(cameraRotation.y, rotationLimitsY.x, rotationLimitsY.y);
        }

        transform.localRotation = Quaternion.AngleAxis(cameraRotation.x, Vector3.up);
        transform.localRotation *= Quaternion.AngleAxis(cameraRotation.y, Vector3.left);

        if (Input.GetKey(KeyCode.LeftShift))
        {
            transform.position += transform.right * (normalMoveSpeed * fastMoveSpeed) * Input.GetAxis("Horizontal") * Time.deltaTime;
            transform.position += transform.forward * (normalMoveSpeed * fastMoveSpeed) * Input.GetAxis("Vertical") * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            transform.position += transform.right * (normalMoveSpeed * slowMoveSpeed) * Input.GetAxis("Horizontal") * Time.deltaTime;
            transform.position += transform.forward * (normalMoveSpeed * slowMoveSpeed) * Input.GetAxis("Vertical") * Time.deltaTime;
        }
        else
        {
            transform.position += transform.right * normalMoveSpeed * Input.GetAxis("Horizontal") * Time.deltaTime;
            transform.position += transform.forward * normalMoveSpeed * Input.GetAxis("Vertical") * Time.deltaTime;
        }

        if (Input.GetKeyUp(KeyCode.A))
        {
            transform.position += transform.up * climbSpeed * Time.deltaTime;
        }

        if (Input.GetKeyUp(KeyCode.Z))
        {
            transform.position -= transform.up * climbSpeed * Time.deltaTime;
        }
    }
}
