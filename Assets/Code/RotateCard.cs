using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RotateCard : MonoBehaviour {

    float rotationsPerMinute = 25.0f;

	// Update is called once per frame
	void Update () {
        transform.Rotate(Vector3.up * Time.deltaTime * rotationsPerMinute);
       
    }
}
