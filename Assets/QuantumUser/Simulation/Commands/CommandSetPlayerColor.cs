namespace Quantum
{
    using Photon.Deterministic;

    public class CommandSetPlayerColor : DeterministicCommand
    {
        public FPVector3 ColorRGB;

        public override void Serialize(BitStream stream)
        {
            stream.Serialize(ref ColorRGB);
        }
    }
}
