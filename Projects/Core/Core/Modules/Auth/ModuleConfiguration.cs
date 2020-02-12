namespace OnXap.Modules.Auth
{
    using Core.Configuration;

    public class ModuleConfiguration : ModuleConfiguration<ModuleAuth>
    {
        /// <summary>
        /// Допустимое количество одновременно неудачных попыток входа, после которых возможность совершения новых попыток блокируется на определенное время (<see cref="AuthorizationAttemptsBlock"/>).
        /// </summary>
        public int AuthorizationAttempts
        {
            get => Get("AuthorizationAttemptsBlock", 0);
            set => Set("AuthorizationAttemptsBlock", value);
        }

        /// <summary>
        /// Время блокировки после исчерпания лимита неудачных попыток входа (<see cref="AuthorizationAttempts"/>).
        /// </summary>
        public int AuthorizationAttemptsBlock
        {
            get => Get("AuthorizationAttemptsBlock", 0);
            set => Set("AuthorizationAttemptsBlock", value);
        }

        /// <summary>
        /// Сообщение для пользователя, показываемое после исчерпания лимита неудачных попыток входа (<see cref="AuthorizationAttempts"/>).
        /// </summary>
        public string AuthorizationAttemptsMessage
        {
            get => Get("AuthorizationAttemptsMessage", "");
            set => Set("AuthorizationAttemptsMessage", value);
        }

        /// <summary>
        /// Сообщение для пользователя во время последующих попыток входа, показываемое после исчерпания лимита неудачных попыток входа (<see cref="AuthorizationAttempts"/>).
        /// </summary>
        public string AuthorizationAttemptsBlockMessage
        {
            get => Get("AuthorizationAttemptsBlockMessage", "");
            set => Set("AuthorizationAttemptsBlockMessage", value);
        }

    }
}
