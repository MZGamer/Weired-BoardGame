using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ACTION {
    NULL,
    GAMESTART,
    NEW_TURN,
    NEXT_PLAYER,
    CARD_ACTIVE,
    ASSIGN_PLAYER_ID,
    GET_NEW_CARD,
    HARVEST
}
public class Package {
    public int src;
    public ACTION ACTION;
    public int index;
    public int target;
    public List<PlayerStatus> playerStatuses;

    public Package(int src, ACTION ACTION = ACTION.NULL,int index =-1,int target = -1, List<PlayerStatus> playerStatuses = null) {
        this.src = src;
        this.ACTION = ACTION;
        this.playerStatuses = playerStatuses;
        this.index = index;
        this.target = target;
    }
}
