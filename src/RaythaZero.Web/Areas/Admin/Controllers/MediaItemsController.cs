using CSharpVitamins;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaythaZero.Application.Common.Interfaces;
using RaythaZero.Application.Common.Utils;
using RaythaZero.Application.MediaItems.Commands;
using RaythaZero.Application.MediaItems.Queries;

namespace RaythaZero.Web.Areas.Admin.Controllers;

[Route("admin/media-items")]
[Authorize] 
public class MediaItemsController : Controller
{
    private ISender _mediator;
    private IFileStorageProvider _fileStorageProvider;
    private IRelativeUrlBuilder _relativeUrlBuilder;
    protected IRelativeUrlBuilder RelativeUrlBuilder => _relativeUrlBuilder ??= HttpContext.RequestServices.GetRequiredService<IRelativeUrlBuilder>();
    protected IFileStorageProvider FileStorageProvider => _fileStorageProvider ??= HttpContext.RequestServices.GetRequiredService<IFileStorageProvider>();
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();
    
    [HttpPost]
    [Route($"presign", Name = "mediaitemspresignuploadurl")]
    public async Task<IActionResult> CloudUploadPresignRequest([FromBody] MediaItemPresignRequestViewModel body)
    {
        var idForKey = ShortGuid.NewGuid();
        var objectKey = FileStorageUtility.CreateObjectKeyFromIdAndFileName(idForKey, body.filename);
        var url = await FileStorageProvider.GetUploadUrlAsync(objectKey, body.filename, body.contentType, FileStorageUtility.GetDefaultExpiry());

        return Json(new { url, fields = new { id = idForKey.ToString(), fileName = body.filename, body.contentType, objectKey } });
    }

    [HttpPost]
    [Route($"create-after-upload", Name = "mediaitemscreateafterupload")]
    public async Task<IActionResult> CloudUploadCreateAfterUpload([FromBody] MediaItemCreateAfterUploadViewModel body)
    {
        var input = new CreateMediaItem.Command
        {
            Id = body.id,
            FileName = body.filename,
            Length = body.length,
            ContentType = body.contentType,
            FileStorageProvider = FileStorageProvider.GetName(),
            ObjectKey = body.objectKey
        };

        var response = await Mediator.Send(input);
        if (response.Success)
        {
            return Json(new { success = true });
        }
        else
        {
            this.HttpContext.Response.StatusCode = 403;
            return Json(new { success = false, error = response.Error });
        }
    }

    [HttpPost]
    [Route($"upload", Name = "mediaitemslocalstorageupload")]
    public async Task<IActionResult> LocalStorageUpload(IFormFile file)
    {
        if (file.Length <= 0)
        {
            this.HttpContext.Response.StatusCode = 403;
            return Json(new { success = false });
        }
        using (var stream = new MemoryStream())
        {
            await file.CopyToAsync(stream);
            var data = stream.ToArray();

            var idForKey = ShortGuid.NewGuid();
            var objectKey = FileStorageUtility.CreateObjectKeyFromIdAndFileName(idForKey, file.FileName);
            await FileStorageProvider.SaveAndGetDownloadUrlAsync(data, objectKey, file.FileName, file.ContentType, FileStorageUtility.GetDefaultExpiry());

            var input = new CreateMediaItem.Command
            {
                Id = idForKey,
                FileName = file.FileName,
                Length = data.Length,
                ContentType = file.ContentType,
                FileStorageProvider = FileStorageProvider.GetName(),
                ObjectKey = objectKey,
            };

            var response = await Mediator.Send(input);
            if (response.Success)
            {
                var url = Url.Action("RedirectToFileUrlById", "MediaItems", new { id = response.Result });
                return Json(new { url, location = url, success = true, fields = new { id = idForKey.ToString(), fileName = file.FileName, file.ContentType, objectKey } });
            }
            else
            {
                this.HttpContext.Response.StatusCode = 403;
                return Json(new { success = false, error = response.Error });
            }
        }     
    }

    [AllowAnonymous]
    [Route($"objectkey/{{objectKey}}", Name = "mediaitemsredirecttofileurlbyobjectkey")]
    public IActionResult RedirectToFileUrlByObjectKey(string objectKey)
    {
        var downloadUrl = FileStorageProvider.GetDownloadUrlAsync(objectKey, FileStorageUtility.GetDefaultExpiry()).Result;
        return Redirect(downloadUrl);
    }

    [AllowAnonymous]
    [Route($"id/{{id}}", Name = "mediaitemsredirecttofileurlbyid")]
    public async Task<IActionResult> RedirectToFileUrlById(string id)
    {
        var input = new GetMediaItemById.Query { Id = id };
        var response = await Mediator.Send(input);

        var downloadUrl = FileStorageProvider.GetDownloadUrlAsync(response.Result.ObjectKey, FileStorageUtility.GetDefaultExpiry()).Result;
        return Redirect(downloadUrl);
    }
}

public class MediaItemPresignRequestViewModel
{
    public string filename { get; set; }
    public string contentType { get; set; }
    public string extension { get; set; }
}

public class MediaItemCreateAfterUploadViewModel
{
    public string id { get; set; }
    public string filename { get; set; }
    public string contentType { get; set; }
    public string extension { get; set; }
    public string objectKey { get; set; }
    public long length { get; set; }
    public string contentDisposition { get; set; }
}