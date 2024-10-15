using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun; 

public class Laser : MonoBehaviour
{
    public PhotonView PV;

    public float duration = 2f, expandDuration = 0.5f, closeDuration = 0.3f, size = 7f;

    [SerializeField] protected Hero hero;

    [SerializeField] GameObject sparks; 
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Setup(PhotonView playerPV, float _duration, float _expand, float _close, Hero _hero) {
        PV = playerPV;
        duration = _duration;
        expandDuration = _expand;
        closeDuration = _close;
        hero = _hero; 
    }

    public void Expand() {
        StartCoroutine(ExpandAnim(0.03f)); 
    }

    public void Close() {
        StartCoroutine(CloseAnim(0.03f)); 
    }

    public IEnumerator ExpandAnim(float step) {
        gameObject.SetActive(true); 

        for (float i = 0f; i < expandDuration; i += step) {
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, (i / expandDuration) * size); 
            yield return new WaitForSeconds(step); 
        }
    }

    public IEnumerator CloseAnim(float step) {
        for (float i = expandDuration; i >+ 0f; i -= step) {
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, (i / expandDuration) * size); 
            yield return new WaitForSeconds(step); 
        }

        gameObject.SetActive(false); 
    }

    public void PlayAnim() {
        gameObject.SetActive(true); 
        StartCoroutine(Anim()); 
    }

    IEnumerator Anim() {
        Expand(); 
        yield return new WaitForSeconds(duration);
        Close(); 
    }
}
