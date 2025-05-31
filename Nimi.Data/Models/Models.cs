using Nimi.Core.Attributes;
using Nimi.Core.Entities;
using Nimi.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nimi.Data.Models
{
    public class Partner : EntityBase
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string Director { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public int Rating { get; set; }


        [Hidden]
        [EntityRelation(typeof(Sale))]
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();

        [NotMapped]
        [Hidden]
        public decimal TotalSales
            => Sales.Sum(s => s.TotalPrice);

        [NotMapped]
        public int Discount
        {
            get
            {
                UnitOfWork _uow;
                _uow = UowProvider.GetInstance();

                var sales =_uow.Sales
                    .Query()
                    .Where(s => s.PartnerId == Id)
                    .Include(s => s.Product)
                    .AsEnumerable();

                decimal total = sales.Sum(s => (s.Product?.Price ?? 0m) * s.Quantity);

                if (total < 10_000) return 0;
                if (total < 50_000) return 5;
                if (total < 300_000) return 10;
                return 15;
            }
        }
    }

    public class Product : EntityBase
    {
        public string Name { get; set; } = "";
        public decimal Price { get; set; }

        [Hidden]
        public int PartnerId { get; set; }

        [EntityRelation(typeof(Sale))]
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();

    }

    public class Sale : EntityBase
    {
        public DateTime Date { get; set; }
        public int Quantity { get; set; }

        public int PartnerId { get; set; }
        public Partner? Partner { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Hidden]
        [NotMapped]
        public decimal TotalPrice
            => (Product?.Price ?? 0m) * Quantity;
    }
}

