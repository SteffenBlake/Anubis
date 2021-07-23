using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Anubis.Models;

namespace Anubis.Services
{
    public class ConfigurationManager
    {
        private Configuration Config { get; }
        private MachineState State { get; }

        public ConfigurationManager(Configuration config, MachineState state)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            State = state ?? throw new ArgumentNullException(nameof(state));
        }

        public void ModifyConfig(int direction)
        {
            switch (State.ConfigMode)
            {
                case ConfigModes.Temp:
                    Config.TempCutoffC += direction * 0.5m;
                    if (Config.TempCutoffC > State.Temps.Cool)
                        Config.TempCutoffC = State.Temps.Cool;
                    break;
                case ConfigModes.QueryRate:
                    Config.ConfigTimeout += TimeSpan.FromSeconds(direction);
                    if (Config.ConfigTimeout.Seconds < 1)
                        Config.ConfigTimeout = TimeSpan.FromSeconds(1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void CycleConfig()
        {
            var modes = Enum.GetValues<ConfigModes>().ToList();
            var index = modes.IndexOf(State.ConfigMode) +1;
            if (index == modes.Count -1)
                index = 0;

            State.ConfigMode = modes[index];
        }

        public Task ReWriteConfigAsync()
        {
            return File.WriteAllTextAsync("./AppSettings.json", JsonSerializer.Serialize(Config, new JsonSerializerOptions
            {
                WriteIndented = true
            }));
        }
    }
}
