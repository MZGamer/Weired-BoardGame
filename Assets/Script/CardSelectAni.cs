using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSelectAni : MonoBehaviour
{
    public void close() {
        gameObject.SetActive(false);
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
    }
}
