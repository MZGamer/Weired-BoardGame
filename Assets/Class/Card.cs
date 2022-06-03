using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum cardAction {
    NONE,
    SKIP_TURN,
    GIVE_MONEY,
    DECREASE_MONEY,
    DEFEND,
    SPECIALEFFECT
}

public enum EFFECT_ID {
    NONE,
    BILL_RATIO
}

public struct roll {
    int min;
    int max;
    int effect;
}

public class Card {
    public string Name;
    [Multiline(5)]
    public string description;
    public Sprite cardImg;

    [Header("1xx:機會 2xx:作物 3xx:命運 4xx:特效")]
    public int ID;
}

[System.Serializable]
public class corpCard : Card {
    public List<int> reward;
    private int turn;

    //收成
    public int getReward() {
        return reward[turn];
    }
    public void plant() {
        turn = 0;
    }
    public bool grow() {
        turn++;
        if (turn >=5) {
            return true;
        }
        return false;
    }
    public int getTurn() {
        return turn;
    }
}

[System.Serializable]
public class actionCard : Card {
    public int turn;
    [Header("-2 : 自己, -1 : 需指定 , 5 : 全場")]
    public int target;
    public cardAction Action;
    [Header("1.")]
    public EFFECT_ID effect;
    public int getTarget() {
        return target;
    }
    public int setTarget() {
        return target;
    }
}

[System.Serializable]
public class FateCard : Card {
    public cardAction Action;
    List<roll> rools;
}


[System.Serializable]
public class  EventCard: Card {
    public EFFECT_ID effect;
}

