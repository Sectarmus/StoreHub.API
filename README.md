# StoreHub.API

StoreHub.API, C# ve .NET 10 teknolojileri kullanılarak geliştirilmiş kapsamlı bir E-Ticaret REST API (Backend) çözümüdür. Kullanılan modern yazılım mimarileri sayesinde yüksek ölçeklenebilirlik, performans ve güvenlik amaçlanmıştır.

## Proje Altyapısı ve Özellikler

Bu proje, sektör standartlarında modern bir Web API geliştirme pratiğini yansıtacak şekilde inşa edilmiştir:

*   **Veritabanı ve ORM:** PostgreSQL veritabanı altyapısı, Entity Framework Core (Code First yaklaşımı). Yük getirmeyen veri listelemeleri için `AsNoTracking()` mimarisi.
*   **Güvenlik ve Kimlik Doğrulama:** BCrypt kütüphanesi kullanılarak sağlanan şifreleme ve JWT (JSON Web Token) tabanlı Role-based Authorization tablosu.
*   **Veri Bütünlüğü:** 
    * Tüm HTTP veri akışları DTO (Data Transfer Object) deseni üzerinden gerçekleşmekte ve AutoMapper kütüphanesi ile eşleştirilmektedir.
    * Gelen kullanıcı verileri özel katmanlar aracılığıyla uçtan uca FluentValidation üzerinden süzülüp filtrelemeye tabi tutulmaktadır.
    * Sipariş ve finans işlemleri, tutarlılığı garantiye almak amacıyla Transaction bloklarıyla (`BeginTransactionAsync`) güven altına alınmıştır.
*   **Hata Yönetimi (Error Handling):** Sistemin hiçbir aşamasında try-catch yığınları kullanılmamış; uygulamanın tüm istisnai durumları merkezi Global Exception Middleware üzerinden tek bir JSON nesnesi formatında istemciye yansıtılmıştır.
*   **Medya Yönetimi:** Ürün fotoğrafı (`IFormFile`) işlemleri için tam donanımlı dosya yükleme mekanizması.
*   **Erişilebilirlik:** Ön yüz (React, Vue vb.) entegrasyonu için yapılandırılmış Global CORS mekanizması.

## Kurulum ve Çalıştırma Seçenekleri

Aşağıdaki adımları takip ederek projeyi yerel ortamınızda çalıştırabilirsiniz:

1. Depoyu bilgisayarınıza klonlayın.
2. PostgreSQL veritabanınızda bir şema oluşturun ve proje içerisindeki `appsettings.json` altındaki `DefaultConnection` satırını kendi ortamınıza göre güncelleyin.
3. Terminalde `StoreHub.API` dizinine giderek sırasıyla şu komutları çalıştırın:
    * `dotnet clean` (İsteğe bağlı)
    * `dotnet ef database update`
    * `dotnet run`
4. Uygulama çalıştıktan sonra Scalar OpenAPI arayüzü ile dökümantasyon sayfasına ulaşabilirsiniz.

Geliştirici: Alper (Sectarmus)
