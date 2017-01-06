using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Facebook.Unity;

public class FBScript : MonoBehaviour {

    public MenuManager m_menuManager;

    public GameObject m_dialogLoggedIn;
    public GameObject m_dialogLoggedOut;
    public GameObject m_dialogUsername;
    public GameObject m_dialogProfilePic;

    void Awake()
    {
        FB.Init(setInit, onHideUnity);
    }

    void setInit()
    {
        determineFBMenus(FB.IsLoggedIn);
    }

    void onHideUnity(bool isGameShown)
    {
        if (!isGameShown)
            Time.timeScale = 0;
        else
            Time.timeScale = 1;
    }

    public void fbLogin()
    {
        List<string> permissions = new List<string>();
        permissions.Add("public_profile");

        FB.LogInWithReadPermissions(permissions, authCallback);
    }

    public void fbLogout()
    {
        FB.LogOut();
    }

    void authCallback(IResult result)
    {
        if(result.Error != null)
        {
            Debug.Log(result.Error); //if 400 error, you need to generate a new user token
        }
        else
        {
            determineFBMenus(FB.IsLoggedIn);
        }
    }

    void determineFBMenus(bool isLoggedIn)
    {
        m_menuManager.initUI(isLoggedIn);
        if (isLoggedIn)
        {
            FB.API("/me?fields=first_name", HttpMethod.GET, displayUsername);
            FB.API("/me/picture?type=square&height=128&&width=128", HttpMethod.GET, displayProfilePic);
        }
    }

    void displayUsername(IResult result)
    {
        if (result.Error != null)
        {
            Debug.Log(result.Error);
        }
        else
        {
            m_menuManager.updateUsername( "Hi there, " + result.ResultDictionary["first_name"] );
        }
    }

    void displayProfilePic(IGraphResult result)
    {
        if (result.Texture == null)
        {
            Debug.Log(result.Error);
        }
        else
        {
            m_menuManager.updateUserProfilePic(Sprite.Create(result.Texture, new Rect(0, 0, 128, 128), new Vector2()));
        }
    }

    public static bool getLoggedIn()
    {
        return FB.IsLoggedIn;
    }

    public void share(string contentDescription)
    {
        System.Uri contentURL = new System.Uri("https://www.facebook.com/"); ;
        string contentTitle = "Playing concentration card game!";
        System.Uri photoURL = null;
        FB.ShareLink(contentURL, contentTitle, contentDescription, photoURL, shareCallback);
    }

    void shareCallback(IResult result)
    {
        if(result.Cancelled)
        {
            m_menuManager.onClick_ReturnToMain();
        }
        else if(!string.IsNullOrEmpty(result.Error))
        {
            m_menuManager.onClick_ReturnToMain();
        }
        else if(!string.IsNullOrEmpty(result.RawResult))
        {
            m_menuManager.onClick_ReturnToMain();
        }
    }
}
