using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResolutionChange : MonoBehaviour
{
    public Texture2D texture;

    Texture2D textInstance;
    Vector2Int nativeResolution;

    // Start is called before the first frame update
    void Start()
    {
        //Record the texture's native resolution
        nativeResolution = new Vector2Int(texture.width, texture.height);

        //Apply an instance of the texture to the model
        textInstance = Instantiate(texture);
        GetComponent<MeshRenderer>().material.mainTexture = textInstance;
        print(new Vector2Int(GetComponent<MeshRenderer>().material.mainTexture.width, GetComponent<MeshRenderer>().material.mainTexture.height));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //Doesn't actually change the image, but the resolution should be right for manipulation now
            Texture2D scaled = new Texture2D(1024, 512, textInstance.format, true);

            for (int y = 0; y < 512; y++)
            {
                for (int x = 0; x < 1024; x++)
                {
                    scaled.SetPixel(x, y, textInstance.GetPixelBilinear(1.0f / 1024f * x, 1.0f / 512f * y), 0);
                }
            }

            scaled.Apply();
            GetComponent<MeshRenderer>().material.mainTexture = scaled;
            print(new Vector2Int(GetComponent<MeshRenderer>().material.mainTexture.width, GetComponent<MeshRenderer>().material.mainTexture.height));
        }
    }
}
