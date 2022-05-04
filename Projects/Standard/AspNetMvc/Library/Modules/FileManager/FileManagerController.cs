using OnUtils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;

namespace OnXap.Modules.FileManager
{
    using Core.Modules;

    public class FileManagerController : ModuleControllerUser<FileManager>
    {
        public override ActionResult Index()
        {
            return Content("");
        }

        [HttpPost]
        public ActionResult UploadFile(Guid? uniqueKey = null)
        {
            var result = JsonAnswer<int>();

            try
            {
                var hpf = HttpContext.Request.Files["file"] as HttpPostedFileBase;
                if (hpf == null) throw new ArgumentNullException("Не найден загружаемый файл.");

                var rootDirectory = System.Web.Hosting.HostingEnvironment.MapPath("/");
                var filePathRelative = Path.Combine("data/filesClosed/", Guid.NewGuid().ToString() + Path.GetExtension(hpf.FileName));
                var filePath = Path.Combine(rootDirectory, filePathRelative);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                hpf.SaveAs(filePath);
                switch (Module.Register(out var file, hpf.FileName, filePathRelative, uniqueKey, DateTime.Now.AddDays(1)))
                {
                    case RegisterResult.Error:
                    case RegisterResult.NotFound:
                        result.FromFail("Не удалось зарегистрировать переданный файл.");
                        break;

                    case RegisterResult.Success:
                        result.Data = file.IdFile;
                        result.FromSuccess("");
                        break;
                }
            }
            catch (Exception ex)
            {
                RegisterEventWithCode(HttpStatusCode.InternalServerError, "Ошибка во время регистрации файла", $"uniqueKey='{uniqueKey}'.", ex);
                result.FromFail("Неожиданная ошибка во время регистрации файла.");
            }

            return ReturnJson(result);
        }

        [IgnoreCompression]
        [ModuleAction("file")]
        public FileResult File(int? IdFile = null)
        {
            try
            {
                if (!IdFile.HasValue) throw new ArgumentNullException(nameof(IdFile), "Не указан номер файла.");

                using (var db = new Db.DataContext())
                {
                    var file = db.File.
                        Where(x => x.IdFile == IdFile.Value && !x.IsRemoved && !x.IsRemoving).
                        Select(x => new { x.PathFile, x.NameFile, x.TypeConcrete, x.DateChange }).
                        FirstOrDefault();

                    if (file == null) throw new Exception("Файл не найден.");

                    var rootDirectory = System.Web.Hosting.HostingEnvironment.MapPath("/");
                    var filePath = Path.Combine(rootDirectory, file.PathFile);

                    var mimeType = !string.IsNullOrEmpty(file.TypeConcrete) ? file.TypeConcrete : MediaTypeNames.Application.Octet;
                    Response.Cache.SetCacheability(HttpCacheability.Public);
                    Response.Cache.SetLastModified(file.DateChange.FromTimestamp());
                    return base.File(filePath, mimeType, file.NameFile);
                }
            }
            catch (Exception ex)
            {
                RegisterEventWithCode(HttpStatusCode.InternalServerError, "Ошибка во время вывода файла", $"File.IdFile='{IdFile}'.", ex);
                return null;
            }
        }

        [ModuleAction("image")]
        public ActionResult FileImage(int? IdFile = null, int? MaxWidth = null, int? MaxHeight = null)
        {
            try
            {
                var rootDirectory = System.Web.Hosting.HostingEnvironment.MapPath("/");
                var filePath = string.Empty;
                var fileName = string.Empty;
                DateTime? dbChangeTime = null;
                var mimeType = MediaTypeNames.Application.Octet;

                if (!IdFile.HasValue) filePath = "data/img/files/argumentzero.jpg"; //Не указан номер файла.
                else
                {
                    using (var db = new Db.DataContext())
                    {
                        var file = db.File.
                            Where(x => x.IdFile == IdFile.Value && !x.IsRemoved && !x.IsRemoving).
                            Select(x => new { x.PathFile, x.NameFile, x.TypeConcrete, x.DateChange }).
                            FirstOrDefault();


                        if (file == null) filePath = "data/img/files/notfound.jpg"; //Файл не найден.
                        else
                        {
                            if (!System.IO.File.Exists(Path.Combine(rootDirectory, file.PathFile)) && !System.IO.File.Exists(Path.Combine(rootDirectory, "bin", file.PathFile)))
                            {
                                filePath = "data/img/files/notfound.jpg"; //Файл не найден.
                                if (Module.ExternalFileSourceDomain != null)
                                {
                                    var paramss = string.Join("&", new List<string>() { !MaxWidth.HasValue ? null : "MaxWidth=" + MaxWidth.Value, !MaxHeight.HasValue ? null : "MaxHeight=" + MaxHeight.Value }.
                                    Where(x => !string.IsNullOrEmpty(x)));

                                    var url = Routing.UrlManager.CombineUrlParts(Module.UrlName, nameof(FileImage), IdFile.Value.ToString(), (!string.IsNullOrEmpty(paramss) ? "?" + paramss : ""));
                                    return Redirect(new Uri(Module.ExternalFileSourceDomain, url)?.ToString());
                                }
                            }
                            else
                            {
                                filePath = file.PathFile;
                                fileName = file.NameFile;
                                dbChangeTime = file.DateChange.FromTimestamp();
                                if (!string.IsNullOrEmpty(file.TypeConcrete)) mimeType = file.TypeConcrete;
                            }
                        }
                    }
                }

                string path = null;

                if (System.IO.File.Exists(Path.Combine(rootDirectory, filePath))) path = Path.Combine(rootDirectory, filePath);
                if (System.IO.File.Exists(Path.Combine(rootDirectory, "bin", filePath))) path = Path.Combine(rootDirectory, "bin", filePath);

                if (!string.IsNullOrEmpty(path))
                {
                    var isNeedResize = MaxWidth.HasValue || MaxHeight.HasValue;
                    if (!isNeedResize)
                    {
                        var fileNameFinal = string.IsNullOrEmpty(fileName) ? Path.GetFileName(filePath) : fileName;
                        Response.Headers["Content-Disposition"] = $"inline; filename={fileNameFinal}";
                        Response.Cache.SetCacheability(HttpCacheability.Public);
                        if (dbChangeTime.HasValue) Response.Cache.SetLastModified(dbChangeTime.Value);
                        return base.File(path, mimeType);
                    }
                    else
                    {
                        using (var imageSource = Image.FromFile(path))
                        using (var imageResized = Module.ImageResize(imageSource, MaxWidth.HasValue ? MaxWidth.Value : 0, MaxHeight.HasValue ? MaxHeight.Value : 0))
                        {
                            var stream = new MemoryStream();
                            imageResized.Save(stream, imageSource.RawFormat);
                            stream.Position = 0;

                            var fileNameFinal = string.IsNullOrEmpty(fileName) ? Path.GetFileName(filePath) : fileName;
                            Response.Headers["Content-Disposition"] = $"inline; filename={fileNameFinal}";
                            Response.Cache.SetCacheability(HttpCacheability.Public);
                            if (dbChangeTime.HasValue) Response.Cache.SetLastModified(dbChangeTime.Value);
                            return base.File(stream, mimeType);
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                RegisterEventWithCode(HttpStatusCode.InternalServerError, "Ошибка во время вывода файла", $"FileImage.IdFile='{IdFile}', MaxWidth={MaxWidth}, MaxHeight={MaxHeight}.", ex);
                return null;
            }
        }

        [ModuleAction("imageCrop")]
        public ActionResult FileImageCrop(int? IdFile = null, int? MaxWidth = null, int? MaxHeight = null)
        {
            try
            {
                var rootDirectory = System.Web.Hosting.HostingEnvironment.MapPath("/");
                var filePathRelative = string.Empty;
                var fileName = string.Empty;
                DateTime? dbChangeTime = null;
                var mimeType = MediaTypeNames.Application.Octet;

                if (!IdFile.HasValue) filePathRelative = "data/img/files/argumentzero.jpg"; //Не указан номер файла.
                else
                {
                    using (var db = new Db.DataContext())
                    {
                        var file = db.File.
                            Where(x => x.IdFile == IdFile.Value && !x.IsRemoved && !x.IsRemoving).
                            Select(x => new { x.PathFile, x.NameFile, x.TypeConcrete, x.DateChange }).
                            FirstOrDefault();

                        if (file == null)
                        {
                            filePathRelative = "data/img/files/notfound.jpg"; //Файл не найден.
                            IdFile = null; // сбрасывается для формирования правильного пути временного изображения.
                        }
                        else
                        {
                            if (!System.IO.File.Exists(Path.Combine(rootDirectory, file.PathFile)) && !System.IO.File.Exists(Path.Combine(rootDirectory, "bin", file.PathFile)))
                            {
                                filePathRelative = "data/img/files/notfound.jpg"; //Файл не найден.
                                if (Module.ExternalFileSourceDomain != null)
                                {
                                    var paramss = string.Join("&", new List<string>() { !MaxWidth.HasValue ? null : "MaxWidth=" + MaxWidth.Value, !MaxHeight.HasValue ? null : "MaxHeight=" + MaxHeight.Value }.
                                    Where(x => !string.IsNullOrEmpty(x)));

                                    var url = Routing.UrlManager.CombineUrlParts(Module.UrlName, nameof(FileImageCrop), IdFile.Value.ToString(), (!string.IsNullOrEmpty(paramss) ? "?" + paramss : ""));
                                    return Redirect(new Uri(Module.ExternalFileSourceDomain, url)?.ToString());
                                }
                            }
                            else
                            {
                                filePathRelative = file.PathFile;
                                fileName = file.NameFile;
                                dbChangeTime = file.DateChange.FromTimestamp();
                                if (!string.IsNullOrEmpty(file.TypeConcrete)) mimeType = file.TypeConcrete;
                            }
                        }
                    }
                }

                string filePathFull = null;

                if (System.IO.File.Exists(Path.Combine(rootDirectory, filePathRelative))) filePathFull = Path.Combine(rootDirectory, filePathRelative);
                if (System.IO.File.Exists(Path.Combine(rootDirectory, "bin", filePathRelative))) filePathFull = Path.Combine(rootDirectory, "bin", filePathRelative);

                if (filePathFull == null && Debug.IsDeveloper)
                {
                    var filePathVirtual = AppCore.Get<Core.Storage.ResourceProvider>().GetFilePath(null, filePathRelative, true, out var searchLocations);
                    if (filePathVirtual != null)
                    {
                        var filePathFullTmp = Server.MapPath(filePathVirtual);
                        if (System.IO.File.Exists(filePathFullTmp)) filePathFull = filePathFullTmp;
                    }
                }

                if (!string.IsNullOrEmpty(filePathFull))
                {
                    var isNeedResize = MaxWidth.HasValue || MaxHeight.HasValue;
                    if (!isNeedResize)
                    {
                        var fileNameFinal = string.IsNullOrEmpty(fileName) ? Path.GetFileName(filePathRelative) : fileName;
                        Response.Headers["Content-Disposition"] = $"inline; filename={fileNameFinal}";
                        Response.Cache.SetCacheability(HttpCacheability.Public);
                        if (dbChangeTime.HasValue) Response.Cache.SetLastModified(dbChangeTime.Value);
                        return base.File(filePathFull, mimeType);
                    }
                    else
                    {
                        var isAllowed = IsFileImageCropAllowed(IdFile, MaxWidth, MaxHeight, filePathRelative, filePathFull);
                        if (!isAllowed.IsSuccess)
                        {
                            return base.Content(isAllowed.Message);
                        }
                        else
                        {
                            var name = (IdFile.HasValue ? IdFile.Value + Path.GetExtension(filePathFull) : Path.GetFileName(filePathFull));
                            var filePathRelativeNew = Path.Combine($"data/filesModified/cropped", $"{(MaxWidth ?? 0)}_{(MaxHeight ?? 0)}", name);
                            var filePathFullNew = Path.Combine(rootDirectory, filePathRelativeNew);
                            Directory.CreateDirectory(Path.GetDirectoryName(filePathFullNew));

                            var isNeedUpdateFile = false;
                            var isFileExists = System.IO.File.Exists(filePathFullNew);
                            if (isFileExists)
                            {
                                var fileChangeTime = System.IO.File.GetLastWriteTimeUtc(filePathFullNew);
                                if (dbChangeTime.HasValue && dbChangeTime.Value > fileChangeTime) isNeedUpdateFile = true;
                            }
                            else
                            {
                                isNeedUpdateFile = true;
                            }

                            if (isNeedUpdateFile)
                            {
                                using (var imageSource = Image.FromFile(filePathFull))
                                {
                                    var imageCropped = Module.ImageCrop(imageSource, MaxWidth.HasValue ? MaxWidth.Value : 0, MaxHeight.HasValue ? MaxHeight.Value : 0);
                                    if (imageCropped != null)
                                    {
                                        try
                                        {
                                            for (int i = 0; i < 3; i++)
                                            {
                                                try
                                                {
                                                    using (var fileStream = new FileStream(filePathFullNew, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
                                                    {
                                                        fileStream.SetLength(0);
                                                        imageCropped.Item1.Save(fileStream, imageCropped.Item2);
                                                        fileStream.Dispose();
                                                    }
                                                    isFileExists = true;
                                                    if (dbChangeTime.HasValue) System.IO.File.SetLastWriteTimeUtc(filePathFullNew, dbChangeTime.Value);
                                                    break;
                                                }
                                                catch
                                                {
                                                    System.Threading.Thread.Sleep(500);
                                                }
                                            }
                                        }
                                        finally
                                        {
                                            imageCropped.Item1.Dispose();
                                        }
                                    }
                                }
                            }

                            if (isFileExists)
                            {
                                var fileNameFinal = string.IsNullOrEmpty(fileName) ? Path.GetFileName(filePathRelative) : fileName;
                                Response.Headers["Content-Disposition"] = $"inline; filename={fileNameFinal}";
                                Response.Cache.SetCacheability(HttpCacheability.Public);
                                if (dbChangeTime.HasValue) Response.Cache.SetLastModified(dbChangeTime.Value);
                                return base.File(filePathFullNew, mimeType);
                            }
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                RegisterEventWithCode(HttpStatusCode.InternalServerError, "Ошибка во время вывода файла", $"FileImageCrop.IdFile='{IdFile}', MaxWidth={MaxWidth}, MaxHeight={MaxHeight}.", ex);
                return null;
            }
        }

        /// <summary>
        /// Позволяет управлять разрешением на изменение размера изображения.
        /// </summary>
        /// <param name="idFile"></param>
        /// <param name="maxWidth"></param>
        /// <param name="maxHeight"></param>
        /// <param name="filePathRelative"></param>
        /// <param name="filePathFull"></param>
        /// <returns></returns>
        protected ExecutionResult IsFileImageCropAllowed(
            int? idFile,
            int? maxWidth,
            int? maxHeight,
            string filePathRelative,
            string filePathFull
        )
        {
            return new ExecutionResult(true);
        }

    }
}
