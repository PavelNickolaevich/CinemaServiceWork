//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CinemaServiceWork
{
    using System;
    using System.Collections.Generic;
    
    public partial class Favorites
    {
        public int FavoritID { get; set; }
        public int UserID { get; set; }
        public int MovieID { get; set; }
    
        public virtual Movies Movies { get; set; }
        public virtual Users Users { get; set; }
    }
}
