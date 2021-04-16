using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Method that is used to determine an object's position relative to the world for sprite layering purposes.
public class PositionRenderSorter : MonoBehaviour
{

    [SerializeField]
    private int sortingOrderBase = 5000;
    [SerializeField]
    private int offset = 0;
    [SerializeField]
    private bool runOnlyOnce = false;

    private Renderer myRenderer;

    // Retrive render component on load
    private void Awake()
    {
        myRenderer = gameObject.GetComponent<Renderer>();
    }

    // Every 0.1f seconds, check sorting order (as opposed to doing it every frame with Update()).
    void Start()
    {
        InvokeRepeating("CheckSortingOrder", 0.0f, 0.1f);

    }

    // Static objects only need to have their sorting positions determined once, otherwise determine what sorting layer object should have.
    // (objects with higher lower Y values will be sorted in front of objects with higher Y values)
    private void CheckSortingOrder() { 
        myRenderer.sortingOrder = (int)(sortingOrderBase - 4*transform.position.y - offset);
        if (runOnlyOnce)
        {
            Destroy(this);
        }
    }
}
