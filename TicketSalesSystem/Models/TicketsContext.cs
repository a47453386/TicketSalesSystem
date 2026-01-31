using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using TicketSalesSystem.Models;

namespace TicketSalesSystem.Models
{
    public class TicketsContext : DbContext
    {
        public TicketsContext(DbContextOptions<TicketsContext> options) : base(options)
        {
        }
        // ===== Programme =====
        public DbSet<Programme> Programme { get; set; }
        public DbSet<ProgrammeStatus> ProgrammeStatus { get; set; }
        public DbSet<DescriptionImage> DescriptionImage { get; set; }
       
        // ===== Member =====
        public DbSet<Member> Member { get; set; }
        public DbSet<MemberLogin> MemberLogin { get; set; }
        public DbSet<AccountStatus> AccountStatus { get; set; }

        // ===== Employee =====
        public DbSet<Employee> Employee { get; set; }
        public DbSet<EmployeeLogin> EmployeeLogin { get; set; }
        public DbSet<Role> Role { get; set; }

        // ===== Question / Reply =====
        public DbSet<Question> Question { get; set; }
        public DbSet<QuestionType> QuestionType { get; set; }
        public DbSet<Reply> Reply { get; set; }
        public DbSet<ReplyStatus> ReplyStatus { get; set; }

        // ===== FAQ / Notice =====
        public DbSet<FAQ> FAQ { get; set; }
        public DbSet<FAQType> FAQType { get; set; }
        public DbSet<PublicNotice> PublicNotice { get; set; }
        public DbSet<FAQPublishStatus> FAQPublishStatus { get; set; }

        // ===== Area =====
        public DbSet<Session> Session { get; set; }
        public DbSet<Place> Place { get; set; }
        public DbSet<SessionArea> SessionArea { get; set; }
        public DbSet<TicketsArea> TicketsArea { get; set; }

        // ===== Order / Payment =====
        public DbSet<Order> Order { get; set; }
        public DbSet<OrderStatus> OrderStatus { get; set; }
        public DbSet<Payment> Payment { get; set; }
        public DbSet<PaymentMethod> PaymentMethod { get; set; }
        public DbSet<PaymentStatus> PaymentStatus { get; set; }
        // ===== Ticket / Seat =====
        public DbSet<Tickets> Tickets { get; set; }        
        public DbSet<TicketsAreaStatus> TicketsAreaStatus { get; set; }
        public DbSet<TicketsStatus> TicketsStatus { get; set; }       
        public DbSet<Venue> Venue { get; set; }
        public DbSet<VenueStatus> VenueStatus { get; set; }
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmployeeLogin>()
                .HasKey(l => l.EmployeeID);  // 單一主鍵 EmployeeID

            modelBuilder.Entity<EmployeeLogin>()
                .HasIndex(l => l.Account)  // 保證帳號唯一
                .IsUnique();

            modelBuilder.Entity<MemberLogin>()
                .HasKey(l => l.MemberID);  // 單一主鍵 MemberID

            modelBuilder.Entity<MemberLogin>()
                .HasIndex(l => l.Account)  // 保證 Account 唯一
                .IsUnique();

            modelBuilder.Entity<DescriptionImage>()
               .HasKey(di => new { di.ProgrammeID, di.DescriptionImageID });

            // =========================
            // One-to-One
            // =========================
            modelBuilder.Entity<Member>()
                .HasOne(m => m.MemberLogin)
                .WithOne(l => l.Member)
                .HasForeignKey<MemberLogin>(l => l.MemberID);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.EmployeeLogin)
                .WithOne(l => l.Employee)
                .HasForeignKey<EmployeeLogin>(l => l.EmployeeID);           

            modelBuilder.Entity<Payment>()
                .HasOne(o => o.Order)
                .WithOne(p => p.Payment)
                .HasForeignKey<Order>(l => l.PaymentTradeNO);

            // =========================
            // One-to-Many
            // =========================

            // Programme相關
            modelBuilder.Entity<Employee>()
               .HasMany(e => e.Programme)
               .WithOne(a => a.Employee)
               .HasForeignKey(a => a.EmployeeID);

            modelBuilder.Entity<Place>()
                .HasMany(p => p.Programme)
                .WithOne(a => a.Place)
                .HasForeignKey(a => a.PlaceID);

            modelBuilder.Entity<ProgrammeStatus>()
               .HasMany(s => s.Programme)
               .WithOne(a => a.ProgrammeStatus)
               .HasForeignKey(a => a.ProgrammeStatusID);

            modelBuilder.Entity<Programme>()
                .HasMany(a => a.DescriptionImage)
                .WithOne(d => d.Programme)
                .HasForeignKey(d => d.ProgrammeID);

            modelBuilder.Entity<Programme>()
                .HasMany(a => a.Session)
                .WithOne(s => s.Programme)
                .HasForeignKey(s => s.ProgrammeID);


            // Question / Reply相關
            modelBuilder.Entity<Member>()
                .HasMany(m => m.Question)
                .WithOne(q => q.Member)
                .HasForeignKey(q => q.MemberID);

            modelBuilder.Entity<QuestionType>()
                .HasMany(t => t.Question)
                .WithOne(q => q.QuestionType)
                .HasForeignKey(q => q.QuestionTypeID);

            modelBuilder.Entity<Question>()
               .HasMany(q => q.Reply)
               .WithOne(r => r.Question)
               .HasForeignKey(r => r.QuestionID);

            modelBuilder.Entity<ReplyStatus>()
                .HasMany(s => s.Reply)
                .WithOne(r => r.ReplyStatus)
                .HasForeignKey(r => r.ReplyStatusID);

            modelBuilder.Entity<Employee>()
                .HasMany(e => e.Reply)
                .WithOne(r => r.Employee)
                .HasForeignKey(r => r.EmployeeID);


            //Role相關
            modelBuilder.Entity<Role>()
                .HasMany(m => m.Employee)
                .WithOne(o => o.Role)
                .HasForeignKey(o => o.RoleID);

            // FAQ / Notice相關
            modelBuilder.Entity<FAQType>()
             .HasMany(t => t.FAQ)
             .WithOne(f => f.FAQType)
             .HasForeignKey(f => f.FAQTypeID);

            modelBuilder.Entity<Employee>()
                .HasMany(e => e.FAQ)
                .WithOne(f => f.Employee)
                .HasForeignKey(f => f.EmployeeID);

            modelBuilder.Entity<FAQPublishStatus>()
                .HasMany(e => e.FAQ)
                .WithOne(f => f.FAQPublishStatus)
                .HasForeignKey(f => f.FAQPublishStatusID);

            modelBuilder.Entity<Employee>()
                .HasMany(e => e.PublicNotice)
                .WithOne(p => p.Employee)
                .HasForeignKey(p => p.EmployeeID);

            // Order / Payment相關
            modelBuilder.Entity<Member>()
                .HasMany(m => m.Order)
                .WithOne(o => o.Member)
                .HasForeignKey(o => o.MemberID);          

            modelBuilder.Entity<Session>()
                .HasMany(a => a.Order)
                .WithOne(o => o.Session)
                .HasForeignKey(o => o.SessionID);

            modelBuilder.Entity<OrderStatus>()
                .HasMany(s => s.Order)
                .WithOne(o => o.OrderStatus)
                .HasForeignKey(o => o.OrderStatusID);

            modelBuilder.Entity<PaymentMethod>()
                .HasMany(p => p.Order)
                .WithOne(o => o.PaymentMethod)
                .HasForeignKey(o => o.PaymentMethodID);

        
            modelBuilder.Entity<PaymentStatus>()
                .HasMany(p => p.Payment)
                .WithOne(o => o.PaymentStatus)
                .HasForeignKey(o => o.PaymentStatusID);


            // Ticket / Seat相關
            modelBuilder.Entity<Order>()
                .HasMany(o => o.Tickets)
                .WithOne(t => t.Order)
                .HasForeignKey(t => t.OrderID);
           

            modelBuilder.Entity<TicketsStatus>()
                .HasMany(s => s.Tickets)
                .WithOne(t => t.TicketsStatus)
                .HasForeignKey(t => t.TicketsStatusID);                  

            modelBuilder.Entity<TicketsAreaStatus>()
                .HasMany(s => s.TicketsArea)
                .WithOne(t => t.TicketsAreaStatus)
                .HasForeignKey(t => t.TicketsAreaStatusID);

            modelBuilder.Entity<Programme>()
               .HasMany(s => s.TicketsArea)
               .WithOne(t => t.Programme)
               .HasForeignKey(t => t.ProgrammeID)
               .OnDelete(DeleteBehavior.Restrict);



            // Area相關
            modelBuilder.Entity<Venue>()
               .HasMany(s => s.TicketsArea)
               .WithOne(t => t.Venue)
               .HasForeignKey(t => t.VenueID);

            modelBuilder.Entity<Venue>()
              .HasMany(s => s.Session)
              .WithOne(t => t.Venue)
              .HasForeignKey(t => t.VenueID);

            modelBuilder.Entity<VenueStatus>()
              .HasMany(s => s.Venue)
              .WithOne(t => t.VenueStatus)
              .HasForeignKey(t => t.VenueStatusID);

            modelBuilder.Entity<Place>()
              .HasMany(s => s.Venue)
              .WithOne(t => t.Place)
              .HasForeignKey(t => t.PlaceID);

            modelBuilder.Entity<Session>()
              .HasMany(s => s.TicketsArea)
              .WithOne(t => t.Session)
              .HasForeignKey(t => t.SessionID)
              .OnDelete(DeleteBehavior.Restrict);

            // AccountStatus相關
            modelBuilder.Entity<Member>()
               .HasOne(m => m.AccountStatus)
               .WithMany(a => a.Member)
               .HasForeignKey(m => m.AccountStatusID)
                .OnDelete(DeleteBehavior.Restrict);  // 避免 cascade delete

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.AccountStatus)
                .WithMany(a => a.Employee)
                .HasForeignKey(e => e.AccountStatusID)
                .OnDelete(DeleteBehavior.Restrict);



            // =========================
            // Many-to-Many
            // =========================
            // 1. 設定複合主鍵 (Composite Key)            
            modelBuilder.Entity<SessionArea>()
                .HasKey(sa => new { sa.SessionID, sa.TicketsAreaID });

            // 2. 設定 Session 與 SessionArea 的一對多關係
            modelBuilder.Entity<SessionArea>()
                .HasOne(sa => sa.Session)
                .WithMany(s => s.SessionArea) // 請確保 Session Model 裡有 ICollection<SessionArea> SessionAreas
                .HasForeignKey(sa => sa.SessionID)
                .OnDelete(DeleteBehavior.Cascade); // 如果刪除場次，對應的票價區域設定也一起刪除

            // 3. 設定 TicketsArea 與 SessionArea 的一對多關係
            modelBuilder.Entity<SessionArea>()
                .HasOne(sa => sa.TicketsArea)
                .WithMany(ta => ta.SessionArea) // 請確保 TicketsArea Model 裡有 ICollection<SessionArea> SessionAreas
                .HasForeignKey(sa => sa.TicketsAreaID)
                .OnDelete(DeleteBehavior.Restrict); // 為了安全，如果票區還在使用中，限制刪除


            // =========================
            // Column Type Override
            // =========================
            modelBuilder.Entity<SessionArea>()
                .Property(t => t.Price)
                .HasColumnType("money");



            base.OnModelCreating(modelBuilder);

        }
        public DbSet<TicketSalesSystem.Models.Seats> Seats { get; set; } = default!;
    }
}
