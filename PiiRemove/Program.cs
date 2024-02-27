using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PiiRemove
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please provide image path");
                return;
            }

            var imagePath = args[0];

            if (!File.Exists(imagePath))
            {
                Console.WriteLine($"File not found: {imagePath}");
                return;
            }

            var awsServiceClient = new AwsServiceClient();
            var piiDetector = new PiiDetector(awsServiceClient);
            var piiRemover = new PiiRemover();

            var textInImage = await piiDetector.DetectTextInImageAsync(imagePath);
            var detectedText = string.Join(" ", textInImage.Select((t => t.DetectedText)));
            var piiEntities = await piiDetector.DetectPiiAsync(detectedText);
            
            piiRemover.RemovePiiFromImage(imagePath, textInImage, piiEntities);
            
            Console.WriteLine("PII Removal Complete...");
        }
    }
}