using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Anubis.Models;

namespace Anubis.Services
{
    public class TemperatureService
    {
        private Configuration Config { get; }

        public TemperatureService(Configuration config)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task<(decimal? Temp, decimal? Cool, decimal? Heat)> GetTemp()
        {
            try
            {
                using var client = new HttpClient();

                var accessTokenUri =
                    $"https://www.googleapis.com/oauth2/v4/token?client_id={Config.ClientId}&client_secret={Config.ClientSecret}&refresh_token={Config.RefreshToken}&grant_type=refresh_token";

                var accessTokenResult = await client.PostAsync(accessTokenUri, new StringContent(""));
                if (!accessTokenResult.IsSuccessStatusCode)
                {
                    await Console.Error.WriteLineAsync("Failed to get refresh token");
                    throw new HttpRequestException(accessTokenResult.ReasonPhrase);
                }

                var accessTokenRaw = await accessTokenResult.Content.ReadAsStringAsync();
                var accessToken = JsonSerializer.Deserialize<RefreshTokenResult>(accessTokenRaw)?.access_token;

                if (string.IsNullOrEmpty(accessToken))
                {
                    throw new InvalidOperationException("Could not obtain access token");
                }

                var dataUri = $"https://smartdevicemanagement.googleapis.com/v1/enterprises/{Config.ProgramId}/devices/{Config.DeviceId}";
                var bearerString = $"Bearer {accessToken}";
                var request = new HttpRequestMessage(HttpMethod.Get, dataUri);
                request.Headers.Add("Authorization", bearerString);

                var devicesResult = await client.SendAsync(request);
                if (!devicesResult.IsSuccessStatusCode)
                {
                    await Console.Error.WriteLineAsync("Failed to get device data");
                    throw new HttpRequestException(devicesResult.ReasonPhrase);
                }

                var devicesRaw = await devicesResult.Content.ReadAsStringAsync();
                var device = JsonSerializer.Deserialize<DeviceInfo>(devicesRaw);

                if (device == null)
                {
                    await Console.Error.WriteLineAsync("Malformed response from device");
                    throw new HttpRequestException(devicesResult.ReasonPhrase);
                }

                var coolRaw = device.traits["sdm.devices.traits.ThermostatTemperatureSetpoint"].ContainsKey("coolCelsius") ?
                    device.traits["sdm.devices.traits.ThermostatTemperatureSetpoint"]["coolCelsius"] :
                    (JsonElement?)null;
                var cool = coolRaw?.ValueKind == JsonValueKind.Number ? coolRaw.Value.GetDecimal() : (decimal?)null;

                var heatRaw = device.traits["sdm.devices.traits.ThermostatTemperatureSetpoint"].ContainsKey("heatCelsius") ?
                    device.traits["sdm.devices.traits.ThermostatTemperatureSetpoint"]["heatCelsius"] :
                    (JsonElement?)null;
                var heat = heatRaw?.ValueKind == JsonValueKind.Number ? heatRaw.Value.GetDecimal() : (decimal?)null;

                var tempRaw =
                    device.traits["sdm.devices.traits.Temperature"]["ambientTemperatureCelsius"];

                var temp = tempRaw.ValueKind == JsonValueKind.Number ? tempRaw.GetDecimal() : (decimal?)null;

                return (temp, cool, heat);
            }
            catch (Exception e)
            {
                await Console.Error.WriteLineAsync(e.Message);
                await Console.Error.WriteLineAsync(e.StackTrace);

                return (null, null, null);
            }
        }

    }
}
