using UnityEngine;
using Unity.Cinemachine;

[ExecuteInEditMode]
[SaveDuringPlay]
[AddComponentMenu("")]
public class CMBounds : CinemachineExtension
{
    [Header("Ustawienia wysokoœci")]
    public float minY = 0f;
    public float maxY = 10f;

    [Header("Ograniczenia osi X (Szerokoœæ)")]
    public bool useMinX = true;
    public float minX = 0f;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime)
    {
        if (stage == CinemachineCore.Stage.Body)
        {
            Vector3 pos = state.RawPosition;

            pos.y = Mathf.Clamp(pos.y, minY, maxY);

            if (useMinX)
            {
                pos.x = Mathf.Max(pos.x, minX);
            }

            state.RawPosition = pos;
        }
    }
}