using System;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;
using Anubis.Models;
using Iot.Device.Subscriptions.Abstractions;

namespace Anubis.Services
{
    public class MachineRunner
    {
        private Configuration Config { get; }
        private StateManager StateManager { get; }
        private ISubscriptionService SubService { get; }
        private PanelController Controller { get; }
        private LcdService LcdService { get; }

        public MachineRunner(Configuration config, StateManager stateManager, ISubscriptionService subService, PanelController controller, LcdService lcdService)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            StateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
            SubService = subService ?? throw new ArgumentNullException(nameof(subService));
            Controller = controller ?? throw new ArgumentNullException(nameof(controller));
            LcdService = lcdService ?? throw new ArgumentNullException(nameof(lcdService));
        }

        public async Task RunAsync()
        {
            await foreach (var e in SubService.Run(Controller.Board, CancellationToken.None))
            {
                await StateManager.IncrementDeltasAsync(e.Delta);

                if (e.PinNumber == Config.PowerBtn)
                {
                    StateManager.UpdatePowerState();
                }

                if (e.PinNumber == Config.LeftBtn && StateManager.ButtonsAvailable())
                {
                    StateManager.ResetButtonDelay();
                    StateManager.DecrementConfig();
                    StateManager.UpdatePowerState();
                }
                if (e.PinNumber == Config.RightBtn && StateManager.ButtonsAvailable())
                {
                    StateManager.ResetButtonDelay();
                    StateManager.IncrementConfig();
                    StateManager.UpdatePowerState();
                }
                if (e.PinNumber == Config.CenterBtn && StateManager.ButtonsAvailable())
                {
                    StateManager.ResetButtonDelay();
                    StateManager.CycleConfig();
                }

                LcdService.RenderFrame();
            }

            Controller.Board.Write(Config.PwrRelay, PinValue.High);
            Controller.Board.Write(Config.PwrLED, PinValue.High);
        }

    }
}
