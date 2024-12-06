using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class controleJoueur : MonoBehaviour
{
    public GameObject joueur;
    private GameObject joueurObj;
    public float speed = 5f;
    public Texture2D customCursor;
    private Camera thirdPersonCamera;
    private Camera firstPersonCamera;
    private Vector3 targetPosition;
    private Camera maincamera;
    private bool isMoving = false;
    private bool onmaincamera = true;
    private bool isFirstPerson = false;
    private bool isNavigationMode = false;
    public bool canmoveperso;
    public Vector3 positionjoueur;

    void Start()
    {
        maincamera = Camera.main;
        joueurObj = Instantiate(joueur);
        joueurObj.GetComponent<DeplacementJoueur>().speed = speed;
        joueurObj.transform.position = positionjoueur;
        thirdPersonCamera = joueurObj.GetComponentsInChildren<Camera>()[1];
        firstPersonCamera = joueurObj.GetComponentsInChildren<Camera>()[0];
        thirdPersonCamera.gameObject.SetActive(false);
        firstPersonCamera.gameObject.SetActive(false);
        if (canmoveperso)
        {
            isNavigationMode = !isNavigationMode;
            onmaincamera = false;
            
            // Activation de la caméra en fonction du mode de vue
            maincamera.gameObject.SetActive(!isNavigationMode);
            thirdPersonCamera.gameObject.SetActive(isNavigationMode && !isFirstPerson);
            firstPersonCamera.gameObject.SetActive(isNavigationMode && isFirstPerson);

        }
        else
        {
            if (customCursor != null)
            {
                Cursor.SetCursor(customCursor, Vector2.zero, CursorMode.Auto);
            }
            else
            {
                Debug.LogWarning("La texture du curseur personnalisé n'est pas assignée.");
            }
        }
        joueurObj.GetComponent<DeplacementJoueur>().canmovekeyboard = canmoveperso;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isMoving = false;
            maincamera.gameObject.SetActive(true);
            onmaincamera = true;
            thirdPersonCamera.gameObject.SetActive(false);
            firstPersonCamera.gameObject.SetActive(false);
        }
        if (!canmoveperso)
        {
            if (onmaincamera == true)
            {
                HandleMovement();
            }
            if (isMoving)
            {
                if (joueurObj.GetComponent<DeplacementJoueur>().MoveCharacter(targetPosition) == false)
                {
                    isMoving = false;
                    maincamera.gameObject.SetActive(true);
                    onmaincamera = true;
                    thirdPersonCamera.gameObject.SetActive(false);
                    firstPersonCamera.gameObject.SetActive(false);
                    Cursor.SetCursor(customCursor, Vector2.zero, CursorMode.Auto);
                }
            }
        }
        // Basculer entre les vues première et troisième personne avec Ctrl en mode navigation
        if (isNavigationMode && Input.GetKeyDown(KeyCode.LeftControl))
        {
            ToggleCamera();
        }
       

    }

    void HandleMovement()
    {
        if (Input.GetMouseButtonDown(0)) // appui du clic gauche
        {
                Ray ray = maincamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    isNavigationMode = !isNavigationMode;
                    targetPosition = hit.point;
                    isMoving = true;
                    maincamera.gameObject.SetActive(!isNavigationMode);
                    thirdPersonCamera.gameObject.SetActive(isNavigationMode && !isFirstPerson);
                    firstPersonCamera.gameObject.SetActive(isNavigationMode && isFirstPerson);
                    joueurObj.GetComponent<DeplacementJoueur>().ChangeRotation(targetPosition);
                    maincamera.gameObject.SetActive(false);
                    onmaincamera = false;
                    thirdPersonCamera.gameObject.SetActive(true);
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                }

                
            }
    }
    void ToggleCamera()
    {
        isFirstPerson = !isFirstPerson;
        thirdPersonCamera.gameObject.SetActive(!isFirstPerson);
        firstPersonCamera.gameObject.SetActive(isFirstPerson);
    }

    private void OnDestroy()
    {
        Destroy(joueurObj);
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        maincamera.gameObject.SetActive(true);
    }
}
