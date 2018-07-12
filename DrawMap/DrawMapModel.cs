namespace DrawMap
{
    using System;
    using System.Data.Entity;
    using System.Linq;

    public class DrawMapModel : DbContext
    {
        // Your context has been configured to use a 'DrawMapModel' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'DrawMap.DrawMapModel' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'DrawMapModel' 
        // connection string in the application configuration file.
        public DrawMapModel()
            : base("name=DrawMapModel")
        {
        }

        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Note> Notes { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class Note
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public int UserId { get; set; }
    }
}