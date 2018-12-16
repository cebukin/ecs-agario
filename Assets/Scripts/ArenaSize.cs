using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaSize : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Settings settings = Bootstrap.Settings;
        transform.localScale = new Vector3(settings.ArenaSize, 1, settings.ArenaSize);
    }
    
}
