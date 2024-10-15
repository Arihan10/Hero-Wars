using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun; 

public abstract class Hero : MonoBehaviour
{
    [SerializeField] protected float moveSpeed = 5000f, jumpForce = 500f, primaryRate = 10f, secondaryRate = 4f, specialRate = 6f, primaryDuration, secondaryDuration, specialDuration, ultimateDuration; 
    protected float lastPrimary, lastSecondary, lastSpecial; 
    [SerializeField] public float maxHealth = 100f, stepSmooth = 50f; 

    public bool reflectiveShield = false; 
    protected bool primaryUse, secondaryUse, specialUse, ultimateUse; 

    [SerializeField] protected PhotonView PV; 

    public GameObject[] effects;
    // public GameObject taseEffect, radioactiveEffect; 

    public GameObject lowerRaycast, upperRaycast, eyeRaycastPoint; 

    [SerializeField] protected Animator animator;

    public void Start() {
        SetTagRecursively(transform, "Player"); 
    }

    public abstract void Move(Vector3 movement); 

    public abstract void Jump(); 

    public abstract void Primary();

    public abstract void PrimaryClicked(); 

    #region PrimaryHit overrides
    public virtual void PrimaryHit() { }
    public virtual void PrimaryHit(Collision col) { }
    public virtual void PrimaryHit(Collision col, float dist) {  }
    public virtual void PrimaryHit(Collider col) { }
    public virtual void PrimaryHit(PlayerController con, Vector3 pos) { }
    #endregion

    public abstract void Secondary(); 

    public abstract void SecondaryClicked(); 

    public virtual void SecondaryHit() { }
    public virtual void SecondaryHit(PlayerController controller, bool toggle, int layer) { }

    public abstract void Special(); 

    public abstract void SpecialClicked();

    #region SpecialHit overrides
    public virtual void SpecialHit() { }
    public virtual void SpecialHit(Vector2 touchPos) { }
    #endregion

    public abstract void Ultimate(); 

    public abstract void UltimateClicked(); 

    public abstract void UltimateHit();

    public void Update() {
        if (!PV.IsMine) return; 

        PlayerUI UI = GetComponentInParent<PlayerController>().UI;
        float primaryY = !primaryUse ? Mathf.Min((Time.time - lastPrimary) / (1f / primaryRate), 1f) : Mathf.Min((Time.time - lastPrimary) / (primaryDuration), 1f); 
        // if (primaryUse) primaryY = 1f - primaryY; 
        float secondaryY = !secondaryUse ? Mathf.Min((Time.time - lastSecondary) / (1f / secondaryRate), 1f) : Mathf.Min((Time.time - lastSecondary) / (secondaryDuration), 1f); 
        // if (secondaryUse) primaryY = 1f - primaryY; 
        float specialY = !specialUse ? Mathf.Min((Time.time - lastSpecial) / (1f / specialRate), 1f) : Mathf.Min((Time.time - lastSpecial) / (specialDuration), 1f);
        // if (specialUse) primaryY = 1f - primaryY; 
        // UI.primaryBtnFill.transform.localScale = new Vector3(UI.primaryBtnFill.transform.localScale.x, primaryY, UI.primaryBtnFill.transform.localScale.z); 
        UI.primaryBtnFill.GetComponent<Image>().fillAmount = primaryY; 
        // UI.secondaryBtnFill.transform.localScale = new Vector3(UI.secondaryBtnFill.transform.localScale.x, secondaryY, UI.secondaryBtnFill.transform.localScale.z); 
        UI.secondaryBtnFill.GetComponent<Image>().fillAmount = secondaryY; 
        // UI.specialBtnFill.transform.localScale = new Vector3(UI.specialBtnFill.transform.localScale.x, specialY, UI.specialBtnFill.transform.localScale.z); 
        UI.specialBtnFill.GetComponent<Image>().fillAmount = specialY; 
    }

    public void SetTagRecursively(Transform parent, string tag) {
        parent.gameObject.tag = tag;

        foreach (Transform child in parent) {
            SetTagRecursively(child, tag);
        }
    }
}
