using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace eBookCommerce.Models
{
    [MetadataType(typeof(BookMetaData))]
    public partial class Book
    {
        public class BookMetaData
        {
            public int bookId { get; set; }

            [Required]
            [Display(Name = "Book Name")]
            public string bookName { get; set; }

            [Required]
            [Display(Name = "Book Author")]
            public string bookAuthor { get; set; }

            [Required]
            [Display(Name = "Book Pages")]
            public int bookPages { get; set; }

            [Required]
            [Display(Name = "Book Price")]
            public Nullable<decimal> bookPrice { get; set; }

            //[Required]
            //[Display(Name = "Book Image Url")]
            //public string bookImageUrl { get; set; }

            [Required]
            [Display(Name = "Book Description")]
            public string bookDescription { get; set; }

            public string personId { get; set; }

            [Required]
            [Display(Name = "Book Genre")]
            public Nullable<int> genreId { get; set; }            
        }
    }
}