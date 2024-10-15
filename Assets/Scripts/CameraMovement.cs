using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun; 

public class CameraMovement : MonoBehaviour
{
    [SerializeField] Transform target; 

    [SerializeField] float smoothSpeed = 5f; 

    public Vector3 offset, ogTargetPos; 

    private void Start() {
        FindPlayer(); 
    }

    IEnumerator WaitForTarget(float duration) {
        ogTargetPos = target.position; 

        yield return new WaitForSeconds(duration);

        ogTargetPos = target.position; 
        offset = transform.position - target.position; 
    }

    private void Update() {
        if (!target) {
            FindPlayer();
            return; 
        }
        offset.x = 0f;
        offset.z = 0f; 

        if (target.gameObject.activeSelf) transform.position = Vector3.Lerp(transform.position, new Vector3(target.position.x, ogTargetPos.y, target.position.z) + offset, smoothSpeed * Time.deltaTime);
    }

    void FindPlayer() {
        GameObject[] _players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject _player in _players) {
            // Debug.Log(_player.name);
            if (_player.GetComponent<PlayerController>() && _player.GetComponent<PhotonView>().IsMine) {
                target = _player.transform;
                break;
            }
        }

        if (target) {
            // StartCoroutine(WaitForTarget(1.5f)); 
            offset = transform.position - target.position;
            offset.y = transform.position.y; 
        }
    }
}
