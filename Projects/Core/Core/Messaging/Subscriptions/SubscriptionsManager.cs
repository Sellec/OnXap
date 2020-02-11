using OnUtils.Data;

namespace OnXap.Messaging.Subscriptions
{
    using Core;
    using Core.Db;

    class SubscriptionsManager : CoreComponentBase, ISubscriptionsManager, IUnitOfWorkAccessor<CoreContext>
    {
        #region CoreComponentBase
        protected sealed override void OnStart()
        {
        }

        protected sealed override void OnStop()
        {
        }
        #endregion

    }
}