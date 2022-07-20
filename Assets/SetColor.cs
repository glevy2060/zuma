using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetColor : MonoBehaviour {
    Renderer rend;

	// Use this for initialization
	void Start () {
        rend = GetComponent<Renderer>();
        Color backround = new Color(1, 320, (float)0.30); //#012030
        rend.material.SetColor("_Color", backround);

    }
	
	// Update is called once per frame
	void Update () {
    }
}
