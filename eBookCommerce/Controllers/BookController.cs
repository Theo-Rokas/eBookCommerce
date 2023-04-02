using eBookCommerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eBookCommerce.Controllers
{
    public class BookController : Controller
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

        [HttpPost]
        public JsonResult CreateOrEditBook(eBookCommerceViewModel model, int bookId = 0)
        {
            if (ModelState.IsValid)
            {                
                // Add Mode
                if (bookId == 0)
                {
                    //Book book = model;

                    Book book = new Book()
                    {
                        //bookId = model.book.bookId,
                        bookName = model.book.bookName,
                        bookAuthor = model.book.bookAuthor,
                        bookPages = model.book.bookPages,
                        bookPrice = model.book.bookPrice,
                        bookImageUrl = model.book.bookImageUrl,
                        bookDescription = model.book.bookDescription,
                        personId = model.book.personId,
                        genreId = model.book.genreId
                    };

                    ebcDB.Books.Add(book);
                    ebcDB.SaveChanges();
                    return Json(true);
                }
                // Edit Mode
                else
                {
                    var book = ebcDB.Books.Single(a => a.bookId == bookId);
                    //book = model;

                    book.bookName = model.book.bookName;
                    book.bookAuthor = model.book.bookAuthor;
                    book.bookPages = model.book.bookPages;
                    book.bookPrice = model.book.bookPrice;
                    book.bookImageUrl = model.book.bookImageUrl;
                    book.bookDescription = model.book.bookDescription;
                    book.personId = model.book.personId;
                    book.genreId = model.book.genreId;
                    ebcDB.SaveChanges();
                    return Json(true);
                }
            }

            return Json(false);
        }

        [HttpPost]
        public JsonResult BooksToGrid(int draw)
        {
            var user = ebcDB.AspNetUsers.SingleOrDefault(a => a.Email == User.Identity.Name);

            var booksList = ebcDB.Books.Where(a => a.personId == user.Id).ToList();

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
                    //book.bookDescription,
                    "<button type='button' class='btn btn-primary' onclick='showBookFormModal(" + book.bookId + ")'>Edit Book</button>",
                    "<button type='button' class='btn btn-danger' onclick='removeBook(" + book.bookId + ")'>Delete Book</button>"
                    //removeItemFromBasket
                });
            }

            var totalRecords = data.Count();

            var json = new
            {
                draw = draw,
                recordsFiltered = totalRecords,
                recordsTotal = totalRecords,
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
        
        public ActionResult SingleBook(int bookId)
        {
            if (User.Identity.Name == "")
            {
                return RedirectToAction("Login", "Account");
            }

            eBookCommerceViewModel viewModel = new eBookCommerceViewModel(bookId);
            return View(viewModel);
        }
    }
}