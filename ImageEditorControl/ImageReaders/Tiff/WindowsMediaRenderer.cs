using ImageEditorControl.Models;
using ImageEditorControl.Tools;
using System.IO;
using System.Windows.Media.Imaging;

namespace ImageEditorControl.ImageReaders.Tiff
{
        internal class WindowsMediaRenderer
        {
                internal static BitmapSource? RenderPageFromBytes(PageModel pm, bool forThumbnail)
                {
                        if (!(pm is not null && pm.ImageModelParent is ImageModel im && im.FileBytes is byte[] bytes && bytes.Length > 0))
                        {
                                return null;
                        }

                        try
                        {
                                var tiffBytes = bytes;

                                using Stream ms0 = new MemoryStream(tiffBytes);
                                var decoder = new TiffBitmapDecoder(ms0, BitmapCreateOptions.DelayCreation, BitmapCacheOption.OnDemand); 
                                var frame = decoder.Frames[pm.PageNumberInitial];

                                using MemoryStream ms = new MemoryStream();
                                BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                                encoder.Frames.Add(frame);
                                encoder.Save(ms);
                                ms.Position = 0;

                                var ths = ImageModel.ThumbnailSize.GetFitSize(frame.Height / frame.Width);

                                BitmapImage finalBitmap = new BitmapImage();
                                finalBitmap.BeginInit();
                                finalBitmap.CacheOption = BitmapCacheOption.OnLoad; 
                                finalBitmap.StreamSource = ms;

                                if (forThumbnail)
                                {
                                        finalBitmap.DecodePixelWidth = (int)ths.Width;
                                        finalBitmap.DecodePixelHeight = (int)ths.Height;
                                }

                                finalBitmap.EndInit();
                                finalBitmap.Freeze();

                                return finalBitmap;
                        }
                        catch (Exception ex) 
                        {
                                return null;
                        }
                }

                internal static List<PageSize> GetTiffStructureFromBytes(byte[] fileBytes, ILogger logger)
                {
                        var structureList = new List<PageSize>();

                        if (fileBytes == null || fileBytes.Length == 0)
                        {
                                return structureList;
                        }

                        var tiffBytes = fileBytes;

                        return structureList;
                }

                internal static bool AddPages(ImageModel im, CancellationToken token) 
                {
                        if (im is null || im.FileBytes is not byte[] fileBytes || fileBytes.Length == 0)
                        {
                                return false;
                        }

                        try
                        {
                                if (token.IsCancellationRequested)
                                {
                                        return false;
                                }

                                using var ms = new MemoryStream(fileBytes);
                                var decoder = new TiffBitmapDecoder(ms, BitmapCreateOptions.DelayCreation, BitmapCacheOption.OnDemand);

                                if (decoder is null)
                                {
                                        return false;
                                }

                                try
                                {
                                        foreach (BitmapFrame frame in decoder.Frames)
                                        {
                                                if (token.IsCancellationRequested)
                                                {
                                                        return false;
                                                }

                                                im.AddNewPage(frame);
                                        }
                                }
                                catch (Exception ex) 
                                {
                                        im.Logger?.LogException(ex, "Couldn't get existing page");
                                        return false;
                                }

                                _ = im.TryReadAnnotationsFromTiff(fileBytes, token);

                                return true;
                        }
                        catch (Exception ex)
                        {
                                im.Logger?.LogException(ex, "cannot read file");
                                return false;
                        }
                }
        }
}