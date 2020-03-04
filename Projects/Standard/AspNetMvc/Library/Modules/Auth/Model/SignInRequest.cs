namespace OnXap.Modules.Auth.Model
{
    [reCAPTCHA.Model]
    public class SignInRequest
    {
        public string login { get; set; }

        public string pass { get; set; }

        public string urlFrom { get; set; }


    }
}
