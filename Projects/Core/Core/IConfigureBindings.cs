using OnUtils.Architecture.AppCore;
using OnUtils.Architecture.AppCore.DI;

namespace OnXap
{
    /// <summary>
    /// При создании и запуске ядра создаются экземпляры всех неабстрактных классов, имеющих открытый беспараметрический конструктор, реализующих данный интерфейс,
    /// после чего для каждого экземпляра вызывается метод <see cref="IConfigureBindings{TAppCore}.ConfigureBindings(IBindingsCollection{TAppCore})"/>.
    /// </summary>
    public interface IConfigureBindings : IConfigureBindings<OnXApplication>
    {
    }

    /// <summary>
    /// При загрузке сборок в текущий домен приложения после запуска ядра создаются экземпляры всех неабстрактных классов, 
    /// имеющих открытый беспараметрический конструктор, реализующих данный интерфейс, после чего для каждого экземпляра вызывается 
    /// метод <see cref="IConfigureBindingsLazy{TAppCore}.ConfigureBindingsLazy(IBindingsCollection{TAppCore})" />.
    /// </summary>
    public interface IConfigureBindingsLazy : IConfigureBindingsLazy<OnXApplication>
    {

    }
}
