using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ����� SlotDef �� ��������� MonoBehaviour, ������� ��� ���� �� ��������� ��������� ��������� ���� �� �#
[System.Serializable] // ������� ���������� SlotDef �������� � ���������� Unity
public class SlotDef
{
    public float x;
    public float y;
    public bool faceUp = false;
    public bool isGold = false;
    public string layerName = "Default";
    public int layerID = 0;
    public int id;
    public List<int> hiddenBy = new List<int>();
    public string type = "slot";
    public Vector2 stagger;
}

public class Layout : MonoBehaviour
{
    public PT_XMLReader xmlr; // ��� ��, ��� Deck, ����� PT_XMLReader
    public PT_XMLHashtable xml; // ������������ ��� ��������� ������� � xml
    public Vector2 multiplier; // �������� �� ������ ���������
    // ������ SlotDef
    public List<SlotDef> slotDefs; // ��� ���������� SlotDef ��� ����� 0-3
    public SlotDef drawPile;
    public SlotDef discardPile;
    // ������ ����� ���� �����
    public string[] sortingLayerNames = new string[] { "Row0", "Row1", "Row2", "Row3", "Discard", "Draw" };

    // ��� ������� ���������� ��� ������ ����� LayoutXML.xml
    public void ReadLayout(string xmlText)
    {
        xmlr = new PT_XMLReader();
        xmlr.Parse(xmlText); // ��������� XML
        xml = xmlr.xml["xml"][0]; // � ������������ xml ��� ��������� ������� � XML
        // ��������� ���������, ������������ ���������� ����� �������
        multiplier.x = float.Parse(xml["multiplier"][0].att("x"));
        multiplier.y = float.Parse(xml["multiplier"][0].att("y"));
        // ��������� �����
        SlotDef tSD;
        // slotsX ������������ ��� ��������� ������� � ��������� <slot>
        PT_XMLHashList slotsX = xml["slot"];
        for (int i = 0; i < slotsX.Count; i++)
        {
            tSD = new SlotDef(); // ������� ����� ��������� SlotDef
            if (slotsX[i].HasAtt("type"))
            {
                // ���� <slot> ����� ������� type, ��������� ���
                tSD.type = slotsX[i].att("type");
            }
            else
            {
                // ����� ���������� ��� ��� "slot"; ��� ��������� ����� � ����
                tSD.type = "slot";
            }
            // ������������� ��������� �������� � �������� ��������
            tSD.x = float.Parse(slotsX[i].att("x"));
            tSD.y = float.Parse(slotsX[i].att("y"));
            tSD.layerID = int.Parse(slotsX[i].att("layer") );
            // ������������� ����� ���� layerlD � ����� layerName
            tSD.layerName = sortingLayerNames[tSD.layerID];
            switch (tSD.type)
            {
                // ��������� �������������� ��������, �������� �� ��� �����
                case "slot":
                    tSD.faceUp = (slotsX[i].att("faceup") == "1");
                    tSD.id = int.Parse(slotsX[i].att("id"));
                    if (slotsX[i].HasAtt("hiddenby"))
                    {
                        string[] hiding = slotsX[i].att("hiddenby").Split(',');
                        foreach (string s in hiding)
                        {
                            tSD.hiddenBy.Add(int.Parse(s));
                        }
                    }
                    slotDefs.Add(tSD);
                    break;
                case "drawpile":
                    tSD.stagger.x = float.Parse(slotsX[i].att("xstagger") );
                    drawPile = tSD;
                    break;
                case "discardpile":
                    discardPile = tSD;
                    break;
            }
        }
    }
}
