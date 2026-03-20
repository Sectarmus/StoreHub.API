# 🛒 StoreHub.API

StoreHub.API, C# ve .NET 10 (Preview) teknolojileri kullanılarak geliştirilmiş, sağlam bir E-Ticaret Backend çözümüdür. 

## 🚀 Proje Mimarisi (Neler Yaptık?)

Bu proje bir Junior yazılımcının portföyündeki en güçlü "Arka Uç (Backend)" taşlarından biridir:

*   **Veritabanı:** PostgreSQL (Entity Framework Core kullanılarak).
*   **Güvenlik:** JWT (JSON Web Tokens) ile Kimlik Doğrulama (Login/Register). Şifreler **BCrypt** ile korunmaktadır.
*   **Yetkilendirme:** Sınıflandırılmış Rol Mekanizması (`[Authorize(Roles="Admin")]`).
*   **Mimariler:**
    *   Sıfır amelelik için **AutoMapper**.
    *   Hızlı sorgular için `AsNoTracking()` ve `IQueryable` (Sayfalama - Pagination Desteği).
    *   Kompleks Sepet kayıtları için **Veritabanı İşlemleri (Transactions)**.
*   **Güvenilirlik:**
    *   Tüm form işlemleri **FluentValidation** tarafından kapıda denetlenir!
    *   Sistem genelinde fırlatılan tüm hataları anında Türkçe JSON formatına çeviren özel **Global Exception Middleware** sınıfı içerir.
*   **Medya:** Ürün fotoğrafı (`IFormFile`) ekleyebilme yeteneği.
*   **Erişilebilirlik:** Ön yüz (Frontend) tasarımlarının kapıdan içeri sorunsuzca girebilmesi için yapılandırılmış **CORS** kalkanı.


## 🛠 Kurulum ve Çalıştırma
1. Projeyi bilgisayarınıza indirin (`git clone`).
2. PostgreSQL kurun ve kendi bilgilerinizi `appsettings.json` altındaki `DefaultConnection`'a girin.
3. Terminalde sırasıyla çalıştırın:
    * `dotnet ef database update`
    * `dotnet run`
4. Proje Scalar/Swagger arayüzü eşliğinde ayağa kalkacaktır! Meydan sizin.

*Geliştirici:* **Alper (Sectarmus)** tarafından kodlanmıştır. 😎
