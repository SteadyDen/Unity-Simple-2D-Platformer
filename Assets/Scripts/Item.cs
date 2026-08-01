using UnityEngine;

public class Item : MonoBehaviour
{
    public int point;
    public ItemType itemType;

    public enum ItemType
    {
        Coin,
        Apple
    }
}
