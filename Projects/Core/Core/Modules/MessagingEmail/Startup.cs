namespace OnXap.Modules.MessagingEmail
{
    class Startup : StartupBase
    {
        protected override void ConfigureBindings(BindingsCollection bindingsCollection)
        {
            bindingsCollection.SetSingleton<Module>();
            bindingsCollection.SetSingleton<EmailService>();
            bindingsCollection.SetTransient<Components.SmtpServer>();
        }
    }
}
