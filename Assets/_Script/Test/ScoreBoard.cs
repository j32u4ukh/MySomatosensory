using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBoard : MonoBehaviour
{
    public Text correct_text;
    public Text wrong_text;

    public void setCorrect(int correct)
    {
        correct_text.text = string.Format("{0}", correct);
    }

    public void setWrong(int wrong)
    {
        wrong_text.text = string.Format("{0}", wrong);
    }

    public void display(float display_time)
    {
        gameObject.SetActive(true);
        StartCoroutine(display_score_board(display_time));
    }

    IEnumerator display_score_board(float display_time)
    {

        yield return new WaitForSeconds(display_time);
        gameObject.SetActive(false);
    }
}
