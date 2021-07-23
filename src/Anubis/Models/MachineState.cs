namespace Anubis.Models
{
    public class MachineState
    {
        public bool Power { get; set; }
        public bool PassThrough { get; set; }
        public long ButtonDelay { get; set; }
        public long QueryDelay { get; set; }
        public TempState Temps { get; set; } = new TempState();
        public ConfigModes ConfigMode { get; set; }
        public long ConfigTimeout { get; set; }
    }
}