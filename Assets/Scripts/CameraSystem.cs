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
        public ComponentDataArray<Size> Size;
    }

    [Inject] Data m_Data;

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

        Settings settings = Bootstrap.Settings;

        float progress = 10 * Time.deltaTime;

        _mainCamera.transform.position = Vector3.Lerp(_mainCamera.transform.position, targetPos, progress);
        float sizeRatio = m_Data.Size[0].Value / settings.PlayerInitialSize;
        float newCameraSize = math.sqrt(sizeRatio) * settings.InitialCameraSize;
        _mainCamera.orthographicSize = math.lerp(_mainCamera.orthographicSize, newCameraSize, progress);
    }
}
