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
    
    public partial class Genres
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Genres()
        {
            this.MoviesGenres = new HashSet<MoviesGenres>();
        }
    
        public int GenreID { get; set; }
        public string Name { get; set; }
    
        public virtual Genres Genres1 { get; set; }
        public virtual Genres Genres2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MoviesGenres> MoviesGenres { get; set; }
    }
}
