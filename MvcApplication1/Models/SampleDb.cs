using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace MvcApplication1.Models
{
    /// <summary>
    /// Our main class to describe market event
    /// </summary>
    public class MarketEvent 
    {
         [Required]
        public global::System.Int32 MarketEventId { get; set; }

         [Required]
        public global::System.Int32 CompanyId { get; set; }

         public Company Company { get; set; }

        [Required]
        [StringLength(50)]
        public global::System.String Campaign { get; set; }

        [Required]
        [StringLength(50)]
        public global::System.String CampaignType { get; set; }

        [StringLength(50)]
        public global::System.String Description { get; set; }

        public Nullable<global::System.DateTime> StartDate { get; set; }
        public Nullable<global::System.DateTime> EndDate { get; set; }

        public global::System.Guid RowGuid { get; set; }

        [Required]
        [StringLength(50)]
        public global::System.String EventStatus { get; set; }

        
    }

    public class Company
    {
        public int CompanyId { get; set; }
        public string Name { get; set; }
    }

    public class Customer
    {
        public int CustomerId { get; set; }
         

        public global::System.Guid RowGuid { get; set; }

        public global::System.Int32 CompanyId { get; set; }

        public Company Company { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "First Name*")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name*")]
        public string LastName { get; set; }


        [StringLength(50)]
        [Display(Name = "Email")]
        public string EmailAddress { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Home Address*")]
        public string Address { get; set; }

        [StringLength(50)]
        [Display(Name = "Apt / Unit")]
        public string Address2 { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "City*")]
        public string City { get; set; }

        [Required]
        [StringLength(2)] //Select
        [Display(Name = "State*")]
        public string State { get; set; }

        [Required]
        [StringLength(10)]
        [Display(Name = "Zip Code*", Prompt = "Only Numbers")]
        public string PostalCode { get; set; }


    }


    public class User
    {
        public int UserId { get; set; }
        /// <summary>
        /// 0 for asp.net
        /// </summary>
        public int LoginMethod { get; set; }

        /// <summary>
        /// Reference key to authenticaiton source
        /// asp.net membership will be guid
        /// </summary>
        public string RefKey { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }

        public string Username { get; set; }


    }
    /// <summary>
    /// Individual record
    /// </summary>
    public class MarketEventItem
    {
         [Required]
        public global::System.Int32 MarketEventItemId { get; set; }
         [StringLength(50)]
        public string Barcode { get; set; }
        [Required]
        public int CustomerId { get; set; }
         [Required]
        public DateTime Created { get; set; }
        public DateTime Mailed { get; set; }
        public DateTime? Returned { get; set; }
    }


    public class ItemComment
    {
        [Required]
        public int Id { get; set; }
         [Required]
        public DateTime DateCreated { get; set; }
         [StringLength(140)]
        public string Content { get; set; }
         [Required]
        public int MarketEventItemId { get; set; }
        public MarketEventItem MarketEventItem { get; set; }

        /// <summary>
        /// dont need navig.
        /// </summary>
        public int UserId { get; set; }
        
    }

    public class EventComment
    {
        public int Id { get; set; }
         [Required]
        public DateTime DateCreated { get; set; }
        [StringLength(140)]
        public string Content { get; set; }
         [Required]
        public int MarketEventId { get; set; }
        public MarketEvent MarketEvent { get; set; }
        /// <summary>
        /// dont need navig.
        /// </summary>
        public int UserId { get; set; }
    }

    public class sp_GetCacheUpdateTime_view
    {
        public DateTime UpdateTime { get; set; }
    }


        public class MarketContext : DbContext
        {
            public MarketContext() 
            {
               
            }


            public MarketContext(string  connectionstr)
                : base(connectionstr)
            {
                //inject your connection string here 
            }

            public DbSet<MarketEvent> MarketEvents { get; set; }
            public DbSet<MarketEventItem> MarketEventItems { get; set; }
            public DbSet<ItemComment> ItemComments { get; set; }
            public DbSet<EventComment> EventComments { get; set; }
            public DbSet<User> Users { get; set; }
            public DbSet<Customer> Customers { get; set; }
            public DbSet<Company> Companys { get; set; }

            public IEnumerable<sp_GetCacheUpdateTime_view> ProcGetCacheUpdateTime(string tablename)
            {
                //with param
                //this.Database.SqlQuery<myEntityType>("sp_GetCacheUpdateTime @param1, @param2, @param3", new SqlParameter("param1", param1), new SqlParameter("param2", param2), new SqlParameter("param3", param3));
                //we are returning a select statement so it is best to get first or default here
                return this.Database.SqlQuery<sp_GetCacheUpdateTime_view>("sp_GetCacheUpdateTime @tablename",new SqlParameter("tablename",tablename));
            }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                // add your extra model rules here
                //we have seed db calls in our initializer class
            }
        }
}