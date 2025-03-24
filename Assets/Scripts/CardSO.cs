using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Card", menuName = "Card", order = 1)]
public class CardSO : ScriptableObject
{
    public int Value;
    public int Color;
    public Sprite sprite;
}
