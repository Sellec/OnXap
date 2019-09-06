using Newtonsoft.Json;

namespace OnXap.Modules.reCAPTCHA
{
    class ReCaptcha2Answer
    {
        [JsonProperty("success")]
        public bool Success = false;

        [JsonProperty("error-codes")]
        public string[] Errors = null;
    }


}