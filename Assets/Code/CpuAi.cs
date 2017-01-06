using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CpuAi : MonoBehaviour {

    public enum DIFFICULTY
    {
        EASY,
        NORMAL,
        HARD
    }
    
    DIFFICULTY m_difficulty;
    Card m_firstCardTurn;
    List<Card> m_preSelectedCards = new List<Card>();
    bool m_firstCard;
    bool m_color;

    public CpuAi()
    {
        m_difficulty = DIFFICULTY.EASY;
        m_firstCard = true;
        m_color = false;
    }

    public CpuAi( DIFFICULTY difficulty, bool color )
    {
        m_difficulty = difficulty;
        m_firstCard = true;
        m_color = true;
    }

    public int play( List<Card> cardList )
    {
        if(m_firstCard)
        {
            int index = chooseRandomCard(cardList);
            m_firstCardTurn = cardList[index];
            m_preSelectedCards.Add(m_firstCardTurn);
            m_firstCard = false;
            return index;
        }

        switch (m_difficulty)
        {
            case DIFFICULTY.EASY:
                return chooseRandomCard(cardList);
            case DIFFICULTY.NORMAL:
                return chooseCard(cardList);
            case DIFFICULTY.HARD:
                return chooseCard(cardList);
            default:
                return chooseRandomCard(cardList);
        }
    }

    int chooseRandomCard(List<Card> cardList)
    {
        int randomIndex = UnityEngine.Random.Range(0, cardList.Count);
        m_preSelectedCards.Add(cardList[randomIndex]);
        return randomIndex;
    }

    int chooseCard(List<Card> cardList)
    {
        if (m_preSelectedCards.Count == 0)
        {
            return chooseRandomCard(cardList);
        }
        else
        {
            Card tmpCard = cardList[0];
            for(int i = 0; i < m_preSelectedCards.Count; i++)
            {
                if(m_preSelectedCards[i] != null && m_firstCardTurn != null
                    && m_preSelectedCards[i].compareAttribute(m_firstCardTurn, m_color)
                    && !m_preSelectedCards[i].isCard(m_firstCardTurn.gameObject) )
                {
                    int index = cardList.IndexOf(m_preSelectedCards[i]);
                    if (index > -1)
                    {
                        m_preSelectedCards.Add(cardList[index]);
                        return index;
                    }
                }
            }

            return chooseRandomCard(cardList);
        }
    }

    public void resetTurn()
    {
        m_firstCard = true;
    }

    public void resetAI()
    {
        m_firstCard = true;
        m_preSelectedCards = new List<Card>();
    }

    public void removeFromAIList(Card card)
    {
        if(m_preSelectedCards.Contains(card) )
        {
            m_preSelectedCards.Remove(card);
        }
    }

}
