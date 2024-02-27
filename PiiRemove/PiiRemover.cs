using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using Amazon.Rekognition.Model;
using Amazon.Comprehend.Model;

namespace PiiRemove
{
    public class PiiRemover
    {
        public void RemovePiiFromImage(string imagePath, List<TextDetection> detectedText, List<PiiEntity> piiEntities)
        {
            using (var image = System.Drawing.Image.FromFile(imagePath))
            using (var graphics = Graphics.FromImage(image))
            {
                foreach (var text in detectedText)
                {
                    if (!IsPii(text, piiEntities)) continue;
                    
                    var blackBar = new Rectangle(
                        (int)Math.Round(text.Geometry.BoundingBox.Left * image.Width),
                        (int)Math.Round(text.Geometry.BoundingBox.Top * image.Height),
                        (int)Math.Round(text.Geometry.BoundingBox.Width * image.Width),
                        (int)Math.Round(text.Geometry.BoundingBox.Height * image.Height));
                        
                    graphics.FillRectangle(Brushes.Black, blackBar);
                }
                
                // save redacted image
                var outputImagePath = Path.Combine(
                    Path.GetDirectoryName(imagePath) ?? string.Empty, 
                    Path.GetFileNameWithoutExtension(imagePath) + "_redacted" + Path.GetExtension(imagePath));
                image.Save(outputImagePath);
                Console.WriteLine($"Processed image saved to: {outputImagePath}");
            }
        }

        private static bool IsPii(TextDetection detectedText, List<PiiEntity> piiEntities)
        {
            if (detectedText == null || piiEntities.Count == 0)
            {
                return false;
            }
            
            var text = detectedText.DetectedText;
            
            var detectedTextStart = detectedText.Geometry.BoundingBox.Left;
            var detectedTextEnd = detectedText.Geometry.BoundingBox.Left + detectedText.Geometry.BoundingBox.Width;

            foreach (var piiEntity in piiEntities)
            {
                var piiStart = piiEntity.BeginOffset;
                var piiEnd = piiEntity.EndOffset;
                
                if (piiStart <= detectedTextEnd && piiEnd >= detectedTextStart)
                {
                    return true;
                }
            }

            return false;
        }
    }
}