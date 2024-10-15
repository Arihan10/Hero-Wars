using UnityEngine;

public class RaycastCapsule : MonoBehaviour {
    public float debugDrawDuration = 1.0f; 

    PlayerController[] players; 

    [SerializeField] float adjustThreshold = 15f; 

    public Vector3 Shoot(Vector3 currPoint) {
        players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None); 

        CapsuleCollider col = null; 
        float leastAngle = Mathf.Infinity; 
        foreach (PlayerController controller in players) {
            if (controller == GetComponent<PlayerController>()) continue; 

            float playerAngle = Mathf.Abs(Vector3.SignedAngle(transform.forward, controller.transform.position - transform.position, Vector3.up));
            RaycastHit? hit = GetComponentInParent<RayCaster>().Shoot(GetComponentInParent<PlayerController>().hero.eyeRaycastPoint, controller.transform.position - transform.position); 
            if (hit.HasValue && hit.Value.collider.tag == "Player" && playerAngle <= adjustThreshold && playerAngle < leastAngle) {
                col = controller.hero.GetComponentInChildren<CapsuleCollider>();
            }
        }

        if (col == null) return currPoint; 

        float startAngle, endAngle;
        RayHitsCapsule(GetComponent<PlayerController>().hero.eyeRaycastPoint.transform.position, col, out startAngle, out endAngle); 
        // startAngle = NormalAngle(startAngle); 
        // endAngle = NormalAngle(endAngle); 

        // Debug.Log("Hit range: " + startAngle + " to " + endAngle);

        // For visualization
        Vector3 startDir = Quaternion.Euler(0, startAngle, 0) * Vector3.forward;
        Vector3 endDir = Quaternion.Euler(0, endAngle, 0) * Vector3.forward;

        Debug.DrawRay(GetComponent<PlayerController>().hero.eyeRaycastPoint.transform.position, startDir * 10f, Color.green, 0.1f); 
        Debug.DrawRay(GetComponent<PlayerController>().hero.eyeRaycastPoint.transform.position, endDir * 10f, Color.red, 0.1f); 

        // (transform.eulerAngles.y >= startAngle && transform.eulerAngles.y <= endAngle) || (transform.eulerAngles.y <= startAngle && transform.eulerAngles.y >= endAngle)

        Debug.DrawRay(GetComponent<PlayerController>().hero.eyeRaycastPoint.transform.position, transform.forward * 10f, Color.blue); 

        float diff = Vector3.SignedAngle(transform.forward, col.transform.position - transform.position, Vector3.up);
        // diff = NormalAngle(diff);
        // Debug.Log(diff); 
        Vector3 dir; 
        float targetPercent = (diff + adjustThreshold) / (adjustThreshold * 2);
        // Debug.Log(diff + " " + targetPercent + " " + startAngle + " " + endAngle + " " + targetPercent); 
        float angle = Mathf.Lerp(endAngle, startAngle, targetPercent);

        dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;

        Debug.DrawRay(GetComponent<PlayerController>().hero.eyeRaycastPoint.transform.position, dir * 10f, Color.yellow); 

        return GetComponent<RayCaster>().Shoot(GetComponent<PlayerController>().hero.eyeRaycastPoint, dir).Value.point; 
    }

    private void Update() {
        players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None); 

        float startAngle, endAngle;
        CapsuleCollider col = null;
        foreach (PlayerController con in players) if (con != GetComponent<PlayerController>()) col = con.hero.GetComponentInChildren<CapsuleCollider>();

        if (col == null) return; 

        RayHitsCapsule(GetComponent<PlayerController>().hero.eyeRaycastPoint.transform.position, col, out startAngle, out endAngle); 
        startAngle = NormalAngle(startAngle); 
        endAngle = NormalAngle(endAngle); 

        // Debug.Log("Hit range: " + startAngle + " to " + endAngle);

        // For visualization
        Vector3 startDir = Quaternion.Euler(0, startAngle, 0) * Vector3.forward;
        Vector3 endDir = Quaternion.Euler(0, endAngle, 0) * Vector3.forward;

        Debug.DrawRay(transform.position, startDir * 10f, Color.green); 
        Debug.DrawRay(transform.position, endDir * 10f, Color.red); 
    }

    void RayHitsCapsule(Vector3 origin, CapsuleCollider capsule, out float startAngle, out float endAngle) {
        Vector3 toCapsuleCenter = capsule.transform.position - origin;
        toCapsuleCenter.y = 0; 

        float distanceToCenter = toCapsuleCenter.magnitude;

        if (distanceToCenter <= capsule.radius) {
            // Ray origin is inside the capsule
            startAngle = 0;
            endAngle = 360;
            return; 
        }

        float angleToCenter = Vector3.SignedAngle(Vector3.forward, toCapsuleCenter, Vector3.up);
        float deviationAngle = Mathf.Asin((capsule.radius * capsule.transform.localScale.x) / distanceToCenter) * Mathf.Rad2Deg;

        startAngle = angleToCenter - deviationAngle;
        endAngle = angleToCenter + deviationAngle; 
    }

    float NormalAngle(float angle) {
        if (angle < 0f) angle += 360f; 
        return angle; 
    }
}
