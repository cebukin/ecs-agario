
using UnityEngine;
using Unity.Mathematics;

public class Settings : MonoBehaviour
{
    public float PlayerInitialSize;
    public float PlayerMaxSize;
    public float FoodSize;
    public float PlayerMaxSpeed;
    public int ArenaSize;
    public int FoodCount;
    public int BotCount;
    public int InitialCameraSize;

    public float CellSize => PlayerMaxSize * 4;

}
