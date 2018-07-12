using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
                HttpCookie authorizationCookie = Request.Cookies.Get("authorization");

                if (authorizationCookie == null)
                {
                    return View("Login");
                }

                User currentUser = GetCurrentUser();
                if (currentUser == null)
                {
                    return View("Error");
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

                return View("Error");
            }
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

        //private ActionResult ReturnErrorView(string message)
        //{
        //    return 
        //}

        public ActionResult AddNote(string name, string content)
        {
            User currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return View("Error");
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
            if (currentUser == null)
            {
                return View("Error");
            }

            using (DrawMapModel context = new DrawMapModel())
            {
                Note note = context.Notes.SingleOrDefault(n => n.Id == noteId);
                if (note == null || note.UserId != currentUser.Id)
                {
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
            if (currentUser == null)
            {
                return View("Error");
            }

            using (DrawMapModel context = new DrawMapModel())
            {
                Note note = context.Notes.SingleOrDefault(n => n.Id == noteId);
                if (note == null || note.UserId != currentUser.Id)
                {
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
                    context.Users.Add(new User { Email = email, Password = password });
                    context.SaveChanges();
                }
                else if (dbUser.Password != password)
                {
                    return View("Error");
                }

                Response.Cookies.Add(new HttpCookie("authorization", String.Concat(email, " - ", password)));
                return RedirectToAction("Index");             
            }
        }
    }
}
