using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Toolbox : MonoBehaviour
{
    //Enums
    enum Tool { HAMMER, DRILL }

    //Constants
    const float SECOND_LAYER = 0.4f;
    const float THIRD_LAYER = 0.7f;

    //Miscellaneous public variables
    public Button firstTool;

    [Header("Rock layers")]
    public Texture2D firstLayer;
    public Texture2D secondLayer;
    public Texture2D thirdLayer;

    [Header("Hammer")]
    public int hammerArea;
    public float hammerForce;
    [Range(0, 1)] public float immediateAreaHammer;
    public Texture2D hammerBrush;
    public SpriteRenderer hammerEffect;

    [Header("Drill")]
    public int drillArea;
    public float drillForce;
    [Range(0, 1)] public float immediateAreaDrill;
    public Texture2D drillBrush;
    public SpriteRenderer drillEffect;

    //Private variables
    Tool currentTool;
    Texture2D textInstance;
    float[,] pixMap;
    bool hammerLockout;
    GameObject hitEffectInstance;
    GameObject lastSelected;

    // Start is called before the first frame update
    void Start()
    {
        //By default the starting tool is the hammer. The buttons should be changed to reflect this
        GameObject.Find("EventSystem").GetComponent<EventSystem>().SetSelectedGameObject(firstTool.gameObject);
        firstTool.onClick.Invoke();

        //Resize the textures so they fit on the screen
        firstLayer = ResizeTexture(firstLayer, (int)(firstLayer.width * transform.localScale.x), (int)(firstLayer.height * transform.localScale.y));
        secondLayer = ResizeTexture(secondLayer, (int)(secondLayer.width * transform.localScale.x), (int)(secondLayer.height * transform.localScale.y));
        thirdLayer = ResizeTexture(thirdLayer, (int)(thirdLayer.width * transform.localScale.x), (int)(thirdLayer.height * transform.localScale.y));

        //Initialize the texture instance
        textInstance = Instantiate(firstLayer);
        textInstance.wrapMode = TextureWrapMode.Clamp;
        GetComponent<MeshRenderer>().material.mainTexture = textInstance;

        //Resize the brush and hammer effect so it is as long as the radius
        int hammerRadius = hammerBrush.width * hammerArea;
        hammerBrush = ResizeTexture(hammerBrush, hammerRadius, hammerRadius);
        hammerEffect.transform.localScale = new Vector3(hammerArea, hammerArea, 1);

        //Resize the drill based on the hit effect
        int drillRadius = drillBrush.width * drillArea;
        drillBrush = ResizeTexture(drillBrush, drillRadius, drillRadius);
        drillEffect.transform.localScale = new Vector3(drillArea, drillArea, 1);

        print(new Vector2Int(drillBrush.width, drillBrush.height));

        //Size the pixel map
        pixMap = new float[textInstance.width, textInstance.height];
    }

    // Update is called once per frame
    void Update()
    {
        //Process hammer
        if (Input.GetMouseButtonDown(0) && currentTool == Tool.HAMMER && !hammerLockout)
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
            {
                //Get the pixel clicked
                Vector2Int pixelClicked = new Vector2Int((int)(hit.textureCoord.x * textInstance.width), (int)(hit.textureCoord.y * textInstance.height));

                //Validate the surrounding area for work
                bool validate = false;
                for (int y = 0; y < (int)(hammerBrush.height * immediateAreaHammer); y++)
                {
                    for (int x = 0; x < (int)(hammerBrush.width * immediateAreaHammer); x++)
                    {
                        try
                        {
                            if (textInstance.GetPixel(pixelClicked.x + x - (int)(hammerBrush.width * immediateAreaHammer / 2), 
                                pixelClicked.y + y - (int)(hammerBrush.height * immediateAreaHammer / 2)).a == 1)
                            {
                                validate = true;
                                break;
                            }
                        }
                        catch
                        {

                        }
                    }
                    if (validate)
                    {
                        break;
                    }
                }

                //Apply a brush to the pixels if the surrounding area is validated
                if (validate)
                {
                    //Create half measures for pixel health calculations
                    int xHalf = -(hammerBrush.width / 2);
                    int yHalf = -(hammerBrush.height / 2);

                    //Calculate and change the pixels based on the red value of the brush texture times the hammer force
                    for (int y = 0; y < hammerBrush.height; y++)
                    {
                        for (int x = 0; x < hammerBrush.width; x++)
                        {
                            try
                            {
                                //Pixels above a value of 1 in damage aren't affected if they're too far away from the hit threshold (Feature to add later)
                                pixMap[pixelClicked.x + x + xHalf, pixelClicked.y + y + yHalf] += hammerBrush.GetPixel(x, y).r * hammerForce;
                                ChangePixel(pixelClicked.x + x + xHalf, pixelClicked.y + y + yHalf);
                            }
                            catch
                            {

                            }
                        }
                    }

                    //Apply any changes
                    textInstance.Apply();

                    //Apply hit effect and lock out tool use until the hit effect is done
                    hammerLockout = true;
                    hitEffectInstance = Instantiate(hammerEffect.gameObject, hit.point, Quaternion.identity);
                    StartCoroutine(HammerLock(hitEffectInstance.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length));
                }
            }
        }

        //Make sure there's always a selected item
        if (GameObject.Find("EventSystem").GetComponent<EventSystem>().currentSelectedGameObject == null)
        {
            GameObject.Find("EventSystem").GetComponent<EventSystem>().SetSelectedGameObject(lastSelected);
        }
        else
        {
            lastSelected = GameObject.Find("EventSystem").GetComponent<EventSystem>().currentSelectedGameObject;
        }
    }

    void FixedUpdate()
    {
        //Process drill
        if (Input.GetMouseButton(0) && currentTool == Tool.DRILL && !hammerLockout)
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
            {
                //Get the pixel clicked
                Vector2Int pixelClicked = new Vector2Int((int)(hit.textureCoord.x * textInstance.width), (int)(hit.textureCoord.y * textInstance.height));

                //Validate the surrounding area for work
                bool validate = false;
                for (int y = 0; y < (int)(drillBrush.height * immediateAreaDrill); y++)
                {
                    for (int x = 0; x < (int)(drillBrush.width * immediateAreaDrill); x++)
                    {
                        try
                        {
                            if (textInstance.GetPixel(pixelClicked.x + x - (int)(drillBrush.width * immediateAreaDrill / 2),
                                pixelClicked.y + y - (int)(drillBrush.height * immediateAreaDrill / 2)).a == 1)
                            {
                                validate = true;
                                break;
                            }
                        }
                        catch
                        {

                        }
                    }
                    if (validate)
                    {
                        break;
                    }
                }

                //Apply a brush to the pixels if the surrounding area is validated
                if (validate)
                {
                    //Create half measures for pixel health calculations
                    int xHalf = -(drillBrush.width / 2);
                    int yHalf = -(drillBrush.height / 2);

                    //Calculate and change the pixels based on the red value of the brush texture times the hammer force
                    for (int y = 0; y < drillBrush.height; y++)
                    {
                        for (int x = 0; x < drillBrush.width; x++)
                        {
                            try
                            {
                                //Pixels above a value of 1 in damage aren't affected if they're too far away from the hit threshold (Feature to add later)
                                pixMap[pixelClicked.x + x + xHalf, pixelClicked.y + y + yHalf] += drillBrush.GetPixel(x, y).r * drillForce;
                                ChangePixel(pixelClicked.x + x + xHalf, pixelClicked.y + y + yHalf);
                            }
                            catch
                            {

                            }
                        }
                    }

                    //Apply any changes
                    textInstance.Apply();

                    //Apply hit effect and make it follow the cursor
                    if (hitEffectInstance == null)
                    {
                        hitEffectInstance = Instantiate(drillEffect.gameObject, hit.point, Quaternion.identity);
                    }
                    else
                    {
                        hitEffectInstance.transform.position = hit.point;
                    }
                }
            }

            //Nullify hit effect
            else
            {
                Destroy(hitEffectInstance);
                hitEffectInstance = null;
            }
        }
        //Otherwise make sure the drill effect is null
        else if (currentTool == Tool.DRILL && !hammerLockout)
        {
            Destroy(hitEffectInstance);
            hitEffectInstance = null;
        }
    }

    Texture2D ResizeTexture(Texture2D source, int width, int height)
    {
        //Set up the container
        Texture2D returnValue = new Texture2D(width, height, source.format, true);

        //Fill it out
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                returnValue.SetPixel(x, y, source.GetPixelBilinear(1.0f / (float)width * (float)x, 1.0f / (float)height * (float)y), 0);
            }
        }

        //Initialize it
        returnValue.Apply();

        //Return it
        return returnValue;
    }

    //Resize brush function here

    void ChangePixel(int x, int y)
    {
        //Change to second layer
        if (pixMap[x, y] >= SECOND_LAYER && pixMap[x, y] < THIRD_LAYER)
        {
            textInstance.SetPixel(x, y, secondLayer.GetPixel(x, y));
        }

        //Else change to the third layer
        else if (pixMap[x, y] >= THIRD_LAYER && pixMap[x, y] < 1)
        {
            textInstance.SetPixel(x, y, thirdLayer.GetPixel(x, y));
        }

        //For now if it's above one just set it to clear
        else if (pixMap[x, y] >= 1)
        {
            textInstance.SetPixel(x, y, Color.clear);
        }
    }

    IEnumerator HammerLock(float duration)
    {
        yield return new WaitForSeconds(duration);
        Destroy(hitEffectInstance);
        hitEffectInstance = null;
        hammerLockout = false;
    }

    //Button methods
    public void HammerSwitch()
    {
        currentTool = Tool.HAMMER;
    }

    public void DrillSwitch()
    {
        currentTool = Tool.DRILL;
    }
}
