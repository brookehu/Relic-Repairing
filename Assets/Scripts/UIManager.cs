using System.Runtime.ExceptionServices;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class UIManager : MonoBehaviour
{

    public static UIManager instance;

    //启动时创建单例，并启动游戏1
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
        HideAllUI();
        RunGame1();
    }

    [Header("游戏1")]
    public GameObject[] Items_1;
    public GameObject[] Code_1;
    public GameObject[] Buttom_1;
    public GameObject[] Background_1;

    [Header("游戏2")]
    public GameObject[] Items_2;
    public GameObject[] ProgressBar_2;
    public GameObject[] Background_2;
    public GameObject[] Buttom_2;

    static int progress = 0;    //游戏二的进度条
    static int codeInput = 0;   //游戏一的密码输入

    //游戏2进度条增加
    public void ProgressUp()
    {
        progress++;
        if (progress == 5)
        {
            //event
            HideAllUI();
            return;
        }
        ProgressBar_2[progress - 1].SetActive(false);
        ProgressBar_2[progress].SetActive(true);
    }

    //游戏1输入密码
    public void InputCode(int num)
    {
        Code_1[num].SetActive(true);
        Buttom_1[num].SetActive(false);
        codeInput++;
        if(codeInput == 3)
        {
            Pause(1);
            //event
            HideAllUI();
            RunGame2();
        }
    }

    //隐藏所有的游戏元素
    public void HideAllUI()
    {
        for (int i = 0; i < Code_1.Length; i++)
        {
            Code_1[i].SetActive(false);
        }
        for (int i = 0; i < Items_1.Length; i++)
        {
            Items_1[i].SetActive(false);
        }
        for (int i = 0; i < Background_1.Length; i++)
        {
            Background_1[i].SetActive(false);
        }
        for (int i = 0; i < Buttom_1.Length; i++)
        {
            Buttom_1[i].SetActive(false);
        }


        for (int i = 0; i < ProgressBar_2.Length; i++)
        {
            ProgressBar_2[i].SetActive(false);
        }
        for (int i = 0; i < Items_2.Length; i++)
        {
            Items_2[i].SetActive(false);
        }
        for (int i = 0; i < Background_2.Length; i++)
        {
            Background_2[i].SetActive(false);
        }
        for (int i = 0; i < Buttom_2.Length; i++)
        {
            Buttom_2[i].SetActive(false);
        }
    }

    //游戏1的启动方法
    public void RunGame1()
    {
        HideAllUI();
        for (int i = 0; i < Items_1.Length; i++)
        {
            Items_1[i].SetActive(true);
        }
        for (int i = 0; i < Background_1.Length; i++)
        {
            Background_1[i].SetActive(true);
        }
        for (int i = 0; i < Buttom_1.Length; i++)
        {
            Buttom_1[i].SetActive(true);
        }
    }

    //游戏2的启动方法
    public void RunGame2()
    {  
        HideAllUI();
        ProgressBar_2[0].SetActive(true);
        for (int i = 0; i < Items_2.Length; i++)
        {
            Items_2[i].SetActive(true);
        }
        for (int i = 0; i < Background_2.Length; i++)
        {
            Background_2[i].SetActive(true);
        }
        for (int i = 0; i < Buttom_2.Length; i++)
        {
            Buttom_2[i].SetActive(true);
        }
    }

    //暂停若干秒
    IEnumerator Pause(int n)
    {
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0;
        yield return new WaitForSeconds(n);  
        Time.timeScale = originalTimeScale;
    }

}

