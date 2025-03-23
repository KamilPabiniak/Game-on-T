using Unity.Mathematics;
using Unity.NetCode;

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct PlayerInputData : IInputComponentData
{
    public float2 Move;
    public bool Rotate;
}