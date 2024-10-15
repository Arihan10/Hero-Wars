using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Photonix : Hero {
    Rigidbody rb;

    [SerializeField] GameObject[] rings; 
    [SerializeField] GameObject sunBeam, character; 

    [SerializeField] Transform sunBeamSpawnPoint;

    [SerializeField] float beamDamage = 15f, dashForce = 20f, invincibleRotateSpeed = 60f, ringRotateSpeed = 20f;
    List<float> ringOffsets = new List<float>(); 
    float xOff, yOff, zOff; 

    [SerializeField] Material shieldMat, invincibleMat; 

    // Start is called before the first frame update
    public void Awake() {
        rb = GetComponentInParent<Rigidbody>();
        PV = GetComponent<PhotonView>();

        xOff = Random.Range(0f, 360f); 
        yOff = Random.Range(0f, 360f); 
        zOff = Random.Range(0f, 360f); 

        foreach (GameObject ring in rings) {
            ringOffsets.Add(Random.Range(0f, 360f)); 
        }
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

            PV.RPC("RPC_LaunchBeam", RpcTarget.All); 

            lastPrimary = Time.time;
        }
    }

    [PunRPC]
    void RPC_LaunchBeam() {
        // GameObject beam = Instantiate(sunBeam, sunBeamSpawnPoint.position, sunBeam.transform.rotation); 
        // sunBeam.GetComponent<Laser>().PlayAnim(); 
        StartCoroutine(BeamAnim()); 
    }

    IEnumerator BeamAnim() {
        sunBeam.SetActive(true);
        primaryUse = true; 
        yield return StartCoroutine(sunBeam.GetComponent<Laser>().ExpandAnim(0.03f)); 

        yield return new WaitForSeconds(sunBeam.GetComponent<Laser>().duration); 

        yield return StartCoroutine(sunBeam.GetComponent<Laser>().CloseAnim(0.03f));
        sunBeam.SetActive(false); 
        lastPrimary = Time.time;
        primaryUse = false; 

    }

    public override void PrimaryClicked() { }

    public override void PrimaryHit(Collider col) {
        col.GetComponentInParent<PlayerController>().playerManager.TakeDamage(beamDamage * Time.fixedDeltaTime, GetComponentInParent<PlayerController>().playerManager.GetComponent<PhotonView>().ViewID); 
    }

    public override void PrimaryHit(Collision col) {
        col.collider.GetComponentInParent<PlayerController>().playerManager.TakeDamage(beamDamage * Time.fixedDeltaTime, GetComponentInParent<PlayerController>().playerManager.GetComponent<PhotonView>().ViewID);
    }

    public override void Secondary() {
        if (Time.time - lastSecondary > 1f / secondaryRate && !secondaryUse) {
            Debug.Log("Used secondary");

            PV.RPC("RPC_LaunchReflectiveShield", RpcTarget.All); 

            lastSecondary = Time.time; 
        }
    }

    [PunRPC]
    void RPC_LaunchReflectiveShield() {
        StartCoroutine(LaunchReflectiveShield()); 
    }

    IEnumerator LaunchReflectiveShield() {
        StartCoroutine(SetMatsRecursively(character.transform, secondaryDuration + 0.2f, shieldMat)); 

        reflectiveShield = true;
        secondaryUse = true;
        SetAnimProp("shield", true); 

        yield return new WaitForSeconds(secondaryDuration);

        SetAnimProp("shield", false); 
        reflectiveShield = false;
        secondaryUse = false; 

        lastSecondary = Time.time; 
        // transform.rotation = Quaternion.identity; 
    }

    public override void SecondaryClicked() { }

    public override void SecondaryHit() {
        Debug.Log("secondary hit"); 
    }

    public override void Special() {
        if (Time.time - lastSpecial > 1f / specialRate) {
            Debug.Log("Used special");

            rb.AddForce(transform.forward * dashForce);
            PV.RPC("RPC_MakeInvincible", RpcTarget.All); 

            lastSpecial = Time.time; 
        }
    }

    [PunRPC]
    void RPC_MakeInvincible() {
        StartCoroutine(Invincibility()); 
    }

    IEnumerator Invincibility() {
        StartCoroutine(SetMatsRecursively(character.transform, specialDuration, invincibleMat)); 

        GetComponentInParent<PlayerController>().SetLayerRecursively(GetComponentInParent<PlayerController>().transform, "Invincible"); 
        specialUse = true; 

        yield return new WaitForSeconds(specialDuration);

        lastSpecial = Time.time; 
        GetComponentInParent<PlayerController>().SetLayerRecursively(GetComponentInParent<PlayerController>().transform, PV.IsMine ? "LocalPlayer" : "RemotePlayer");
        specialUse = false; 
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

        for (int i = 0; i < rings.Length; ++i) {
            rings[i].transform.rotation = Quaternion.Euler(Mathf.Sin(Time.time * ringRotateSpeed + xOff + ringOffsets[i]) * 360f, Mathf.Sin(Time.time * ringRotateSpeed + yOff + ringOffsets[i]) * 360f, Mathf.Sin(Time.time * ringRotateSpeed + zOff + ringOffsets[i]) * 360f); 
        }

        /*if (secondaryUse) {
            // transform.Rotate(new Vector3(0f, 0f, invincibleRotateSpeed * Time.deltaTime)); 
            transform.Rotate(new Vector3(0f, invincibleRotateSpeed * Time.deltaTime, 0f)); 
        }*/
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
