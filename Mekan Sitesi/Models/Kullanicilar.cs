//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Mekan_Sitesi.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Kullanicilar
    {
        public int ID { get; set; }
        public string KullaniciAd { get; set; }
        public string Sifre { get; set; }
        public string Ad { get; set; }
        public string Soyad { get; set; }
        public System.DateTime KatilimTarihi { get; set; }
        public string Email { get; set; }
        public bool EmailOnay { get; set; }
    }
}
