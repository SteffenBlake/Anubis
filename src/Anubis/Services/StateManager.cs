using System;
using System.Device.Gpio;
using System.Threading.Tasks;
using Anubis.Models;

namespace Anubis.Services
{
    public class StateManager
    {
        private Configuration Config { get; }
        private MachineState State { get; }
        private PanelController Controller { get; }
        private ConfigurationManager ConfigurationManager { get; }
        private TemperatureService TempService { get; }

        public StateManager(Configuration config, MachineState state, PanelController controller, ConfigurationManager configurationManager, TemperatureService tempService)
        {
            Config = config;
            State = state ?? throw new ArgumentNullException(nameof(state));
            Controller = controller ?? throw new ArgumentNullException(nameof(controller));
            ConfigurationManager = configurationManager ?? throw new ArgumentNullException(nameof(configurationManager));
            TempService = tempService ?? throw new ArgumentNullException(nameof(tempService));
        }

        public async Task IncrementDeltasAsync(long delta)
        {
            if (delta <= 0)
                return;

            if (State.ButtonDelay <= Config.ButtonDelayTicks)
                State.ButtonDelay += delta;

            if (State.ConfigTimeout <= Config.ConfigTimeoutTicks)
            {
                State.ConfigTimeout += delta;
                if (State.ConfigTimeout > Config.ConfigTimeoutTicks)
                {
                    Console.WriteLine("Rewriting Configuration");
                    await ConfigurationManager.ReWriteConfigAsync();
                }
            }

            State.QueryDelay += delta;

            if (State.QueryDelay >= Config.QueryRateTicks)
            {
                Console.WriteLine("Temperature Event");
                State.QueryDelay -= Config.QueryRateTicks;
                await UpdateTemperaturesAsync();
                UpdatePowerState();
            }
        }

        public void ResetButtonDelay()
        {
            State.ButtonDelay = 0;
        }

        public bool ButtonsAvailable()
        {
            return State.ButtonDelay >= Config.ButtonDelayTicks;
        }

        public void IncrementConfig()
        {
            if (State.ConfigTimeout < Config.ConfigTimeoutTicks)
            {
                ConfigurationManager.ModifyConfig(1);
            }

            State.ConfigTimeout = 0;
        }
        public void DecrementConfig()
        {
            if (State.ConfigTimeout < Config.ConfigTimeoutTicks)
            {
                ConfigurationManager.ModifyConfig(-1);
            }

            State.ConfigTimeout = 0;
        }

        public void CycleConfig()
        {
            if (State.ConfigTimeout < Config.ConfigTimeoutTicks)
            {
                ConfigurationManager.CycleConfig();
            }

            State.ConfigTimeout = 0;
        }

        public async Task UpdateTemperaturesAsync()
        {
            var tempData = await TempService.GetTemp();
            State.Temps.Temp = tempData.Temp ?? 0;
            State.Temps.Cool = tempData.Cool ?? 0;
            State.Temps.Heat = tempData.Heat ?? 0;
        }

        public void UpdatePowerState()
        {
            State.Power = Controller.Board.Read(Config.PowerBtn) == PinValue.Low;
            State.PassThrough = !State.Power || State.Temps.Temp > Config.TempCutoffC;

            // Relay Power
            Controller.Board.Write(Config.PwrRelay, State.PassThrough ? PinValue.Low : PinValue.High);
            // LED Power
            Controller.Board.Write(Config.PwrLED, State.Power ? PinValue.Low : PinValue.High);
        }

    }
}
