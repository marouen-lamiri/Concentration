using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Facebook.Unity;
using UnityEngine.Advertisements;
using GoogleMobileAds.Api;


public class MenuManager : MonoBehaviour {

    public GameObject m_dialogLoggedIn;
    public GameObject m_dialogLoggedOut;
    public GameObject m_dialogUsername;
    public GameObject m_dialogProfilePic;
    public GameObject m_dialogSinglePlayer;
    public GameObject m_dialogInGame;
    public GameObject m_dialogModeMenu;
    public GameObject m_dialogEndGameMenu;
    public GameObject m_btnShare;
    public GameObject m_menuCards;

    public GameManager m_gameManager;
    public FBScript m_fbScript;

    public Material defaultMaterial;
    public Material defaultMaterial_Transparent;
    public Material playerMaterial;
    public Material playerMaterial_Transparent;
    public Material cpuMaterial;
    public Material cpuMaterial_Transparent;
    public Material textMaterial;
    public Material textMaterial_Transparent;

    public Image m_playerProfilePic;
    public Image m_cpuProfilePic;

    public Text m_playerScoreLbl;
    public Text m_CPUScoreLbl;
    public Text m_resultLbl;
    public Text m_playerLbl;
    public Text m_cpuLbl;

    public AudioSource m_clickSfx;

    string m_shareMsg;
    CpuAi.DIFFICULTY m_difficulty;
    bool m_easyMode;
    int m_winStreak = 0;
    int m_counterToWatchAds = 0;
    const int m_adChecker = 2;

    void Awake()
    {
        Advertisement.Initialize("1246730");
        RequestBanner();
        m_gameManager.gameObject.SetActive(false);
    }

    void Update()
    {
        updateTurn(m_gameManager.getPlayerTurn());
    }

    public void initUI( bool isLoggedIn )
    {
        if (isLoggedIn)
        {
            m_dialogLoggedIn.SetActive(true);
            m_dialogLoggedOut.SetActive(false);
        }
        else
        {
            m_dialogLoggedIn.SetActive(false);
            m_dialogLoggedOut.SetActive(true);
        }

        m_dialogInGame.SetActive(false);
        m_dialogSinglePlayer.SetActive(false);
        m_dialogModeMenu.SetActive(false);
        m_dialogEndGameMenu.SetActive(false);
        m_menuCards.SetActive(true);
    }

    public void updateUsername( string name )
    {
        Text username = m_dialogUsername.GetComponent<Text>();
        username.text = name;
    }

    public void updateUserProfilePic( Sprite imgSprite )
    {
        Image profilePic = m_dialogProfilePic.GetComponent<Image>();
        profilePic.sprite = imgSprite;
        m_playerProfilePic.material = defaultMaterial;
    }

    public void onClick_SinglePlayer()
    {
        m_clickSfx.Play();
        m_dialogLoggedIn.SetActive(false);
        m_dialogLoggedOut.SetActive(false);
        m_dialogInGame.SetActive(false);
        m_dialogSinglePlayer.SetActive(false);
        m_dialogModeMenu.SetActive(true);
        m_dialogEndGameMenu.SetActive(false);
        m_gameManager.setChallenge(false);
        m_menuCards.SetActive(true);
    }

    public void onClick_ChallengeMode()
    {
        m_clickSfx.Play();
        m_dialogLoggedIn.SetActive(false);
        m_dialogLoggedOut.SetActive(false);
        m_dialogInGame.SetActive(false);
        m_dialogSinglePlayer.SetActive(false);
        m_dialogModeMenu.SetActive(true);
        m_dialogEndGameMenu.SetActive(false);
        m_gameManager.setChallenge(true);
        m_menuCards.SetActive(true);
    }

    public void onClick_Mode(bool easy)
    {
        m_easyMode = easy;
        m_clickSfx.Play();
        m_dialogLoggedIn.SetActive(false);
        m_dialogLoggedOut.SetActive(false);
        m_dialogInGame.SetActive(false);
        m_dialogSinglePlayer.SetActive(true);
        m_dialogModeMenu.SetActive(false);
        m_dialogEndGameMenu.SetActive(false);
        m_gameManager.setMode(m_easyMode);
        m_menuCards.SetActive(true);
    }

    public void onClick_Difficulty(CpuAi.DIFFICULTY difficulty)
    {
        ShowAd();
        m_clickSfx.Play();
        m_difficulty = difficulty;
        m_dialogLoggedIn.SetActive(false);
        m_dialogLoggedOut.SetActive(false);
        m_dialogInGame.SetActive(true);
        m_dialogSinglePlayer.SetActive(false);
        m_dialogModeMenu.SetActive(false);
        m_dialogEndGameMenu.SetActive(false);
        m_gameManager.setDifficulty(m_difficulty);
        m_gameManager.gameObject.SetActive(true);
        m_menuCards.SetActive(false);
        setFBProfilePic(m_dialogProfilePic.GetComponent<Image>());
    }

    public void onClick_Difficulty_Easy()
    {
        onClick_Difficulty(CpuAi.DIFFICULTY.EASY);
    }

    public void onClick_Difficulty_Normal()
    {
        onClick_Difficulty(CpuAi.DIFFICULTY.NORMAL);
    }

    public void onClick_Difficulty_Hard()
    {
        onClick_Difficulty(CpuAi.DIFFICULTY.HARD);
    }

    public void onEnd_Play(GameManager.Result won, int playerScore, int oppScore)
    {
        m_dialogEndGameMenu.SetActive(true);
        m_menuCards.SetActive(true);
        if (FB.IsLoggedIn)
        {
            m_btnShare.SetActive(true);
        }
        else
        {
            m_btnShare.SetActive(false);
        }

        switch (won)
        {
            case GameManager.Result.LOSS:
                m_resultLbl.text = "YOU LOST!";
                m_winStreak = 0;
                m_shareMsg = "I lost at an " + difficultyToString(m_difficulty) + " difficulty " 
                    + playerScore + " to " + oppScore + ". I'm disappointed!";

                break;
            case GameManager.Result.DRAW:
                m_resultLbl.text = "YOU DREW!";
                m_winStreak = 0;
                m_shareMsg = "I drew at an " + difficultyToString(m_difficulty) + " difficulty. So close yet so far! ";
                break;
            case GameManager.Result.WIN:
                m_resultLbl.text = "YOU WON!";
                m_winStreak++;
                if (m_winStreak % 5 == 0)
                {
                    m_shareMsg = "I won " + m_winStreak + " in a row. Always winning!";
                }
                else
                {
                    m_shareMsg = "I won at an " + difficultyToString(m_difficulty) + " difficulty "
                    + playerScore + " to " + oppScore + ". Winning is in my blood!";
                }
                break;
            default:
                m_resultLbl.text = "YOU DREW!";
                m_winStreak = 0;
                m_shareMsg = "I drew at an " + difficultyToString(m_difficulty) + " difficulty. So close yet so far! ";
                break;
        }
    }

    public string difficultyToString(CpuAi.DIFFICULTY difficulty)
    {
        if (difficulty == CpuAi.DIFFICULTY.EASY)
            return "easy";
        else if (difficulty == CpuAi.DIFFICULTY.NORMAL)
            return "normal";
        else if (difficulty == CpuAi.DIFFICULTY.HARD)
            return "hard";
        else
            return "normal";
    }

    public void onClick_ReturnToMain()
    {
        m_clickSfx.Play();
        m_gameManager.resetManager();
        m_gameManager.gameObject.SetActive(false);
        m_menuCards.SetActive(true);
        initUI(FBScript.getLoggedIn());
    }

    public void onClick_PlayAgain()
    {
        ShowAd();
        m_gameManager.resetManager();
        m_gameManager.gameObject.SetActive(true);
        m_dialogEndGameMenu.SetActive(false);
        m_menuCards.SetActive(true);
        onClick_Difficulty(m_difficulty);
    }

    public void onClick_Multiplayer()
    {
        m_clickSfx.Play();
        m_dialogLoggedIn.SetActive(false);
        m_dialogLoggedOut.SetActive(false);
        m_dialogInGame.SetActive(false);
        m_dialogSinglePlayer.SetActive(false);
        m_dialogModeMenu.SetActive(false);
        m_dialogEndGameMenu.SetActive(false);
        m_menuCards.SetActive(true);
    }

    public void onClick_Share()
    {
        m_dialogLoggedIn.SetActive(false);
        m_dialogLoggedOut.SetActive(false);
        m_dialogInGame.SetActive(false);
        m_dialogSinglePlayer.SetActive(false);
        m_dialogModeMenu.SetActive(false);
        m_dialogEndGameMenu.SetActive(false);
        m_menuCards.SetActive(true);
        m_fbScript.share(m_shareMsg);
    }

    public void updatePlayerScore(int playerScore )
    {
        m_playerScoreLbl.text = "Score: " + playerScore.ToString();
    }

    public void updateOpponentScore(int CPUScore)
    {
        m_CPUScoreLbl.text = "Score: " + CPUScore.ToString();
    }

    public void updateTurn(bool playerTurn)
    {
        if (playerTurn)
        {
            if(FB.IsLoggedIn)
            {
                m_playerLbl.gameObject.SetActive(false);
                m_playerLbl.material = textMaterial;
                m_playerProfilePic.material = defaultMaterial;
            }
            else
            {
                m_playerLbl.gameObject.SetActive(true);
                m_playerLbl.material = textMaterial;
                m_playerProfilePic.material = playerMaterial;
            }

            m_cpuLbl.material = textMaterial_Transparent;
            m_cpuProfilePic.material = cpuMaterial_Transparent;
        }
        else
        {
            if (FB.IsLoggedIn)
            {
                m_playerLbl.gameObject.SetActive(false);
                m_playerLbl.material = textMaterial_Transparent;
                m_playerProfilePic.material = defaultMaterial_Transparent;
            }
            else
            {
                m_playerLbl.gameObject.SetActive(true);
                m_playerLbl.material = textMaterial_Transparent;
                m_playerProfilePic.material = playerMaterial_Transparent;
            }

            m_cpuLbl.material = textMaterial;
            m_cpuProfilePic.material = cpuMaterial;
        }
    }

    public void ShowAd()
    {
        if (m_adChecker != ++m_counterToWatchAds)
            return;

        if (Advertisement.IsReady() 
            && Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
        {
            Advertisement.Show();
        }
        else if(Advertisement.IsReady("pictureZone")
            && Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
        {
            Advertisement.Show("pictureZone");
        }
        else
        {
            Debug.Log("Ads not playing");
        }

        m_counterToWatchAds = 0;
    }

    public bool isAdShowing()
    {
        return Advertisement.isShowing;
    }

    void RequestBanner()
    {
#if UNITY_EDITOR
        string adUnitId = "unused";
#elif UNITY_ANDROID
        string adUnitId = "ca-app-pub-3907298537302631/9580302905";
#elif UNITY_IPHONE
        string adUnitId = "INSERT_IOS_BANNER_AD_UNIT_ID_HERE";
#else
        string adUnitId = "unexpected_platform";
#endif

        // Create a 320x50 banner at the top of the screen.
        BannerView bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);
        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().TagForChildDirectedTreatment(true).Build();
        // Load the banner with the request.
        bannerView.LoadAd(request);
    }

    void setFBProfilePic(Image img)
    {
        if (img != null)
        {
            m_playerProfilePic.sprite = img.sprite;
            m_playerProfilePic.material = defaultMaterial;
        }
    }
}
