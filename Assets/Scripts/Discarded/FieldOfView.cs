using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    public bool isVisible;
    public float distance;
    [SerializeField] public float viewRadius = 5;
    [SerializeField] public float viewAngle = 0;
    [SerializeField] public LayerMask obstacleMask, targetMask;
    Collider2D[] targetsInRadius;
    public List<Transform> visibleTargets = new List<Transform>();
    private void Start()
    { 
        //Physics2D.queriesStartInColliders = false;
    }
    public Vector3 DirFromAngle (float angleDeg, bool global)
    {
        if (global)
        {
            angleDeg += transform.eulerAngles.z;
        }
        return new Vector2(Mathf.Cos(angleDeg * Mathf.Deg2Rad), Mathf.Sin(angleDeg * Mathf.Deg2Rad));
    }

    private void Update()
    {
        FindVisibileTargets();
    }
    void FindVisibileTargets()
    {
        
        targetsInRadius = Physics2D.OverlapCircleAll(transform.position, viewRadius, targetMask);
        visibleTargets.Clear();
        for (int i = 0; i < targetsInRadius.Length; i++)
        {
            Transform target = targetsInRadius[i].transform;
            Vector2 dirToTarget = new Vector2(target.position.x - transform.position.x, target.position.y - transform.position.y);
            //if (Vector2.Angle(dirToTarget, transform.right) < viewAngle/2)
            //{
                float distanceToTarget = Vector2.Distance(transform.position, target.position);
                if (!Physics2D.Raycast(transform.position, dirToTarget, distanceToTarget, obstacleMask))
                {
                    visibleTargets.Add(target);
                    target.gameObject.GetComponent<Visibility>().isVisible = true;
                }
        }
    }
}
