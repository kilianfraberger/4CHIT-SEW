using System;
using System.Collections.Generic;

namespace Database;

public partial class Test
{
    public int Id { get; set; }

    public string Value { get; set; } = null!;

    public override string ToString()
    {
        return Id + " " + Value;
    }   
}
