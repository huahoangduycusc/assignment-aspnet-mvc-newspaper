//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Project.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class follow
    {
        public int follow_id { get; set; }
        public Nullable<int> from_user { get; set; }
        public Nullable<int> to_user { get; set; }
    
        public virtual User User { get; set; }
    }
}
