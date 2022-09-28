using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// ����� Scoreboard ��������� ������������ ����� ������
public class Scoreboard : MonoBehaviour
{
    public static Scoreboard S; // ��� ������-�������� Scoreboard

    [Header("Set in Inspector")]
    public GameObject prefabFloatingScore;

    [Header("Set Dynamically")]
    [SerializeField] private int _score = 0;
    [SerializeField] private string _scoreString;

    private Transform canvasTrans;

    // �������� score ����� ������������� scorestring
    public int score
    {
        get
        {
            return (_score);
        }
        set
        {
            _score = value;
            scoreString = _score.ToString("N0");
        }
    }

    // �������� scorestring ����� ������������� Text.text
    public string scoreString
    {
        get
        {
            return (_scoreString);
        }
        set
        {
            _scoreString = value;
            GetComponent<Text>().text = _scoreString;
        }
    }
    void Awake()
    {
        if (S == null)
        {
            S = this; // ���������� �������� �������-��������
        }
        else
        {
            Debug.LogError("ERROR: Scoreboard.Awake(): S is already set!");
        }
        canvasTrans = transform.parent;
    }

    // ����� ���������� ������� SendMessage, ���������� fs.score � this.score
    public void FSCallback(FloatingScore fs)
    {
        score += fs.score;
    }

    // ������� ����� ������� ������ FloatingScore.
    // ���������� ��������� �� ��������� ��������� FloatingScore, ����� ���������� ������� ����� ��������� � ��� �������������� ��������
    public FloatingScore CreateFloatingScore(int amt, List<Vector2> pts)
    {
        GameObject go = Instantiate<GameObject>(prefabFloatingScore);
        go.transform.SetParent(canvasTrans);
        FloatingScore fs = go.GetComponent<FloatingScore>();
        fs.score = amt;
        fs.reportFinishTo = this.gameObject; // ��������� �������� �����
        fs.Init(pts);
        return (fs);
    }
}
