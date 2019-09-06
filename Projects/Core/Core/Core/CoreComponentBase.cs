using OnUtils.Architecture.AppCore;

namespace OnXap.Core
{
    /// <summary>
    /// Базовая реализация компонента ядра.
    /// </summary>
    public abstract class CoreComponentBase : CoreComponentBase<OnXApplication>, IComponentStartable, IComponent
    {
    }
}
