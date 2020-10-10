using System;
using System.Threading.Tasks;

namespace ClientConsole
{
    static class Program
    {
        static async Task Main()
        {
            string id;

            Console.Write("Enter id: ");
            while (string.IsNullOrWhiteSpace(id = Console.ReadLine())) ;

            await new SignalRListener(id).Connect().ConfigureAwait(false);

            await Task.Delay(-1);
        }
    }
}