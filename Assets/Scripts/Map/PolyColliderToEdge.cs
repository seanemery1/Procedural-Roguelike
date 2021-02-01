using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolyColliderToEdge : MonoBehaviour
{
    void Awake()
    {
        PolygonCollider2D poly = GetComponent<PolygonCollider2D>();
        Vector2[] points = poly.points;
        EdgeCollider2D edge = gameObject.AddComponent<EdgeCollider2D>();
        edge.points = points;
        Destroy(poly);
    }

 
}
