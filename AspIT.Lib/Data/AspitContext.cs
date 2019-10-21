using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace AspIT.Lib.Models
{
    public partial class AspitContext : IdentityDbContext<ApplicationUser>
    {
        public AspitContext()
        {
        }

        public AspitContext(DbContextOptions<AspitContext> options)
            : base(options)
        {
        }
     
        //public virtual DbSet<User> Users { get; set; }
       

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);           
            
        }      
    }
}