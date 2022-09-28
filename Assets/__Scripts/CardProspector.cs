using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ������������, ������������ ��� ����������, ������� ����� ��������� ��������� ���������������� ��������
public enum eCardState
{
    drawpile,
    tableau,
    target,
    discard
}

public class CardProspector : Card
{
    [Header("Set Dynamically: CardProspector")]
    // ��� ������������ ������������ eCardState
    public eCardState state = eCardState.drawpile;
    // hiddenBy - ������ ������ ����, �� ����������� ����������� ��� ����� �����
    public List<CardProspector> hiddenBy = new List<CardProspector>();
    // layoutID ���������� ��� ���� ����� ��� � ���������
    public int layoutID;
    // ����� SlotDef ������ ���������� �� �������� <slot> � LayoutXML
    public SlotDef slotDef;
    // ���������� ������� ���� �� ������ ����
    override public void OnMouseUpAsButton()
    {
        // ������� ����� CardClicked �������-�������� Prospector
        Prospector.S.CardClicked(this);
        // � ����� ������ ����� ������ � ������� ������ (Card.cs)
        base.OnMouseUpAsButton();
    }
}
