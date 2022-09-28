using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Globalization;

public class Prospector : MonoBehaviour
{
    static public Prospector S;
    [Header("Set in Inspector")]
    public TextAsset deckXML;
    public TextAsset layoutXML;
    public float xOffset = 3;
    public float yOffset = -2.5f;
    public float chanceToMakeGoldCard = 0.1f;
    public Vector3 layoutCenter;
    public Vector2 fsPosMid = new Vector2(0.5f, 0.90f);
    public Vector2 fsPosRun = new Vector2(0.5f, 0.75f);
    public Vector2 fsPosMid2 = new Vector2(0.4f, 1.0f);
    public Vector2 fsPosEnd = new Vector2(0.5f, 0.95f);
    public float reloadDelay = 2f; // �������� ����� �������� 2 �������
    public Text gameOverText, roundResultText, highScoreText;

    [Header("Set Dynamically")]
    public Deck deck;
    public Layout layout;
    public List<CardProspector> drawPile;
    public Transform layoutAnchor;
    public CardProspector target;
    public List<CardProspector> tableau;
    public List<CardProspector> discardPile;
    public FloatingScore fsRun;

    void Awake()
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        S = this; // ���������� �������-�������� Prospector
        SetUpUITexts();
    }

    void Start()
    {
        Scoreboard.S.score = ScoreManager.SCORE;

        deck = GetComponent<Deck>(); // �������� ��������� Deck
        deck.InitDeck(deckXML.text); // �������� ��� DeckXML
        Deck.Shuffle(ref deck.cards); // ���������� ������, ������� �� �� ������
        /*       Card c;
               for (int cNum = 0; cNum < deck.cards.Count; cNum++)
               {
                   c = deck.cards[cNum];
                   c.transform.localPosition = new Vector3((cNum % 13) * 3, cNum / 13 * 4, 0);
               }*/
        layout = GetComponent<Layout>(); // �������� ��������� Layout
        layout.ReadLayout(layoutXML.text); // �������� ��� ���������� LayoutXML
        drawPile = ConvertListCardsToListCardProspectors(deck.cards);
        LayoutGame();
    }

    List<CardProspector> ConvertListCardsToListCardProspectors(List<Card> lCD)
    {
        List<CardProspector> lCP = new List<CardProspector>();
        CardProspector tCP;
        foreach (Card tCD in lCD)
        {
            tCP = tCD as CardProspector;
            lCP.Add(tCP);
        }
        return (lCP);
    }

    // ������� Draw ������� ���� ����� � ������� drawPile � ���������� ��
    CardProspector Draw()
    {
        CardProspector cd = drawPile[0]; // ����� 0-� ����� CardProspector
        drawPile.RemoveAt(0); // ������� �� Listo drawPile
        return (cd); // � ������� ��
    }

    // LayoutGame() ��������� ����� � ��������� ��������� - "�����"
    void LayoutGame()
    {
        // ������� ������ ������� ������, ������� ����� ������� ������� ���������
        if (layoutAnchor == null)
        {
            GameObject tGO = new GameObject("-LayoutAnchor");
            // ������� ������ ������� ������ � ������ _LayoutAnchor � ��������
            layoutAnchor = tGO.transform; // �������� ��� ��������� Transform
            layoutAnchor.transform.position = layoutCenter; // ��������� � �����
        }

        CardProspector cp;
        // ��������� �����
        foreach (SlotDef tSD in layout.slotDefs)
        {
            // ��������� ����� ���� ����������� SlotDef � layout.slotDefs
            cp = Draw(); // ������� ������ ����� (������) �� ������ drawPile
            cp.faceUp = tSD.faceUp; // ���������� �� ������� faceup � ������������ � ������������ � SlotDef
            cp.transform.parent = layoutAnchor; // ��������� layoutAnchor �� ���������
            // ��� �������� ������� ����������� ��������: deck.deckAnchor, ������� ����� ������� ���� ������������ � �������� � ������ _Deck.
            cp.transform.localPosition = new Vector3(
                layout.multiplier.x * tSD.x,
                layout.multiplier.y * tSD.y,
                -tSD.layerID);
            // ���������� localPosition ����� � ������������ � ������������ � SlotDef
            cp.layoutID = tSD.id;
            cp.slotDef = tSD;
            // ����� CardProspector � �������� ��������� ����� ��������� Cardstate.tableau
            cp.state = eCardState.tableau;
            cp.SetSortingLayerName(tSD.layerName); // ��������� ���� ����������
            if (Random.value < chanceToMakeGoldCard)
                ToGold(cp);
            tableau.Add(cp); // �������� ����� � ������ tableau
        }
        // ��������� ������ ����, �������� ����������� ������
        foreach (CardProspector tCP in tableau)
        {
            foreach (int hid in tCP.slotDef.hiddenBy)
            {
                cp = FindCardByLayoutID(hid);
                tCP.hiddenBy.Add(cp);
            }
        }
        // ������� ��������� ������� �����
        MoveToTarget(Draw());
        // ��������� ������ ��������� ����
        UpdateDrawPile();
    }

    // ����������� layoutID CardProspector � ���� �������
    CardProspector FindCardByLayoutID(int layoutID)
    {
        foreach (CardProspector tCP in tableau)
        {
            // ����� �� ���� ������ � ������ tableau
            if (tCP.layoutID == layoutID)
            {
                // ���� ����� ����� ����� ��������� � �������, ������� ��
                return (tCP);
            }
        }
        // ���� ������ �� �������, ������� null
        return (null);
    }

    // ������������ ����� � �������� ��������� ������� �������� ����� ��� ����
    void SetTableauFaces()
    {
        foreach (CardProspector cd in tableau)
        {
            bool faceUp = true; // ������������, ��� ����� ������ ���� ��������� ������� �������� �����
            foreach (CardProspector cover in cd.hiddenBy)
            {
                // ���� ����� �� ����, ������������� �������, ������������ � �������� ���������
                if (cover.state == eCardState.tableau)
                {
                    faceUp = false; // ��������� ������� �������� ����
                }
            }
            cd.faceUp = faceUp; // ��������� ����� ��� ��� �����
        }
    }

    void ToGold(CardProspector cp)
    {
        SpriteRenderer tSR = cp.GetComponent<SpriteRenderer>();
        tSR.sprite = GetComponent<Deck>().cardFrontGold;
        tSR = cp.back.GetComponent<SpriteRenderer>();
        tSR.sprite = GetComponent<Deck>().cardBackGold;
        cp.isGold = true;
    }

    // ���������� ������� ������� ����� � ������ ���������� ����
    void MoveToDiscard(CardProspector cd)
    {
        // ���������� ��������� ����� ��� discard (��������)
        cd.state = eCardState.discard;
        discardPile.Add(cd); // �������� �� � ������ discardPile
        cd.transform.parent = layoutAnchor; // �������� �������� transform.parent
        // ����������� ��� ����� � ������� ������ ���������� ����
        cd.transform.localPosition = new Vector3(
            layout.multiplier.x * layout.discardPile.x,
            layout.multiplier.y * layout.discardPile.y,
            -layout.discardPile.layerID + 0.5f);
        cd.faceUp = true;
        // ��������� ������ ������ ��� ���������� �� �������
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(-100 + discardPile.Count);
    }

    // ������ ����� cd ����� ������� ������
    void MoveToTarget(CardProspector cd)
    {
        // ���� ������� ����� ����������, ����������� �� � ������ ���������� ����
        if (target != null) MoveToDiscard(target);
        target = cd; // cd - ����� ������� �����
        cd.state = eCardState.target;
        cd.transform.parent = layoutAnchor;
        // ����������� �� ����� ��� ������� �����
        cd.transform.localPosition = new Vector3(
            layout.multiplier.x * layout.discardPile.x,
            layout.multiplier.y * layout.discardPile.y,
            -layout.discardPile.layerID);
        cd.faceUp = true; // ��������� ������� �������� �����
        // ��������� ���������� �� �������
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(0);
    }

    // ������������ ������ ��������� ����, ����� ���� �����, ������� ���� ��������
    void UpdateDrawPile()
    {
        CardProspector cd;
        // ��������� ����� ���� ���� � drawPile
        for (int i = 0; i < drawPile.Count; i++)
        {
            cd = drawPile[i];
            cd.transform.parent = layoutAnchor;
            // ����������� � ������ �������� layout.drawPile.stagger
            Vector2 dpStagger = layout.drawPile.stagger;
            cd.transform.localPosition = new Vector3(
                layout.multiplier.x * (layout.drawPile.x + i * dpStagger.x),
                layout.multiplier.y * (layout.drawPile.y + i * dpStagger.y),
                -layout.drawPile.layerID + 0.1f * i);
            cd.faceUp = false; // ��������� ������� �������� ����
            cd.state = eCardState.drawpile;
            // ��������� ���������� �� �������
            cd.SetSortingLayerName(layout.drawPile.layerName);
            cd.SetSortOrder(-10 * i);
        }
    }

    // CardClicked ���������� � ����� �� ������ �� ����� �����
    public void CardClicked(CardProspector cd)
    {
        // ������� ������������ ���������� �����
        switch (cd.state)
        {
            case eCardState.target:
                // ������ �� ������� ����� ������������
                break;
            case eCardState.drawpile:
                // ������ �� ����� ����� � ������ ��������� ���� �������� � ����� ������� �����
                MoveToDiscard(target); // ����������� ������� ����� � discardpile
                MoveToTarget(Draw()); // ����������� ������� ��������� ����� �� ����� �������
                UpdateDrawPile(); // �������� ��������� ������ ��������� ����
                ScoreManager.EVENT(eScoreEvent.draw);
                FloatingScoreHandler(eScoreEvent.draw);
                break;
            case eCardState.tableau:
                // ��� ����� � �������� ��������� ����������� ����������� �� ����������� �� ����� �������
                bool validMatch = true;
                if (!cd.faceUp)
                {
                    // �����, ���������� ������� �������� ����, �� ����� ������������
                    validMatch = false;
                }
                if (!AdjacentRank(cd, target))
                {
                    // ���� ������� ����������� �� �����������, ����� �� ����� ������������
                    validMatch = false;
                }
                if (!validMatch) return; // �����, ���� ����� �� ����� ������������
                // ����� ����� �����������
                tableau.Remove(cd); // ������� �� ������ tableau
                MoveToTarget(cd); // ������� ��� ����� �������
                ScoreManager.lastRemovedCard = cd;
                SetTableauFaces(); // ��������� ����� � �������� ��������� ������� �������� ���� ��� �����
                ScoreManager.EVENT(eScoreEvent.mine);
                FloatingScoreHandler(eScoreEvent.mine);
                break;
        }
        // ��������� ���������� ����
        CheckForGameOver();
    }

    // ��������� ���������� ����
    void CheckForGameOver()
    {
        // ���� �������� ��������� ��������, ���� ���������
        if (tableau.Count == 0)
        {
            // ������� GameOver() � ��������� ������
            GameOver(true);
            return;
        }
        // ���� ��� ���� ��������� �����, ���� �� �����������
        if (drawPile.Count > 0)
        {
            return;
        }
        // ��������� ������� ���������� �����
        foreach (CardProspector cd in tableau)
        {
            if (AdjacentRank(cd, target))
            {
                // ���� ���� ���������� ���, ���� �� �����������
                return;
            }
        }
        // ��� ��� ���������� ����� ���, ���� �����������
        // ������� GameOver � ��������� ���������
        GameOver(false);
    }

    // ����������, ����� ���� �����������
    void GameOver(bool won)
    {
        int score = ScoreManager.SCORE;
        if (fsRun != null) score += fsRun.score;
        if (won)
        {
            gameOverText.text = "Round Over";
            roundResultText.text = "You won this round!\nRound Score: " + score;
            ShowResultsUI(true);
            // print("Game over. You won! :)");
            ScoreManager.EVENT(eScoreEvent.gameWin);
            FloatingScoreHandler(eScoreEvent.gameWin);
        }
        else
        {
            gameOverText.text = "Game Over";
            if (ScoreManager.HIGH_SCORE <= score)
            {
                string str = "You got the high score!\nHigh score: " + score;
                roundResultText.text = str;
            }
            else
            {
                roundResultText.text = "Your final score was: " + score;
            }
            ShowResultsUI(true);
            // print("Game Over. You Lost. :(");
            ScoreManager.EVENT(eScoreEvent.gameLoss);
            FloatingScoreHandler(eScoreEvent.gameLoss);
        }
        // ������������� ����� � �������� ���� � �������� ���������
        // SceneManager.LoadScene("__Prospector_Scene_0");
        // ������������� ����� ����� reloadDelay ������
        // ��� �������� ����� � ������ �������� �� ����� ����������
        Invoke("ReloadLevel", reloadDelay);
    }

    void ReloadLevel()
    {
        // ������������� ����� � �������� ���� � �������� ���������
        SceneManager.LoadScene("__Prospector_Scene_0");
}

    // ���������� true, ���� ��� ����� ������������� ������� ����������� (� ������ ������������ �������� ����������� ����� ����� � �������)
    public bool AdjacentRank(CardProspector �0, CardProspector cl)
    {
        // ���� ����� �� ���� ��������� ������� �������� ����, ������� ����������� �� �����������.
        if (!�0.faceUp || !cl.faceUp) return (false);
        // ���� ������������ ���� ���������� �� 1, ������� ����������� �����������
        if (Mathf.Abs(�0.rank - cl.rank) == 1)
        {
            return (true);
        }
        // ���� ���� ����� - ���, � ������ - ������, ������� ����������� �����������
        if (�0.rank == 1 && cl.rank == 13) return (true);
        if (�0.rank == 13 && cl.rank == 1) return (true);
        // ����� ������� false
        return (false);
    }

    // ������������ �������� FloatingScore
    void FloatingScoreHandler(eScoreEvent evt)
    {
        List<Vector2> fsPts;
        switch (evt)
        {
            // � ������ ������, ��������� � ���������� ���� ����������� ���� � �� �� ��������
            case eScoreEvent.draw: // ����� ��������� �����
            case eScoreEvent.gameWin: // ������ � ������
            case eScoreEvent.gameLoss: // �������� � ������
                // �������� fsRun � Scoreboard
                if (fsRun != null)
                {
                    // ������� ����� ��� ������ �����
                    fsPts = new List<Vector2>();
                    fsPts.Add(fsPosRun);
                    fsPts.Add(fsPosMid2);
                    fsPts.Add(fsPosEnd);
                    fsRun.reportFinishTo = Scoreboard.S.gameObject;
                    fsRun.Init(fsPts, 0, 1);
                    // ����� ��������������� fontSize
                    fsRun.fontSizes = new List<float>(new float[] { 28, 36, 4 });
                    fsRun = null; // �������� fsRun, ����� ������� ������
                }
                break;
            case eScoreEvent.mine: // �������� ����� �� �������� ���������
                // ������� FloatingScore ��� ����������� ����� ���������� �����
                FloatingScore fs;
                // ����������� �� ������� ��������� ���� mousePosition � fsPosRun
                Vector2 p0 = Input.mousePosition;
                p0.x /= Screen.width;
                p0.y /= Screen.height;
                fsPts = new List<Vector2>();
                fsPts.Add(p0);
                fsPts.Add(fsPosMid);
                fsPts.Add(fsPosRun);
                fs = Scoreboard.S.CreateFloatingScore(ScoreManager.CHAIN * (ScoreManager.MULTIPLIER + 1), fsPts);
                fs.fontSizes = new List<float>(new float[] { 4, 50, 28 });
                if (fsRun == null)
                {
                    fsRun = fs;
                    fsRun.reportFinishTo = null;
                }
                else
                {
                    fs.reportFinishTo = fsRun.gameObject;
                }
                break;
        }
    }

    void SetUpUITexts()
    {
        // ��������� ������ HighScore
        GameObject go = GameObject.Find("HighScore");
        if (go != null)
        {
            highScoreText = go.GetComponent<Text>();
        }
        int highScore = ScoreManager.HIGH_SCORE;
        string hScore = "High Score: " + Utils.AddCommasToNumber(highScore);
        go.GetComponent<Text>().text = hScore;
        // ��������� �������, ������������ � ����� ������
        go = GameObject.Find("GameOver");
        if (go != null)
        {
            gameOverText = go.GetComponent<Text>();
        }
        go = GameObject.Find("RoundResult");
        if (go != null)
        {
            roundResultText = go.GetComponent<Text>();
        }
        // ������ �������
        ShowResultsUI(false);
    }
    void ShowResultsUI(bool show)
    {
        gameOverText.gameObject.SetActive(show);
        roundResultText.gameObject.SetActive(show);
    }
}
