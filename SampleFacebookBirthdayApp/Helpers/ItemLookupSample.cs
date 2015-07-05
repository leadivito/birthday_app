using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Threading.Tasks;
using System.Configuration;
using System.Net.Http;
using SampleFacebookBirthdayApp.Models;

namespace SampleFacebookBirthdayApp
{
    class ItemLookupSample
    {
        private const string MY_AWS_ACCESS_KEY_ID = "AKIAIK3YY4ZU57NORW5A";
        private const string MY_AWS_SECRET_KEY = "pGJqx5JwdMeNDssgDsp3dyOkRdBHKpS+c99ZGNTn";
        private const string MY_AWS_ASSOCIATION_ID = "fb0a0-21";
        private const string DESTINATION          = "ecs.amazonaws.com";

        private static HttpClient client = new HttpClient();
        
       // private const Task<string> NAMESPACE = "http://webservices.amazon.com/AWSECommerceService/2009-03-31";
       // private const string ITEM_ID   = "0545010225";

        public static  List<Product> SearchAmazon(String keyword)
        {

            SignedRequestHelper helper = new SignedRequestHelper(MY_AWS_ACCESS_KEY_ID, MY_AWS_SECRET_KEY, DESTINATION);

            String requestUrl;

            String requestString = "Service=AWSECommerceService" 
                + "&Version=2009-03-31"
                + "&Operation=ItemSearch"
                + "&SearchIndex=Books"
                + "&ResponseGroup=Large"
                + "&Keywords=" + keyword
                + "&AssociateTag=" + MY_AWS_ASSOCIATION_ID
                ;
           requestUrl = helper.Sign(requestString);

           try
           {
               WebRequest request = HttpWebRequest.Create(requestUrl);
               WebResponse response = request.GetResponse();
               XmlDocument doc = new XmlDocument();
               doc.Load(response.GetResponseStream());

               XmlNodeList errorMessageNodes = doc.GetElementsByTagName("Message");
               if (errorMessageNodes != null && errorMessageNodes.Count > 0)
               {
                   String message = errorMessageNodes.Item(0).InnerText;
                   //return "Error: " + message + " (but signature worked)";
               }

               List<Product> listProduct = new List<Product>();

               using (XmlReader reader = XmlReader.Create(new StringReader(doc.InnerXml)))
               {

                   while (reader.ReadToFollowing("Item"))
                   {
                       //inizializzo gli elementi della lista di oggetti
                       Product p = new Product();
                       ProductImage pi = new ProductImage();
                       Sizes s = new Sizes();
                       BestImage bi = new BestImage();

                       //leggo il file XML di ritorno
                       reader.ReadToFollowing("ASIN");
                       p.Description = reader.ReadElementContentAsString();
                       reader.ReadToFollowing("DetailPageURL");
                       p.ClickUrl = reader.ReadElementContentAsString();
                       reader.ReadToFollowing("MediumImage");
                       reader.ReadToFollowing("URL");
                       bi.Url = reader.ReadElementContentAsString();
                       reader.ReadToFollowing("Height");
                       bi.Height = reader.ReadElementContentAsString();
                       reader.ReadToFollowing("Width");
                       bi.Width = reader.ReadElementContentAsString();
                       s.Large = bi;
                       pi.Sizes = s;
                       p.Image = pi;
                       reader.ReadToFollowing("FormattedPrice");
                       p.PriceLabel = reader.ReadElementContentAsString();
                       reader.ReadToFollowing("Title");
                       p.Name = reader.ReadElementContentAsString();
                       //aggiungo alla lista degli oggetti da visualizzare
                       listProduct.Add(p);
                   }
               }

               return listProduct;
           }
           catch (Exception e)
           {
               System.Console.WriteLine("Caught Exception: " + e.Message);
               System.Console.WriteLine("Stack Trace: " + e.StackTrace);
           }
           return null;

        }

    }
}
