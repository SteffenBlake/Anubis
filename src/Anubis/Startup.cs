using System;
using System.Device.Gpio;
using Anubis.Models;
using Anubis.Services;
using Iot.Device.Subscriptions;
using Iot.Device.Subscriptions.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Anubis
{
    public class Startup
    {
        private Configuration Config { get; }
        private PanelController Controller { get; }

        public Startup(Configuration config, PanelController controller)
        {
            Config = config ?? throw new ArgumentNullException(nameof(Config));
            Controller = controller;
        }

        public IServiceProvider BuildServices()
        {
            return RegisterServices()
                .BuildServiceProvider();
        }

        private IServiceCollection RegisterServices()
        {
            return new ServiceCollection()
                .AddSingleton(Config)
                .AddSingleton(Controller)
                .AddSingleton(BuildState())
                .AddSingleton(BuildSubscriptionService())
                .AddSingleton<ConfigurationManager>()
                .AddSingleton<LcdService>()
                .AddSingleton<MachineRunner>()
                .AddSingleton<StateManager>()
                .AddSingleton<TemperatureService>();
        }

        public MachineState BuildState()
        {
            return new MachineState
            {
                ButtonDelay = Config.ButtonDelayTicks,
                ConfigTimeout = Config.ConfigTimeoutTicks,
                QueryDelay = Config.QueryRateTicks,
                ConfigMode = ConfigModes.Temp,
            };
        }

        public ISubscriptionService BuildSubscriptionService()
        {
            return new SubscriptionCollection
                {
                    ClockEnabled = true,
                    ClockRate = Config.ClockRate
                }
                .Subscribe(Config.PowerBtn, Config.PowerMode, PinEventTypes.Rising)
                .Subscribe(Config.LeftBtn, Config.PowerMode, PinEventTypes.Rising)
                .Subscribe(Config.CenterBtn, Config.PowerMode, PinEventTypes.Rising)
                .Subscribe(Config.RightBtn, Config.PowerMode, PinEventTypes.Rising)
                .Build();
        }
    }
}
