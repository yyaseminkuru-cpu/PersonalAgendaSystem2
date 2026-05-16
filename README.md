# Personal Agenda System

Kisisel gorev ve ajanda takip sistemi. Proje ASP.NET MVC, Entity Framework DB First ve SQL Server LocalDB kullanilarak gelistirildi.

## Kullanilan Teknolojiler

- ASP.NET MVC
- Entity Framework 6 DB First
- SQL Server LocalDB
- Bootstrap
- LINQ
- DataAnnotations

## Veritabani Yapisi

Proje DB First yaklasimi ile olusturuldu. Model dosyasi:

```text
PersonalAgendaSystem/Models/AgendaModel.edmx
```

Tablolar:

```text
Users
Categories
AgendaItems
```

Bu projede `AgendaItems` tablosu gorev kayitlarini temsil eder. Isterlerdeki `Tasks`
tablosunun karsiligi olarak kullanilmistir. `EndDate` alani da gorevin bitis/teslim
tarihi, yani `DueDate` mantigi ile kullanilir.

Iliskiler:

```text
Users 1 - N AgendaItems
Categories 1 - N AgendaItems
```

## Test Giris Bilgileri

Admin:

```text
E-posta: admin5109@gmail.com
Sifre: 5109
```

Normal kullanici:

```text
E-posta: user1234@gmail.com
Sifre: 1234
```

## Ozellikler

- Kullanici girisi
- Admin ve kullanici rol ayrimi
- Gorev listeleme
- Gorev ekleme
- Gorev detay goruntuleme
- Gorev duzenleme
- Gorev silme yerine pasife alma
- Gorev tamamlama
- Admin gorev onaylama
- Gorev basligi ve aciklamasina gore arama
- Oncelik, durum ve tarih filtreleme
- Tarihe gore siralama
- Takvim gorunumu
- Onceki/sonraki ay gecisi
- Admin kullanici listeleme
- Admin kullanici ekleme
- Admin kullanici duzenleme
- Admin kullaniciyi pasife alma
- Kullanici arama ve role gore filtreleme
- DataAnnotations ile validation

## Yetkilendirme

- Giris yapmayan kullanici gorev ve kullanici yonetimi ekranlarina erisemez.
- Normal kullanici sadece kendi gorevlerini gorur.
- Normal kullanici kullanici yonetimi ekranina erisemez.
- Admin tum gorevleri gorebilir ve tamamlanan gorevleri onaylayabilir.
- Admin kullanici yonetimi islemlerini yapabilir.

## Calistirma

1. Visual Studio ile `PersonalAgendaSystem.slnx` dosyasini acin.
2. SQL Server Object Explorer uzerinden LocalDB baglantisini kontrol edin.
3. Gerekirse `Models/AgendaModel.edmx.sql` scriptini calistirin.
4. Projeyi IIS Express ile calistirin.
5. Login ekranindan test kullanicilariyla giris yapin.

## Gorev Dagilimi

Yasemin:

```text
Controllers/UsersController.cs
Views/Users/*
Kullanici yonetimi, arama, rol filtreleme, soft delete
```

Melek:

```text
Controllers/AgendaController.cs
Views/Agenda/*
Gorev yonetimi, filtreleme, takvim, tamamlama ve onaylama
```
