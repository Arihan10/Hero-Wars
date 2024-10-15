using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunBeam : Laser
{
    private void OnTriggerStay(Collider other) {
        if (other.tag == "Player") {
            if (other.GetComponentInParent<PlayerController>().playerManager.playerTeam == PV.GetComponentInParent<PlayerController>().playerManager.playerTeam) {
                return;
            }
            // if (PV.ViewID == col.collider.transform.root.GetComponent<PhotonView>().ViewID) return; 
            if (PV.IsMine) {
                hero.PrimaryHit(other); 
            }

            Debug.Log("player hit");
        }

        // Debug.Log("UNITY FUCK YOU IM GONNA MOTHERFUCKING KILL YOU UNITY KILL YOURSELVES"); 

        // Destroy(Instantiate(sparks, col.GetContact(0).point - transform.forward * 0.05f, sparks.transform.rotation), 2f); 
    }
}
