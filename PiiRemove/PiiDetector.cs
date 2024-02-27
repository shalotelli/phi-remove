using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Amazon.Rekognition.Model;
using Amazon.Comprehend.Model;

namespace PiiRemove
{
    public class PiiDetector
    {
        private readonly AwsServiceClient _awsServiceClient;
        private readonly string[] _piiTypesToRedact =
        {
            "ADDRESS", 
            "AGE",
            "CREDIT_DEBIT_CVV",
            "CREDIT_DEBIT_EXPIRY",
            "CREDIT_DEBIT_NUMBER",
            "DATE_TIME",
            "DRIVER_ID",
            "EMAIL",
            "LICENSE_PLATE",
            "NAME",
            "PHONE",
            "SWIFT_CODE",
            "VEHICLE_IDENTIFICATION_NUMBER",
            "BANK_ACCOUNT_NUMBER",
            "BANK_ROUTING",
            "PASSPORT_NUMBER",
            "US_INDIVIDUAL_TAX_IDENTIFICATION_NUMBER",
            "SSN"
        };

        public PiiDetector(AwsServiceClient awsServiceClient)
        {
            _awsServiceClient = awsServiceClient;
        }

        public async Task<List<TextDetection>> DetectTextInImageAsync(string imagePath)
        {
            var detectedText = new List<TextDetection>();

            try
            {
                using (var imageStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                {
                    var detectTextRequest = new DetectTextRequest()
                    {
                        Image = new Image
                        {
                            Bytes = new MemoryStream()
                        }
                    };
                    
                    await imageStream.CopyToAsync(detectTextRequest.Image.Bytes);

                    var response = await _awsServiceClient.RekognitionClient.DetectTextAsync(detectTextRequest);

                    if (response.TextDetections != null && response.TextDetections.Count > 0)
                    {
                        detectedText.AddRange(response.TextDetections);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error detecting text in image; {ex.Message}");
            }

            return detectedText;
        }
        
        public async Task<List<PiiEntity>> DetectPiiAsync(string text)
        {
            var piiEntities = new List<PiiEntity>();

            try
            {
                var detectPiiEntitiesRequest = new DetectPiiEntitiesRequest
                {
                    Text = text,
                    LanguageCode = "en"
                };

                var response = await _awsServiceClient.ComprehendClient.DetectPiiEntitiesAsync(detectPiiEntitiesRequest);

                if (response.Entities != null && response.Entities.Count > 0)
                {
                    //piiEntities.AddRange(response.Entities.Where(e => _piiTypesToRedact.Contains(e.Type.ToString())));
                    piiEntities.AddRange(response.Entities);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error detecting PII: {ex.Message}");
            }

            return piiEntities;
        }
    }
}