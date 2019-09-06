namespace OnXap.Messaging.Components
{
    interface IInternal
    {
        string SerializedSettings { set; }
        bool OnStartComponent();
    }
}
