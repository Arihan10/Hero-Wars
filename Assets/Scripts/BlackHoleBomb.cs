using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun; 

public class BlackHoleBomb : MonoBehaviour
{
    [SerializeField] float radius, whiteHoleTime = 3f; 
    
    PhotonView PV;

    Hero hero;

    bool hit = false;

    [SerializeField] GameObject blackHoleEffect, whiteHole; 
    
    private void OnCollisionEnter(Collision collision) {
        if (hit || (collision.collider.tag == "Player" && collision.collider.GetComponentInParent<PlayerController>().GetComponent<PhotonView>().Owner.NickName == PV.Owner.NickName)) return; 

        StartCoroutine(Explode(1f)); 
    }

    IEnumerator Explode(float duration) {
        hit = true;

        yield return new WaitForSeconds(duration);

        if (PV.IsMine) {
            Collider[] cols = Physics.OverlapSphere(transform.position, radius);
            HashSet<PlayerController> cons = new HashSet<PlayerController>();
            for (int i = 0; i < cols.Length; ++i) {
                // Debug.Log(cols[i].name);
                if (cols[i].gameObject.layer == LayerMask.NameToLayer("RemotePlayer")) {
                    Debug.Log("added"); 
                    cons.Add(cols[i].GetComponentInParent<PlayerController>());
                }
            }

            foreach (PlayerController con in cons) {
                // Debug.Log(con.name); 
                hero.PrimaryHit(con, transform.position); 
            }
        }

        Destroy(Instantiate(blackHoleEffect, transform.position, blackHoleEffect.transform.rotation), 1f);

        yield return new WaitForSeconds(blackHoleEffect.GetComponent<ParticleSystem>().main.duration); 

        GameObject whiteHoleGO = Instantiate(whiteHole, transform.position + new Vector3(0f, 0.1f, 0f), whiteHole.transform.rotation);
        whiteHoleGO.GetComponentInChildren<WhiteHole>().Setup(PV); 
        // Destroy(whiteHoleGO, whiteHoleTime); 
        Destroy(gameObject); 
    }

    public void Setup(PhotonView playerPV, Hero _hero, float _radius) {
        PV = playerPV;
        hero = _hero;
        radius = _radius; 

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
