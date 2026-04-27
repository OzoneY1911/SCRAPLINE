using Photon.Deterministic;

namespace Quantum
{
    public class CommandSetEmote : DeterministicCommand
    {
        public AssetRef<EmoteConfig> EmoteConfig;

        public override void Serialize(BitStream stream) 
        {
            stream.Serialize(ref EmoteConfig);
        }
    }
}
