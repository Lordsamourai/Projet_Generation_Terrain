using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
//using TreeEditor;
using UnityEngine;

public class Generationterrain : MonoBehaviour
{
    [Header("Deformation")]
    [Range(1f, 30f)]
    public float radius;

    [Range(0.5f, 10f)]
    public float deformationStrength;

    private float deformationCooldown = 0.2f;
    private float nextDeformationTime = 0.2f;

    [Header("Animation Curves")]
    public List<AnimationCurve> curves = new List<AnimationCurve>(); // Liste pour stocker les courbes
    private int currentCurveIndex = 0;

    public int resolution;
    public int dimension;
    //public int tailleCote;
    public int tailleCoteX;
    public int tailleCoteY;

    private Mesh[] p_sharedmeshs_all;

    private Vector3[][] p_vertices_all;
    private Vector3[][] p_normals_all;
    private int[][] p_triangles_all;
    private MeshFilter[] p_meshfilters_all;
    private MeshCollider[] p_meshcolliders_all;
    private bool CentrerPivot = true;

    private Camera p_cam;
    public LayerMask maskPickingTerrain;
    private RaycastHit hit;

    private int lastMeshNumber = -1;

    private GameObject parentObject;

    // Vitesse de rotation en degrés par seconde
    float rotationSpeed = 30f;
    private GameObject pivot;

    public Texture2D[] brushes;
    private int currentBrushIndex = 0;
    private bool ActiveCurves;
    private bool ActiveBrushes;

    delegate float CalculDistance(Vector3 a, Vector3 b);
    public bool isaddingchunk;
    public bool isaskterrain =false;
    public GameObject camaddchunk;
    private int numberchunk;
    private CanvasInformation canvasinfo;
    private int currentcalculvertex = 0;
    private int currentcalculvoisin = 0;
    CalculDistance[] calculDistances;
    private bool iscalculatingallnormals = true;
    List<int> allchunkschanged;

    private void Créer_Terrain(int num, float x, float y)
    {
        GameObject obj = new GameObject();
        obj.name = "Chunk " + num;
        obj.AddComponent<MeshFilter>();
        obj.AddComponent<MeshCollider>();
        obj.AddComponent<MeshRenderer>();
        obj.layer = LayerMask.NameToLayer("Chunk");

        Material blueMaterial = new Material(Shader.Find("Standard"));
        blueMaterial.color = Color.blue;
        obj.GetComponent<MeshRenderer>().material = blueMaterial;

        // Création du mesh partagé si ce n'est pas déjà fait
        if (p_sharedmeshs_all[num] == null)
        {
            p_sharedmeshs_all[num] = new Mesh();
            p_sharedmeshs_all[num].name = "ProceduralTerrainMESH" + num;

            p_vertices_all[num] = new Vector3[resolution * resolution];
            p_normals_all[num] = new Vector3[p_vertices_all[num].Length];
            p_triangles_all[num] = new int[3 * 2 * resolution * resolution];
            int indice_vertex = 0;
            float distance = (float)dimension / ((float)resolution - 1);
            for (float j = 0; j < resolution; j++)
            {
                for (float i = 0; i < resolution; i++)
                {
                    p_vertices_all[num][indice_vertex] = new Vector3(i * distance, 0, j * distance);
                    p_normals_all[num][indice_vertex] = Vector3.up;
                    indice_vertex++;
                }
            }

            if (CentrerPivot)
            {
                Vector3 decalCentrage = new Vector3((float)dimension / 2, 0, (float)dimension / 2);
                for (int k = 0; k < p_vertices_all[num].Length; k++)
                {
                    p_vertices_all[num][k] -= decalCentrage;
                }
            }

            int indice_triangle = 0;
            for (int j = 0; j < resolution - 1; j++)
            {
                for (int i = 0; i < resolution - 1; i++)
                {
                    p_triangles_all[num][indice_triangle + 0] = j * resolution + i;
                    p_triangles_all[num][indice_triangle + 1] = (j + 1) * resolution + i + 1;
                    p_triangles_all[num][indice_triangle + 2] = j * resolution + i + 1;
                    indice_triangle += 3;
                    p_triangles_all[num][indice_triangle + 0] = j * resolution + i;
                    p_triangles_all[num][indice_triangle + 1] = (j + 1) * resolution + i;
                    p_triangles_all[num][indice_triangle + 2] = (j + 1) * resolution + i + 1;
                    indice_triangle += 3;
                }
            }

            p_sharedmeshs_all[num].vertices = p_vertices_all[num];
            p_sharedmeshs_all[num].normals = p_normals_all[num];
            p_sharedmeshs_all[num].triangles = p_triangles_all[num];
            //Debug.Log("Mesh partagé configuré avec " + p_sharedmeshs_all[num].vertexCount + " sommets.");
        }

        p_meshfilters_all[num] = obj.GetComponent<MeshFilter>();
        p_meshcolliders_all[num] = obj.GetComponent<MeshCollider>();

        // position du chunk à partir de l'angle inférieur gauche
        obj.transform.position = new Vector3(x, 0, y);
        obj.transform.position -= new Vector3(dimension * ((float)tailleCoteX-1)/2, 0, dimension * ((float)tailleCoteY - 1) / 2);
        // on définit le parent ("Terrain") du chunk
        obj.transform.parent = parentObject.transform;

        p_meshfilters_all[num].sharedMesh = p_sharedmeshs_all[num];
        p_meshcolliders_all[num].sharedMesh = p_sharedmeshs_all[num];
        p_meshcolliders_all[num].convex = false;

        //Debug.Log("Terrain créé avec le mesh : " + p_meshfilters_all[num].sharedMesh.name);
    }

    void Start()
    {
    calculDistances = new CalculDistance[] {
    distanceEuclidienne,
    distanceManhattan,
    distanceTchebychev
};
        pivot = new GameObject("Pivot");
        pivot.transform.position = Vector3.zero; // Centre du pivot

        parentObject = new GameObject("Terrain");
        parentObject.transform.parent = pivot.transform;

        camaddchunk.transform.position = pivot.transform.position + new Vector3(0,200,0);
        camaddchunk.transform.LookAt(pivot.transform.position);
        camaddchunk.gameObject.SetActive(false);
        BuildChunks(resolution, dimension);

        radius = 6f;
        deformationStrength = 2f;

        maskPickingTerrain = LayerMask.GetMask("Chunk");
        p_cam = Camera.main;
        numberchunk = 0;
        // Création des chunks de terrain
        brushes = Resources.LoadAll<Texture2D>("textures brush");
        if (brushes.Length == 0)
        {
            Debug.LogError("Aucun brush trouvé dans le dossier Resources/brush");
        }

        // Ajout des courbes d'animation
        AnimationCurve curve1 = new AnimationCurve(new Keyframe(0, 0.8f),
                                    new Keyframe(0.25f, 1),
                                    new Keyframe(0.5f, 0.25f, -1.5f, -1.5f),
                                    new Keyframe(1, 0));

        AnimationCurve curve2 = new AnimationCurve(new Keyframe(0, 1, 0, -0.75f),
                                    new Keyframe(0.8f, 0.5f, -1.5f, -1.5f),
                                    new Keyframe(1, 0, -3, 0));

        AnimationCurve curve3 = new AnimationCurve(new Keyframe(0, -0.25f),
                                    new Keyframe(0.25f, -0.2f, 0.5f, 0.5f),
                                    new Keyframe(0.6f, 0.1f),
                                    new Keyframe(1, 0));
        allchunkschanged = new List<int>();
        curves.Add(curve1);
        curves.Add(curve2);
        curves.Add(curve3);
        ActiveCurves = true;
        ActiveBrushes = false;
        canvasinfo = GameObject.Find("gestionmode").GetComponent<CanvasInformation>();
    }

    void Update()
    {
        if (isaddingchunk)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                Array.Resize(ref p_sharedmeshs_all, p_sharedmeshs_all.Length + tailleCoteX);
                Array.Resize(ref p_vertices_all, p_vertices_all.Length + tailleCoteX);
                Array.Resize(ref p_normals_all, p_normals_all.Length + tailleCoteX);
                Array.Resize(ref p_triangles_all, p_triangles_all.Length + tailleCoteX);
                Array.Resize(ref p_meshfilters_all, p_meshfilters_all.Length + tailleCoteX);
                Array.Resize(ref p_meshcolliders_all, p_meshcolliders_all.Length + tailleCoteX);
                for (int i = 0; i < tailleCoteX; i++)
                {
                    Créer_Terrain(numberchunk, dimension * i, dimension * tailleCoteY);
                    numberchunk++;
                }
                tailleCoteY++;
                GameObject[] allchunks = FindChunks();
                foreach (GameObject chunk in allchunks)
                {
                    chunk.transform.position -= new Vector3(0, 0, (float)(dimension * 0.5));
                }
                canvasinfo.UpdateModeTexte();
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                Array.Resize(ref p_sharedmeshs_all, p_sharedmeshs_all.Length + tailleCoteX);
                Array.Resize(ref p_vertices_all, p_vertices_all.Length + tailleCoteX);
                Array.Resize(ref p_normals_all, p_normals_all.Length + tailleCoteX);
                Array.Resize(ref p_triangles_all, p_triangles_all.Length + tailleCoteX);
                Array.Resize(ref p_meshfilters_all, p_meshfilters_all.Length + tailleCoteX);
                Array.Resize(ref p_meshcolliders_all, p_meshcolliders_all.Length + tailleCoteX);
                for (int i = 0; i < tailleCoteX; i++)
                {
                    Créer_Terrain(numberchunk, dimension * i, -dimension);
                    numberchunk++;
                }
                tailleCoteY++;
                GameObject[] allchunks = FindChunks();
                foreach (GameObject chunk in allchunks)
                {
                    chunk.transform.position += new Vector3(0, 0, (float)(dimension * 0.5));
                }
                canvasinfo.UpdateModeTexte();
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Array.Resize(ref p_sharedmeshs_all, p_sharedmeshs_all.Length + tailleCoteY);
                Array.Resize(ref p_vertices_all, p_vertices_all.Length + tailleCoteY);
                Array.Resize(ref p_normals_all, p_normals_all.Length + tailleCoteY);
                Array.Resize(ref p_triangles_all, p_triangles_all.Length + tailleCoteY);
                Array.Resize(ref p_meshfilters_all, p_meshfilters_all.Length + tailleCoteY);
                Array.Resize(ref p_meshcolliders_all, p_meshcolliders_all.Length + tailleCoteY);
                for (int i = 0; i < tailleCoteY; i++)
                {
                    Créer_Terrain(numberchunk, -dimension, dimension * i);
                    numberchunk++;
                }
                tailleCoteX++;
                GameObject[] allchunks = FindChunks();
                foreach (GameObject chunk in allchunks)
                {
                    chunk.transform.position += new Vector3((float)(dimension * 0.5), 0, 0);
                }
                canvasinfo.UpdateModeTexte();
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                Array.Resize(ref p_sharedmeshs_all, p_sharedmeshs_all.Length + tailleCoteY);
                Array.Resize(ref p_vertices_all, p_vertices_all.Length + tailleCoteY);
                Array.Resize(ref p_normals_all, p_normals_all.Length + tailleCoteY);
                Array.Resize(ref p_triangles_all, p_triangles_all.Length + tailleCoteY);
                Array.Resize(ref p_meshfilters_all, p_meshfilters_all.Length + tailleCoteY);
                Array.Resize(ref p_meshcolliders_all, p_meshcolliders_all.Length + tailleCoteY);
                for (int i = 0; i < tailleCoteY; i++)
                {
                    Créer_Terrain(numberchunk, dimension * tailleCoteX, dimension * i);
                    numberchunk++;
                }
                tailleCoteX++;
                GameObject[] allchunks = FindChunks();
                foreach (GameObject chunk in allchunks)
                {
                    chunk.transform.position -= new Vector3((float)(dimension * 0.5), 0, 0);
                }
                canvasinfo.UpdateModeTexte();
            }

        }
        else
        {
            if (!isaskterrain)
            {
                if (Input.GetMouseButton(0) && Time.time >= nextDeformationTime)
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, maskPickingTerrain))
                    {
                        MeshCollider meshCollider = hit.collider as MeshCollider;
                        if (meshCollider != null)
                        {
                            string input = meshCollider.sharedMesh.name;
                            string output = string.Concat(input.Where(Char.IsDigit));
                            int meshnumber = Int32.Parse(output);
                            Vector3 targetVertex = RechercherVertexCible(hit, meshnumber);

                            bool isDepression = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

                            if (ActiveCurves)
                            {
                                DeformerTerrain(targetVertex, isDepression, meshnumber);
                                //Debug.Log("Curve activé : " + currentCurveIndex);
                            }
                            else
                            {
                                //Debug.Log("Brush activé : " + currentBrushIndex);
                                //Debug.Log("Brush Défromation ");
                                DeformerTerrainAvecBrush(targetVertex, meshnumber);
                            }

                            nextDeformationTime = Time.time + deformationCooldown;
                            lastMeshNumber = meshnumber;
                        }
                    }
                }
                if (lastMeshNumber > 0 && lastMeshNumber < p_meshcolliders_all.Length)
                {
                    if (Input.GetMouseButtonUp(0))
                        RecalculerMeshCollider();
                }
                //Changer de Bruhes
                if (Input.GetKeyDown(KeyCode.B))
                {
                    if (ActiveCurves)
                    {
                        ActiveCurves = false;
                        ActiveBrushes = true;
                        return;
                    }
                    currentBrushIndex = (currentBrushIndex + 1) % brushes.Length;
                    Debug.Log("Brush changé : " + currentBrushIndex);
                }

                //Changer de Curves
                if (Input.GetKeyDown(KeyCode.P))
                {
                    if (ActiveBrushes)
                    {
                        ActiveCurves = true;
                        ActiveBrushes = false;
                        return;
                    }
                    currentCurveIndex = (currentCurveIndex + 1) % curves.Count;
                    Debug.Log("Pattern changé : " + currentCurveIndex);
                }
                if (Input.GetKeyDown(KeyCode.RightAlt))
                {
                    deformationStrength -= 0.5f;
                    Debug.Log("AltGr");
                }
                else if (Input.GetKeyDown(KeyCode.LeftAlt))
                {
                    deformationStrength += 0.5f;
                }


                if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus))
                {
                    radius += 0.5f;
                }
                else if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
                {
                    radius -= 0.5f;
                }
                if (Input.GetKeyDown(KeyCode.F))
                {
                    currentcalculvertex = (currentcalculvertex + 1) % calculDistances.Length;
                    Debug.Log("Calcul vertex changé : " + currentcalculvertex);
                }
                if (Input.GetKeyDown(KeyCode.V))
                {
                    currentcalculvoisin = (currentcalculvoisin + 1) % calculDistances.Length;
                    Debug.Log("Calcul voisin changé : " + currentcalculvoisin);
                }
                if (Input.GetKeyDown(KeyCode.N))
                {
                    iscalculatingallnormals = !iscalculatingallnormals;
                }

                if (Input.GetKey(KeyCode.RightControl))
                {
                    float rotationAngle = rotationSpeed * Time.deltaTime;

                    if (Input.GetKey(KeyCode.LeftArrow))
                    {
                        pivot.transform.Rotate(Vector3.up, rotationAngle);
                    }
                    else if (Input.GetKey(KeyCode.RightArrow))
                    {
                        pivot.transform.Rotate(Vector3.up, -rotationAngle);
                    }
                }
            }
        }
    }
    
    private void DeformerTerrain(Vector3 targetVertex, bool Depression, int meshnumber)
    {
        float deformationBase = deformationStrength * (Depression ? -1 : 1);
        List<Voisin> voisins = RechercherVoisins(targetVertex, radius, meshnumber);
        AnimationCurve currentCurve = curves[currentCurveIndex];

        foreach (Voisin voisin in voisins)
        {
            float relativeDistance = Mathf.Clamp01(voisin.distance / radius);
            float curveFactor = currentCurve.Evaluate(relativeDistance);

            float deformation = deformationBase * curveFactor;
            p_vertices_all[voisin.meshnumber][voisin.indice].y += deformation;
        }
        RecalculateNormalsChunk();
    }

    private void RecalculerMeshCollider()
    {
        foreach (MeshCollider meshchunk in p_meshcolliders_all)
        {
                string input = meshchunk.transform.name;
                string output = string.Concat(input.Where(Char.IsDigit));
                int numberofthechunk = Int32.Parse(output);
                p_meshcolliders_all[numberofthechunk].sharedMesh = null; // Supprime l'ancienne mesh
                p_meshcolliders_all[numberofthechunk].sharedMesh = p_sharedmeshs_all[numberofthechunk];
        }
    }
    private Vector3 RechercherVertexCible(RaycastHit hit, int meshnumber)
    {
        Vector3 closestVertex = Vector3.zero;
        float minDistance = float.MaxValue;
        int triangleIndex = hit.triangleIndex;

        if (triangleIndex < 0 || triangleIndex * 3 + 2 >= p_sharedmeshs_all[meshnumber].triangles.Length)
        {
            Debug.LogWarning("Indice de triangle invalide ou hors des limites.");
            return closestVertex;
        }

        int[] triangleVertices = new int[] {
            p_sharedmeshs_all[meshnumber].triangles[triangleIndex * 3],
            p_sharedmeshs_all[meshnumber].triangles[triangleIndex * 3 + 1],
            p_sharedmeshs_all[meshnumber].triangles[triangleIndex * 3 + 2]
        };

        foreach (int vertexIndex in triangleVertices)
        {
            if (vertexIndex < 0 || vertexIndex >= p_vertices_all[meshnumber].Length)
            {
                Debug.LogWarning("Indice de vertex invalide ou hors des limites.");
                continue;
            }
            Vector3 vertexPosition = p_vertices_all[meshnumber][vertexIndex];
            float distance = (float)calculDistances[currentcalculvertex](hit.point, vertexPosition);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestVertex = vertexPosition;
            }
        }
        return closestVertex;
    }

    private List<Voisin> RechercherVoisins(Vector3 cible, float radius, int meshnumber)
    {
        cible += p_meshcolliders_all[meshnumber].transform.position;
        List<Voisin> voisins = new List<Voisin>();
        List<int> listechunksvoisins = new List<int>();
        float Distancechunk = new float();
        Distancechunk =Vector3.Distance( new Vector3(dimension,0,dimension), Vector3.zero);
        foreach (MeshCollider meshchunk in p_meshcolliders_all) {
            if ((float)calculDistances[currentcalculvoisin](meshchunk.transform.position, p_meshcolliders_all[meshnumber].transform.position)<=Distancechunk)
            {
                string input = meshchunk.transform.name;
                string output = string.Concat(input.Where(Char.IsDigit));
                int numberofthechunk = Int32.Parse(output);
                listechunksvoisins.Add(numberofthechunk);
                if (!allchunkschanged.Contains(numberofthechunk))
                {
                    allchunkschanged.Add(numberofthechunk);
                }
            }
        }
        foreach (int numbermesh in listechunksvoisins)
        {
            for (int i = 0; i < p_vertices_all[numbermesh].Length; i++)
            {
                Vector3 vertex = p_vertices_all[numbermesh][i]+ p_meshcolliders_all[numbermesh].transform.position;
                float distance = (float)calculDistances[currentcalculvoisin](vertex, cible);
                if (distance <= radius)
                {
                    voisins.Add(new Voisin { indice = i, distance = distance, meshnumber =numbermesh });
                }
            }
        }
        return voisins;
    }
    private void DeformerTerrainAvecBrush(Vector3 targetVertex, int meshnumber)
    {
        List<Voisin> voisins = RechercherVoisins(targetVertex, radius, meshnumber);
        Texture2D Currentbrushes = brushes[currentBrushIndex];

        foreach (Voisin voisin in voisins)
        {
            if (voisin.distance < radius)
            {
                Vector3 voisinPosition = p_vertices_all[voisin.meshnumber][voisin.indice];

                float uvX = (voisinPosition.x - targetVertex.x + (radius / 2)) / radius * Currentbrushes.width;
                float uvY = (voisinPosition.z - targetVertex.z + (radius / 2)) / radius * Currentbrushes.height;

                uvX = Mathf.Clamp(uvX, 0, Currentbrushes.width - 1);
                uvY = Mathf.Clamp(uvY, 0, Currentbrushes.height - 1);

                Color pixelColor = Currentbrushes.GetPixel((int)uvX, (int)uvY);
                float heightDelta = pixelColor.grayscale * deformationStrength;

                p_vertices_all[voisin.meshnumber][voisin.indice].y += heightDelta;
            }
        }
        RecalculateNormalsChunk();
    }
    private GameObject[] FindChunks()
    {
        var goArray = FindObjectsOfType(typeof(GameObject)) as GameObject[];
        var goList = new System.Collections.Generic.List<GameObject>();
        for (int i = 0; i < goArray.Length; i++)
        {
            if (goArray[i].layer == 6)
            {
                goList.Add(goArray[i]);
            }
        }
        if (goList.Count == 0)
        {
            return null;
        }
        return goList.ToArray();
    }
    public void BuildChunks(int resolutionbuild, int dimensionbuild)
    {
        resolution = resolutionbuild;
        dimension = dimensionbuild;
        if(resolution == 0)
        {
            resolution = 10;
        }
        if(dimension == 0)
        {
            dimension = 10;
        }
        GameObject[] gameObjects = FindChunks();
        if (gameObjects !=null)
        {
            for (int i = gameObjects.Length - 1; i >= 0; i--)
            {
                Destroy(gameObjects[i]);
            }
        }
        p_sharedmeshs_all = new Mesh[tailleCoteX * tailleCoteY];
        p_vertices_all = new Vector3[tailleCoteX * tailleCoteY][];
        p_normals_all = new Vector3[tailleCoteX * tailleCoteY][];
        p_triangles_all = new int[tailleCoteX * tailleCoteY][];
        p_meshfilters_all = new MeshFilter[tailleCoteX * tailleCoteY];
        p_meshcolliders_all = new MeshCollider[tailleCoteX * tailleCoteY];

        numberchunk = 0;

        for (int i = 0; i < tailleCoteX; i++)
        {
            for (int j = 0; j < tailleCoteY; j++)
            {
                Créer_Terrain(numberchunk, dimension * i, dimension * j);
                numberchunk++;
            }
        }
    }
    private void RecalculateNormalsChunk()
    {
        if (iscalculatingallnormals)
        {
            for (int i = 0; i < p_sharedmeshs_all.Length; i++)
            {
                p_sharedmeshs_all[i].vertices = p_vertices_all[i];
                p_sharedmeshs_all[i].RecalculateNormals();
            }
        }
        else
        {
            foreach ( int numberchunk in allchunkschanged)
            {
                p_sharedmeshs_all[numberchunk].vertices = p_vertices_all[numberchunk];
                p_sharedmeshs_all[numberchunk].RecalculateNormals();
            }
        }
        allchunkschanged.Clear();
    }
    public float distanceEuclidienne(Vector3 a, Vector3 b)
    {
        return (float)Math.Sqrt((a.x-b.x)* (a.x - b.x)+(a.y-b.y)* (a.y - b.y)+ (a.z - b.z)* (a.z - b.z));
    }
    public float distanceManhattan(Vector3 a, Vector3 b)
    {
        return Math.Abs(b.x - a.x) + Math.Abs(b.y - a.y) + Math.Abs(b.z - a.z);
    }
    public float distanceTchebychev(Vector3 a, Vector3 b)
    {
        float[] arraycalculdistance = new float[3];
        arraycalculdistance[0] = Math.Abs(a.x -b.x);
        arraycalculdistance[1] = Math.Abs(a.y - b.y);
        arraycalculdistance[2] = Math.Abs(a.z - b.z);
        float maxVal =-1; 
        for (int i = 0; i < arraycalculdistance.Length; i++)
        {
            float thisNum = arraycalculdistance[i];
            if (thisNum > maxVal)
            {
                maxVal = thisNum;
            }
        }
        return maxVal;
    }
    public bool getActiveCurves() { return ActiveCurves; }
    public int getCurrBrushIndex() { return currentBrushIndex; }
    public int getCurrCurveIndex() { return currentCurveIndex; }
}


[Serializable]
public class Voisin
{
    public int indice;
    public float distance;
    public int meshnumber;
}
