using OnUtils;

namespace OnXap.Users
{
#pragma warning disable CS1591 // todo внести комментарии.
    /// <summary>
    /// Описывает пользовательскую сущность (см. <see cref="IEntitiesManager"/>).
    /// </summary>
    public abstract class UserEntity
    {
        public int IdEntity { get; set; } = 0;

        public int IdUser { get; set; } = 0;

        public string Tag { get; set; } = string.Empty;

        public ExecutionResult Init(int idEntity, string tag, string data = null)
        {
            IdEntity = idEntity;
            Tag = tag;
            return OnInit(data);
        }

        protected abstract ExecutionResult OnInit(string data);

        protected virtual object GetDataForSave()
        {
            return null;
        }
    }

}
