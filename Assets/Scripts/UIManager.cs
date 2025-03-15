using System.Runtime.ExceptionServices;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    public static UIManager instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            instance = this;
        }
    }

    public GameObject Background;
    public GameObject Desk;
    public GameObject[] Items;
    public GameObject[] ProgressBar;

    static int progress = 0;

    public void ProgressUp()
    {
        progress++;
        if (progress == 5)
        {
            //event
            hideAllUI();
        }
        ProgressBar[progress - 1].SetActive(false);
        ProgressBar[progress].SetActive(true);

    }

    public void hideAllUI()
    {
        Background.SetActive(false);
        Desk.SetActive(false);
        for (int i = 0; i < ProgressBar.Length; i++)
        {
            ProgressBar[i].SetActive(false);
        }
        for (int i = 0; i < Items.Length; i++)
        {
            Items[i].SetActive(false);
        }

    }
    public void FirstShowUI()
    {
        Background.SetActive(true);
        Desk.SetActive(true);
        for (int i = 0; i < ProgressBar.Length; i++)
        {
            ProgressBar[i].SetActive(false);
        }
        for (int i = 0; i < Items.Length; i++)
        {
            Items[i].SetActive(true);
        }
        ProgressBar[0].SetActive(true);
    }


}

