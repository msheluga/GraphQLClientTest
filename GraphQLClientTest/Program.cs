// See https://aka.ms/new-console-template for more information


using GraphQLClientTest;
using Microsoft.Extensions.Configuration;

using System.Security.Cryptography.X509Certificates;

internal class GraphQLClientText
{
    protected static IConfigurationRoot Config { get; set; }

    static void Main(string[] args)
    { 
        Console.WriteLine("1. Hit any key to send a message");
        string response;
        response = Console.ReadLine();

        GraphQL_Client.Run();
    }
}