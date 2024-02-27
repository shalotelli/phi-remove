using System;
using Amazon;
using Amazon.Comprehend;
using Amazon.Rekognition;
using Amazon.Runtime.CredentialManagement;

namespace PiiRemove
{
    public class AwsServiceClient
    {
        public AmazonRekognitionClient RekognitionClient { get; private set; }
        public AmazonComprehendClient ComprehendClient { get; private set; }

        public AwsServiceClient()
        {
            var sharedFile = new SharedCredentialsFile();
            
            if (sharedFile.TryGetProfile("development", out var profile) &&
                AWSCredentialsFactory.TryGetAWSCredentials(profile, sharedFile, out var credentials))
            {
                var region = RegionEndpoint.USEast1;

                RekognitionClient = new AmazonRekognitionClient(credentials, region);
                ComprehendClient = new AmazonComprehendClient(credentials, region);
            }
            else
            {
                throw new InvalidOperationException("AWS credentials could not be loaded.");
            }
        }
    }
}