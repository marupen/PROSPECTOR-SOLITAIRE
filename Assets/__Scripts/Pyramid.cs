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
        S = this; // Подготовка объекта-одиночки Pyramid
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
    }

    void Start()
    {
        deck = GetComponent<Deck>(); // Получить компонент Deck
        deck.InitDeck(deckXML.text); // Передать ему DeckXML
        Deck.Shuffle(ref deck.cards); // Перемешать колоду, передав ее по ссылке
        layout = GetComponent<LayoutPyramid>(); // Получить компонент Layout
        layout.ReadLayout(layoutXML.text); // Передать ему содержимое LayoutXML
    }
}
