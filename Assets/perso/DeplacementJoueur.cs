using UnityEngine;

public class DeplacementJoueur : MonoBehaviour
{
    public float speed = 5f;
    public float sensitivityX = 60f; 

    public bool canmovekeyboard;
    private Camera cameraperso;
    private void Start()
    {
        cameraperso = GetComponent<Camera>();
    }
    private void Update()
    {
        if (canmovekeyboard)
        {
            MoveCharacterWithKeys();
        }
    }
    private void MoveCharacterWithKeys()
    {
        Vector3 direction = Vector3.zero;

        // Vérifie les entrées utilisateur pour les touches ZQSD
        if (Input.GetKey(KeyCode.W)) direction += transform.forward;
        if (Input.GetKey(KeyCode.S)) direction -= transform.forward;
        if (Input.GetKey(KeyCode.A)) direction -= transform.right;
        if (Input.GetKey(KeyCode.D)) direction += transform.right;

        transform.position += direction.normalized * speed * Time.deltaTime;

        float mouseX = Input.GetAxis("Mouse X") * sensitivityX * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);
    }

    public bool MoveCharacter(Vector3 targetPosition)
    {
        Vector3 movement = speed * Time.deltaTime * (targetPosition - transform.position).normalized;
        transform.position += movement;

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            return false;
        }
        return true;
    }

    public void ChangeRotation(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;
        transform.rotation = Quaternion.LookRotation(direction);
    }
}
