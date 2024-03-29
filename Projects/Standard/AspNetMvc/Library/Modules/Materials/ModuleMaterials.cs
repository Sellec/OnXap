﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace OnXap.Modules.Materials
{
    using Core.Items;
    using Core.Modules;

    [ModuleCore("Контент", DefaultUrlName = "Content")]
    public class ModuleMaterials : ModuleCore<ModuleMaterials>
    {
        public override IReadOnlyList<KeyValuePair<ItemBase, Uri>> GenerateLinks(IEnumerable<ItemBase> items)
        {
            var news = items.Where(x => x is DB.News).ToDictionary(x => x, x => new Uri("/" + UrlName + "/news/" + x.IdBase, UriKind.Relative));
            var pages = items.Where(x => x is DB.Page).ToDictionary(x => x, x => new Uri("/" + UrlName + "/page/" + x.IdBase, UriKind.Relative));

            return news.Union(pages).ToList();
        }

        public IList<DB.Page> getPagesList()
        {
            try
            {
                using (var db = new DB.DataLayerContext())
                {
                    return (from p in db.Pages
                            where p.status > 0
                            orderby p.name ascending
                            select p).ToList();
                }
            }
            catch (Exception ex)
            {
                Debug.Logs(ex.Message);
                return null;
            }
        }

        public DB.Page getPageByID(int IdPage)
        {
            try
            {
                using (var db = new DB.DataLayerContext())
                {
                    return (from p in db.Pages where p.id == IdPage select p).FirstOrDefault();
                }
            }
            catch(Exception ex)
            {
                Debug.Logs(ex.Message);
                return null;
            }
        }

        public override Uri GenerateLink(ItemBase item)
        {
            if (item.OwnerModule == this && item is DB.Page page) return new Uri(string.Format("/{0}", page.urlname), UriKind.Relative);
            return null;
        }
    }
}
