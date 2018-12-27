
using UnityEngine;
using Unity.Mathematics;

public class Settings : MonoBehaviour
{
    public int PlayerInitialSize;
    public int PlayerMaxSize;
    public int FoodSize;
    public float PlayerMaxSpeed;
    public int ArenaSize;
    public int FoodCount;
    public int BotCount;
    public int InitialCameraSize;

    public int CellSize => PlayerMaxSize * 4;

}
