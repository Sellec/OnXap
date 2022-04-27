using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace OnXap.Modules.Universal.Pagination
{
    using Core.Modules;
    using Exceptions;

    public abstract class PaginationHelper<TController, TItemSource, TItemView>
        where TController : ModuleControllerBase
        where TItemSource : class
        where TItemView : class
    {
        //private Lazy<MethodInfo> _mGetPaginatedItemsListOptions = new Lazy<MethodInfo>(() => typeof(PaginationHelper<,,,>).GetMethod(nameof(GetPaginatedItemsListOptions), BindingFlags.Instance | BindingFlags.NonPublic));
        private Func<TItemSource, TItemView> _itemSourceToViewConverter = null;

        public PaginationHelper(TController controller, Func<TItemSource, TItemView> itemSourceToViewConverter = null)
        {
            Controller = controller;
            _itemSourceToViewConverter = itemSourceToViewConverter;
        }

        private Func<TItemSource, TItemView> GetItemSourceToViewConverter()
        {
            if (_itemSourceToViewConverter == null)
            {
                if (typeof(TItemSource) != typeof(TItemView))
                    throw new ArgumentNullException(nameof(_itemSourceToViewConverter), "Может не указываться только в том случае, когда {TItemSource} равен {TItemVeiew}.");
                else
                    _itemSourceToViewConverter = (s) => s as TItemView;
            }

            return _itemSourceToViewConverter;
        }

        /// <summary>
        /// Вызывается внутри <see cref="ViewPaginatedItemsList{TModel}(TModel, IQueryable{TItemSource}, ListViewOptions, int?)"/>. 
        /// Должен возвращать объект типа <see cref="ListViewOptions"/> (или наследующий тип). 
        /// В случае возврата значения null генерируется исключение <see cref="NullReferenceException"/>.
        /// </summary>
        protected virtual ListViewOptions GetItemsListOptions<TModel>(TModel model) where TModel : class
        {
            return new ListViewOptions();
        }

        /// <summary>
        /// Обрабатывает данные из запроса <paramref name="queryBase"/>, проводит ряд преобразований (сортировка, группировка и т.п.)
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="model"></param>
        /// <param name="queryBase"></param>
        /// <param name="listViewOptions"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public ActionResult ViewPaginatedItemsList<TModel>(
            TModel model,
            IQueryable<TItemSource> queryBase,
            ListViewOptions listViewOptions = null,
            int? pageIndex = null
        )
            where TModel : class
        {
            if (listViewOptions == null)
            {
                //listViewOptions = (ListViewOptions)_mGetPaginatedItemsListOptions.Value.MakeGenericMethod(typeof(TModel)).Invoke(this, new object[] { model });
                listViewOptions = GetItemsListOptions(model);
                if (listViewOptions == null) throw new NullReferenceException("Метод 'UniversalController{TModule, TContext, TItemSource, TItemView}.GetPaginatedItemsListOptions' не должен возвращать null.");
            }

            var itemsList = GetSortedList(queryBase, out PagedView pages, out InfoCount infoCount, listViewOptions, pageIndex ?? 0);
            if (!pages.PageFound) return Controller.ErrorHandled(new ErrorCodeException(HttpStatusCode.NotFound, "Нет такой страницы."));

            return ExecuteView(model, itemsList, listViewOptions, pages, infoCount);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryBase"></param>
        /// <param name="pages"></param>
        /// <param name="infoCount"></param>
        /// <param name="listViewOptions"></param>
        /// <param name="pageIndex">Если не задан, то отображается весь список объектов</param>
        /// <returns></returns>
        public virtual List<TItemView> GetSortedList(
            IQueryable<TItemSource> queryBase,
            out PagedView pages,
            out InfoCount infoCount,
            ListViewOptions listViewOptions,
            int? pageIndex = null)
        {
            pages = null;
            infoCount = null;

            var startPosition = 0;

            var itemsCount = queryBase.Count();

            if (pageIndex.HasValue)
            {
                /*
                 * Рассчитываем страницы
                 * */
                pages = new PagedView();

                //Выбираем из настроек или из listViewOptions количество объектов на одной странице.
                var itemsPerPage = listViewOptions != null && listViewOptions.ItemsPerPage != 0 ? listViewOptions.ItemsPerPage : 10;

                //Считаем текущую страницу. Первая страница начинается с 1.
                var currentPage = Math.Max(1, pageIndex.Value);

                //Считаем стартовую позицию в списке объектов.
                startPosition = (currentPage - 1) * itemsPerPage;

                //Считаем количество страниц.
                var countPages = itemsCount == 0 ? 0 : Math.Max(1, (int)Math.Ceiling(1.0 * itemsCount / itemsPerPage));

                if (startPosition >= itemsCount)
                {
                    if (itemsCount == 0 && currentPage == 1)
                    {
                    }
                    else
                    {
                        pages.PageFound = false;
                        return null;
                    }
                }

                var stpages = new Dictionary<int, int>();
                var currentPage2 = currentPage - 1;

                if (currentPage2 > 3)
                    for (int i = currentPage2 - 1; i <= currentPage2; i++)
                        stpages[i] = i;
                else
                    for (int i = 1; i <= currentPage2; i++)
                        stpages[i] = i;

                var fnpages = new Dictionary<int, int>();
                if (currentPage2 < (countPages - 3))
                    for (int i = currentPage2 + 2; i <= currentPage2 + 3; i++)
                        fnpages[i] = i;
                else
                    for (int i = currentPage2 + 2; i <= countPages; i++)
                        fnpages[i] = i;

                pages.PageCount = countPages;
                pages.PageIndex = currentPage;
                pages.stpg = stpages;
                pages.fnpg = fnpages;
                pages.np = countPages - 3;

                infoCount = new InfoCount()
                {
                    ItemsCount = itemsCount,
                    ItemPositionStart = startPosition + 1,
                    ItemPositionEnd = currentPage * itemsPerPage,
                    ItemsPerPageTheory = itemsPerPage,
                    ItemsPerPage = Math.Min(itemsCount - (currentPage - 1) * itemsPerPage, itemsPerPage)
                };
            }
            else
            {
                infoCount = new InfoCount()
                {
                    ItemsCount = itemsCount,
                    ItemPositionStart = 1,
                    ItemPositionEnd = 0,
                    ItemsPerPageTheory = itemsCount,
                    ItemsPerPage = itemsCount
                };
            }

            if (itemsCount > 0)
            {
                var querySorted = listViewOptions.BuildSortedQuery(queryBase);

                var list = querySorted.
                    Skip(infoCount.ItemPositionStart - 1).
                    Take(infoCount.ItemsPerPage).
                    ToList().
                    Select(x => GetItemSourceToViewConverter()(x.SourceRow)).ToList();

                return list;
            }
            else
            {
                return new List<TItemView>();
            }
        }

        protected abstract ActionResult ExecuteView<TModel>(
            TModel model,
            List<TItemView> objectsToView,
            ListViewOptions listViewOptions,
            PagedView pages,
            InfoCount infoCount
        ) where TModel : class;

        protected TController Controller { get; private set; }
    }

}