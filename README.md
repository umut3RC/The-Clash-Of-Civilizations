# The Clash Of Civilizations ⚔️

![Unity](https://img.shields.io/badge/Made%20with-Unity-2022-black?style=flat&logo=unity) ![Platform](https://img.shields.io/badge/Platform-WebGL-blue) ![Status](https://img.shields.io/badge/Status-In%20Development-orange)

**The Clash Of Civilizations** is a 1v1 online Real-Time Strategy (RTS) game developed with Unity and deployed via GitHub Pages.

🎮 **PLAY DEMO:** [**Click Here to Play in Browser**](https://umut3rc.github.io/The-Clash-Of-Civilizations/)

---

## 🇬🇧 ENGLISH INFO

### About The Project
The Clash Of Civilizations allows players to manage their economy, build structures, and command armies to destroy the opponent's main building. It utilizes Photon (PUN 2) for real-time multiplayer synchronization.

### Repository Structure & Assets
This repository is organized to separate the source code from the deployment build:
* **`main` Branch:** Contains the Unity **source project**.
    * *Note:* Paid assets and copyrighted packages have been excluded from this branch to comply with licensing agreements.
* **`gh-pages` Branch:** Contains the **WebGL build** files used for the live demo.

### Game Features
* **1v1 Online Multiplayer:** Real-time battles powered by Photon.
* **RTS Mechanics:** Economy management, unit production, and strategic combat.
* **Cross-Platform:** Playable directly in web browsers (Mobile/Desktop) via WebGL.

### Roadmap
* [ ] **New Civilizations:** Currently features a standard faction. Unique civilizations (e.g., Romans, Huns) will be added.
* [ ] UI/UX Overhaul.
* [ ] Matchmaking improvements.

### ⚠️ Technical Note (Server Limit)
This project currently runs on the **Photon Free Tier**.
* **Limit:** Maximum **20 Concurrent Users (CCU)**.
* If the server is full, you may experience delays in connecting or creating rooms.

---

## 🇹🇷 TÜRKÇE BİLGİ

### Proje Hakkında
**The Clash Of Civilizations**, Unity ile geliştirilmiş, 1'e 1 (1v1) online oynanabilen gerçek zamanlı bir strateji (RTS) oyunudur. Oyuncular kaynak toplayarak ekonomilerini yönetir, askeri birlikler üretir ve rakibin ana binasını yok etmeye çalışır.

### Repository Yapısı ve Dosyalar
Bu depo, kaynak kodları ve oynanabilir oyun dosyalarını ayırmak için şu şekilde yapılandırılmıştır:
* **`main` Branch'i:** Unity **kaynak kodlarını (source files)** içerir.
    * *Not:* Projede kullanılan ücretli market assetleri ve telifli paketler, lisans hakları nedeniyle bu branch'ten çıkarılmıştır.
* **`gh-pages` Branch'i:** GitHub Pages üzerinde çalışan **WebGL oyun çıktılarını (Build)** içerir.

### Özellikler
* **1v1 Online Savaş:** Photon altyapısı ile anlık çok oyunculu deneyim.
* **RTS Mekanikleri:** Ekonomi yönetimi, bina inşası ve ordu kontrolü.
* **Tarayıcı Tabanlı:** İndirme gerektirmeden direkt web üzerinden (WebGL) oynanabilir.

### Gelecek Planları (Roadmap)
* [ ] **Farklı Medeniyetler:** Şu an tek tip standart medeniyet var. İleride Roma, Hun gibi farklı özelliklere sahip ırklar eklenecektir.
* [ ] Arayüz (UI) ve Efekt geliştirmeleri.
* [ ] Lobi ve Eşleştirme sisteminin iyileştirilmesi.

### ⚠️ Teknik Not (Sunucu Limiti)
Oyun şu anda **Photon Ücretsiz Sürüm** altyapısını kullanmaktadır.
* **Limit:** Anlık maksimum **20 Oyuncu (CCU)**.
* Eğer sunucu kapasitesi doluysa, odaya bağlanırken beklemeniz gerekebilir.

---

### 🛠 Tech Stack / Teknolojiler
* **Engine:** Unity 2022+
* **Networking:** Photon Unity Networking 2 (PUN 2)
* **Platform:** WebGL
* **Hosting:** GitHub Pages
* **Input Support:** WebGLInput (Mobile Keyboard Support)

---
*Developed by Umut*