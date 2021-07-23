using System;
using System.Device.Gpio;
using System.Text.Json.Serialization;

namespace Anubis.Models
{
    public class Configuration
    {
        public decimal TempCutoffC { get; set; }

        public string RefreshToken { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string ProgramId { get; set; }
        public string DeviceId { get; set; }

        public TimeSpan QueryRate { get; set; }

        [JsonIgnore]
        public long QueryRateTicks => QueryRate.Ticks;

        public TimeSpan ClockRate { get; set; }

        public TimeSpan ConfigTimeout { get; set; }

        [JsonIgnore]
        public long ConfigTimeoutTicks => ConfigTimeout.Ticks;


        public PinMode PowerMode { get; set; }

        public TimeSpan ButtonDelay { get; set; }

        [JsonIgnore]
        public long ButtonDelayTicks => ButtonDelay.Ticks;

        public int PowerBtn { get; set; }
        public int LeftBtn { get; set; }
        public int CenterBtn { get; set; }
        public int RightBtn { get; set; }
        public int PwrLED { get; set; }
        public int PwrRelay { get; set; }
    }
}
