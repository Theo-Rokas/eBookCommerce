using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using PagedList.Mvc;

namespace eBookCommerce.Models
{
    public class eBookCommerceViewModel
    {
        public AspNetUser user { get; set; }

        public Book book { get; set; }

        public List<Book> books { get; set; }

        public IPagedList<Book> booksPagedList { get; set; }

        public Genre genre { get; set; }

        public List<Genre> genres { get; set; }

        public List<SelectListItem> genresSelectList { get; set; }

        [Required]
        [Display(Name = "Book Image File")]
        public HttpPostedFileBase webBookImageFile { get; set; }

        public string email { get; set; }

        [Required]
        [Display(Name = "Book Image File")]
        public string mobileBookImageFile { get; set; }

        public eBookCommerceViewModel()
        {
        }

        public eBookCommerceViewModel(int bookId = 0, int genreId = 0, int? page = null)
        {
            var ebcDB = new eBookCommerceEntities();            

            user = ebcDB.AspNetUsers.SingleOrDefault(a => a.Email == HttpContext.Current.User.Identity.Name);

            if (bookId != 0)
            {
                book = ebcDB.Books.SingleOrDefault(a => a.bookId == bookId);
            }
            else
            {
                books = ebcDB.Books.Where(a => a.personId != user.Id && !a.Baskets.Any(b => b.personId == user.Id)).ToList();

                booksPagedList = books.ToPagedList(page ?? 1, 8);
            }

            genres = ebcDB.Genres.ToList();

            genresSelectList = getGenreSelectList();

            if (genreId != 0)
            {
                books = books.Where(a => a.genreId == genreId).ToList();

                booksPagedList = books.ToPagedList(page ?? 1, 8);
            }
        }   
        
        public List<SelectListItem> getGenreSelectList()
        {
            var ebcDB = new eBookCommerceEntities();

            genres = ebcDB.Genres.ToList();

            var genresSelectList = new List<SelectListItem>();

            foreach (var item in genres)
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

            return genresSelectList;
        }
    }
}