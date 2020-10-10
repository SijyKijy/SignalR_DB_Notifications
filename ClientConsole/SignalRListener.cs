using ClientConsole.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ClientConsole
{
    public class SignalRListener
    {
        private readonly HubConnection _hub;
        private readonly IConfiguration _configuration;

        public SignalRListener(string token)
        {
            _configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true).Build();

            _hub = new HubConnectionBuilder()
                .WithUrl($"{_configuration.GetConnectionString("TestHub")}hub?token={token}")
                .Build();
        }

        public async Task Connect()
        {
            _hub.On<string>("Notify", Console.WriteLine);
            _hub.On<User>("DbNotify", user => Console.WriteLine($"Your data has been updated. Name: {user.Name} Money: {user.Money}"));
            _hub.On<string>("SessionError", async msg => await SessionError(msg).ConfigureAwait(false));

            _hub.Closed += Hub_Closed;

            try
            {
                await _hub.StartAsync().ConfigureAwait(false);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private Task Hub_Closed(Exception arg)
        {
            Console.WriteLine($"\nDisconnected {arg?.Message ?? ""}");
            return Task.CompletedTask;
        }

        private async Task SessionError(string error)
        {
            bool isPressed;

            Console.WriteLine(error);
            Console.WriteLine("User already connect. Disconnect him? [y/n]");
            do
            {
                isPressed = false;

                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.Y:
                        {
                            await _hub.InvokeAsync("CloseOtherSessions").ConfigureAwait(false);
                            break;
                        }
                    case ConsoleKey.N:
                        {
                            await _hub.InvokeAsync("AddSession");
                            break;
                        }
                    default:
                        isPressed = true;
                        break;
                }

            } while (isPressed);
        }
    }
}
