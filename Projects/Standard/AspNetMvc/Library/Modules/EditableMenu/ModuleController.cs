using System;
using System.Linq;
using System.Web.Mvc;

namespace OnXap.Modules.EditableMenu
{
    using AdminForModules.Menu;
    using Core.Modules;
    using Modules.Routing;

    public class ModuleController : ModuleControllerAdmin<Module>
    {
        [MenuAction("Редактируемое меню", "editablemenu", Module.PERM_EDITABLEMENU)]
        public ActionResult EditableMenu()
        {
            using (var db = new Db.DataContext())
            {
                var model = db.Menu.OrderBy(x => x.id).ToList();
                return View("EditableMenu.cshtml", model);
            }
        }

        [ModuleAction("editablemenu_edit", Module.PERM_EDITABLEMENU)]
        public ActionResult EditableMenuEdit(int IdMenu = 0)
        {
            using (var db = new Db.DataContext())
            {
                var menu = IdMenu == 0 ? new Db.Menu() : db.Menu.Where(x => x.id == IdMenu).FirstOrDefault();
                if (menu == null) throw new Exception(string.Format("Меню с id={0} не найдено.", IdMenu));

                return View("EditableMenuEdit.cshtml", new Design.Model.EditableMenu()
                {
                    Menu = menu,
                    Modules = AppCore.GetModulesManager().GetModules().OfType<IModuleCore>().OrderBy(x => x.Caption).ToList()
                });
            }
        }

        [ModuleAction("editablemenu_getmenu", Module.PERM_EDITABLEMENU)]
        public JsonResult EditableMenuLinks(int? idModule = 0)
        {
            var success = false;
            var result = "";
            object data = null;

            try
            {
                if (!idModule.HasValue || idModule == 0) throw new Exception("Не указан модуль, для которого необходимо получить список ссылок.");

                var module = (IModuleCore)AppCore.GetModulesManager().GetModule(idModule.Value);
                if (module == null) throw new Exception("Не удалось найти указанный модуль.");

                var itemsAll = module.GetItemTypes().
                                SelectMany(x => module.GetItems(x.IdItemType)).
                                OfType<IItemRouted>().
                                OrderBy(x => x.CaptionBase).
                                ToDictionary(x => x.Url.ToString(), x => x.CaptionBase);


                data = itemsAll;

                success = true;
            }
            catch (Exception ex)
            {
                success = false;
                result = ex.Message;
            }

            return ReturnJson(success, result, data);
        }

        [ModuleAction("editablemenu_edit_save", Module.PERM_EDITABLEMENU)]
        public JsonResult EditableMenuSave(Db.Menu menu)
        {
            var success = false;
            var result = "";

            try
            {
                using (var db = new Db.DataContext())
                {
                    if (menu == null) throw new Exception("Не удалось получить данные из формы.");

                    var dbmenu = menu.id > 0 ? db.Menu.Where(x => x.id == menu.id).FirstOrDefault() : menu;
                    if (dbmenu == null) throw new Exception(string.Format("Не удалось найти меню с id={0}", menu.id));

                    if (string.IsNullOrEmpty(menu.name)) throw new Exception("Название меню не может быть пустым.");

                    if (dbmenu.id == 0) db.Menu.Add(menu);
                    else
                    {
                        dbmenu.name = menu.name;
                        dbmenu.code = menu.code;
                    }

                    db.SaveChanges();
                }
                success = true;
                result = "Изменения в свойствах меню успешно сохранены.";
            }
            catch (Exception ex)
            {
                success = false;
                result = ex.Message;
            }

            return ReturnJson(success, result);
        }

        [ModuleAction("editablemenu_delete", Module.PERM_EDITABLEMENU)]
        public ActionResult EditableMenuDelete(int IdMenu = 0)
        {
            var success = false;
            var result = "";

            try
            {
                if (IdMenu == 0) throw new Exception("Следует указать номер существующего меню.");
                using (var db = new Db.DataContext())
                {
                    var menu = db.Menu.Where(x => x.id == IdMenu).FirstOrDefault();
                    if (menu == null) throw new Exception(string.Format("Меню с id={0} не найдено.", IdMenu));

                    db.Menu.Remove(menu);
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