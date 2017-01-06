using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
     
    public int m_value;
    public bool m_color;
    bool m_shown = false;

    public bool isCard( GameObject obj ) { return obj.transform.position == transform.position; }
    public bool isShown() { return m_shown; }
    public bool getColor() { return m_color; }
    public int getValue() { return m_value; }

    public void showCard()
    {
        transform.rotation = Quaternion.Euler(.0f, .0f, .0f);
        m_shown = true;
    }

    public void hideCard()
    {
        transform.rotation = Quaternion.Euler(180.0f, .0f, .0f);
        m_shown = false;
    }

    public bool compareAttribute( Card card, bool color )
    {
        return m_value == card.getValue() && ( !color || m_color == card.getColor() );
    }
}
