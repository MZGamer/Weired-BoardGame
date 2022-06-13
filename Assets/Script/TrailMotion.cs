using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailMotion : MonoBehaviour
{
    public Vector3 start,mid, end;
    public float t;
    public bool playing = false;
    int count;
    public Animator animator;
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        if (playing) {
            UI_Manager.Animationing = true;

            Vector3 pos = (Mathf.Pow(1 - t, 2) * start) + (2 * t * (1 - t) * mid) + (Mathf.Pow(t, 2) * end);

            gameObject.transform.localPosition =  new Vector3(pos.x, pos.y, pos.z );

        }

    }

    public void playcountcheck () {
        if (count<5) {
            count++;

        } else {
            count = 0;
            animator.Play("Idle");
            playing = false;
            gameObject.SetActive(false);
            UI_Manager.Animationing = false;
        }
    }
}
