using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DrawMap.Models;

namespace DrawMap.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            using (DrawMapModel context = new DrawMapModel())
            {
                User currentUser = GetCurrentUser();
                if (currentUser == null || !UserHasLoggedIn(currentUser.Id))
                {
                    return View("Login", new LogInModel());
                }

                return View(context.Notes.Where(n => n.UserId == currentUser.Id).ToList());
            }
        }

        public ActionResult ViewMap(int noteId)
        {
            using (DrawMapModel context = new DrawMapModel())
            {
                Note note = context.Notes.SingleOrDefault(n => n.Id == noteId);
                if (note != null)
                {
                    return View("ViewMap", note);
                }

                ViewBag.ErrorMessage = "Note with id = '" + noteId + "' does not exist";
                return View("Error");
            }
        }

        public ActionResult AddNote(string name, string content)
        {
            User currentUser = GetCurrentUser();
            if (currentUser == null || !UserHasLoggedIn(currentUser.Id))
            {
                return View("Login", new LogInModel());
            }

            using (DrawMapModel context = new DrawMapModel())
            {
                context.Notes.Add(new Note {Name = name, Content = content, UserId = currentUser.Id});
                context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public ActionResult RenameNote(int noteId, string name)
        {
            User currentUser = GetCurrentUser();
            if (currentUser == null || !UserHasLoggedIn(currentUser.Id))
            {
                return View("LogIn", new LogInModel());
            }

            using (DrawMapModel context = new DrawMapModel())
            {
                Note note = context.Notes.SingleOrDefault(n => n.Id == noteId);
                if (note == null || note.UserId != currentUser.Id)
                {
                    ViewBag.ErrorMessage = "Note with id = '" + noteId +
                                           "' does not exist or it is unavailable for the current user";
                    return View("Error");
                }

                note.Name = name;
                context.SaveChanges();

                return RedirectToAction("Index");
            }
        }

        public ActionResult DeleteNote(int noteId)
        {
            User currentUser = GetCurrentUser();
            if (currentUser == null || !UserHasLoggedIn(currentUser.Id))
            {
                return View("LogIn", new LogInModel());
            }

            using (DrawMapModel context = new DrawMapModel())
            {
                Note note = context.Notes.SingleOrDefault(n => n.Id == noteId);
                if (note == null || note.UserId != currentUser.Id)
                {
                    ViewBag.ErrorMessage = "Note with id = '" + noteId +
                                           "' does not exist or it is unavailable for the current user";
                    return View("Error");
                }

                context.Notes.Remove(note);
                context.SaveChanges();

                return RedirectToAction("Index");
            }
        }

        public ActionResult LogIn(string email, string password)
        {
            using (DrawMapModel context = new DrawMapModel())
            {
                User dbUser = context.Users.SingleOrDefault(u => u.Email == email);

                if (dbUser == null)
                {
                    dbUser = new User {Email = email, Password = password};
                    context.Users.Add(dbUser);
                    context.SaveChanges();
                }
                else if (dbUser.Password != password)
                {
                    return View("Login",
                        new LogInModel
                        {
                            HasError = true,
                            ErrorMessage = "User with the same login exists, but the password is incorrect"
                        });
                }

                Response.Cookies.Add(new HttpCookie("authorization", String.Concat(email, " - ", password)));
                Session.Add(dbUser.Id.ToString(), true);

                return RedirectToAction("Index");             
            }
        }

        public ActionResult SignOut()
        {
            User currentUser = GetCurrentUser();
            if (currentUser != null)
            {
                Session[currentUser.Id.ToString()] = false;
            }

            return RedirectToAction("Index");
        }

        private bool UserHasLoggedIn(int userId)
        {
            return (Session[userId.ToString()] != null) && (bool)Session[userId.ToString()];
        }

        private User GetCurrentUser()
        {
            HttpCookie authorizationCookie = Request.Cookies.Get("authorization");

            if (authorizationCookie == null)
            {
                return null;
            }

            int separatorPosition = authorizationCookie.Value.IndexOf(" - ");

            string email = authorizationCookie.Value.Substring(0, separatorPosition);
            string password = authorizationCookie.Value.Substring(separatorPosition + 3);

            using (DrawMapModel context = new DrawMapModel())
            {
                return context.Users.SingleOrDefault(u => u.Email == email && u.Password == password);
            }
        }
    }
}
