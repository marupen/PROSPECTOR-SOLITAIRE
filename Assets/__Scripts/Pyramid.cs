using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

public class Pyramid : MonoBehaviour
{
    static public Pyramid S;

    [Header("Set in Inspector")]
    public TextAsset deckXML;
    public TextAsset layoutXML;

    [Header("Set Dynamically")]
    public Deck deck;
    public LayoutPyramid layout;

    void Awake()
    {
        S = this; // ���������� �������-�������� Pyramid
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
    }

    void Start()
    {
        deck = GetComponent<Deck>(); // �������� ��������� Deck
        deck.InitDeck(deckXML.text); // �������� ��� DeckXML
        Deck.Shuffle(ref deck.cards); // ���������� ������, ������� �� �� ������
        layout = GetComponent<LayoutPyramid>(); // �������� ��������� Layout
        layout.ReadLayout(layoutXML.text); // �������� ��� ���������� LayoutXML
    }
}
