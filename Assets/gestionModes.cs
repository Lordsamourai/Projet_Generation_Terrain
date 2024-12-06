using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gestionModes : MonoBehaviour
{
    [Header("Deplacement Perso")]
    public GameObject Perso;
    public float SpeedPerso;
    public Texture2D CursorNav;
    private controleJoueur scriptmovement;

    [Header("Information")]
    public Canvas canvasinfo;

    private bool isinonemode = false;
    //private bool movementcamera = true;
    private bool affichefenetre = false;
    private GameObject genterrain;
    private GameObject camaddchunk;
    private Camera maincamera;

    [Header("Génération Terrain")]
    public int resolution;
    public int dimension;
    //public int tailleCote;
    public int tailleCoteX;
    public int tailleCoteY;
    private Vector3 positionjoueur;
    // Start is called before the first frame update
    void Start()
    {
        positionjoueur = new Vector3(0, 2, 0);
        maincamera = Camera.main;
        camaddchunk = new GameObject();
        camaddchunk.AddComponent<Camera>();
        camaddchunk.name = "Cameraajoutchunks";
        
        genterrain = new GameObject();
        genterrain.name = "Génération Terrain";
        genterrain.AddComponent<Generationterrain>();
        genterrain.GetComponent<Generationterrain>().resolution = resolution;
        genterrain.GetComponent<Generationterrain>().dimension = dimension;
        genterrain.GetComponent<Generationterrain>().tailleCoteX = tailleCoteX;
        genterrain.GetComponent<Generationterrain>().tailleCoteY = tailleCoteY;
        genterrain.GetComponent<Generationterrain>().isaddingchunk = false;
        genterrain.GetComponent<Generationterrain>().camaddchunk = camaddchunk;
        //genterrain.AddComponent<UndoRedo>();
        gameObject.AddComponent<CanvasInformation>();
        gameObject.GetComponent<CanvasInformation>().panelinfo = canvasinfo;

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isinonemode)
            {
                genterrain.gameObject.SetActive(true);
                isinonemode = false;
                Destroy(gameObject.GetComponent<controleJoueur>());
                gameObject.GetComponent<Controlor>().Deactivate();
                maincamera.gameObject.SetActive(true);
                genterrain.GetComponent<Generationterrain>().isaddingchunk = false;
                genterrain.GetComponent<Generationterrain>().camaddchunk.gameObject.SetActive(false);
            }
        }
        
        if (Input.GetKeyDown(KeyCode.F2) && !isinonemode)
        {
            genterrain.gameObject.SetActive(false);
            isinonemode = true;
            gameObject.AddComponent<controleJoueur>();
            gameObject.GetComponent<controleJoueur>().joueur = Perso;
            gameObject.GetComponent<controleJoueur>().speed= SpeedPerso;
            gameObject.GetComponent<controleJoueur>().customCursor = CursorNav;
            gameObject.GetComponent<controleJoueur>().positionjoueur = positionjoueur;
            gameObject.GetComponent<controleJoueur>().canmoveperso = false;
            genterrain.GetComponent<Generationterrain>().camaddchunk.gameObject.SetActive(false) ;
            genterrain.GetComponent<Generationterrain>().isaddingchunk = false;
            
        }
        if (Input.GetKeyDown(KeyCode.F3) && !isinonemode)
        {
            genterrain.gameObject.SetActive(false);
            isinonemode = true;
            gameObject.AddComponent<controleJoueur>();
            gameObject.GetComponent<controleJoueur>().joueur = Perso;
            gameObject.GetComponent<controleJoueur>().speed = SpeedPerso;
            gameObject.GetComponent<controleJoueur>().customCursor = CursorNav;
            gameObject.GetComponent<controleJoueur>().positionjoueur = positionjoueur;
            gameObject.GetComponent<controleJoueur>().canmoveperso = true;
            genterrain.GetComponent<Generationterrain>().camaddchunk.gameObject.SetActive(false);
            genterrain.GetComponent<Generationterrain>().isaddingchunk = false;
        }
        if (Input.GetKeyDown(KeyCode.F1) && !gameObject.GetComponent<Controlor>().isPanelActive)
        {
            affichefenetre = !affichefenetre;
            gameObject.GetComponent<CanvasInformation>().ToggleOnOff();
        }
        if (affichefenetre)
        {
            gameObject.GetComponent<CanvasInformation>().GetCanvasInstance().worldCamera = Camera.main;
        }
        if (Input.GetKeyDown(KeyCode.F5) && !isinonemode)
        {
            isinonemode = !isinonemode;
            genterrain.GetComponent<Generationterrain>().isaddingchunk = !genterrain.GetComponent<Generationterrain>().isaddingchunk;
            genterrain.GetComponent<Generationterrain>().camaddchunk.gameObject.SetActive(genterrain.GetComponent<Generationterrain>().isaddingchunk);
            maincamera.gameObject.SetActive(!genterrain.GetComponent<Generationterrain>().isaddingchunk);
        }
        if(Input.GetKeyDown(KeyCode.F10) && !isinonemode)
        {
            if (gameObject.GetComponent<CanvasInformation>().iswindowopen)
                gameObject.GetComponent<CanvasInformation>().ToggleOnOff();
            isinonemode = !isinonemode;
            gameObject.GetComponent<Controlor>().Activate();
        }
    }
    public bool getIsInOneMode() { return isinonemode; }
    public void setIsInOneMode(bool set) { isinonemode = set; }
}
