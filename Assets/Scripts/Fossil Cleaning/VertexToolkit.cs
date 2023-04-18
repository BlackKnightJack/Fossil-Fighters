using System.Collections;
using Unity.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VertexToolkit : MonoBehaviour
{
    //Enums
    enum Tool { HAMMER, DRILL }

    //Miscellaneous public variables
    public Button firstTool;
    public Button otherTool;

    [Header("Hammer")]
    public float hammerRadius;
    public float hammerForce;
    public SpriteRenderer hammerEffect;
    bool hammerLockout;

    [Header("Drill")]
    public float drillRadius;
    public float drillForce;
    public SpriteRenderer drillEffect;

    //General variables
    EventSystem eventSystem;
    GameObject hitEffectInstance;
    Tool currentTool;
    GameObject lastSelected;

    // Start is called before the first frame update
    void Start()
    {
        //Assign the default tool
        eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
        eventSystem.SetSelectedGameObject(firstTool.gameObject);
        lastSelected = firstTool.gameObject;
        firstTool.onClick.Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit) && !hammerLockout && currentTool == Tool.HAMMER)
        {
            //Put hit control and validation measures here later

            {
                //Just in case I'm going to store the fossil manager here
                FossilManager fossilHit = hit.collider.GetComponent<FossilManager>();
                Vector3[] vertexList = hit.collider.GetComponent<MeshFilter>().sharedMesh.vertices;

                //Iterate through the mesh vertices and change them as necessary
                for (int i = 0; i < vertexList.Length; i++)
                {
                    if (Vector3.Distance(vertexList[i], hit.point) <= hammerRadius)
                    {
                        //Process the hit itself if it's in radius range
                        fossilHit.DamageVertex(vertexList[i], (1 - (Vector3.Distance(hit.point, vertexList[i]) / hammerRadius)) * hammerForce);
                    }
                }

                //Apply mesh changes to the fossil manager
                fossilHit.ApplyChanges();

                //Apply hit effect and lock out tool use until the hit effect is done
                hammerLockout = true;
                hitEffectInstance = Instantiate(hammerEffect.gameObject, hit.point, Quaternion.identity);
                StartCoroutine(HammerLock(hitEffectInstance.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length));
            }
        }

        //*Make sure the selected item is either the drill or the hammer
        if (eventSystem.currentSelectedGameObject != firstTool.gameObject && eventSystem.currentSelectedGameObject != otherTool.gameObject)
        {
            eventSystem.SetSelectedGameObject(lastSelected);
        }
        else
        {
            lastSelected = eventSystem.currentSelectedGameObject;
        }
        //*/
    }

    void FixedUpdate()
    {
        if (Input.GetMouseButton(0) && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit) && !hammerLockout && currentTool == Tool.DRILL)
        {

            //Put hit control and validation measures here later

            {
                //Just in case I'm going to store the fossil manager here
                FossilManager fossilHit = hit.collider.GetComponent<FossilManager>();
                Vector3[] vertexList = hit.collider.GetComponent<MeshFilter>().sharedMesh.vertices;

                //Iterate through the mesh vertices and change them as necessary
                for (int i = 0; i < vertexList.Length; i++)
                {
                    if (Vector3.Distance(vertexList[i], hit.point) <= drillRadius)
                    {
                        //Process the hit itself if it's in radius range
                        fossilHit.DamageVertex(vertexList[i], (1 - (Vector3.Distance(hit.point, vertexList[i]) / drillRadius)) * drillForce);
                    }
                }

                //Apply mesh changes to the fossil manager
                fossilHit.ApplyChanges();

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

            //If the hit is not valid, remove the effect later

        }

        //Otherwise make sure the drill effect is null
        else if (currentTool == Tool.DRILL && !hammerLockout)
        {
            Destroy(hitEffectInstance);
            hitEffectInstance = null;
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
