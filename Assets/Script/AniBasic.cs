using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AniBasic : MonoBehaviour
{
    // Start is called before the first frame update
    void aniStart() {
        UI_Manager.Animationing = true;
    }
    void aniDone() {
        UI_Manager.Animationing = false;
    }
}
