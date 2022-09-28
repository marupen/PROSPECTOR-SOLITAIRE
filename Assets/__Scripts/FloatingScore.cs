using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// ������������ �� ����� ���������� ����������� FloatingScore
public enum eFSState
{
    idle,
    pre,
    active,
    post
}

// FloatingScore ����� ������������ �� ������ �� ����������, ������� ������������ ������ �����
public class FloatingScore : MonoBehaviour
{
    [Header("Set Dynamically")]
    public eFSState state = eFSState.idle;

    [SerializeField]
    protected int _score = 0;
    public string scoreString;

    // �������� score ������������� ��� ����, _score � scorestring
    public int score
    {
        get
        {
            return (_score /* * (ScoreManager.MULTIPLIER + 1)*/);
        }
        set
        {
            _score = value;
            scoreString = score.ToString("N0"); // �������� N0 ������� �������� ����� � �����
            GetComponent<Text>().text = scoreString;
        }
    }

    public List<Vector2> bezierPts; // �����, ������������ ������ �����
    public List<float> fontSizes; // ����� ������ ����� ��� ��������������� ������
    public float timeStart = -1f;
    public float timeDuration = 1f;
    public string easingCurve = Easing.InOut; // ������� �����������

    // ������� ������, ��� �������� ����� ������ ����� SendMessage, ����� ���� ��������� FloatingScore �������� ��������
    public GameObject reportFinishTo = null;
    
    private RectTransform rectTrans;
    private Text txt;

    // ��������� FloatingScore � ��������� ��������
    public void Init(List<Vector2> ePts, float eTimeS = 0, float eTimeD = 1)
    {
        rectTrans = GetComponent<RectTransform>();
        rectTrans.anchoredPosition = Vector2.zero;
        txt = GetComponent<Text>();
        bezierPts = new List<Vector2>(ePts);
        if (ePts.Count == 1) { // ���� ������ ������ ���� ����� ������ ������������� � ���.
            transform.position = ePts[0];
            return;
        }
        // ���� eTimeS ����� �������� �� ���������, ��������� ������ �� �������� �������
        if (eTimeS == 0) eTimeS = Time.time;
        timeStart = eTimeS;
        timeDuration = eTimeD;
        state = eFSState.pre; // ���������� ��������� pre - ���������� ������ ��������
    }

    public void FSCallback(FloatingScore fs)
    {
        // ����� SendMessage ������� ��� �������, ��� ������ �������� ���� �� ���������� ���������� FloatingScore
        score = ScoreManager.SCORE_RUN;
    }

    // Update ���������� � ������ �����
    void Update()
    {
        // ���� ���� ������ ������ �� ������������, ������ �����
        if (state == eFSState.idle) return;
        if (ScoreManager.lastRemovedCard.isGold == true)
            GetComponent<Text>().color = new Color(255, 215, 0);
        // ��������� u �� ������ �������� ������� � ����������������� �������� � ���������� �� 0 �� 1 (������)
        float u = (Time.time - timeStart) / timeDuration;
        // ������������ ����� Easing �� Utils ��� ������������� �������� u
        float uC = Easing.Ease(u, easingCurve);
        if (u < 0)
        { // ���� �<0, ������ �� ������ ���������.
            state = eFSState.pre;
            txt.enabled = false; // ���������� ������ �����
        }
        else
        {
            if (u >= 1)
            { // ���� u>=1, ����������� ��������
                uC = 1; // ���������� uC = 1, ����� �� ����� �� ������� �����
                state = eFSState.post;
                if (reportFinishTo != null)
                {
                    // ���� ������� ������ ������ ������������ SendMessage ��� ������ ������ FSCallback � �������� ��� �������� ���������� � ���������.
                    reportFinishTo.SendMessage("FSCallback", this);
                    // ����� �������� ��������� ���������� gameObject
                    Destroy(gameObject);
                }
                else
                {
                    // ���� �� ������ �� ���������� ������� ���������. ������ �������� ��� � �����.
                    state = eFSState.idle;
                }
            }
            else
            {
                // ���� 0<=�<1, ������, ������� ��������� ������� � ��������
                state = eFSState.active;
                txt.enabled = true; // �������� ����� �����
            }
            // ������������ ������ ����� ��� ����������� � �������� �����
            Vector2 pos = Utils.Bezier(uC, bezierPts);
            // ������� ����� RectTransform ����� ������������ ��� ���������������� �������� ����������������� ���������� ������������ ������ ������� ������
            rectTrans.anchorMin = rectTrans.anchorMax = pos;
            if (fontSizes != null && fontSizes.Count > 0)
            {
                // ���� ������ fontsizes �������� �������� ��������������� fontSize ����� ������� GUIText
            int size = Mathf.RoundToInt(Utils.Bezier(uC, fontSizes));
                GetComponent<Text>().fontSize = size;
            }
        }
    }
}
