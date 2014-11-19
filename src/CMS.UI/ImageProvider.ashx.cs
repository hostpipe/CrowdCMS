using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using CMS.BL.Entity;
using CMS.Services;
using CMS.Utils.Diagnostics;
using CMS.Utils.Drawing.Imaging;
using CMS.Utils.Extension;

namespace CMS.UI
{
    /// <summary>
    ///     Summary description for Handler1
    /// </summary>
    public class Handler1 : IHttpAsyncHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            throw new InvalidOperationException();
        }

        public bool IsReusable
        {
            get { return false; }
        }

        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            var async = new AsynchOperation(cb, context, extraData);
            async.StartAsyncWork();
            return async;
        }

        public void EndProcessRequest(IAsyncResult result)
        {
        }

        private class AsynchOperation : IAsyncResult
        {
            private readonly AsyncCallback _callback;
            private readonly HttpContext _context;
            private readonly Object _state;
            private bool _completed;

            public AsynchOperation(AsyncCallback callback, HttpContext context, Object state)
            {
                _callback = callback;
                _context = context;
                _state = state;
                _completed = false;
            }

            public object AsyncState
            {
                get { return _state; }
            }

            public WaitHandle AsyncWaitHandle
            {
                get { throw null; }
            }

            public bool CompletedSynchronously
            {
                get { return false; }
            }

            public bool IsCompleted
            {
                get { return _completed; }
            }

            public void StartAsyncWork()
            {
                ThreadPool.QueueUserWorkItem(StartAsyncTask, null);
            }

            private void StartAsyncTask(Object workItemState)
            {
                try
                {
                    string location = _context.Request.QueryString["image"];

                    if (location == null)
                        //throw new HttpException("Invalid request.");
                        return;

                    string extension;
                    string contentType;

                    try
                    {
                        extension = Path.GetExtension(location).ToLower();
                    }
                    catch
                    {
                        throw new HttpException("Invalid request: " + location);
                    }

                    switch (extension)
                    {
                        case ".jpg":
                            contentType = "image/jpeg";
                            break;
                        case ".jpeg":
                            contentType = "image/jpeg";
                            break;
                        case ".png":
                            contentType = "image/png";
                            break;
                        case ".gif":
                            contentType = "image/gif";
                            break;
                        case ".bmp":
                            contentType = "image/bmp";
                            break;
                        default:
                            throw new HttpException(404, "Invalid request: " + location);
                    }


                    string absoluteLocation = _context.Server.MapPath("~/" + location);
                    var ecommerceService = (IECommerce) DependencyResolver.Current.GetService(typeof (IECommerce));

                    //var config = ImageProviderWebSectionGroup.FromCurrentConfiguration().ImageProvider;
                    //if (config.Presets.Count == 0)
                    //    throw new ConfigurationErrorsException("No resize presets defined.");
                    //ImageProviderPresetElement preset;

                    var preset = new tbl_ProdImageVerNames();
                    if (_context.Request["preset"] != null)
                    {
                        preset = ecommerceService.GetProductImageVersionByName(_context.Request.QueryString["preset"]);

                        if (preset == null)
                            throw new ArgumentException("The specified preset name is not valid.", "preset");
                    }

                    _context.Response.Cache.SetCacheability(HttpCacheability.Public);
                    _context.Response.Cache.SetExpires(DateTime.Now.AddDays(3));

                    if (preset.VN_Width == 0 && preset.VN_Width == 0 && File.Exists(absoluteLocation))
                    {
                        _context.Response.ContentType = contentType;
                        using (FileStream f = File.OpenRead(absoluteLocation))
                        {
                            f.Write(_context.Response.OutputStream, 65536);
                        }
                        return;
                    }

                    string absoluteCacheDirectoryLocation =
                        _context.Server.MapPath(String.IsNullOrWhiteSpace(preset.VN_Path)
                            ? "/Images/Versions"
                            : preset.VN_Path.TrimEnd('/'));

                    string absoluteCacheFileLocation = new StringBuilder()
                        .Append(absoluteCacheDirectoryLocation).Append(Path.DirectorySeparatorChar)
                        .Append(preset.VN_Width).Append("x").Append(preset.VN_Height)
                        .Append("_")
                        .Append(
                            (ImageResizer.ResizeMode)
                                Enum.Parse(typeof (ImageResizer.ResizeMode),
                                    preset.VN_Mode.GetValueOrDefault(
                                        (int) ImageResizer.ResizeMode.CropToDestinationAspect).ToString(CultureInfo.InvariantCulture)))
                        .Append("_").Append(Path.GetFileName(location)).ToString();

                    if (File.Exists(absoluteCacheFileLocation))
                    {
                        _context.Response.ContentType = contentType;

                        using (FileStream f = File.OpenRead(absoluteCacheFileLocation))
                        {
                            f.Write(_context.Response.OutputStream, 65536);
                        }

                        return;
                    }

                    if (File.Exists(absoluteLocation))
                    {
                        using (var resizer = new ImageResizer(absoluteLocation))
                        {
                            resizer.Background = String.IsNullOrEmpty(preset.VN_Background)
                                ? ColorTranslator.FromHtml("#FFF")
                                : ColorTranslator.FromHtml(preset.VN_Background);
                            resizer.Mode = preset.VN_Mode.HasValue
                                ? (ImageResizer.ResizeMode)
                                    Enum.Parse(typeof (ImageResizer.ResizeMode), preset.VN_Mode.Value.ToString(CultureInfo.InvariantCulture))
                                : ImageResizer.ResizeMode.CropToDestinationAspect;
                            resizer.Width = preset.VN_Width;
                            resizer.Height = preset.VN_Height;

                            resizer.Process();

                            _context.Response.ContentType = contentType;

                            if (!Directory.Exists(absoluteCacheDirectoryLocation))
                                Directory.CreateDirectory(absoluteCacheDirectoryLocation);

                            using (var output = new MemoryStream())
                            {
                                resizer.SaveAs(output);
                                output.WriteTo(_context.Response.OutputStream);
                            }

                            resizer.SaveAs(absoluteCacheFileLocation);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e.Message);
                }
                finally
                {
                    _completed = true;
                    _callback(this);
                }
            }
        }
    }
}