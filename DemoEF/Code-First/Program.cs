// See https://aka.ms/new-console-template for more information

using Code_First;
using Microsoft.EntityFrameworkCore;

class main
{
    public static void Main(string[] args){}
}

public partial class ExampleContext : DbContext
{
    public ExampleContext()
    {

    }

    public ExampleContext(DbContextOptions<ExampleContext> options)
        : base(options)
    {

    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySQL(@"Server=127.0.0.1;uid=root;pwd=insy;database=demo");

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    { //FLUENT api to configure our Database Model
        modelBuilder.Entity<Example4>(entity =>
        {
            entity.HasKey(e => e.Nr).HasName("Primary");
        });
        modelBuilder.Entity<Example5>(entity =>
        {
            //entity.Ignore(x=>x.Value3);
            //entity.Property(t=>t.Value5).HasColumnType("varchar(20)");
            entity.Property(e => e.Value6).HasColumnName("Value11");
        });
    }
    
    public virtual DbSet<Example> Examples { get; set; } //PrimaryKey wird durch den Namen "Id" automatisch erkannt
    public virtual DbSet<Example2> Examples2 { get; set; } //PrimaryKey manuell mit [Key] / [PrimaryKey(nameof(Nr))] definiert
    public virtual DbSet<Example3> Examples3 { get; set; } //PrimaryKey wird durch den Namen "id" automatisch erkannt
    public virtual DbSet<Example4> Examples4 { get; set; } //PrimaryKey wird durch FLUENT api konfiguriert
    public virtual DbSet<Example5> Examples5 { get; set; }
}

// dotnet tool install --global dotnet-ef
// dotnet ef migrations add InitalCreate 
// dotnet ef database update  