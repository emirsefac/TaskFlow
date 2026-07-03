# TaskFlow — Görev Yönetim Sistemi (Mini Jira)

ASP.NET Core 8 Web API + Entity Framework Core + SQL Server + SignalR ile geliştirilmiş,
JWT kimlik doğrulamalı, gerçek zamanlı bildirim destekli görev yönetim sistemi.

## Özellikler

- **JWT Authentication** — Kayıt/giriş, BCrypt ile şifre hash'leme
- **Proje yönetimi** — Proje oluşturma, email ile üye ekleme, rol bazlı yetki (Owner/Member)
- **Görev yönetimi** — Kanban durumları (Backlog → InProgress → Review → Done), öncelik, atama, son tarih
- **Yorum sistemi** — Görevlere yorum ekleme
- **SignalR ile gerçek zamanlı bildirim** — Görev oluşturulduğunda/durumu değiştiğinde proje üyelerine anlık bildirim
- **Katmanlı mimari** — Core / Infrastructure / API ayrımı, Repository Pattern, Dependency Injection
- **Global hata yönetimi** — Middleware ile merkezi exception handling
- **Swagger** — JWT destekli interaktif API dokümantasyonu
- **Hazır web arayüzü (Kanban board)** — Uygulama çalışınca ana adreste açılan,
  sürükle-bırak destekli görev panosu; SignalR ile canlı güncellenir

## Mimari

```
TaskFlow.Core            → Entity'ler, DTO'lar, Enum'lar, Interface'ler (hiçbir dış bağımlılık yok)
TaskFlow.Infrastructure  → EF Core DbContext, Repository'ler, Servisler (iş mantığı)
TaskFlow.API             → Controller'lar, SignalR Hub, Middleware, Program.cs
```

Bağımlılık yönü: **API → Infrastructure → Core** (Core hiçbir katmana bağımlı değildir).
SignalR bildirimleri `ITaskNotifier` soyutlaması üzerinden gönderilir; böylece iş mantığı
katmanı SignalR'ı doğrudan bilmez (Dependency Inversion Principle).

## Veritabanı Şeması

```
Users ──< ProjectMembers >── Projects
Users ──< TaskItems (AssignedUser, nullable)
Projects ──< TaskItems
TaskItems ──< Comments >── Users
```

- `ProjectMembers`: User–Project many-to-many ara tablosu, `(ProjectId, UserId)` unique index
- `Email` alanı unique index
- Enum'lar veritabanında string olarak saklanır (okunabilirlik için)

## Kurulum

### Gereksinimler
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB, Express veya tam sürüm)
- Visual Studio 2022 (veya VS Code + C# eklentisi)

### Adımlar

1. **Projeyi aç:** `TaskFlow.sln` dosyasına çift tıkla (Visual Studio açılır)

2. **Connection string'i ayarla:** `TaskFlow.API/appsettings.json` içindeki
   `DefaultConnection` değerini kendi SQL Server'ına göre düzenle.
   - LocalDB kullanıyorsan: `Server=(localdb)\\MSSQLLocalDB;Database=TaskFlowDb;Trusted_Connection=True;`
   - SQL Express: `Server=.\\SQLEXPRESS;Database=TaskFlowDb;Trusted_Connection=True;TrustServerCertificate=True;`

3. **Veritabanını oluştur** (Package Manager Console — Tools > NuGet Package Manager):
   ```
   Add-Migration InitialCreate -Project TaskFlow.Infrastructure -StartupProject TaskFlow.API
   Update-Database -Project TaskFlow.Infrastructure -StartupProject TaskFlow.API
   ```
   Ya da terminalden:
   ```
   dotnet tool install --global dotnet-ef
   dotnet ef migrations add InitialCreate --project TaskFlow.Infrastructure --startup-project TaskFlow.API
   dotnet ef database update --project TaskFlow.Infrastructure --startup-project TaskFlow.API
   ```

4. **Çalıştır:** F5'e bas (veya `dotnet run --project TaskFlow.API`).
   Tarayıcıda Swagger açılır: `https://localhost:xxxx/swagger`

### Web arayüzü (Kanban board)

Uygulama çalıştıktan sonra ana adrese git: `https://localhost:xxxx/`
(Swagger yerine kök adres). Karşına giriş ekranı çıkar:

1. **Kayıt ol** → otomatik giriş yapılır
2. **+ Yeni Proje** ile proje oluştur
3. **+ Yeni Görev** ile görev ekle, kartları sütunlar arasında **sürükle-bırak** ile taşı
4. **SignalR canlı demo:** İki farklı tarayıcı (veya gizli sekme) aç, iki farklı
   kullanıcıyla aynı projeye gir. Birinde görevi taşıdığında diğerinde board'un
   **anında güncellendiğini** ve bildirim geldiğini görürsün. Mülakatta göstermelik
   en etkileyici özellik budur.

### Swagger'dan test etme

1. `POST /api/auth/register` ile kullanıcı oluştur → dönen `token` değerini kopyala
2. Sağ üstteki **Authorize** butonuna tıkla → `Bearer {token}` yapıştır
3. Artık tüm endpoint'leri deneyebilirsin:
   - `POST /api/projects` → proje oluştur
   - `POST /api/projects/{id}/tasks` → görev ekle
   - `PATCH /api/tasks/{id}/status` → görevi Kanban'da taşı (SignalR bildirimi tetiklenir)

## API Endpoint'leri

| Metot | Endpoint | Açıklama |
|-------|----------|----------|
| POST | /api/auth/register | Kayıt ol + JWT al |
| POST | /api/auth/login | Giriş yap + JWT al |
| GET | /api/projects | Projelerimi listele |
| GET | /api/projects/{id} | Proje detayı (üyeler + görevler) |
| POST | /api/projects | Proje oluştur |
| POST | /api/projects/{id}/members | Üye ekle (sadece Owner) |
| GET | /api/projects/{id}/tasks | Projenin görevleri |
| GET | /api/tasks/{id} | Görev detayı |
| POST | /api/projects/{id}/tasks | Görev oluştur ⚡ SignalR |
| PUT | /api/tasks/{id} | Görev güncelle |
| PATCH | /api/tasks/{id}/status | Durum değiştir ⚡ SignalR |
| PATCH | /api/tasks/{id}/assign | Görev ata |
| DELETE | /api/tasks/{id} | Görev sil (sadece Owner) |
| GET | /api/tasks/{id}/comments | Yorumları listele |
| POST | /api/tasks/{id}/comments | Yorum ekle |

SignalR hub adresi: `/hubs/tasks` — istemci `JoinProject(projectId)` çağırarak
o projenin bildirim grubuna katılır; `TaskCreated` ve `TaskStatusChanged`
event'lerini dinler.

## Tasarım Kararları

- **Neden katmanlı mimari?** Test edilebilirlik ve bağımlılık yönetimi. Core katmanı
  hiçbir framework'e bağımlı değil; yarın EF Core yerine Dapper'a geçsem sadece
  Infrastructure değişir.
- **Repository Pattern neden var?** Veri erişimini soyutlar; servisler DbContext'i
  doğrudan bilmez, mock'lanabilir interface'lerle çalışır.
- **ITaskNotifier neden Core'da interface, API'de implementasyon?** Dependency
  Inversion — iş mantığı "bildirim gönder" der ama nasıl gönderileceğini
  (SignalR, email, push...) bilmez.
- **Güvenlik detayları:** Şifreler BCrypt ile hash'lenir; login hatasında
  "email mi şifre mi yanlış" bilgisi sızdırılmaz (user enumeration önlemi);
  her endpoint'te proje üyeliği kontrolü yapılır (yatay yetki kontrolü).
- **Enum'lar neden string saklanıyor?** Veritabanına bakan birinin `Status = 2`
  yerine `Status = 'Review'` görmesi bakımı kolaylaştırır.
