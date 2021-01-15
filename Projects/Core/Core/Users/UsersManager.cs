using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace OnXap.Users
{
    using Core;
    using Core.Data;
    using Core.Db;
    using Journaling;

    /// <summary>
    /// Представляет менеджер, позволяющий управлять данными пользователей.
    /// </summary>
    public sealed class UsersManager : CoreComponentBase, IComponentSingleton
    {
        #region CoreComponentBase
        /// <summary>
        /// </summary>
        protected sealed override void OnStarting()
        {
            this.RegisterJournal("Журнал менеджера данных пользователей");
        }

        /// <summary>
        /// </summary>
        protected sealed override void OnStop()
        {
        }
        #endregion

        /// <summary>
        /// Возвращает список пользователей, у которых есть роли из переданного списка
        /// </summary>
        /// <param name="roleIdList">Список ролей для поиска пользователей</param>
        /// <param name="onlyActive">Если true, то возвращает только активных пользователей.</param>
        /// <param name="exceptSuperuser">Если false, то в список будут включены суперпользователи (у суперпользователей по-умолчанию есть все роли).</param>
        /// <param name="orderBy">Сортировка выдачи</param>
        /// <returns>Возвращает список пар {пользователь:список ролей из <paramref name="roleIdList"/>} для пользователей, обладающих ролями из списка.</returns>
        [ApiReversible]
        public Dictionary<User, int[]> UsersByRoles(int[] roleIdList, bool onlyActive = true, bool exceptSuperuser = false, Dictionary<string, bool> orderBy = null)
        {
            try
            {
                if (roleIdList.IsNullOrEmpty()) return new Dictionary<User, int[]>();
                if (orderBy != null) throw new ArgumentException("Параметр не поддерживается.", nameof(orderBy));

                using (var db = new CoreContext())
                {
                    var queryUsersBase = db.Users.AsQueryable();
                    if (onlyActive) queryUsersBase = queryUsersBase.Where(x => x.State == 0);

                    var idRoleUser = AppCore.AppConfig.RoleUser;
                    if (!roleIdList.Contains(idRoleUser))
                    {
                        var queryRoleUser = db.RoleUser.In(roleIdList, x => x.IdRole);
                        var queryRolesWithUsers = from user in queryUsersBase
                                                  join role in queryRoleUser on user.IdUser equals role.IdUser
                                                  select new { user.IdUser, role.IdRole };
                        if (!exceptSuperuser)
                        {
                            var queryRole = db.Role.In(roleIdList, x => x.IdRole);
                            queryRolesWithUsers = queryRolesWithUsers.Union(from user in queryUsersBase
                                                                            from role in queryRole
                                                                            where user.Superuser != 0
                                                                            select new { user.IdUser, role.IdRole });
                        }

                        var data = queryRolesWithUsers.Distinct().ToList().GroupBy(x => x.IdUser, x => x.IdRole).ToDictionary(x => x.Key, x => x.Distinct().ToArray());

                        var queryUsers = db.Users.In(data.Keys, x => x.IdUser);

                        var data2 = queryUsers.ToList().ToDictionary(x => x, x => data[x.IdUser]);
                        return data2;
                    }
                    else
                    {
                        return queryUsersBase.ToDictionary(x => x, x => new int[] { idRoleUser });
                    }
                }
            }
            catch (ArgumentException) { throw; }
            catch (Exception ex)
            {
                this.RegisterEvent(
                    EventType.Error,
                    "Ошибка получения списка пользователей, обладающих ролями.",
                    $"Идентификаторы ролей: {string.Join(", ", roleIdList)}.\r\nПо активности: {(onlyActive ? "только активных" : "всех")}.\r\nСуперпользователи: {(exceptSuperuser ? "только если роль назначена напрямую" : "добавлять всегда")}.\r\nСортировка: {orderBy?.ToString()}.",
                    ex);
                throw;
            }
        }

        /// <summary>
        /// Возвращает список ролей указанного пользователя.
        /// </summary>
        /// <param name="idUser">Идентификатор пользователя.</param>
        [ApiReversible]
        public List<Role> RolesByUser(int idUser)
        {
            return RolesByUsers(new int[] { idUser }).Select(x => x.Value).FirstOrDefault() ?? new List<Role>();
        }


        /// <summary>
        /// Возвращает списки ролей указанных пользователей.
        /// </summary>
        /// <param name="userIdList">Список пользователей.</param>
        /// <returns></returns>
        [ApiReversible]
        public Dictionary<int, List<Role>> RolesByUsers(int[] userIdList)
        {
            try
            {
                if (userIdList == null || userIdList.Length == 0) return new Dictionary<int, List<Role>>();

                using (var db = new CoreContext())
                {
                    var query = from roleJoin in db.RoleUser
                                join role in db.Role on roleJoin.IdRole equals role.IdRole
                                select new { roleJoin.IdUser, role };
                    query = query.In(userIdList, x => x.IdUser);

                    return query.ToList().GroupBy(x => x.IdUser).ToDictionary(x => x.Key, x => x.Select(y => y.role).ToList());
                }
            }
            catch (Exception ex)
            {
                Debug.Logs($"rolesByUser: {userIdList}; {ex.Message}");
                this.RegisterEvent(EventType.Error, "Ошибка получения списка ролей, назначенных пользователям.", $"Идентификаторы пользователей: {(userIdList?.Any() == true ? "не задано" : string.Join(", ", userIdList))}", ex);
                throw;
            }
        }

        /// <summary>
        /// Устанавливает новый список пользователей <paramref name="userIdList"/>, обладающих указанной ролью <paramref name="idRole"/>. С пользователей, не включенных в <paramref name="userIdList"/>, либо, если <paramref name="userIdList"/> пуст или равен null, то со всех пользователей, данная роль снимается.
        /// </summary>
        [ApiReversible]
        public NotFound SetRoleUsers(int idRole, IEnumerable<int> userIdList)
        {
            try
            {
                using (var db = new CoreContext())
                using (var scope = db.CreateScope(TransactionScopeOption.Required))
                {
                    if (db.Role.Where(x => x.IdRole == idRole).Count() == 0) return NotFound.NotFound;

                    var userIdList2 = userIdList?.Distinct()?.ToArray();
                    if (userIdList2?.Any() == true)
                    {
                        var query = db.RoleUser.Where(x => x.IdRole == idRole).NotIn(userIdList2, x => x.IdUser);
                        db.RoleUser.RemoveRange(query);

                        var context = AppCore.GetUserContextManager().GetCurrentUserContext();
                        var IdUserChange = context.IdUser;

                        var usersInRole = db.RoleUser.Where(x => x.IdRole == idRole).Select(x => x.IdUser).ToList();
                        userIdList2.Where(x => !usersInRole.Contains(x)).ToList().ForEach(IdUser =>
                        {
                            db.RoleUser.Add(new RoleUser()
                            {
                                IdRole = idRole,
                                IdUser = IdUser,
                                IdUserChange = IdUserChange,
                                DateChange = DateTime.Now.Timestamp()
                            });
                        });
                    }
                    else
                    {
                        db.RoleUser.RemoveRange(db.RoleUser.Where(x => x.IdRole == idRole));
                    }

                    db.SaveChanges();
                    scope.Complete();
                }

                return NotFound.Success;
            }
            catch (Exception ex)
            {
                this.RegisterEvent(EventType.Error, "Ошибка при замене пользователей роли.", $"Идентификатор роли: {idRole}\r\nИдентификаторы пользователей: {(userIdList?.Any() == true ? "не задано" : string.Join(", ", userIdList))}", ex);
                return NotFound.Error;
            }
        }

        /// <summary>
        /// Удаляет роль <paramref name="idRole"/> у пользователей из списка <paramref name="userIdList"/>. Если <paramref name="userIdList"/> пуст или равен null, то роль снимается со всех пользователей.
        /// </summary>
        [ApiReversible]
        public NotFound RemoveRoleUsers(int idRole, IEnumerable<int> userIdList)
        {
            try
            {
                using (var db = new CoreContext())
                using (var scope = db.CreateScope(TransactionScopeOption.Required))
                {
                    if (db.Role.Where(x => x.IdRole == idRole).Count() == 0) return NotFound.NotFound;

                    var userIdList2 = userIdList?.Distinct()?.ToArray();

                    if (userIdList2?.Any() == true)
                    {
                        var query = db.RoleUser.Where(x => x.IdRole == idRole).In(userIdList2, x => x.IdUser);
                        db.RoleUser.RemoveRange(query);
                    }
                    else
                    {
                        db.RoleUser.RemoveRange(db.RoleUser.Where(x => x.IdRole == idRole));
                    }

                    db.SaveChanges();
                    scope.Complete();
                }

                return NotFound.Success;
            }
            catch (Exception ex)
            {
                this.RegisterEvent(EventType.Error, "Ошибка при удалении роли у пользователей.", $"Идентификатор роли: {idRole}\r\nИдентификаторы пользователей: {(userIdList?.Any() == true ? "не задано" : string.Join(", ", userIdList))}", ex);
                return NotFound.Error;
            }
        }

        /// <summary>
        /// Добавляет роль <paramref name="idRole"/> пользователям из списка <paramref name="userIdList"/>.
        /// </summary>
        [ApiReversible]
        public NotFound AddRoleUsers(int idRole, IEnumerable<int> userIdList)
        {
            try
            {
                using (var db = new CoreContext())
                using (var scope = db.CreateScope(TransactionScopeOption.Required))
                {
                    if (db.Role.Where(x => x.IdRole == idRole).Count() == 0) return NotFound.NotFound;

                    var context = AppCore.GetUserContextManager().GetCurrentUserContext();
                    var IdUserChange = context.IdUser;

                    var userIdList2 = userIdList.Distinct().ToArray();

                    var query = db.Users.In(userIdList2, x => x.IdUser);
                    var usersToRole = query.ToList().Select(x => new RoleUser()
                    {
                        IdRole = idRole,
                        IdUser = x.IdUser,
                        IdUserChange = IdUserChange,
                        DateChange = DateTime.Now.Timestamp()
                    }).ToList();
                    if (usersToRole.Count > 0)
                    {
                        db.RoleUser.
                            UpsertRange(usersToRole).
                            On(x => new { x.IdUser, x.IdRole }).
                            WhenMatched((xDb, xIns) => new RoleUser()
                            {
                                IdUserChange = xIns.IdUserChange,
                                DateChange = xIns.DateChange
                            }).
                            Run();
                    }

                    db.SaveChanges();
                    scope.Complete();
                }

                return NotFound.Success;
            }
            catch (Exception ex)
            {
                this.RegisterEvent(EventType.Error, "Ошибка при регистрации роли для списка пользователей.", $"Идентификатор роли: {idRole}\r\nИдентификаторы пользователей: {(userIdList?.Any() == true ? "не задано" : string.Join(", ", userIdList))}", ex);
                return NotFound.Error;
            }
        }

#pragma warning disable CS1591 // todo внести комментарии.
        [ApiReversible]
        public bool getUsers(Dictionary<int, User> users)
        {
            try
            {
                if (users == null || !users.Any()) return true;

                var listIDForRequest = new List<int>();

                foreach (var pair in users)
                    if (pair.Key > 0 && pair.Value == null)
                        if (!listIDForRequest.Contains(pair.Key))
                            listIDForRequest.Add(pair.Key);

                if (listIDForRequest.Count > 0)
                {
                    using (var db = new CoreContext())
                    {
                        var query = db.Users.In(listIDForRequest, x => x.IdUser);
                        foreach (var res in query) users[res.IdUser] = res;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("user: {0}; ", ex.Message);
                this.RegisterEvent(EventType.Error, "Ошибка при получении данных пользователей.", $"Идентификаторы пользователей: {(users?.Any() == true ? "не задано" : string.Join(", ", users.Keys))}", ex);
                throw ex;
            }
        }
    }
}
