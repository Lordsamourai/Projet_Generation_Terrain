using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.XR;
using Button = UnityEngine.UI.Button;
using Slider = UnityEngine.UI.Slider;

public class Controlor : MonoBehaviour
{
    //public Canvas canvas;
    public Slider SliderDimension;
    public Slider SliderResolution;
    public Canvas Canvasinst;
    private FlyCamera flyCamera; // Référence au script FlyCamera
    public bool isPanelActive = false;
    public int maxDimRes;
    public int minDimRes;
    private Generationterrain terrainGenTer;
    void Start()
    {
        terrainGenTer = GameObject.Find("Génération Terrain").GetComponent<Generationterrain>();
        // Canvasinst = Instantiate(canvas);
        Canvasinst.enabled = true;
        Canvasinst.worldCamera = Camera.main;
        Canvasinst.planeDistance = 0.5f;
        Canvasinst.gameObject.SetActive(false);
        Canvasinst.GetComponentInChildren<Button>().onClick.AddListener(Getslidersvalue);
        // Trouver le script FlyCamera attaché à la caméra
        flyCamera = Camera.main.GetComponent<FlyCamera>();
    }

    void Getslidersvalue()
    {
        terrainGenTer.BuildChunks(minDimRes + (int)(SliderResolution.value * (maxDimRes-minDimRes)), minDimRes + (int)(SliderDimension.value * (maxDimRes-minDimRes)));
        Deactivate();
    }
    public void Activate()
    {
        isPanelActive =true;
        Canvasinst.gameObject.SetActive(isPanelActive);
        terrainGenTer.isaskterrain = isPanelActive;
        if (flyCamera != null)
        {
            flyCamera.canMove = !isPanelActive;
        }
        SliderDimension.value = (float)terrainGenTer.dimension / ((float)maxDimRes - (float)minDimRes);
        SliderResolution.value = (float)terrainGenTer.resolution / ((float)maxDimRes - (float)minDimRes);
    }

    public void Deactivate()
    {
        isPanelActive = false;
        Canvasinst.gameObject.SetActive(isPanelActive);
        terrainGenTer.isaskterrain = isPanelActive;
        flyCamera.canMove = !isPanelActive;
        gameObject.GetComponent<gestionModes>().setIsInOneMode(false);

    }

}