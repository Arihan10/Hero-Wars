using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun; 

public class SimpleProjectile : MonoBehaviour
{
    public PhotonView PV;

    public Vector3 moveDir; 

    [SerializeField] float moveSpeed = 0.01f;

    Vector3 target;

    protected Hero hero;

    protected int ability; 
    
    // Start is called before the first frame update
    void Start()
    {
        transform.forward = moveDir; 
    }

    public void Setup(PhotonView playerPV, Vector3 dir, Vector3 _target, Hero _hero) {
        // moveDir = dir; 
        moveDir = _target - transform.position; 
        transform.forward = moveDir; 
        PV = playerPV;
        target = _target;
        hero = _hero;

        if (!PV.IsMine) {
            SetLayerRecursively(transform, "RemoteProjectile");
        }
    }

    // Update is called once per frame
    public void Update()
    {
        // transform.position = Vector3.Lerp(transform.position, target.position, speed * Time.deltaTime); 

        transform.position += transform.forward * (moveSpeed * Time.deltaTime); 

        // transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime); 
    }

    public void Reverse(PhotonView playerPV) {
        moveDir *= -1f;
        transform.forward = moveDir;
        transform.rotation = new Quaternion(0f, transform.rotation.y, transform.rotation.z, transform.rotation.w); 
        PV = playerPV;
        if (PV.IsMine) {
            SetLayerRecursively(transform, "LocalProjectile"); 
        } else {
            SetLayerRecursively(transform, "RemoteProjectile"); 
        }
    }

    public void SetLayerRecursively(Transform parent, string layer) {
        parent.gameObject.layer = LayerMask.NameToLayer(layer);

        foreach (Transform child in parent) {
            SetLayerRecursively(child, layer);
        }
    }
}
