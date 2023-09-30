using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using eBookCommerce.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace eBookCommerce.Helpers
{
    public class SNSHelper
    {
		private static readonly string topicArn = ConfigurationManager.AppSettings["TopicArn"];
		private static readonly string accesskey = ConfigurationManager.AppSettings["AWSAccessKey"];
		private static readonly string secretkey = ConfigurationManager.AppSettings["AWSSecretKey"];
		private static readonly RegionEndpoint topicRegion = RegionEndpoint.EUCentral1;			

		public static void SendEmail(string subject, string message)
		{
			eBookCommerceEntities ebcDB = new eBookCommerceEntities();

			var user = ebcDB.AspNetUsers.SingleOrDefault(a => a.Email == HttpContext.Current.User.Identity.Name);

			var snsClient = new AmazonSimpleNotificationServiceClient(accesskey, secretkey, topicRegion);

			var subscribeResponse = snsClient.Subscribe(new SubscribeRequest
			{
				Endpoint = user.Email,
				Protocol = "email",
				TopicArn = topicArn
			});

			snsClient.Publish(new PublishRequest
			{
				Subject = subject,
				Message = message,
				TargetArn = subscribeResponse.SubscriptionArn
			});
		}
	}
}