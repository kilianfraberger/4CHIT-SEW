using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Code_First;

[PrimaryKey(nameof(Nr))]
public class Example2
{
    //[Key]
    public int Nr { get; set; }
    public string Value { get; set; }
}