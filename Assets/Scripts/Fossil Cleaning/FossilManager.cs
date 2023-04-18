using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FossilManager : MonoBehaviour
{
    //Constants
    const float BREAKPOINT = -0.5f;
    const float HEALTH_THRESHOLD = 0.5f;
    const float SECOND_LAYER = 0.6f;
    const float THIRD_LAYER = 0.3f;

    [Header("Rock data")]
    public Texture2D firstLayer;
    public Texture2D secondLayer;
    public Texture2D thirdLayer;

    [Header("Fossil data")]
    public Texture2D fossil;
    public Texture2D brokenFossil;
    public Image progressBar;
    public Image healthBar;
    public TMP_Text scoreDisplay;
    public float xRayDuration;
    GameObject progressFill;
    GameObject healthFill;
    float healthMax;
    float fossilHealth;

    //Material variables
    Material theMaterial;
    Texture2D displayTexture;
    float xRayTimer;
    float xRayStrength;
    bool xRayActive;

    //Dictionaries
    Dictionary<Vector3, float> vertexHealth;
    Dictionary<Vector3Int, List<Vector2>> pixelIndex;
    List<Vector3Int>[] triangleIndex;
    List<Vector3> vertexList;
    List<int> fossilIndexList;
    List<int> fossilIndicesDiscovered;

    //Miscellaneous variables
    Mesh mesh;
    Vector3 canvasScale;

    // Start is called before the first frame update
    void Start()
    {
        //Create and assign the material
        theMaterial = new Material(Shader.Find("Shader Graphs/XRay"));
        displayTexture = Instantiate(firstLayer);
        theMaterial.SetTexture("_Rock", displayTexture);
        theMaterial.SetTexture("_Fossil", fossil);
        theMaterial.SetFloat("_BlendFactor", 0);
        GetComponent<MeshRenderer>().material = theMaterial;

        //Cache mesh
        mesh = GetComponent<MeshFilter>().mesh;

        //Cache canvas scale
        canvasScale = GameObject.Find("Canvas").transform.localScale;

        //Intialize dictionaries
        vertexHealth = new Dictionary<Vector3, float>();
        pixelIndex = new Dictionary<Vector3Int, List<Vector2>>();
        triangleIndex = new List<Vector3Int>[mesh.vertices.Length];
        vertexList = new List<Vector3>(mesh.vertices);
        fossilIndexList = new List<int>();
        fossilIndicesDiscovered = new List<int>();

        //Sample mesh data
        using (var meshData = Mesh.AcquireReadOnlyMeshData(mesh))
        {
            //Create mesh data containers
            NativeArray<Vector3> vertices = new NativeArray<Vector3>(mesh.vertices.Length, Allocator.TempJob);
            NativeArray<Vector2> uvs = new NativeArray<Vector2>(mesh.uv.Length, Allocator.TempJob);
            NativeArray<int> indices = new NativeArray<int>(mesh.triangles.Length, Allocator.TempJob);

            //Populate mesh data container
            meshData[0].GetVertices(vertices);
            meshData[0].GetUVs(0, uvs);
            meshData[0].GetIndices(indices, 0);

            //Cycle through triangles to simplify everything
            for (int i = 0; i < indices.Length; i += 3)
            {
                //Add this vertex to the vertexHealth list if it doesn't currently already exist
                if (!vertexHealth.ContainsKey(vertices[indices[i]])) { vertexHealth.Add(vertices[indices[i]], 1); }
                if (!vertexHealth.ContainsKey(vertices[indices[i + 1]])) { vertexHealth.Add(vertices[indices[i + 1]], 1); }
                if (!vertexHealth.ContainsKey(vertices[indices[i + 2]])) { vertexHealth.Add(vertices[indices[i + 2]], 1); }

                //Add to content vertex dictionaries and populate the pixel list
                List<Vector2> pixelList = new List<Vector2>();
                if (HasFossilFragment(new Vector3Int(indices[i], indices[i + 1], indices[i + 2]), ref uvs, ref pixelList))
                {
                    //Add to fossil index list if not present
                    if (!fossilIndexList.Contains(indices[i])) { fossilIndexList.Add(indices[i]); }
                    if (!fossilIndexList.Contains(indices[i + 1])) { fossilIndexList.Add(indices[i + 1]); }
                    if (!fossilIndexList.Contains(indices[i + 2])) { fossilIndexList.Add(indices[i + 2]); }
                }

                //Initialize triangle list for this index if it hasn't been initialized already
                if (triangleIndex[indices[i]] == null) { triangleIndex[indices[i]] = new List<Vector3Int>(); }
                if (triangleIndex[indices[i + 1]] == null) { triangleIndex[indices[i + 1]] = new List<Vector3Int>(); }
                if (triangleIndex[indices[i + 2]] == null) { triangleIndex[indices[i + 2]] = new List<Vector3Int>(); }

                //Add this triangle to the necessary lists
                Vector3Int triangle = new Vector3Int(indices[i], indices[i + 1], indices[i + 2]);
                triangleIndex[indices[i]].Add(triangle);
                triangleIndex[indices[i + 1]].Add(triangle);
                triangleIndex[indices[i + 2]].Add(triangle);

                //Add pixel list to the triangle dictionary
                pixelIndex.Add(triangle, pixelList);
            }

            //Clean up mesh data containers
            vertices.Dispose();
            uvs.Dispose();
            indices.Dispose();
        }

        //Set up fossil health
        healthMax = vertexList.Count * Mathf.Abs(BREAKPOINT);
        fossilHealth = healthMax;

        //Create progress and health bars
        CreateFillBar(ref progressBar, ref progressFill);
        CreateFillBar(ref healthBar, ref healthFill);
    }

    // Update is called once per frame
    void Update()
    {
        //Set up necessary variables
        float progressRatio = (float)fossilIndicesDiscovered.Count / fossilIndexList.Count;
        float healthRatio = Mathf.Clamp(((fossilHealth / healthMax) - HEALTH_THRESHOLD) * (1 / HEALTH_THRESHOLD), 0, Mathf.Infinity);
        float barLength = Screen.width * (1 / canvasScale.x) + progressBar.rectTransform.sizeDelta.x;

        //Update progress display
        progressFill.GetComponent<RectTransform>().sizeDelta = new Vector2(progressRatio * barLength, progressBar.rectTransform.sizeDelta.y);

        //Update health display
        healthFill.GetComponent<RectTransform>().sizeDelta = new Vector2(healthRatio * barLength, healthBar.rectTransform.sizeDelta.y);

        //Update point display
        scoreDisplay.text = Mathf.RoundToInt((progressRatio - Mathf.Abs(1 - healthRatio)) * 100).ToString();

        //Operate X-ray
        if (xRayActive)
        {
            //Increment timer regardless
            xRayTimer += Time.deltaTime;

            //Increment up if the timer is less than half
            if (xRayTimer < xRayDuration / 2)
            {
                xRayStrength = Mathf.Clamp(xRayTimer, 0, 1);
            }

            //Decrement if above
            else
            {
                xRayStrength = Mathf.Clamp(xRayDuration - xRayTimer, 0, 1);
            }

            //Turn it off if the duration is up
            if (xRayTimer >= xRayDuration)
            {
                xRayTimer = 0;
                xRayActive = false;
            }

            //Apply xRayStrength to shader
            theMaterial.SetFloat("_BlendFactor", xRayStrength);
        }
    }

    //Public functions
    public void ActivateXRay()
    {
        xRayActive = true;
    }

    public void ApplyChanges()
    {
        displayTexture.Apply();
    }

    public void DamageVertex(Vector3 vertex, float amount)
    {
        //Get vertex index
        int index = vertexList.IndexOf(vertex);

        //Get a prospective amount to use for layer switching
        float prospective = Mathf.Clamp(vertexHealth[vertex] - amount, BREAKPOINT, 1);

        //Switch layers as necessary
        if (ShiftLayer(vertexHealth[vertex], prospective))
        {
            //Set up shift layer
            Texture2D shiftLayer = null;

            //Get second layer as the shift layer
            if (prospective <= SECOND_LAYER && prospective > THIRD_LAYER)
            {
                shiftLayer = secondLayer;
            }

            //Get third layer as the shift layer
            else if (prospective <= THIRD_LAYER && prospective > 0)
            {
                shiftLayer = thirdLayer;
            }

            //Get fossil layer as the shift layer
            else if (prospective <= 0 && prospective > BREAKPOINT)
            {
                shiftLayer = fossil;
            }

            //Get broken fossil layer as the shift layer
            else if (prospective == BREAKPOINT)
            {
                shiftLayer = brokenFossil;
            }

            //Replace pixels as necessary
            if (shiftLayer != null)
            {
                foreach (Vector3Int triangle in triangleIndex[index])
                {
                    foreach (Vector2 pixelCoord in pixelIndex[triangle])
                    {
                        displayTexture.SetPixel((int)(pixelCoord.x * displayTexture.width), (int)(pixelCoord.y * displayTexture.height),
                            shiftLayer.GetPixelBilinear(pixelCoord.x, pixelCoord.y));
                    }
                }
            }

            //Add vertex to fossilIndicesDiscovered list if necessary
            if (prospective <= 0 && fossilIndexList.Contains(index) && !fossilIndicesDiscovered.Contains(index))
            {
                fossilIndicesDiscovered.Add(index);
            }
        }

        //Subtract fossil health as necessary
        if (prospective < 0 && fossilIndexList.Contains(index))
        {
            //Calculate amount to subtract the fossil health by
            float healthSubtract = 0;

            //Calculate health subtraction
            if (vertexHealth[vertex] > 0)
            {
                healthSubtract = amount - vertexHealth[vertex];
            }
            else if (vertexHealth[vertex] - amount < BREAKPOINT)
            {
                healthSubtract = amount - Mathf.Abs(vertexHealth[vertex] - amount + Mathf.Abs(BREAKPOINT));
            }
            else
            {
                healthSubtract = amount;
            }

            //Subtract from the fossil health
            fossilHealth -= healthSubtract;
        }

        //Finalize the vertex health amount
        vertexHealth[vertex] = prospective;
    }

    //Private functions
    void CreateFillBar(ref Image input, ref GameObject output)
    {
        //Create the fill bar and parent it to the background
        output = new GameObject();
        output.transform.SetParent(input.transform);
        output.transform.SetAsFirstSibling();
        output.transform.localScale = Vector3.one;

        //Add components to the fill bar
        output.AddComponent<RectTransform>();
        output.AddComponent<Image>();

        //Change the pivot, anchor, and position
        output.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.5f);
        output.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0.5f);
        output.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f); //I have no clue why, but it actually works when I set the pivot here

        //Change the transform
        output.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        output.GetComponent<RectTransform>().sizeDelta = Vector2.up * input.GetComponent<RectTransform>().sizeDelta.y;

        //Change the sprite
        output.GetComponent<Image>().sprite = input.GetComponent<Image>().sprite;
        output.GetComponent<Image>().color = input.GetComponent<Image>().color * 2;
        output.GetComponent<Image>().type = Image.Type.Sliced;
    }

    bool HasFossilFragment(Vector3Int indices, ref NativeArray<Vector2> uvs, ref List<Vector2> pixelList)
    {
        bool returnValue = false;

        //Get bounds (in pixels) to check
        Vector2Int min = new Vector2Int((int)(Mathf.Min(uvs[indices.x].x, uvs[indices.y].x, uvs[indices.z].x) * fossil.width),
            (int)(Mathf.Min(uvs[indices.x].y, uvs[indices.y].y, uvs[indices.z].y) * fossil.height));
        Vector2Int max = new Vector2Int((int)(Mathf.Max(uvs[indices.x].x, uvs[indices.y].x, uvs[indices.z].x) * fossil.width),
            (int)(Mathf.Max(uvs[indices.x].y, uvs[indices.y].y, uvs[indices.z].y) * fossil.height));

        Vector2Int uv1 = new Vector2Int((int)(uvs[indices.x].x * fossil.width), (int)(uvs[indices.x].y * fossil.height));
        Vector2Int uv2 = new Vector2Int((int)(uvs[indices.y].x * fossil.width), (int)(uvs[indices.y].y * fossil.height));
        Vector2Int uv3 = new Vector2Int((int)(uvs[indices.z].x * fossil.width), (int)(uvs[indices.z].y * fossil.height));

        //Check triangle bounds for content
        for (float y = min.y; y < max.y; y++)
        {
            for (float x = min.x; x < max.x; x++)
            {
                Vector2 point = new Vector2(x, y);
                if (Semiperimeter(point, uv1, uv2) + Semiperimeter(point, uv2, uv3) + Semiperimeter(point, uv3, uv1) == Semiperimeter(uv1, uv2, uv3))
                {
                    //If in triangle bounds, at least add the pixel to the list
                    pixelList.Add(new Vector2(point.x / fossil.width, point.y / fossil.height));

                    //Flag content existing if the pixel's alpha is 1
                    if (fossil.GetPixel((int)x, (int)y).a == 1)
                    {
                        returnValue = true;
                    }
                }
            }
        }

        return returnValue;
    }

    float Semiperimeter(Vector2 a, Vector2 b, Vector2 c)
    {
        return Mathf.Abs((a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y)) / 2.0f);
    }

    bool ShiftLayer(float startAmount, float finishAmount)
    {
        return ((startAmount > SECOND_LAYER && finishAmount <= SECOND_LAYER) || 
                (startAmount > THIRD_LAYER && finishAmount <= THIRD_LAYER) ||
                (startAmount > 0 && finishAmount <= 0) ||
                (startAmount > BREAKPOINT && finishAmount <= BREAKPOINT));
    }
}
