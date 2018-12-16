using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms;

public class CameraSystem : ComponentSystem
{
    Camera _mainCamera;
    
    public struct Data
    {
        public readonly int Length;
        public ComponentDataArray<PlayerInput> PlayerInput;
        public ComponentDataArray<Position> Position;
    }
    
    [Inject] private Data m_Data;

    public void SetupGameObjects()
    {
        _mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
    }

    protected override void OnUpdate()
    {
        if (_mainCamera == null || m_Data.Length == 0)
        {
            return;
        }
            
        Vector3 targetPos = new Vector3
        (
            m_Data.Position[0].Value.x, 
            m_Data.Position[0].Value.y,
            _mainCamera.transform.position.z
        );

        _mainCamera.transform.position = Vector3.Lerp(_mainCamera.transform.position, targetPos, 0.1f);
    }
}
