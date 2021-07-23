using System;
using Anubis.Models;

namespace Anubis.Services
{
    public class LcdService
    {
        private MachineState State { get; }
        private Configuration Config { get; }
        private PanelController Controller { get; }

        public LcdService(MachineState state, Configuration config, PanelController controller)
        {
            State = state ?? throw new ArgumentNullException(nameof(state));
            Config = config ?? throw new ArgumentNullException(nameof(config));
            Controller = controller ?? throw new ArgumentNullException(nameof(controller));
        }

        public void RenderFrame()
        {
            var powerText = $"Power:{(State.Power ? "On" : "Off")}".PadRight(10);
            var relayText = $"Relay:{(State.PassThrough ? "On" : "Off")}".PadRight(10);
            var line1 = powerText + relayText;

            var tempText = $"Temp:{State.Temps.Temp:F1}".PadRight(10);
            var cutOffText = $"Flip:{Config.TempCutoffC:F1}".PadRight(10);
            var line2 = tempText + cutOffText;

            var coolText = (State.Temps.Cool > 0 ? $"Cool:{State.Temps.Cool:F1}" : "Cool: N/A").PadRight(10);
            var heatText = (State.Temps.Heat > 0 ? $"Heat:{State.Temps.Heat:F1}" : "Heat: N/A").PadRight(10);
            var line3 = coolText + heatText;

            var line4 = "<       Mode       >";
            if (State.ConfigTimeout <= Config.ConfigTimeoutTicks)
            {
                line4 = State.ConfigMode switch
                {
                    ConfigModes.Temp => $"Flip:{Config.TempCutoffC:F1}",
                    ConfigModes.QueryRate => $"QueryRate:{Config.ConfigTimeout.Seconds}s",
                    ConfigModes.Max => "",
                    _ => throw new ArgumentOutOfRangeException(),
                };

                // Center the mode text and append the arrows to each end
                var append = (int)Math.Floor((18 - line4.Length) / 2m);
                line4 = "<" + (new string(' ', append) + line4).PadRight(18) + ">";
            }

            Controller.Lcd.SetCursorPosition(0, 0);
            Controller.Lcd.Write(line1);
            Controller.Lcd.SetCursorPosition(0, 1);
            Controller.Lcd.Write(line2);
            Controller.Lcd.SetCursorPosition(0, 2);
            Controller.Lcd.Write(line3);
            Controller.Lcd.SetCursorPosition(0, 3);
            Controller.Lcd.Write(line4);
        }

    }
}
