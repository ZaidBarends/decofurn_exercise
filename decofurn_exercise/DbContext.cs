using Microsoft.EntityFrameworkCore;

public class InvoiceContext : DbContext
{
    public DbSet<InvoiceHeader> InvoiceHeaders { get; set; }
    public DbSet<InvoiceLine> InvoiceLines { get; set; }

    public InvoiceContext(DbContextOptions<InvoiceContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<InvoiceHeader>().ToTable("InvoiceHeader");
        modelBuilder.Entity<InvoiceLine>().ToTable("InvoiceLines");
        modelBuilder.Entity<InvoiceHeader>()
            .HasKey(i => i.InvoiceID); 
        modelBuilder.Entity<InvoiceLine>()
            .HasKey(i => i.LineId); 

        base.OnModelCreating(modelBuilder);
    }
}

public class InvoiceHeader
{
    public string InvoiceNumber { get; set; }
    public DateTime InvoiceDate { get; set; }
    public string Address { get; set; }
    public double InvoiceTotal { get; set; }
    public int InvoiceID { get; internal set; }
}

public class InvoiceLine
{
    public string InvoiceNumber { get; set; }
    public string Description { get; set; }
    public int Quantity { get; set; }
    public double UnitSellingPriceExVAT { get; set; }
    public int LineId { get; internal set; }
}
