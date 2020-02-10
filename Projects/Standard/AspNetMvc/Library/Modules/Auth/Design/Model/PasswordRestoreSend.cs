namespace OnXap.Modules.Auth.Design.Model
{
    using Core.Db;

    public class PasswordRestoreSend
    {
        public User User { get; set; }

        public string Code { get; set; }

        public string CodeType { get; set; }
    }
}