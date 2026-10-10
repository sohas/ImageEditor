using System.Threading.Channels;

namespace OperationDispatcher
{
	public record ChainKey(Guid ChainUid, string ChainName);

	internal class ChainContext<TOperationID>(ChainKey key, Action onQueueEmpty, IOperationLogger<TOperationID> logger) where TOperationID : IID
	{
		private readonly Channel<DispatchedOperation<TOperationID>> operationsChannel = Channel.CreateUnbounded<DispatchedOperation<TOperationID>>(
			    new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });
		private CancellationTokenSource chainCts = new();
		private CancellationTokenSource? currentActiveTaskCts;
		private int activeTasksCount = 0; // Счетчик активных тасок в этой очереди

		// Канал строго для одного читателя (воркера этой цепочки)
		public Channel<DispatchedOperation<TOperationID>> OperationsChannel => operationsChannel;

		public async void StartWorker()
		{
			// Запускаем асинхронный цикл обработки этой конкретной цепочки
			await ProcessChainQueueAsync(chainCts.Token);
		}

		public void CancelAll()
		{
			currentActiveTaskCts?.Cancel(); // Прерываем то, что считается сейчас
			chainCts.Cancel();              // Сжигаем всю цепочку
			operationsChannel.Writer.TryComplete();    // Закрываем канал для записи
		}

		private async Task ProcessChainQueueAsync(CancellationToken chainToken)
		{
			try
			{
				while (await operationsChannel.Reader.WaitToReadAsync(chainToken))
				{
					while (operationsChannel.Reader.TryRead(out var operation))
					{
						Interlocked.Increment(ref activeTasksCount);

						if (chainToken.IsCancellationRequested)
						{
							operation.SetCanceled();
							logger.LogOperation(LogType.Cancel, operation.OperationID);
							continue;
						}

						currentActiveTaskCts = new CancellationTokenSource();
						var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(chainToken, currentActiveTaskCts.Token).Token;

						try
						{
							// Выполнение конкретной задачи внутри цепочки
							await operation.Action(linkedToken, operation.ProgressReporter, operation.OperationID);
							operation.SetComplete();
							logger.LogOperation(LogType.Complete, operation.OperationID);
						}
						catch (OperationCanceledException)
						{
							operation.SetCanceled();
							logger.LogOperation(LogType.Cancel, operation.OperationID);
						}
						catch (Exception ex)
						{
							operation.SetException(ex);
							logger.LogOperation(LogType.Exception, operation.OperationID, ex);
						}
						finally
						{
							currentActiveTaskCts.Dispose();
							currentActiveTaskCts = null;
							Interlocked.Decrement(ref activeTasksCount);
						}
					}
				}
			}
			catch (OperationCanceledException) { /* Очередь была отменена */ }
			finally
			{
				// Канал пуст, и новых задач нет — уничтожаем цепочку, чтобы освободить RAM
				if (activeTasksCount == 0)
				{
					onQueueEmpty();
				}
			}
		}
	}
}