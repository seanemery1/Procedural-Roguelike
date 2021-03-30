using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visibility : MonoBehaviour
{
    public bool isVisible;
    private void Start()
    {
        isVisible = true;
      
    }
    private void Update()
    {
        if (!isVisible)
        {
            GetComponent<SpriteRenderer>().enabled = false;
        }
        
    }
    private void LateUpdate()
    {
        if (isVisible)
        {
            GetComponent<SpriteRenderer>().enabled = true;
            isVisible = false;
        }
    }
    //void Seen()
    //{
    //    this.transform.GetComponent<SpriteRenderer>().enabled = true;
    //}
}
