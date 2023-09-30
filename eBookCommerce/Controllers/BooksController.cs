using eBookCommerce.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eBookCommerce.Controllers
{
    public class BooksController : Controller
    {
        eBookCommerceEntities ebcDB = new eBookCommerceEntities();

        // GET: Book       
        public ActionResult Index()
        {
            if (User.Identity.Name == "")
            {
                return RedirectToAction ("Login", "Account");
            }

            eBookCommerceViewModel viewModel = new eBookCommerceViewModel(0, 0);
            return View(viewModel);
        }

        public ActionResult CreateOrEditBook(int bookId = 0)
        {
            eBookCommerceViewModel viewModel = new eBookCommerceViewModel(bookId, 0);
            return PartialView("_BookFormContent", viewModel);
        }

        [HttpPost]
        public ActionResult CreateOrEditBook(eBookCommerceViewModel model, int bookId = 0)
        {
            if (ModelState.IsValid)
            {
                var s3BookImageUrl = Helpers.S3Helper.UploadFile(model.file);

                if (bookId == 0)
                {
                    Book book = new Book()
                    {
                        bookName = model.book.bookName,
                        bookAuthor = model.book.bookAuthor,
                        bookPages = model.book.bookPages,
                        bookPrice = model.book.bookPrice,
                        bookImageUrl = s3BookImageUrl,
                        bookDescription = model.book.bookDescription,
                        personId = model.user.Id,
                        genreId = model.book.genreId
                    };

                    ebcDB.Books.Add(book);
                    ebcDB.SaveChanges();                    
                }
                else
                {
                    var book = ebcDB.Books.Single(a => a.bookId == bookId);

                    book.bookName = model.book.bookName;
                    book.bookAuthor = model.book.bookAuthor;
                    book.bookPages = model.book.bookPages;
                    book.bookPrice = model.book.bookPrice;
                    book.bookImageUrl = s3BookImageUrl;
                    book.bookDescription = model.book.bookDescription;
                    book.personId = model.user.Id;
                    book.genreId = model.book.genreId;
                    ebcDB.SaveChanges();                    
                }

                return Json(true);
            }

            model.genresSelectList = model.getGenreSelectList();

            return PartialView("_BookFormContent", model);
        }

        [HttpPost]
        public JsonResult BooksToGrid(int draw, Dictionary<string, string> search, int start, int length)
        {
            var user = ebcDB.AspNetUsers.SingleOrDefault(a => a.Email == User.Identity.Name);
            var booksList = ebcDB.Books.Where(a => a.personId == user.Id).ToList();

            var recordsTotal = booksList.Count();

            string searchValue = search["value"].ToLower();

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                booksList = booksList.Where(
                    a => a.bookName.ToLower().Contains(searchValue) ||
                    a.bookAuthor.ToLower().Contains(searchValue)).ToList();
            }

            booksList = booksList.Skip(start).Take(length).ToList();
            var recordsFiltered = booksList.Count();

            var data = new List<object>();

            foreach (var book in booksList)
            {
                data.Add(new object[]
                {
                    book.bookName,
                    book.bookAuthor,
                    book.bookPages,
                    book.bookPrice,
                    "<img src='" + book.bookImageUrl + "' class='rounded-circle' style='width: 30px; height: 30px; object-fit: cover'>",
                    "<button type='button' class='btn btn-primary my-button' onclick='showBookFormModal(" + book.bookId + ")'>Edit Book</button>",
                    "<button type='button' class='btn btn-danger my-button' onclick='removeBook(" + book.bookId + ")'>Delete Book</button>"
                });
            }            

            var json = new
            {
                draw = draw,
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                data = data
            };

            return Json(json);
        }

        [HttpPost]
        public JsonResult RemoveBook(int bookId)
        {
            var book = ebcDB.Books.Single(a => a.bookId == bookId);
            ebcDB.Books.Remove(book);
            ebcDB.SaveChanges();
            return Json(true);
        }

        [HttpPost]
        public JsonResult RemoveAllBooks()
        {
            var user = ebcDB.AspNetUsers.SingleOrDefault(a => a.Email == User.Identity.Name);
            var books = ebcDB.Books.Where(a => a.personId == user.Id).ToList();

            foreach(var book in books)
            {
                ebcDB.Books.Remove(book);
            }
            
            ebcDB.SaveChanges();
            return Json(true);
        }  
    }
}