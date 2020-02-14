using OnUtils.Architecture.AppCore;

namespace OnXap
{
    /// <summary>
    /// При создании и запуске ядра создаются экземпляры всех неабстрактных классов, имеющих открытый беспараметрический конструктор, 
    /// реализующих данный интерфейс, после чего для каждого экземпляра вызывается метод <see cref="IExecuteStart{TAppCore}.ExecuteStart(TAppCore)"/>.
    /// </summary>
    public interface IExecuteStart : IExecuteStart<OnXApplication>
    {
    }

    /// <summary>
    /// При загрузке сборок в текущий домен приложения после запуска ядра создаются экземпляры всех неабстрактных классов, имеющих открытый беспараметрический конструктор,
    /// реализующих данный интерфейс, после чего для каждого экземпляра вызывается метод <see cref="IExecuteStartLazy{TAppCore}.ExecuteStartLazy(TAppCore)"/>.
    /// </summary>
    public interface IExecuteStartLazy : IExecuteStartLazy<OnXApplication>
    {

    }
}
