using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public float xRayDuration;
    GameObject progressFill;
    GameObject healthFill;

    //Material variables
    Material theMaterial;
    Texture2D displayTexture;
    Texture2D xRayFossil;
    float xRayStrength;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ApplyChanges()
    {

    }

    public void DamageVertex(Vector3 vertex, float amount)
    {

    }
}
