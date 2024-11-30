public class MonoSingletonTestAwake : MonoSingleton<MonoSingletonTestAwake>
{
    protected override bool isDontDestroyOnLoad { get => false; }
}
