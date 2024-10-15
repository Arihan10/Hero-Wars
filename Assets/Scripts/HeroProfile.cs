using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

[CreateAssetMenu(fileName = "New Hero", menuName = "Hero")]
public class HeroProfile : ScriptableObject {
    public string name, description;

    public float cost;

    public Sprite sprite;  
}
