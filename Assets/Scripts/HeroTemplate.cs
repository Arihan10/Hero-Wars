using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class HeroTemplate : Hero {
    Rigidbody rb; 

    // Start is called before the first frame update
    public void Awake() {
        rb = GetComponentInParent<Rigidbody>();
        PV = GetComponent<PhotonView>();
    }

    public override void Move(Vector3 movement) {
        rb.AddForce(movement * (moveSpeed * Time.fixedDeltaTime));
    }

    public override void Jump() {
        rb.AddForce(Vector3.up * 100f, ForceMode.VelocityChange);
    }

    public override void Primary() {
        if (Time.time - lastPrimary > 1f / primaryRate) {
            Debug.Log("Used primary");

            lastPrimary = Time.time;
        }
    }

    public override void PrimaryClicked() { }

    public override void Secondary() {
        if (Time.time - lastSecondary > 1f / secondaryRate && !secondaryUse) {
            Debug.Log("Used secondary");

            lastSecondary = Time.time;
        }
    }

    public override void SecondaryClicked() { }

    public override void SecondaryHit() {
        Debug.Log("secondary hit");
    }

    public override void Special() {
        if (Time.time - lastSpecial > 1f / specialRate) {
            Debug.Log("Used special");

            lastSpecial = Time.time;
        }
    }

    public override void SpecialClicked() { }

    public override void SpecialHit() {
        Debug.Log("special hit");
    }

    public override void Ultimate() {
        Debug.Log("Used ultimate");
    }

    public override void UltimateClicked() { }

    public override void UltimateHit() {
        Debug.Log("ultimate hit");
    }

    public void Update() {
        base.Update();
        if (!PV.IsMine) return;

        if (rb.velocity.magnitude >= 0.25f) {
            SetAnimProp("running", true);
        } else {
            SetAnimProp("running", false);
        }
    }

    public void SetAnimProp(string propertyName, bool value) {
        PV.RPC("RPC_SetAnimProp", RpcTarget.All, propertyName, value);
    }

    [PunRPC]
    void RPC_SetAnimProp(string propertyName, bool value) {
        animator.SetBool(propertyName, value);
    }

    IEnumerator SetMatsRecursively(Transform parent, float duration, Material mat) {
        Material[] mats = new Material[0];
        if (parent.gameObject.GetComponent<MeshRenderer>()) {
            mats = parent.gameObject.GetComponent<MeshRenderer>().materials;
            Material[] _mats = new Material[mats.Length];
            for (int i = 0; i < _mats.Length; ++i) _mats[i] = mat;
            parent.gameObject.GetComponent<MeshRenderer>().materials = _mats;
        }

        foreach (Transform child in parent) {
            StartCoroutine(SetMatsRecursively(child, duration, mat));
        }

        yield return new WaitForSeconds(duration);

        if (parent.gameObject.GetComponent<MeshRenderer>()) parent.gameObject.GetComponent<MeshRenderer>().materials = mats;
    }
}
