using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
//using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

public class CanvasInformation : MonoBehaviour
{
    public Canvas panelinfo; // The Canvas prefab
    private Canvas panelinfoInstance; // Reference to the Canvas instance
    public bool iswindowopen = false; // Modal window state

    private gestionModes terrainGesMod;
    private Generationterrain terrainGenTer;

    private string texteDeplacements;

    private string texteExo2;
    private string texteMode;

    private string texteExo7;
    private string textePattern;
    private string texteNomPattern;

    // objets des textes TMP
    private TMP_Text texteDeplacementsObject;
    private TMP_Text texteExo2Object;
    private TMP_Text texteExo7Object;

    // variables du dernier type de pattern et indice enregistrés
    private bool lastActiveCurvesState;
    private int lastBrushIndex;
    private int lastCurveIndex;

    void Start()
    {
        terrainGesMod = GameObject.Find("gestionmode").GetComponent<gestionModes>();
        terrainGenTer = GameObject.Find("Génération Terrain").GetComponent<Generationterrain>();

        panelinfoInstance = Instantiate(panelinfo);
        panelinfoInstance.renderMode = RenderMode.ScreenSpaceCamera;
        panelinfoInstance.worldCamera = Camera.main;
        panelinfoInstance.planeDistance = 1f;
        panelinfoInstance.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        panelinfoInstance.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        panelinfoInstance.GetComponent<CanvasScaler>().matchWidthOrHeight = 1.0f;
        panelinfoInstance.enabled = false;
        panelinfoInstance.gameObject.SetActive(false);

        // texte déplacements
        texteDeplacements = "Déplacement caméra : ZQSD + flèches\n" +
                            "Accélération : Shift gauche\n" +
                            "Tourner le terrain : Ctrl droit + flèches\n" +
                            "Elever le terrain : Click gauche\n" + "Creuser le terrain : Ctrl gauche + Click gauche\n" +
                            "Augmenter et diminuer la déformation: Alt et Altgr\n" +
                            "Modifiaction du rayon : + et -\n" +
                            "Changer resolution et dimension : F10\n" +
                            "Ajouter des chunks: F5 puis flèches\n" +
                            "Passer en mode déplacement du perso à la souris : F2\n"+
                            "Passer en mode déplacement du perso au clavier : F3\n" +
                            "Passer au pattern suivant : P\n" +
                            "Passer au brush suivant : B\n" +
                            "Passer au calcul de distance suivant pour le tir : F\n" +
                            "Passer au calcul de distance suivant pour les voisins : V\n"+
                            "Changer calcul normales: N\n";
                            
        texteDeplacementsObject = CreateText(texteDeplacements, new Vector3(75, -400, 0));

        // texte mode (et propriétés)
        UpdateModeTexte();

        // texte pattern
        UpdatePatternTexte();

        // on initialise les variables de "mémoire"
        lastActiveCurvesState = terrainGenTer.getActiveCurves();
        lastBrushIndex = terrainGenTer.getCurrBrushIndex();
        lastCurveIndex = terrainGenTer.getCurrCurveIndex();
    }

    void Update()
    {
        // on vérifie si le mode change
        if ((terrainGesMod.getIsInOneMode() && texteMode != "Déplacement joueur") ||
            (!terrainGesMod.getIsInOneMode() && texteMode != "Déformation terrain"))
        {
            UpdateModeTexte();
        }

        // variables pour ensuite comparer aux variables "mémoire"
        bool currentActiveCurvesState = terrainGenTer.getActiveCurves();
        int currentBrushIndex = terrainGenTer.getCurrBrushIndex();
        int currentCurveIndex = terrainGenTer.getCurrCurveIndex();

        // on vérifie si le pattern change
        if (currentActiveCurvesState != lastActiveCurvesState ||
            (currentActiveCurvesState && currentCurveIndex != lastCurveIndex) ||
            (!currentActiveCurvesState && currentBrushIndex != lastBrushIndex))
        {
            UpdatePatternTexte();

            // on update les derniers états connus
            lastActiveCurvesState = currentActiveCurvesState;
            lastBrushIndex = currentBrushIndex;
            lastCurveIndex = currentCurveIndex;
        }
    }

     public void UpdateModeTexte()
    {
        // on récupère le mode actuel
        texteMode = terrainGesMod.getIsInOneMode() ? "Déplacement joueur" : "Déformation terrain";

        // et on update texteExo2
        texteExo2 = "Dimension : " + terrainGenTer.dimension +
                    "\nRésolution : " + terrainGenTer.resolution +
                    "\nTaille : " + terrainGenTer.tailleCoteX + "x" + terrainGenTer.tailleCoteY + " chunks" +
                    "\nNombre vertices : " + terrainGenTer.resolution* terrainGenTer.resolution +
                    "\nNombre triangles : " + 2 * ((terrainGenTer.resolution-1)* (terrainGenTer.resolution - 1)) + 
                    "\nMode actuel : " + texteMode;

        // update direct du texte sans créer de nouvel objet
        if (texteExo2Object == null)
        {
            texteExo2Object = CreateText(texteExo2, new Vector3(1250, -150, 0));
        }
        else
        {
            texteExo2Object.text = texteExo2;
        }
    }


    void UpdatePatternTexte()
    {
        // on récupère le pattern actuel
        //textePattern = terrainGenTer.getActiveCurves() ? "Curve" : "Brush";
        //texteNomPattern = terrainGenTer.getCurrBrushIndex();

        if (terrainGenTer.getActiveCurves())
        {
            textePattern = "Curve";
            texteNomPattern = terrainGenTer.getCurrCurveIndex().ToString();
        }
        else
        {
            textePattern = "Brush";
            texteNomPattern = terrainGenTer.getCurrBrushIndex().ToString();
        }

        // et on update texteExo7
        texteExo7 = "Pattern actuel : " + textePattern +
                    "\nPattern n°" + texteNomPattern;

        // update direct du texte sans créer de nouvel objet
        if (texteExo7Object == null)
        {
            texteExo7Object = CreateText(texteExo7, new Vector3(1250, -350, 0));
        }
        else
        {
            texteExo7Object.text = texteExo7;
        }
    }


    TMP_Text CreateText(string textContent, Vector3 textPosition)
    {
        GameObject textObject = new("Texte");
        textObject.transform.SetParent(panelinfoInstance.transform);

        // le texte
        TMP_Text myText = textObject.AddComponent<TextMeshProUGUI>();
        myText.text = textContent;
        myText.fontSize = 50;
        myText.alignment = TextAlignmentOptions.Left;

        // police
        myText.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/Arial SDF");

        // on ajuste le RectTransform
        RectTransform rectTransform = myText.GetComponent<RectTransform>();

        rectTransform.localPosition = textPosition;
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        rectTransform.localRotation = Quaternion.identity;

        return myText;
    }
    public void ToggleOnOff()
    {
        iswindowopen = !iswindowopen;
        panelinfoInstance.enabled = iswindowopen;
        panelinfoInstance.gameObject.SetActive(iswindowopen);
    }
    

    public Canvas GetCanvasInstance() { return panelinfoInstance; }
}
