/*
 * SingletonT 版本: V1.0.0，设计思路：
 * 1. 全局只实例化一个对象，方便管理
 * 2. 使用饿汉式实现，调用时才进行实例化
 * 3. 使用泛型实现单例，使其通用
 * 4. 支持游戏运行中或编辑器两种环境
 */
public abstract class Singleton<T> where T : class, new()
{
    private static T _instance;

    public static T instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new T();
            }
            
            return _instance;
        }
    }
    
    public static void Dispose()
    {
        _instance = null;
    }
}
