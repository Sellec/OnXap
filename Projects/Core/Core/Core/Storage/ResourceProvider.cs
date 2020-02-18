using OnUtils.Architecture.AppCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OnXap.Core.Storage
{
    /// <summary>
    /// Провайдер ресурсов движка.
    /// Работает в качестве RazorViewEngine для поиска представлений и в качестве RouteHandler для поиска файлов.
    /// Вся функциональность объединена в одном классе.
    /// </summary>
    public abstract class ResourceProvider : CoreComponentBase, IComponentSingleton, IAutoStart
    {
        private List<string> _listSourcePaths = new List<string>();
        private Dictionary<string, string> _resourceDirectoryPaths = new Dictionary<string, string>();

        #region CoreComponentBase
        /// <summary>
        /// </summary>
        protected sealed override void OnStarting()
        {
            Start();
        }

        /// <summary>
        /// </summary>
        protected sealed override void OnStop()
        {
        }
        #endregion

        #region Методы
        private void Start()
        {
            var physicalApplicationPath = AppCore.ApplicationWorkingFolder;

            /*
             * Формируем корневые пути для поиска файлов - представлений и ресурсов.
             * Во время поиска файлов в качестве базовых расположений будут использоваться <see cref="_listSourcePaths"/> и <see cref="SourceDevelopmentPathList"/>. См. описание.
             * Для разработки это папки библиотек и папка приложения (в последнюю очередь), для релиза - только папка приложения.
             * */
            _listSourcePaths.Add(Path.GetFullPath(Path.Combine(physicalApplicationPath,
                "")));
            _listSourcePaths.Add(Path.GetFullPath(Path.Combine(physicalApplicationPath,
                "bin")));

            /*
             * Пути поиска ресурсов. Каждый путь смешивается с каждым корневым путем при поиске.
             * */
            AddResourceDir("Modules/Module{module}/ViewsModified/");
            AddResourceDir("Modules/Module{module}/Views/");
            AddResourceDir("Modules/{module}/ViewsModified/");
            AddResourceDir("Modules/{module}/Views/");

            /*
             * Устаревшие пути
             * */
            AddResourceDir("Modules/Module{module}/DesignModified/");
            AddResourceDir("Modules/Module{module}/Design/");
            AddResourceDir("Modules/{module}/DesignModified/");
            AddResourceDir("Modules/{module}/Design/");

            AddResourceDir("Modules/Design/");
            AddResourceDir("Modules/");

            AddResourceDir("Plugins/Module{module}/DesignModified/");
            AddResourceDir("Plugins/Module{module}/Design/");
            AddResourceDir("Plugins/{module}/DesignModified/");
            AddResourceDir("Plugins/{module}/Design/");
            AddResourceDir("Plugins/Design/");
            AddResourceDir("Plugins/");

            AddResourceDir("data/temp_modified/");

            AddResourceDir("data/{theme}/{resource}/{module}/{language}");
            AddResourceDir("data/{theme}/{resource}/{module}");

            AddResourceDir("data/{theme}/{resource}/{language}");
            AddResourceDir("data/{theme}/{resource}/");

            AddResourceDir("data/{resource}/{module}/{language}");
            AddResourceDir("data/{resource}/{module}");

            AddResourceDir("data/{resource}/{language}");
            AddResourceDir("data/{resource}/");

            AddResourceDir("");

            OnStartProvider();
        }

        /// <summary>
        /// Выполняется при запуске.
        /// </summary>
        protected virtual void OnStartProvider()
        {

        }
        #endregion

        #region Поиск файлов
        private void AddResourceDir(string path, string name = null)
        {
            if (string.IsNullOrEmpty(name)) name = Guid.NewGuid().ToString();
            if (!_resourceDirectoryPaths.ContainsKey(name)) _resourceDirectoryPaths.Add(name, "");

            _resourceDirectoryPaths[name] = path.Replace("//", "/").Replace("\\\\", "\\").Replace("/", "\\");
        }

        public string GetFilePath(string moduleName, string virtualPath, bool translateIntoVirtual, out IEnumerable<string> searchLocations)
        {
            searchLocations = null;
            if (string.IsNullOrEmpty(virtualPath)) return null;

            var resourceType = "";
            if (virtualPath.Contains(".cshtml")) resourceType = "temp";
            if (virtualPath.Contains(".css")) resourceType = "css";
            if (virtualPath.Contains(".js")) resourceType = "js";

            /*
             * Параметры для пути поиска представлений
             * */
            var themeName = "";
            var languageName = "";

            //*
            // * todo themeName
            // * */
            //dynamic theme = ThemeManager.getActive();
            //if (theme != null) themeName = theme.FolderName;

            foreach (var path in _resourceDirectoryPaths.Values)
            {
                foreach (var sourcePath in SourceDevelopmentPathList)
                {
                    var pathPrepared = path.Replace("/", "\\").
                        Replace("{theme}", themeName).
                        Replace("{module}", moduleName).
                        Replace("{language}", languageName).
                        Replace("{resource}", resourceType);

                    var fullPath = Path.GetFullPath(Path.Combine(sourcePath, pathPrepared, virtualPath.Replace("/", "\\").Trim('\\', '~')));

                    if (File.Exists(fullPath))
                    {
                        var relPath = translateIntoVirtual ? TranslateFullPathTo(fullPath) : fullPath;
                        //Debug.WriteLine("{0}: {1}", virtualPath, relPath);

                        return relPath;
                    }
                }

                foreach (var sourcePath in _listSourcePaths)
                {
                    var pathPrepared = path.Replace("/", "\\").
                        Replace("{theme}", themeName).
                        Replace("{module}", moduleName).
                        Replace("{language}", languageName).
                        Replace("{resource}", resourceType);

                    var fullPath = Path.GetFullPath(Path.Combine(sourcePath, pathPrepared, virtualPath.Replace("/", "\\").Trim('\\', '~')));

                    if (File.Exists(fullPath))
                    {
                        var relPath = translateIntoVirtual ? TranslateFullPathTo(fullPath) : fullPath;
                        //Debug.WriteLine("{0}: {1}", virtualPath, relPath);

                        return relPath;
                    }
                }
            }

            //Debug.WriteLineNoLog("Resource not found {0}", virtualPath);

            var pathCombinations = _resourceDirectoryPaths.Values.
                                    SelectMany(path => _listSourcePaths.
                                                    Select(sourcePath => Path.GetFullPath(Path.Combine(sourcePath, path.Replace("/", "\\").
                        Replace("{theme}", themeName).
                        Replace("{module}", moduleName).
                        Replace("{language}", languageName).
                        Replace("{resource}", resourceType), virtualPath.Replace("/", "\\").Trim('\\', '~'))))).ToList();

            searchLocations = pathCombinations;

            //throw new ArgumentException("Указанный путь не найден", nameof(virtualPath));
            return null;
        }

        protected string TranslateFullPathTo(string fullPath)
        {
            Uri path1 = new Uri(AppCore.ApplicationWorkingFolder);
            Uri diff = path1.MakeRelativeUri(new Uri(fullPath));
            string relPath = diff.ToString();//.OriginalString;

            /*
             * Если это машина разработчика и представление найдено в папке уровнем выше, чем каталог приложения, то создаем жесткую ссылку.
             * */
            if (relPath.StartsWith("../") && SourceDevelopmentPathList.Count > 0)
            {
                try
                {
                    var symlinks = Path.Combine(AppCore.ApplicationWorkingFolder, "Symlinks");
                    if (!Directory.Exists(symlinks)) Directory.CreateDirectory(symlinks);

                    int top = 0;
                    var tmpPath = relPath;
                    string tmpPathRel = "";
                    while (tmpPath.StartsWith("../"))
                    {
                        tmpPath = tmpPath.Substring(3);
                        tmpPathRel += "../";
                        top++;
                    }

                    var name = string.Format("SymlinkLevel_{0}", top);
                    var symlink = Path.Combine(symlinks, name);

                    if (!jeff_brown_Manipulating_NTFS_Junction_Points_in_NET.JunctionPoint.Exists(symlink))
                    {
                        var relp = Path.Combine(AppCore.ApplicationWorkingFolder, tmpPathRel.Replace("/", "\\"));
                        jeff_brown_Manipulating_NTFS_Junction_Points_in_NET.JunctionPoint.Create(symlink, relp, true);
                    }

                    relPath = Path.Combine("Symlinks", name, tmpPath).Replace("\\", "/");
                }
                catch (Exception ex)
                {
                    throw new NotSupportedException("Нет возможности управлять junctions для девелоперской машины. Причина: " + ex.Message);
                }
            }

            return "~/" + relPath;
        }
        #endregion

        #region Свойства
        /// <summary>
        /// Основные расположения (папки) файлов, которые используются для поиска ресурсов. 
        /// Заполняются автоматически - это основная рабочая папка приложения и папка bin в рабочей папке.
        /// </summary>
        public IReadOnlyCollection<string> SourcePathList
        {
            get => _listSourcePaths.AsReadOnly();
        }

        /// <summary>
        /// Дополнительные настраиваемые расположения (папки) файлов, которые используются для поиска ресурсов. 
        /// 
        /// Могут использоваться, например, на этапе разработки. По-умолчанию движок ASP.NET ищет файлы в папке веб-приложения и не может выйти за её пределы.
        /// Можно указать папку дочернего проекта с ресурсами (представления, изображения, css и т.д.), расположенного где угодно (даже на другом диске).
        /// На самом деле, во время компиляции и запуска приложения, все ресурсы дочернего проекта будут скопированы в папку bin основного приложения и будут найдены при поиске в этом расположении,
        /// но есть большое неудобство в том, что удобнее редактировать представления, css- и js-файлы в VisualStudio, открывая их напрямую в дочернем проекте. Это позволило бы например, сохранять изменения в системе контроля версий, не потребовалось бы вручную открывать ресурс в папке "bin/" приложения и т.п.
        /// 
        /// Механизм работает таким образом, что в папке основного проекта создается папка "Symlinks", внутри которой, используя механизм "Точка соединения NTFS" (или Junction Point), создаются трансляции путей на основе <see cref="SourceDevelopmentPathList"/>. 
        /// Далее, во время поиска файлов, путь к файлу, найденному в расположении, будет транслирован через "Symlinks" и ASP.NET сможет его открыть.
        /// </summary>
        public List<string> SourceDevelopmentPathList { get; } = new List<string>();
        #endregion
    }
}

