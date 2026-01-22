using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Code_First;

public class Example5
{
    public int Id { get; set; }
    public string Value1 { get; set; } //required by convention
    public string? Value2 { get; set; } //optional by convention
    [NotMapped]
    public string Value3 { get; set; }
    [Column(TypeName = "varchar(20)")]
    public string Value4 { get; set; }
    [MaxLength(25)]
    public string Value5 { get; set; }
    public string Value6 { get; set; }
    public string Value7 { get; set; }
    public string Value8 { get; set; }
    [Column("Value10")]
    public string Value9 { get; set; }
}