﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace OnXap.Modules.Materials
{
    using AdminForModules.Menu;
    using Core.Items;
    using Core.Modules;
    using Journaling;
    using Modules.Routing;

    /// <summary>
    /// Представляет контроллер для панели управления.
    /// </summary>
    public class ModuleMaterialsAdminController : ModuleControllerAdmin<ModuleMaterials>
    {
        [MenuAction("Новости")]
        public ActionResult News()
        {
            using (var db = new DB.DataLayerContext())
            {
                var showDeleted = Request.Form.GetValues("ShowDeleted")?.Contains("true") ?? false;
                var query = db.News.AsQueryable();
                if (!showDeleted) query = query.Where(x => !x.Block);

                var model = query.OrderByDescending(x => x.date).ToList();

                return View("Admin/NewsList.cshtml", model);
            }
        }

        public ActionResult NewsEdit(int? IdNews = null)
        {
            var success = false;
            var result = "";

            try
            {
                DB.News data = null;
                if (!IdNews.HasValue || IdNews.Value <= 0) data = new DB.News();
                else
                {
                    using (var db = new DB.DataLayerContext())
                    {
                        data = db.News.Where(x => x.id == IdNews.Value).FirstOrDefault();
                    }
                    if (data == null) throw new Exception("Указанная новость не найдена.");

                    if (data.Block)
                    {
                        if (AppCore.GetUserContextManager().GetCurrentUserContext().IsSuperuser) throw new Exception("Указанная новость удалена (сообщение для суперпользователя).");
                        else throw new Exception("Указанная новость не найдена.");
                    }
                }

                return View("Admin/NewsEdit.cshtml", data);
            }
            catch (Exception ex)
            {
                success = false;
                result = ex.Message;
            }

            return ReturnJson(success, result);
        }

        public JsonResult NewsSave(Models.NewsSave model = null)
        {
            var answer = JsonAnswer<int>();

            try
            {
                if (ModelState.IsValid)
                {
                    using (var db = new DB.DataLayerContext())
                    {
                        DB.News data = null;
                        if (model.IdMaterial <= 0)
                        {
                            data = new DB.News() { date = DateTime.Now, user = AppCore.GetUserContextManager().GetCurrentUserContext().IdUser, status = true, Block = false };
                            db.News.Add(data);
                        }
                        else
                        {
                            data = db.News.Where(x => x.id == model.IdMaterial).FirstOrDefault();
                            if (data == null) throw new Exception("Указанная новость не найдена.");

                            if (data.Block)
                            {
                                if (AppCore.GetUserContextManager().GetCurrentUserContext().IsSuperuser) throw new Exception("Указанная новость удалена (сообщение для суперпользователя).");
                                else throw new Exception("Указанная новость не найдена.");
                            }
                        }

                        data.name = model.NameMaterial;
                        data.text = model.BodyFull;
                        data.short_text = model.BodyShort;

                        db.SaveChanges();

                        answer.Data = data.id;

                        var result = AppCore.Get<UrlManager>().Register(
                            Module,
                            data.id,
                            ItemTypeFactory.GetItemType(typeof(DB.News)).IdItemType,
                            nameof(ModuleController.ViewNews),
                            new List<ActionArgument>() { new ActionArgument() { ArgumentName = "IdNews", ArgumentValue = data.id } },
                            "news/" + UrlManager.Translate(data.name),
                            RoutingConstants.MAINKEY
                        );
                        if (!result.IsSuccess) throw new Exception(result.Message);

                        answer.FromSuccess("Новость сохранена");
                    }
                }
            }
            catch (Exception ex)
            {
                answer.FromException(ex);
                Module.RegisterEvent(EventType.Error, "Ошибка сохранения новости", "Модель данных, переданная из формы:\r\n" + Newtonsoft.Json.JsonConvert.SerializeObject(model), null, ex);
            }

            return ReturnJson(answer);
        }

        public ActionResult NewsDelete(int? IdNews = null)
        {
            var success = false;
            var result = "";

            try
            {
                if (!IdNews.HasValue || IdNews.Value <= 0) throw new Exception("Не указан номер новости.");

                using (var db = new DB.DataLayerContext())
                {
                    var data = db.News.Where(x => x.id == IdNews.Value).FirstOrDefault();
                    if (data == null) throw new Exception("Указанная новость не найдена.");

                    if (data.Block)
                    {
                        if (AppCore.GetUserContextManager().GetCurrentUserContext().IsSuperuser) throw new Exception("Указанная новость удалена (сообщение для суперпользователя).");
                        else throw new Exception("Указанная новость не найдена.");
                    }

                    data.Block = true;
                    db.SaveChanges();
                }

                success = true;
                //result = "Меню было успешно удалено.";
            }
            catch (Exception ex)
            {
                success = false;
                result = ex.Message;
            }

            return ReturnJson(success, result);
        }
    }
}
