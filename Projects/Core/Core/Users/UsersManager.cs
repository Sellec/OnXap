using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace OnXap.Users
{
    using Core;
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
                if (roleIdList == null || roleIdList.Length == 0) return new Dictionary<User, int[]>();

                if (orderBy != null) throw new ArgumentException("Параметр не поддерживается.", nameof(orderBy));

                using (var db = new CoreContext())
                {
                    var queryBase = db.Users.AsQueryable();

                    if (onlyActive) queryBase = queryBase.Where(x => x.State == 0);

                    var idRoleUser = AppCore.AppConfig.RoleUser;
                    if (!roleIdList.Contains(idRoleUser))
                    {
                        var queryRolesWithUsers = exceptSuperuser ? (from user in queryBase
                                                                     join role in db.RoleUser on user.IdUser equals role.IdUser
                                                                     where roleIdList.Contains(role.IdRole)
                                                                     select new { role.IdUser, role.IdRole }) : (from user in queryBase
                                                                                                                 join role in db.RoleUser on user.IdUser equals role.IdUser
                                                                                                                 where roleIdList.Contains(role.IdRole) || user.Superuser != 0
                                                                                                                 select new { role.IdUser, role.IdRole });

                        var data = queryRolesWithUsers.ToList().GroupBy(x => x.IdUser, x => x.IdRole).ToDictionary(x => x.Key, x => x.Distinct().ToArray());

                        var queryUsers = from user in db.Users
                                         where data.Keys.Contains(user.IdUser)
                                         select user;

                        var data2 = queryUsers.ToList().ToDictionary(x => x, x => data[x.IdUser]);
                        return data2;
                    }
                    else
                    {
                        return queryBase.ToDictionary(x => x, x => new int[] { idRoleUser });
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

                    if (userIdList.Length == 1)
                    {
                        query = query.Where(x => x.IdUser == userIdList[0]);
                    }
                    else
                    {
                        query = query.Where(x => userIdList.Contains(x.IdUser));
                    }

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
                        db.RoleUser.RemoveRange(db.RoleUser.Where(x => x.IdRole == idRole && !userIdList2.Contains(x.IdUser)));

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
                        db.RoleUser.RemoveRange(db.RoleUser.Where(x => x.IdRole == idRole && userIdList2.Contains(x.IdUser)));
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

                    //test upsert
                    System.Diagnostics.Debugger.Break();

                    var usersToRole = db.Users.Where(x => userIdList2.Contains(x.IdUser)).ToList().Select(x => new RoleUser()
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
                        var sql = (from p in db.Users where listIDForRequest.Contains(p.IdUser) select p);
                        foreach (var res in sql) users[res.IdUser] = res;
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
