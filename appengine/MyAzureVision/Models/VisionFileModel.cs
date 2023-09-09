using System.ComponentModel.DataAnnotations;

namespace MyAzureVision.Models;

public class VisionFileModel
{

    public string? ImageUrl { set; get; }

    public IFormFile? MyImage { set; get; }

}