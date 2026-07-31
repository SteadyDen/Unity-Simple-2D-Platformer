using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    //게임매니저는 점수와 스테이지 이동을 관리
    
    public PlayerMove playerMove;
    public GameObject[] Stages;
    public Image[] UIHealth;
    public TMP_Text UIPoint;
    public TMP_Text UIStage;
    public TMP_Text OnDieTitle;
    public TMP_Text GameClearTitle;
    public Button Retry;

    public int totalPoint = 0;
    public int stagePoint = 0;
    public int stageIndex = 0;

    void Update()
    {
        UIPoint.text = (totalPoint + stagePoint).ToString();
    }


    public void NextStage()
    {   
        if (stageIndex < Stages.Length-1)
        {
            totalPoint += stagePoint;
            stagePoint = 0;
            stageIndex += 1;
            Stages[stageIndex-1].SetActive(false);
            Stages[stageIndex].SetActive(true);
            UIStage.text = "STAGE " + (stageIndex+1).ToString();
            playerMove.transform.position = new Vector3(-8, 1, -5);
        }
        else
        {
            playerMove.PlaySound("clear");
            GameClearTitle.gameObject.SetActive(true);
            Retry.gameObject.SetActive(true);
            Time.timeScale = 0;
        }
    }

    public void RetryGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("SampleScene");
    }
}
