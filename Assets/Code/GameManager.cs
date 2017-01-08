using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public List<Card> m_cardList;
    public List<Card> m_defaultList;
    public Vector2 m_boardSize;
    public bool m_easyMode;
    public CpuAi.DIFFICULTY m_difficulty;
    public bool m_playerTurn = true;
    public MenuManager m_menuManager;
    public AudioSource m_clickSfx;
    public Animator m_extraTurnAnim;

    bool m_waitCompleted = false;
    bool m_deletionCompleted = false;
    bool m_isExtraTurnAnimPlaying = false;
    bool m_challengeMode;
    Coroutine m_slowDownAI;
    int m_handicap;
    int m_playerScore = 0;
    int m_CPUScore = 0;
    Card m_firstCard;
    Card m_secondCard;
    CpuAi m_ai;


    public enum Result
    {
        LOSS,
        DRAW,
        WIN
    }

    public void Awake()
    {
        m_defaultList = new List<Card>(m_cardList);
    }

    // Use this for initialization
    public void setDifficulty(CpuAi.DIFFICULTY difficulty)
    {
        m_difficulty = difficulty;
    }

    // Use this for initialization
    public void setMode(bool mode)
    {
        m_easyMode = mode;
    }

    // Use this for initialization
    public void setChallenge(bool challenge)
    {
        m_challengeMode = challenge;
    }

    // Use this for initialization
    public void setHandicap(int handicap)
    {
        m_handicap = handicap;
    }

    // Use this for initialization
    void Start()
    {
        generateBoard();
        m_extraTurnAnim.gameObject.SetActive(false);
        m_handicap = UnityEngine.Random.Range(1,4);
    }
    
    // Update is called once per frame
    void Update()
    {
        if (m_challengeMode)
            setupChallenge();

        if (m_cardList.Count == 0)
        {
            if (m_playerScore > m_CPUScore)
                m_menuManager.onEnd_Play(Result.WIN, m_playerScore, m_CPUScore);
            else if (m_playerScore == m_CPUScore)
                m_menuManager.onEnd_Play(Result.DRAW, m_playerScore, m_CPUScore);
            else
                m_menuManager.onEnd_Play(Result.LOSS, m_playerScore, m_CPUScore);
        }
        else if (m_cardList.Count > 0)
        {
            if (m_playerTurn && !m_isExtraTurnAnimPlaying)
            {
                m_ai.resetTurn();
                if (m_firstCard == null || m_secondCard == null)
                {
                    compareShownCards(detectCardPress());
                }
            }
            else if(!m_waitCompleted && !m_isExtraTurnAnimPlaying)
            {
                m_slowDownAI = StartCoroutine(slowDownAI());
            }
            else if(!m_playerTurn && m_waitCompleted)
            {
                if(m_firstCard == null || m_secondCard == null)
                {
                    AItimeOut(m_ai.play(m_cardList));
                }
                compareShownCardsForAI();
            }
        }
    }
    
    void randomizeCardPosition(int randomTimes)
    {
        for (int index = 0; index <= randomTimes; index++)
        {
            // Knuth shuffle algorithm
            for (int t = 0; t < m_cardList.Count; t++)
            {
                Card tmp = m_cardList[t];
                int r = UnityEngine.Random.Range(t, m_cardList.Count);
                m_cardList[t] = m_cardList[r];
                m_cardList[r] = tmp;
            }
        }
    }
    
    void createCards()
    {
        Vector3 position = new Vector3(-.25f, .1f, .0f);
        for (int indexX = 0; indexX <= m_boardSize.x; indexX++)
        {
            for (int indexY = 0; indexY <= m_boardSize.y + 1; indexY++)
            {
                int cardIndex = (int)(indexX * m_boardSize.x) + indexY;
                if (cardIndex < m_boardSize.x * m_boardSize.y && cardIndex < m_cardList.Count)
                {
                    m_cardList[cardIndex] = Instantiate(m_cardList[cardIndex], position, Quaternion.Euler(180.0f, .0f, .0f));
                    m_cardList[cardIndex].transform.parent = gameObject.transform;
                    position.Set(position.x + .5f, .1f, position.z);
                }
            }
            position.Set(-.25f, .1f, position.z + .5f);
        }
    }
    
    Card findCard(GameObject obj)
    {
        for (int index = 0; index < m_cardList.Count; index++)
        {
            if (m_cardList[index].isCard(obj))
            {
                return m_cardList[index];
            }
        }
        return null;
    }
    
    bool detectCardPress()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100) && Input.GetMouseButtonDown(0) && !m_menuManager.isAdShowing() )
        {
            if (hit.collider.tag == "Card")
            {
                m_clickSfx.Play();

                if (m_firstCard == null)
                {
                    m_firstCard = findCard(hit.collider.gameObject);
                    m_firstCard.showCard();
                    return true;
                }
                else if (m_firstCard != null)
                {
                    Card tmp = findCard(hit.collider.gameObject);
                    if (!tmp.isCard(m_firstCard.gameObject))
                    {
                        m_secondCard = tmp;
                        m_secondCard.showCard();
                        return true;
                    }
                }
            }
        }
        return false;
    }
    
    void compareShownCards(bool cardPressed)
    {
        if (cardPressed && m_firstCard != null && m_secondCard != null)
        {
            StartCoroutine(slowDownPlay());
        }
    }
    
    void compareShownCardsForAI()
    {
        if (m_firstCard != null && m_secondCard != null)
        {
            handleTurnLogic();
        }
    }
    
    void hideAllCards()
    {
        for (int index = 0; index < m_cardList.Count; index++)
        {
            m_cardList[index].hideCard();
        }
    }

    bool compareCards(Card first, Card Second)
    {
        return first.getValue() == Second.getValue()
                && (m_easyMode || first.getColor() == Second.getColor());
    }
    
    void handleTurnLogic()
    {
        if (m_firstCard == null || m_secondCard == null)
            return;

        else if (m_firstCard.isCard(m_secondCard.gameObject) || m_secondCard.isCard(m_firstCard.gameObject))
        {
            m_secondCard = null;
            return;
        }

        else if (compareCards(m_firstCard, m_secondCard) && !m_firstCard.isCard(m_secondCard.gameObject) &&
            !m_secondCard.isCard(m_firstCard.gameObject) && !m_deletionCompleted)
        {
            Card tmpCard1 = m_firstCard;
            Card tmpCard2 = m_secondCard;
            m_cardList.Remove(m_firstCard);
            m_cardList.Remove(m_secondCard);
            m_ai.removeFromAIList(m_firstCard);
            m_ai.removeFromAIList(m_secondCard);
            m_firstCard = null;
            m_secondCard = null;
            Destroy(tmpCard1.gameObject);
            Destroy(tmpCard2.gameObject);
            updateScore();

            if(!m_playerTurn)
            {
                StopAllCoroutines();
                m_ai.resetTurn();
                m_deletionCompleted = false;
                m_waitCompleted = false;
            }

            m_isExtraTurnAnimPlaying = true;
            m_extraTurnAnim.gameObject.SetActive(true);
            m_extraTurnAnim.Play("ExtraTurn");
            StartCoroutine(completeExtraTurnAnim());
        }
        else if(!m_deletionCompleted)
        {
            hideAllCards();
            m_firstCard = null;
            m_secondCard = null;
            switchTurn();
        }
    }
    
    void removeCard(Card card)
    {
        for (int i = 0; i < m_cardList.Count; i++)
        {
            if (m_cardList[i].isCard(card.gameObject))
            {
                m_cardList.Remove(card);
                return;
            }
        }
        Debug.Log("Failed to delete");
    }
    
    void AItimeOut(int cardIndex)
    {
        if (m_firstCard == null && cardIndex < m_cardList.Count && !m_cardList[cardIndex].isShown())
        {
            m_firstCard = m_cardList[cardIndex];
        }
        else if (m_secondCard == null && cardIndex < m_cardList.Count && !m_cardList[cardIndex].isShown())
        {
            m_secondCard = m_cardList[cardIndex];
        }
    }
    
    IEnumerator slowDownPlay()
    {
        yield return new WaitForSeconds(1);
        handleTurnLogic();
        m_deletionCompleted = false;
        m_waitCompleted = false;
    }

    IEnumerator completeExtraTurnAnim()
    {
        yield return new WaitForSeconds(1);
        m_extraTurnAnim.Play("Idle");
        m_extraTurnAnim.gameObject.SetActive(false);
        m_isExtraTurnAnimPlaying = false;
    }

    IEnumerator slowDownAI()
    {
        yield return new WaitForSeconds(1);
        m_deletionCompleted = false;
        m_waitCompleted = true;
    }

    void switchTurn()
    {
        m_playerTurn = !m_playerTurn;
        m_deletionCompleted = true;
        switchTurnLbl();
    }
    
    void switchTurnLbl()
    {
        m_menuManager.updateTurn(m_playerTurn && m_waitCompleted);
    }
    
    void updateScore()
    {
        if (m_playerTurn)
            m_menuManager.updatePlayerScore(++m_playerScore);
        else
            m_menuManager.updateOpponentScore(++m_CPUScore);
    }

    void resetScore()
    {
         m_menuManager.updatePlayerScore(0);
         m_menuManager.updateOpponentScore(0);
    }

    public void resetManager()
    {
        m_cardList = new List<Card>(m_defaultList);
        resetScore();
        m_playerScore = 0;
        m_CPUScore = 0;
        m_handicap = UnityEngine.Random.Range(1, 4); ;
        int randomTimes = UnityEngine.Random.Range(5, 20);
        randomizeCardPosition(randomTimes);
        createCards();
        m_ai = new CpuAi(m_difficulty, !m_easyMode);
    }
    
    void generateBoard()
    {
        int randomTimes = UnityEngine.Random.Range(5, 20);
        randomizeCardPosition(randomTimes);
        createCards();
        m_playerTurn = true;
        m_ai = new CpuAi(m_difficulty, !m_easyMode);
    }

    public bool getPlayerTurn()
    {
        return m_playerTurn;
    }

    void setupChallenge()
    {
        if (m_handicap > 0 )
        {
            Card firstCard = m_cardList[0];

            for (int i = 0; i < m_cardList.Count; i++)
            {
                if (compareCards(firstCard, m_cardList[i]) && !m_cardList[i].isCard(firstCard.gameObject))
                {
                    Card secondCard = m_cardList[i];
                    m_cardList.Remove(firstCard);
                    m_cardList.Remove(secondCard);
                    m_ai.removeFromAIList(firstCard);
                    m_ai.removeFromAIList(secondCard);
                    Destroy(firstCard.gameObject);
                    Destroy(secondCard.gameObject);
                    m_menuManager.updateOpponentScore(++m_CPUScore);
                    break;
                }
            }
            m_handicap--;
            if (m_handicap > 0)
                setupChallenge();
        }
    }
}
