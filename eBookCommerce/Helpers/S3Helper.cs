using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace eBookCommerce.Helpers
{
    public class S3Helper
    {
        private static readonly string bucketName = ConfigurationManager.AppSettings["BucketName"];
        private static readonly string accesskey = ConfigurationManager.AppSettings["AWSAccessKey"];
        private static readonly string secretkey = ConfigurationManager.AppSettings["AWSSecretKey"];
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.EUCentral1;

        public static string UploadFile(HttpPostedFileBase file)
        {
            AmazonS3Client client = new AmazonS3Client(accesskey, secretkey, bucketRegion);

            var keyName = DateTime.UtcNow.Ticks + file.FileName;

            PutObjectRequest request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = keyName,
                InputStream = file.InputStream
            };

            client.PutObject(request);

            var itemUrl = "https://" + bucketName + ".s3." + bucketRegion.SystemName + ".amazonaws.com/" + keyName;
            return itemUrl;
        }
    }
}