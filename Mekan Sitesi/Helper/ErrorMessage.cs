using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mekan_Sitesi.Helper
{
    public class ErrorMessage
    {
        public const string VeriGirisi = "Bu emaile sahip bir kullanıcı sistemde bulunmakta!";
        public const string VeriGirisi1 = "Bu kullanıcı adı sistemde bulunmakta!";
        public const string ResimBasarisiz = ".gif, .jpg ya da .png uzantılı bir resim dosyası girmelisiniz.";
        public const string GirisBasarisiz = "Kullanıcı adı veya şifrenizi yanlış girdiniz. Ya da üyeliğinizi aktife etmemişsiniz.";
        public const string Location = "Lütfen haritadan adresi işaretleyin";
        public const string AyniYorum = "Bu mekanı daha önce değerlendirmişsiniz.<BR/>Bu nedenle bir daha değerlendiremezsiniz.";
        public const string KullaniciAd = "Kullanıcı adı girmelisiz.";
        public const string Ad = "Adınızı girmelisiniz.";
        public const string Soyad = "Soyadınızı girmelisiniz.";
        public const string Email = "Email adresinizi girmelisiniz.";
        public const string Sifre = "Şifre girmelisiniz.";
    }
}