using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Server;

namespace AssemblyLineManager.AssemblyStation;

public partial class AssemblyStation
{
    private static readonly MqttFactory _mqttFactory;
    private static readonly IMqttClient _mqttClient;

    private static async Task ConnectToClient()
    {
        //Setting up the required info to establish a connection to the MQTT broker.
        IMqttClientOptions options = new MqttClientOptionsBuilder()
            .WithClientId("AssemblyLineManagerClient")
            .WithTcpServer("localhost", 1883)
            .WithCleanSession(true)
            .Build();

        //Setting up code that runs once a connection is established.
        _mqttClient.UseConnectedHandler(e =>
        {
            Console.WriteLine("Connected successfully with MQTT Broker.");
            SubscribeToTopics().Wait();
            Console.WriteLine("Subscribed to topics.");
        });

        //Setting up code that runs once a connection is lost.
        _mqttClient.UseDisconnectedHandler(e =>
        {
            Console.WriteLine("Disconnected from MQTT Broker unexpectedly.");
        });

        //Setting up code that runs every time a message is received.
        _mqttClient.UseApplicationMessageReceivedHandler(MessageReceivedHandling);

        //Establishing the connection.
        await _mqttClient.ConnectAsync(options);
    }

    //Manually disconnect from the MQTT broker.
    private static async Task DisconnectFromClient()
    {
        try
        {
            await _mqttClient.DisconnectAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Disconnect from Broker failed.");
            Console.WriteLine(ex.Message);
        }
    }

    //Subscribe to the topics we want to receive messages from.
    private static async Task SubscribeToTopics()
    {
        var subscribeOptions = _mqttFactory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter("emulator/status")
            .WithTopicFilter("emulator/checkhealth")
            .WithTopicFilter("emulator/echo") //This isn't strictly necessary, but why not receive it?
            .Build();

        await _mqttClient.SubscribeAsync(subscribeOptions);
    }
}