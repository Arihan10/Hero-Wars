using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayCaster : MonoBehaviour
{
    [SerializeField] LayerMask mask;

    [SerializeField] GameObject debug; 
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public RaycastHit? Shoot(GameObject raycastPoint)
    {
        RaycastHit hit; 
        if (Physics.Raycast(raycastPoint.transform.position, raycastPoint.transform.forward, out hit, Mathf.Infinity, mask)) {
            // Instantiate(debug, hit.point, Quaternion.identity); 
            return hit; 
        }

        return null; 
    }

    public RaycastHit? Shoot(GameObject raycastPoint, Vector3 raycastDir) {
        RaycastHit hit;
        if (Physics.Raycast(raycastPoint.transform.position, raycastDir, out hit, Mathf.Infinity, mask)) {
            // Instantiate(debug, hit.point, Quaternion.identity); 
            return hit;
        }

        return null;
    }
}
