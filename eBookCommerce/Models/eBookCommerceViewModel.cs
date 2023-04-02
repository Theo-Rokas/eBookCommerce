using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using X.PagedList;

namespace eBookCommerce.Models
{
    public class eBookCommerceViewModel
    {
        public AspNetUser user { get; set; }

        public AspNetUser seller { get; set; }

        public Book book { get; set; }

        public List<Book> books { get; set; }

        public Genre genre { get; set; }

        public List<Genre> genres { get; set; }

        public List<SelectListItem> genresSelectList { get; set; }

        //public eBookCommerceViewModel()
        //{
        //    var ebcDB = new eBookCommerceEntities();

        //    user = ebcDB.AspNetUsers.SingleOrDefault(a => a.Email == HttpContext.Current.User.Identity.Name);            

        //    books = ebcDB.Books.ToList();

        //    book = books[getRandomBookPosition()];

        //    books = ebcDB.Books.Where(a => a.bookId != book.bookId).ToList();

        //    genres = ebcDB.Genres.ToList();
        //}

        public eBookCommerceViewModel(int bookId = 0, int genreId = 0)
        {
            var ebcDB = new eBookCommerceEntities();

            user = ebcDB.AspNetUsers.SingleOrDefault(a => a.Email == HttpContext.Current.User.Identity.Name);

            if (bookId != 0)
            {
                book = ebcDB.Books.SingleOrDefault(a => a.bookId == bookId);

                seller = book.AspNetUser;

                books = ebcDB.Books.Where(a => a.AspNetUser.Id == seller.Id && a.bookId != bookId).ToList();
            }
            else
            {
                books = ebcDB.Books.ToList();

                book = books[getRandomBookPosition()];                
            }

            genres = ebcDB.Genres.ToList();

            genresSelectList = new List<SelectListItem>();

            foreach(var item in genres)
            {
                genresSelectList.Add(
                    new SelectListItem()
                    {
                        Text = item.genreName.ToString(),
                        Value = item.genreId.ToString(),
                        Selected = false
                    }
                );
            }

            if (genreId != 0)
            {
                books = books.Where(a => a.genreId == genreId).ToList();
            }
        }

        private int getRandomBookPosition()
        {
            Random rnd = new Random();
            int bookPosition = rnd.Next(0, books.Count);
            return bookPosition;
        }        
    }
}