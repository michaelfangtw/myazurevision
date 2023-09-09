using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MyAzureVision.Models;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using System.Net.Mime;
using System.Reflection;
using System;

namespace MyAzureVision.Controllers;
public class UploadVisionController : Controller
{
    private readonly ILogger<UploadVisionController> _logger;
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;

   
    public string AZURE_API_KEY
    {
        get
        {
            var value = string.Format("{0}", Environment.GetEnvironmentVariable("azure_api_key"));
            if (string.IsNullOrEmpty(value)) {
                value = string.Format("{0}", _config.GetValue("azure_api_key",""));
            }
            return value;
        }
    }

    public string AZURE_API_ENDPOINT
    {
        get
        {
            var value = string.Format("{0}", Environment.GetEnvironmentVariable("azure_api_endpoint"));
            if (string.IsNullOrEmpty(value))
            {
                value = string.Format("{0}", _config.GetValue("azure_api_endpoint", ""));
            }
            return value;
        }
    }

    public string Version { 
        get {
            return Assembly.GetEntryAssembly().GetName().Version.ToString(); 
        }
    }



    //讀取設定
    public UploadVisionController(ILogger<UploadVisionController> logger, IConfiguration config, IWebHostEnvironment env)
    {
        _logger = logger;
        _config = config;
        _env= env; 
    }

    //首頁
    public IActionResult Index()
    {
        //return View();        
        return View(new VisionFileModel());        
    }

    //處理上傳
    [HttpPost]
    public IActionResult Create(VisionFileModel model)
    {
        
        var imageUrl = model.ImageUrl;
        var img = model.MyImage;
        var uniqueFileName = "";
        var imageFilePath = "";
        var localFileName = "";
        var contentType = "";
        var ext = "";
        var uploads = "";
        if (img != null)
        {
            uniqueFileName = GetUniqueFileName(img.FileName);
            uploads = Path.Combine(_env.WebRootPath, "upload");
            imageFilePath = Path.Combine(uploads, uniqueFileName);
            localFileName = Path.GetFileName(img.FileName);
            contentType = img.ContentType;
            ext = Path.GetExtension(localFileName);
            if ((ext != ".png") && (ext != ".jpg"))
            {
                TempData["msg"] = "檔案失敗!檔案類型" + ext + "錯誤!";
                return RedirectToAction("Index", "Upload");
            }

            if (!Directory.Exists(uploads))
            {
                Directory.CreateDirectory(uploads);
            }
            try
            {
                using (var fs = new FileStream(imageFilePath, FileMode.OpenOrCreate))
                {
                    model.MyImage.CopyTo(fs);
                }
            }
            catch (Exception ex)
            {
                TempData["FilePath"] = imageFilePath;
                TempData["FileName"] = localFileName + " error=" + ex.ToString();
                TempData["FileSize"] = "0";
                TempData["msg"] = "檔案上傳失敗!";
            }
        }
        
        
        //檢查model是否 有完整填寫
        if (ModelState.IsValid)
        {
            var sw = new Stopwatch();
            sw.Start();
            // Create a client
            ComputerVisionClient client = Authenticate(AZURE_API_ENDPOINT,AZURE_API_KEY);
            
            ImageAnalysis imageAnalysis = new ImageAnalysis();
            //by url
            if (!string.IsNullOrEmpty(imageUrl))
            {
                imageAnalysis = AnalyzeImageUrl(client, imageUrl);
            }
            //by image file
            if (img != null)
            {   
                imageAnalysis = AnalyzeImageFile(client, imageFilePath);
            }
            var adultInfo = imageAnalysis.Adult;
            sw.Stop();
            float cost = sw.ElapsedMilliseconds;


            if (imageAnalysis != null)
            {
                TempData["IsAdultContent"] = adultInfo.IsAdultContent;
                TempData["IsRacyContent"] = adultInfo.IsRacyContent;
                TempData["IsGoryContent"] = adultInfo.IsGoryContent;
                TempData["AdultScore"] = adultInfo.AdultScore.ToString();
                TempData["RacyScore"] = adultInfo.RacyScore.ToString();
                TempData["GoreScore"] = adultInfo.GoreScore.ToString();
                TempData["Format"] = imageAnalysis.Metadata.Format;
                TempData["Height"] = imageAnalysis.Metadata.Height.ToString();
                TempData["Width"] = imageAnalysis.Metadata.Width.ToString();
                TempData["Cost"] = cost.ToString();
                TempData["imageUrl"] = imageUrl;
                TempData["uploadFileName"] = uniqueFileName;
                TempData["msg"] = "辨識成功!,Version="+Version+",AZURE_API_ENDPOINT=" + AZURE_API_ENDPOINT;
            }

        }
        else
        {
            TempData["msg"] = "網址錯誤:"+ imageUrl.ToString(); 
        }
        

        return RedirectToAction("Index","UploadVision");
    }


    public static ComputerVisionClient Authenticate(string endpoint, string key)
    {
        ComputerVisionClient client =
          new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
          { Endpoint = endpoint };
        return client;
    }
    public ImageAnalysis AnalyzeImageFile(ComputerVisionClient client, string imageFilePath)
    {

        // Creating a list that defines the features to be extracted from the image. 

        List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
            {
                VisualFeatureTypes.Tags,
                VisualFeatureTypes.Adult
            };

        Console.WriteLine($"Analyzing the image by file ");
        Console.WriteLine();
        // Analyze the URL image 
        var sw = new Stopwatch();
        sw.Start();
        Console.WriteLine("start=" + DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss.fff"));
        //var stream = new MemoryStream(File.ReadAllBytes(imageFilePath));
        ImageAnalysis results;
        using (var imageStream = new FileStream(imageFilePath, FileMode.Open))
        {
            results = client.AnalyzeImageInStreamAsync(imageStream, visualFeatures: features).Result;
        }

        // Image tags and their confidence score
        //Console.WriteLine("Tags:");
        //foreach (var tag in results.Tags)
        //{
        //    Console.WriteLine($"{tag.Name} {tag.Confidence}");
        //}

        sw.Stop();
        return results;
    }

    public FileStream GetImageFileStream(string filePath)
    {   
        using (var fs = new FileStream(filePath, FileMode.Open))
        {
            return fs;
        }
        
    }
    public  ImageAnalysis AnalyzeImageUrl(ComputerVisionClient client, string imageUrl)
    {

        // Creating a list that defines the features to be extracted from the image. 

        List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
            {
                VisualFeatureTypes.Tags,
                VisualFeatureTypes.Adult
            };

        Console.WriteLine($"Analyzing the image {Path.GetFileName(imageUrl)}...");
        Console.WriteLine();
        // Analyze the URL image 
        var sw = new Stopwatch();
        sw.Start();
        Console.WriteLine("start=" + DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss.fff"));
        ImageAnalysis results =  client.AnalyzeImageAsync(imageUrl, visualFeatures: features).Result;

        // Image tags and their confidence score
        //Console.WriteLine("Tags:");
        //foreach (var tag in results.Tags)
        //{
        //    Console.WriteLine($"{tag.Name} {tag.Confidence}");
        //}
         
        sw.Stop();  
        return results;
    }

    //產生檔名
    private string GetUniqueFileName(string fileName)
    {
        fileName = Path.GetFileName(fileName);
        return  Path.GetFileNameWithoutExtension(fileName)
                  + "_" 
                  + Guid.NewGuid().ToString().Substring(0, 4) 
                  + Path.GetExtension(fileName);
    }

}
