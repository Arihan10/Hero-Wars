using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun; 

public class Bullet : SimpleProjectile
{
    public float damage; 

    Vector3 startPos;

    [SerializeField] GameObject trail, sparks;

    public bool hit = false; 
    
    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position; 
    }

    private void OnCollisionEnter(Collision col) {
        if (col.collider.tag == "Player") {
            Debug.Log("hit");
            if (col.collider.GetComponentInParent<PlayerController>().playerManager.playerTeam == PV.GetComponentInParent<PlayerController>().playerManager.playerTeam) {
                // Debug.Log("team " + col.collider.GetComponentInParent<PlayerController>().name + ": " + col.collider.GetComponentInParent<PlayerController>().playerManager.playerTeam + " | " + PV.GetComponentInParent<PlayerController>().name + ": " + PV.GetComponentInParent<PlayerController>().playerManager.playerTeam); 
                return;
            }
            // if (PV.ViewID == col.collider.transform.root.GetComponent<PhotonView>().ViewID) return; 

            if (col.collider.GetComponentInParent<PlayerController>().hero.reflectiveShield) {
                Reverse(col.collider.GetComponentInParent<PlayerController>().GetComponent<PhotonView>()); 
                // Debug.Log("reflective " + col.collider.GetComponentInParent<PlayerController>().name); 
                return;
            }

            if (PV.IsMine) {
                hero.PrimaryHit(col, Vector3.Distance(transform.position, startPos)); 
            }

            Debug.Log("player hit"); 
            hit = true; 
        }

        if (trail != null) {
            trail.transform.parent = null;
            Destroy(trail, 1f); 
        }
        if (sparks != null) Destroy(Instantiate(sparks, col.GetContact(0).point - transform.forward * 0.05f, sparks.transform.rotation), 2f); 
        
        Destroy(gameObject); 
    }

    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Barrier1") {
            if (other.GetComponentInParent<ProtectiveBarrier>().playerTeam == PV.GetComponentInParent<PlayerController>().playerManager.playerTeam) return;

            if (trail != null) {
                trail.transform.parent = null;
                Destroy(trail, 1f);
            }
            Destroy(Instantiate(other.GetComponentInParent<ProtectiveBarrier>().collisionEffect, transform.position, other.GetComponentInParent<ProtectiveBarrier>().collisionEffect.transform.rotation), 2f); 
            Destroy(gameObject); 
        }
    }
}
