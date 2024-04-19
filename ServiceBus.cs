using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.InteropExtensions;
using RoodFluweel.Printing.Model;

public class ServiceBus
{
    // TODO: Make these configurable
    const string connectionString = "Endpoint=sb://<placeholder>";
    const string queueName = "printers/printer1"; //Settings.Default.ServiceBusQueue;

    static QueueClient queueClient;

    public static async Task ReceiveMessagesAsync(Func<TicketBundle, Task> messageHandler)
    {
        // Initialize the queue client
        queueClient = new QueueClient(connectionString, queueName);

        // Register the message handler and receive messages in a loop
        queueClient.RegisterMessageHandler(async (message, token) =>
        {
            TicketBundle tb = MessageInteropExtensions.GetBody<TicketBundle>(message);
            await messageHandler(tb);

            // Complete the message so that it is not received again
            await queueClient.CompleteAsync(message.SystemProperties.LockToken);
        },
        new MessageHandlerOptions(async args =>
        {
            // Handle any exceptions
            Console.WriteLine($"Exception: {args.Exception}");
            // return; // Task.CompletedTask;
        })
        {
            MaxConcurrentCalls = 1,
            AutoComplete = false // We'll manually complete the message after processing
        }
        );
    }

    public static async Task StopReceivingMessagesAsync()
    {
        // Close the queue client
        await queueClient.CloseAsync();
    }
}