using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ������������ co ����� ���������� ��������� ���������� �����
public enum eScoreEvent
{
    draw,
    mine,
    mineGold,
    gameWin,
    gameLoss
}

// ScoreManager ��������� ��������� �����
public class ScoreManager : MonoBehaviour
{
    static private ScoreManager S;

    static public int SCORE_FROM_PREV_ROUND = 0;
    static public int HIGH_SCORE = 0;
    static public CardProspector lastRemovedCard;

    [Header("Set Dynamically")]
    // ���� ��� �������� ���������� � ������������ �����
    public int chain = 0;
    public int scoreRun = 0;
    public int score = 0;
    public int multiplier = 0;

    private void Awake()
    {
        if(S == null)
        {
            S = this; // ���������� �������-��������
        }
        else
        {
            Debug.LogError("ERROR: SceneManager.Awake(): S is already set!");
        }
        // ��������� ������ � PlayerPrefs
        if (PlayerPrefs.HasKey("ProspectorHighScore"))
        {
            HIGH_SCORE = PlayerPrefs.GetInt("ProspectorHighScore");
        }
        // �������� ����, ������������ � ��������� ������ ������� ������ ���� >0, ���� ����� ���������� �������
        score += SCORE_FROM_PREV_ROUND;
        // � �������� SCORE_FROM_PREV_ROUND
        SCORE_FROM_PREV_ROUND = 0;
    }

    static public void EVENT(eScoreEvent evt)
    {
        try
        {
            // try-catch �� �������� ������ �������� ��������� ���������
            S.Event(evt);
        }
        catch (System.NullReferenceException nre)
        {
            Debug.LogError("ScoreManager: EVENT() called while S = null.\n" + nre);
        }
    }

    void Event(eScoreEvent evt)
    {
        switch (evt)
        {
            // � ������ ������, ��������� � ���������� ���� ����������� ���� � �� �� ��������
            case eScoreEvent.draw: // ����� ��������� �����
            case eScoreEvent.gameWin: // ������ � ������
            case eScoreEvent.gameLoss: // �������� � ������
                chain = 0; // �������� ������� �������� �����
                score += scoreRun; // �������� scoreRun � ������ ����� �����
                scoreRun = 0; // �������� scoreRun
                multiplier = 0; // �������� ��������� ������� ����
                break;

            case eScoreEvent.mine: // �������� ����� �� �������� ���������
                if (lastRemovedCard.isGold)
                {
                    multiplier++;
                    scoreRun = scoreRun * (1 + 1 / multiplier);
                }
                chain++; // ���������� ���������� ����� � �������
                scoreRun += chain * (multiplier + 1); // �������� ���� �� �����
                break;
        }

        // ��� ������ ���������� switch ������������ ������ � �������� � ������
        switch(evt)
        {
            case eScoreEvent.gameWin:
                // � ������ ������ ��������� ���� � ��������� ����� ����������� ���� �� ������������ ������� SceneManager.LoadScene()
                SCORE_FROM_PREV_ROUND = score;
                print("You won this round! Round score: " + score);
                break;

            case eScoreEvent.gameLoss:
                // � ������ ��������� �������� � ��������
                if(HIGH_SCORE <= score)
                {
                    print("You got the high score! High score: " + score);
                    HIGH_SCORE = score;
                    PlayerPrefs.SetInt("ProspectorHighScore", score);
                }
                else
                {
                    print("Your final score for the game was: " + score);
                }
                break;
            default:
                print("score: " + score + " scoreRun:" + scoreRun + " chain:" + chain);
                break;
        }
    }
    static public int CHAIN { get { return S.chain; } }
    static public int SCORE { get { return S.score; } }
    static public int MULTIPLIER { get { return S.multiplier; } }
    static public int SCORE_RUN { get { return S.scoreRun; } }
}
