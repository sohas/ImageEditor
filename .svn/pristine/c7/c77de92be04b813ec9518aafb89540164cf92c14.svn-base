namespace OperationDispatcher
{
	public interface IOperationLogger<TOperationID> where TOperationID : IID
	{
		public abstract void LogOperation(LogType type, TOperationID id, Exception? ex = null);
	}

	public enum LogType
	{
		Enqueue,
		Start,
		Complete,
		Cancel,
		Exception
	}
}