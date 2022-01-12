﻿namespace OnXap.Messaging
{
    class Startup : StartupBase
    {
        protected override void ConfigureBindings(BindingsCollection bindingsCollection)
        {
            bindingsCollection.SetTransient<DbSchema.MessageQueue>();
            bindingsCollection.SetTransient<DbSchema.MessageQueueHistory>();
        }
    }
}

