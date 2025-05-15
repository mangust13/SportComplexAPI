using Microsoft.AspNetCore.Mvc;

namespace SportComplexAPI.DTOs.PurchaseManager
{
    public class OrderDto
    {
        public int OrderId { get; set; }
        public int OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal OrderTotalPrice { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public List<PurchasedProductDto> PurchasedProducts { get; set; } = new();

    }

    public class PurchasedProductDto
    {
        public int PurchasedProductId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string ProductType { get; set; } = string.Empty;
        public List<DeliveryDto> Deliveries { get; set; } = new();
    }
    
    public class DeliveryDto
    {
        public int DeliveryId { get; set; }
        public DateTime DeliveryDate { get; set; }
        public int DeliveredQuantity { get; set; }
        public int GymNumber { get; set; }
        public string ComplexAddress { get; set; } = null!;
        public string ComplexCity { get; set; } = null!;
    }

    public class SupplierDto
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
    }

    public class BrandDto
    {
        public int BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;
    }
}
