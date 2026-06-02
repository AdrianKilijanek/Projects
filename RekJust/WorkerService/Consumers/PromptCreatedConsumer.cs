using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using WorkerService.Data;

namespace WorkerService.Consumers;

/// <summary>
/// Konsumuje wiadomości PromptCreated z kolejki RabbitMQ i przetwarza je przez Semantic Kernel.
/// Po sukcesie MassTransit wysyła ACK; wyjątek powoduje NACK i uruchomienie retry policy.
/// </summary>
public class PromptCreatedConsumer(
    AppDbContext db,
    Kernel kernel,
    ILogger<PromptCreatedConsumer> logger) : IConsumer<PromptCreated>
{
    public async Task Consume(ConsumeContext<PromptCreated> context)
    {
        var promptId = context.Message.PromptId;
        logger.LogInformation("Przetwarzam prompt {PromptId}", promptId);

        // Atomowy optimistic lock: UPDATE WHERE status='pending' zwraca 0 jeśli inny worker już przejął zadanie.
        // Zabezpiecza przed podwójnym przetworzeniem przy at-least-once delivery RabbitMQ.
        var updated = await db.Prompts
            .Where(p => p.Id == promptId && p.Status == "pending")
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.Status, "processing")
                .SetProperty(p => p.ProcessedAt, DateTime.UtcNow));

        if (updated == 0)
        {
            logger.LogWarning("Prompt {PromptId} już przetwarzany lub nie istnieje – pomijam", promptId);
            return;
        }

        var prompt = await db.Prompts.FindAsync(promptId);
        if (prompt is null) return;

        try
        {
            var chatService = kernel.GetRequiredService<IChatCompletionService>();

            var history = new ChatHistory();
            history.AddSystemMessage("Jesteś pomocnym asystentem. Odpowiadaj zwięźle i po polsku.");
            history.AddUserMessage(prompt.Text);

            var response = await chatService.GetChatMessageContentAsync(history);
            var result = response.Content ?? "Brak odpowiedzi";

            await db.Prompts
                .Where(p => p.Id == promptId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.Status, "completed")
                    .SetProperty(p => p.Result, result)
                    .SetProperty(p => p.CompletedAt, DateTime.UtcNow));

            logger.LogInformation("Prompt {PromptId} przetworzony pomyślnie", promptId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Błąd przetwarzania promptu {PromptId}", promptId);

            await db.Prompts
                .Where(p => p.Id == promptId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.Status, "failed")
                    .SetProperty(p => p.ErrorMessage, ex.Message));

            // Re-throw żeby MassTransit wykonał retry zgodnie z konfiguracją.
            throw;
        }
    }
}
