using System.ComponentModel.DataAnnotations;

namespace MyAzureVision.Models;

public class UploadFileModel
{

    [Required]
    public string ImageCaption { set; get; }
    [Required]
    public string ImageDescription { set; get; }
    [Required]
    public IFormFile MyImage { set; get; }
}