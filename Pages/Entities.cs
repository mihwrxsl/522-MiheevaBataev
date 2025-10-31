using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _522_Miheeva
{
    [Table("Users")] // 🌟 ЯВНО УКАЗЫВАЕМ ИМЯ ТАБЛИЦЫ
    public class User
    {
        [Key]
        public int ID { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string FIO { get; set; }
        public byte[] Photo { get; set; }
    }

    public class Category
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }
    }

    public class Payment
    {
        [Key]
        public int ID { get; set; }
        public int CategoryID { get; set; }
        public int UserID { get; set; }
        public System.DateTime Date { get; set; }
        public string Name { get; set; }
        public int Num { get; set; }
        public decimal Price { get; set; }

        public virtual Category Category { get; set; }
        public virtual User User { get; set; }
    }

    public class PaymentEntities : DbContext
    {
        public PaymentEntities() : base("name=PaymentEntities")
        {
            // Отключаем проверку миграций
            Database.SetInitializer<PaymentEntities>(null);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Payment> Payments { get; set; }
    }
}