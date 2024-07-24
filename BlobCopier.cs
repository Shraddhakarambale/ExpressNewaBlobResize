using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using Microsoft.Azure.Functions.Worker;
using Azure.Storage.Blobs;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

using SixLabors.ImageSharp.Processing;
using Microsoft.Extensions.Logging;
using Azure.Core.Extensions;
using Microsoft.Extensions.Azure;

namespace ExpressNewaBlobResize
{
    public class BlobCopier
    {
        private readonly ILogger<BlobCopier> _logger;
        private readonly BlobContainerClient _copyContainerClient;
        private readonly BlobContainerClient _containerClient;

        public BlobCopier(ILogger<BlobCopier> logger, IAzureClientFactory<BlobServiceClient> blobClientFactory)
        {
            _logger = logger;
            _containerClient = blobClientFactory.CreateClient("copyBlob").GetBlobContainerClient("newscontainer");
            _copyContainerClient = blobClientFactory.CreateClient("copyBlob").GetBlobContainerClient("images-sm");



        }

        [Function(nameof(BlobCopier))]
        public async Task Run([BlobTrigger("newscontainer/{name}", Connection = "AzureWebJobsStorage")] Stream myBlob, string name)
        {
            BlobClient blobClient = _containerClient.GetBlobClient(name);
            using (var stream = blobClient.OpenRead())
            {
                IImageFormat format;
                using (Image<Rgba32>input=Image.Load<Rgba32> (stream,out format))
                {
                    var copy = ResizeImage(input, ImageSize.Small, format);
                    copy.Position = 0;
                    BlobClient copyBlobClient=_copyContainerClient.GetBlobClient(name);
                    copyBlobClient.Upload(copy);

                    

                }
            }
            _logger.LogInformation($"Blob{name} copied!");
            //var content = await blobStreamReader.ReadToEndAsync();
            //_logger.LogInformation($"C# Blob trigger function Processed blob\n Name: {name} \n Data: {content}");
        }
        public static Stream ResizeImage(Image<Rgba32>input,ImageSize size,IImageFormat format)
        {
            var dimensions = imageDimensionsTable[size];
            Stream stream = new MemoryStream();
            var writeable = stream.CanWrite;
            input.Mutate(x => x.Resize(dimensions.Item1, dimensions.Item2));
            input.Save(stream, format);
            return stream;
        }
        public enum ImageSize { Small}
        private static Dictionary<ImageSize, (int, int)> imageDimensionsTable = new Dictionary<ImageSize, (int, int)>() { 
        {
            ImageSize.Small,(480,300) }

        };
    }
}
