//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace eBookCommerce.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Basket
    {
        public int basketId { get; set; }
        public string personId { get; set; }
        public Nullable<int> bookId { get; set; }
        public Nullable<int> paymentId { get; set; }
    
        public virtual AspNetUser AspNetUser { get; set; }
        public virtual Book Book { get; set; }
        public virtual Payment Payment { get; set; }
    }
}
