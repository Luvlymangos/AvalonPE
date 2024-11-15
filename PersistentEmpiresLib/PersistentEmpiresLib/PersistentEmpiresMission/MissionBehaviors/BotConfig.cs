namespace PersistentEmpiresMission.MissionBehaviors
{
    public class BotConfig
    {
        public int Id { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public float Range { get; set; }

        public int CastleId { get; set; }

        public float IdleMovement { get; set; }
    }
}