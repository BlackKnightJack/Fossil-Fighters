using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PixelClicker : MonoBehaviour
{
    [Header("Layers")]
    public Texture2D firstLayer;
    public Texture2D secondLayer;
    public Texture2D thirdLayer;

    [Header("Hammer")]
    public float radius;

    int[,] texMap;
    Vector2Int texDim;
    Texture2D textInstance;

    // Start is called before the first frame update
    void Start()
    {
        texDim = new Vector2Int(GetComponent<MeshRenderer>().material.mainTexture.width, GetComponent<MeshRenderer>().material.mainTexture.height);
        texMap = new int[texDim.x, texDim.y];
        textInstance = Instantiate(GetComponent<MeshRenderer>().material.mainTexture as Texture2D);
        GetComponent<MeshRenderer>().material.mainTexture = textInstance;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
        {
            //Get the pixel that was clicked on
            Vector2Int pixelClicked = new Vector2Int((int)(hit.textureCoord.x * texDim.x), (int)(hit.textureCoord.y * texDim.y));

            //Go through each angle between 0 and 90 for pixel arrays to modify if the clicked pixel is not transparent
            for (int theta = 0; theta <= 90; theta++)
            {
                //Get the master radial offset
                Vector2Int radialOffset = new Vector2Int((int)(Mathf.Cos(Mathf.Deg2Rad * theta) * radius), (int)(Mathf.Sin(Mathf.Deg2Rad * theta) * radius));

                //Process the first quadrant
                Vector2Int quadOneOffset = new Vector2Int(Mathf.Min(radialOffset.x, pixelClicked.x), Mathf.Min(radialOffset.y, pixelClicked.y));
                for (int y = 0; y <= quadOneOffset.y; y++)
                {
                    for (int x = 0; x <= quadOneOffset.x; x++)
                    {
                        textInstance.SetPixel(pixelClicked.x - x, pixelClicked.y - y, new Color(0, 0, 0, 0));
                    }
                }

                //Process the second quadrant
                Vector2Int quadTwoOffset = new Vector2Int(Mathf.Min(radialOffset.x, texDim.x - pixelClicked.x), Mathf.Min(radialOffset.y, pixelClicked.y));
                for (int y = 0; y <= quadTwoOffset.y; y++)
                {
                    for (int x = 0; x <= quadTwoOffset.x; x++)
                    {
                        textInstance.SetPixel(pixelClicked.x + x, pixelClicked.y - y, new Color(0, 0, 0, 0));
                    }
                }

                //Process the third quadrant
                Vector2Int quadThreeOffset = new Vector2Int(Mathf.Min(radialOffset.x, texDim.x - pixelClicked.x), Mathf.Min(radialOffset.y, texDim.y - pixelClicked.y));
                for (int y = 0; y <= quadThreeOffset.y; y++)
                {
                    for (int x = 0; x <= quadThreeOffset.x; x++)
                    {
                        textInstance.SetPixel(pixelClicked.x + x, pixelClicked.y + y, new Color(0, 0, 0, 0));
                    }
                }

                //Process the fourth quadrant
                Vector2Int quadFourOffset = new Vector2Int(Mathf.Min(radialOffset.x, pixelClicked.x), Mathf.Min(radialOffset.y, texDim.y - pixelClicked.y));
                for (int y = 0; y <= quadFourOffset.y; y++)
                {
                    for (int x = 0; x <= quadFourOffset.x; x++)
                    {
                        textInstance.SetPixel(pixelClicked.x - x, pixelClicked.y + y, new Color(0, 0, 0, 0));
                    }
                }
            }

            //Apply the texture only at the end of the loop
            textInstance.Apply();
        }
    }
}
