using eBookCommerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using RestSharp;

namespace eBookCommerce.Controllers
{
    public class PaymentsController : Controller
    {
        eBookCommerceEntities ebcDB = new eBookCommerceEntities();

        private static readonly string domain = "http://192.168.1.202:44347/";
        private static readonly string userName = "eBookCommerce-api";
        private static readonly string password = "ceyijX6";

        [HttpGet]
        public ActionResult PaymentInit()
        {
            var user = ebcDB.AspNetUsers.SingleOrDefault(a => a.Email == User.Identity.Name);
            var basketItems = ebcDB.Baskets.Where(a => a.personId == user.Id).ToList();

            var amount = (int)(basketItems.Sum(a => a.Book.bookPrice.Value) * 100);
            var email = user.Email;

            var payment = new Payment()
            {                
                paymentAmount = amount,
                paymentUserName = userName,
                paymentEmail = email,
                paymentCreatedAt = DateTime.UtcNow                
            };

            ebcDB.Payments.Add(payment);
            ebcDB.SaveChanges();

            foreach (var basketItem in basketItems)
            {
                basketItem.paymentId = payment.paymentId;
            }
            ebcDB.SaveChanges();

            var orderNumber = payment.paymentId + "_" + DateTime.UtcNow.Ticks;
            payment.paymentOrderNumber = orderNumber;
            ebcDB.SaveChanges();

            var returnUrl = domain + Url.Action("PaymentComplete", "Payments");

            var paymentUrl = "https://gateway-test.jcc.com.cy/payment/rest/register.do";

            var client = new RestClient(paymentUrl);            
            var request = new RestRequest();

            request.Method = Method.Post;
            request.AddHeader("content-type", "application/x-www-form-urlencoded");            
            request.AddParameter("amount", amount);
            request.AddParameter("userName", userName);
            request.AddParameter("password", password);
            request.AddParameter("orderNumber", orderNumber);
            request.AddParameter("returnUrl", returnUrl);
            request.AddParameter("email", email);
            request.AddParameter("language", "en");
            
            var response = client.Execute(request);

            var content = response.Content;
            dynamic jsonResponse = JsonConvert.DeserializeObject(content);

            if (jsonResponse.errorCode != null && jsonResponse.errorCode > 0)
            {
                int errorCode = jsonResponse.errorCode;
                string errorMessage = jsonResponse.errorMessage;

                payment.paymentErrorCode = errorCode.ToString();
                payment.paymentErrorMessage = errorMessage;
                ebcDB.SaveChanges();

                return Json(new { success = false, message = "Payment Init Failure" }, JsonRequestBehavior.AllowGet);
            }

            string formUrl = jsonResponse.formUrl;
            string orderId = jsonResponse.orderId;

            payment.paymentOrderId = orderId;
            ebcDB.SaveChanges();

            return Json(new { success = true, redirectUrl = formUrl }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult PaymentComplete(string orderId)
        {
            var statusUrl = "https://gateway-test.jcc.com.cy/payment/rest/getOrderStatusExtended.do";
            
            var client = new RestClient(statusUrl);            
            var request = new RestRequest();

            request.Method = Method.Post;
            request.AddHeader("content-type", "application/x-www-form-urlencoded");           
            request.AddParameter("userName", userName);
            request.AddParameter("password", password);
            request.AddParameter("orderId", orderId);
            request.AddParameter("language", "en");
            
            var response = client.Execute(request);

            dynamic jsonResponse = JsonConvert.DeserializeObject(response.Content); 

            int orderStatus = jsonResponse.orderStatus;
            int approvedAmount = jsonResponse.paymentAmountInfo.approvedAmount;
            string maskedPan = jsonResponse.cardAuthInfo.maskedPan;
            string cardholderName = jsonResponse.cardAuthInfo.cardholderName;

            var payment = ebcDB.Payments.Single(a => a.paymentOrderId == orderId);
            payment.paymentStatus = orderStatus;
            payment.paymentApprovedAmount = approvedAmount;
            payment.paymentMaskedPan = maskedPan;
            payment.paymentCardholderName = cardholderName;
            payment.paymentUpdatedAt = DateTime.UtcNow;
            ebcDB.SaveChanges();

            if (orderStatus == 2)
            {
                var basketItems = payment.Baskets.ToList();

                foreach (var basketItem in basketItems)
                {
                    ebcDB.Books.Remove(basketItem.Book);
                    ebcDB.Baskets.Remove(basketItem);
                }

                ebcDB.SaveChanges();

                return View("PaymentSuccess", payment);
            }
            else
            {
                if (jsonResponse.errorCode != null && jsonResponse.errorCode > 0)
                {
                    int errorCode = jsonResponse.errorCode;
                    string errorMessage = jsonResponse.errorMessage;

                    payment.paymentErrorCode = errorCode.ToString();
                    payment.paymentErrorMessage = errorMessage;
                    ebcDB.SaveChanges();
                }

                return View("PaymentFailure", payment);
            }
        }
    }
}