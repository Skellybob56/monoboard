namespace Monoboard;

public abstract class Singleton<T> : IDisposable where T : Singleton<T>
{
	private static T? Instance = null;

	protected Singleton()
	{
		if (Instance is not null)
		{ throw new InvalidOperationException($"{typeof(T).Name} instance already exists."); }
		Instance = (T)this;
	}

	public void Dispose()
	{
		if (!ReferenceEquals(this, Instance)) { return; }
		Instance = null;
		OnDisposal();
	}

	protected virtual void OnDisposal() { }
}
