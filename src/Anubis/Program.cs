#nullable enable
using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.IO;
using System.Threading.Tasks;
using Anubis.Models;
using Anubis.Services;
using Iot.Device.CharacterLcd;
using Iot.Device.Pcx857x;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Anubis
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("Building configuration...");

                var config = BuildConfiguration(args);
                using var controller = BuildController(config);
                var serviceProvider = new Startup(config, controller).BuildServices();

                var runner = serviceProvider.GetRequiredService<MachineRunner>();

                await runner.RunAsync();
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync(ex.Message);
                await Console.Error.WriteLineAsync(ex.StackTrace);
            } 
        }

        private static Configuration BuildConfiguration(string[] args)
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
                .AddEnvironmentVariables()
                .AddJsonFile("AppSettings.json", true)
                .AddUserSecrets<Program>(true)
                .AddCommandLine(args)
                .Build()
                .Get<Configuration>();
        }

        private static PanelController BuildController(Configuration config)
        {
            var i2c = I2cDevice.Create(new I2cConnectionSettings(1, 0x27));
            var driver = new Pcf8574(i2c);
            var lcdController = new GpioController(PinNumberingScheme.Logical, driver);

            Hd44780 lcd = new Lcd2004(0, 2, new[] { 4, 5, 6, 7 }, 3, 0.5f, 1, lcdController);

            var board = new GpioController(PinNumberingScheme.Board);
            board.OpenPin(config.PwrLED, PinMode.Output);
            board.OpenPin(config.PwrRelay, PinMode.Output);

            return new PanelController(i2c, driver, lcdController, lcd, board);
        }

    }
}