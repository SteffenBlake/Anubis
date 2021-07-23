using System;
using System.Device.Gpio;
using System.Device.I2c;
using Iot.Device.CharacterLcd;
using Iot.Device.Pcx857x;

namespace Anubis.Models
{
    public class PanelController : IDisposable
    {
        public PanelController(I2cDevice i2C, Pcf8574 driver, GpioController lcdController, Hd44780 lcd, GpioController board)
        {
            I2C = i2C ?? throw new ArgumentNullException(nameof(i2C));
            Driver = driver ?? throw new ArgumentNullException(nameof(driver));
            LcdController = lcdController ?? throw new ArgumentNullException(nameof(lcdController));
            Lcd = lcd ?? throw new ArgumentNullException(nameof(lcd));
            Board = board ?? throw new ArgumentNullException(nameof(board));
        }

        private I2cDevice I2C { get; }
        private Pcf8574 Driver { get; }
        private GpioController LcdController { get; }
        public Hd44780 Lcd { get; }
        public GpioController Board { get; }

        public void Dispose()
        {
            I2C?.Dispose();
            Driver?.Dispose();
            LcdController?.Dispose();
            Lcd?.Dispose();
            Board?.Dispose();
        }
    }
}