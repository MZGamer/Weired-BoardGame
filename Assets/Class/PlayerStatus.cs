using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class PlayerStatus {
    // Start is called before the first frame update
    public string name;
    public corpCard[] farm;
    public List<int> handCard;
    public int money;
    public int[] effect;

    public PlayerStatus() {
        this.farm = new corpCard[4];
        this.handCard = new List<int>();
        money = 40;
        effect = new int[1] { 1 };
    }
}
