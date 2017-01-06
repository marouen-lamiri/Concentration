using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class resizeText : MonoBehaviour {

    public Text textToBeResized;
    public int maxFontSizeBeforeResize;

    int defaultFontSize;

    void Start()
    {
        defaultFontSize = textToBeResized.fontSize;
    }

    void Update()
    {
        if(textToBeResized.text.Length > maxFontSizeBeforeResize && textToBeResized.fontSize > 10)
        {
            textToBeResized.fontSize = defaultFontSize - (textToBeResized.text.Length - maxFontSizeBeforeResize);
        }
    } 
}
