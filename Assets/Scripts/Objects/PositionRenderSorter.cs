using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionRenderSorter : MonoBehaviour
{
    [SerializeField]
    private int sortingOrderBase = 5000;
    [SerializeField]
    private int offset = 0;
    [SerializeField]
    private bool runOnlyOnce = false;

    private float timer;
    private float timerMax = 0.1f;
    private Renderer myRenderer;

    private void Awake()
    {
        myRenderer = gameObject.GetComponent<Renderer>();
    }
    //private void LateUpdate()
    //{
    //    timer -= Time.deltaTime;
    //    if (timer<= 0f)
    //    {
    //        timer = timerMax;
    //    }
    void Start()
    {
        InvokeRepeating("CheckSortingOrder", 0.0f, 0.1f);

    }

    private void CheckSortingOrder() { 
        myRenderer.sortingOrder = (int)(sortingOrderBase - 4*transform.position.y - offset);
        if (runOnlyOnce)
        {
            Destroy(this);
        }
    }
}
