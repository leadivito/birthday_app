using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SampleFacebookBirthdayApp.Models;
using System;
using System.Text.RegularExpressions;

namespace SampleFacebookBirthdayApp
{
    public static class RecommendationEngine
    {
        private static List<string> MenCategoies = new List<string>()
        {
            "mens-clothes",
            //"mens-bags",
            "mens-shoes"
        };

        private static List<string> WomenCategoies = new List<string>()
        {
            "womens-clothes",
            //"handbags",
            "womens-shoes"
        };

        private static List<string> AmazonMenCategoies = new List<string>()
        {
            "Automotive",
            //"DVD",
            "Electronics"
        };

        private static List<string> AmazonWomenCategoies = new List<string>()
        {
            "GiftCards",
            //"Jewelry",
            "Books"
        };


        //rende randomici gli elementi trovati nelle categorie
        public static void Shuffle<T>(this IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }


        public static async Task<List<Product>> RecommendProductAsync(MyAppUserFriend friend)
        {
            List<Product> recommendedItems = new List<Product>();
            List<string> categoryBasedOnGender = WomenCategoies;

            //amazon version
            List<string> categoryBasedOnGenderForAmazon = AmazonWomenCategoies;

             if (friend.Gender == "male")
            {
                categoryBasedOnGender = MenCategoies;
                categoryBasedOnGenderForAmazon = AmazonMenCategoies;
            }

            foreach (var item in categoryBasedOnGender)
            {
                var result = await ShoppingSearchClient.GetProductsAsync(item);
                //Randomly pick an item from the retrieved items
                Random r = new Random();
                var product = result.Products[r.Next(result.Products.Count())];
                var des = product.Description;
                //Remove html elements from Product Description
                string noHTML = Regex.Replace(product.Description, @"<[^>]+>|&nbsp;", "").Trim();
                product.Description = Regex.Replace(noHTML, @"\s{2,}", " ");
                recommendedItems.Add(product);
            }

            //Ricerca con amazon
            foreach (var item in categoryBasedOnGenderForAmazon)
            {
                List<Product> amazonProducts = ItemLookupSample.SearchAmazon(item, " ");
                foreach (var p in amazonProducts)
                    recommendedItems.Add(p);
            }

            recommendedItems.Shuffle();

            return recommendedItems;
        }
    }
}