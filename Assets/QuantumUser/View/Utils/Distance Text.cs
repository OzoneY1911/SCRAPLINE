using Quantum;
using TMPro;
using UnityEngine;

public unsafe class DistanceText : MonoBehaviour
{
    [SerializeField] private float _opaqueDistance = 3f;
    [SerializeField] private float _transparentDistance = 5f;

    private TextMeshPro _tmp;
    private EntityRef _localPlayerEntity;

    private void Awake()
    {
        _tmp = GetComponent<TextMeshPro>();
    }

    private void Start()
    {
        var frame = QuantumRunner.Default.Game.Frames.Verified;
        if (frame == null) return;
        var localPlayers = QuantumRunner.Default.Game.GetLocalPlayers();
        if (localPlayers.Count == 0) return;

        var activePlayers = frame.ResolveDictionary<PlayerRef, EntityRef>(frame.Global->ActivePlayers);

        if (activePlayers.TryGetValue(localPlayers[0], out var localPlayerEntity))
        {
            _localPlayerEntity = localPlayerEntity;
        }
    }

    private void Update()
    {
        var frame = QuantumRunner.Default.Game.Frames.Predicted;
        if (frame == null) return;

        if (!frame.Unsafe.TryGetPointer<Transform3D>(_localPlayerEntity, out var playerTransform)) return;

        float playerDistance = Vector3.Distance(transform.position, playerTransform->Position.ToUnityVector3());

        float alpha = Mathf.InverseLerp(_transparentDistance, _opaqueDistance, playerDistance);
        _tmp.color = _tmp.color.Alpha(alpha);
    }
}
