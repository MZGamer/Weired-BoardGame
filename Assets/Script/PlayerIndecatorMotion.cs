using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIndecatorMotion : MonoBehaviour
{
    public TrailRenderer emit;

    public float t;
    public int goTo = -2;
    public int maxPlayer;

    public bool move;
    // Update is called once per frame
    void Update()
    {
        if(!move)
            return;
        if(goTo == -1) {
            emit.emitting = false;
            transform.localPosition = new Vector3(0 + 260 * t, 10 - 102 * (maxPlayer - 1), 0 );
            
        } else if (goTo == 0) {
            emit.emitting = true;
            transform.localPosition = new Vector3(260 - 260 * t, 10, 0);
        } else {
            emit.emitting = true;
            transform.localPosition = new Vector3(0f, 10 - 102 * (goTo-1) - 102 * t , 0);
        }
        if(t == 1) {
            move = false;
        }
    }

    public void moveDone() {
        move = false;
        UI_Manager.Animationing = false;
    }
    public void startMove() {
        move = true;
        UI_Manager.Animationing = true;
    }

}
