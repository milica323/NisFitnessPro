# 🏃‍♂️ Niš Fitness PRO
### *Real-time Fitness Tracker zasnovan na Redis Stack tehnologiji*

---

## 📖 Pregled projekta
**Niš Fitness PRO** je savremena web aplikacija za praćenje trkača u realnom vremenu. Projekat demonstrira napredne mogućnosti **Redis** baze podataka kao primarnog engine-a za obradu podataka koji zahtevaju ekstremno nisku latenciju, visoku dostupnost i kompleksne atomske operacije.

##  Arhitektura sistema
Aplikacija je izgrađena korišćenjem **troslojne arhitekture**:
1.  **Frontend:** SPA (Single Page Application) dizajniran u "Cyberpunk/Neon" stilu koristeći Tailwind CSS i Axios za komunikaciju sa API-jem.
2.  **Backend (.NET 8):** REST API koji koristi `StackExchange.Redis` klijent za upravljanje poslovnom logikom.
3.  **Data Layer (Redis):** In-memory baza koja služi za koordinaciju svih real-time funkcionalnosti.

## Zašto Redis? 
Ovaj projekat ne koristi Redis samo za keširanje, već implementira sledeće napredne koncepte:

* **Distribuirano zaključavanje (Locking):** Korišćenje `LockTakeAsync` mehanizma kako bi se osigurala konzistentnost pri ažuriranju globalnog rekorda u konkurentnom okruženju.
* **Redis Transakcije (MULTI/EXEC):** Grupisanje više operacija (upis u ZSET, GEO i LIST) u jednu atomičnu celinu.
* **Geoprostorni upiti (GEO):** Upotreba `GeoAdd` i `GeoRadius` struktura za implementaciju "radara" koji pronalazi trkače u krugu od 5km.
* **Sorted Sets (Leaderboard):** Automatsko rangiranje korisnika prema pretrčanoj distanci u realnom vremenu.
* **Pub/Sub Mehanizam:** Trenutno emitovanje obaveštenja (alerts) svim povezanim klijentima kada se postigne novi globalni rekord.
* **TTL (Time-To-Live):** Automatsko uklanjanje neaktivnih trkača sa radara nakon 30 minuta neaktivnosti radi optimizacije memorije.



##  Model podataka (Redis Structures)
- **`leaderboard` (Sorted Set):** Čuva rang listu (Score = km, Member = username).
- **`runner_locations` (Geo Set):** Čuva koordinate trkača za potrebe radara.
- **`activity_feed` (List):** Čuva poslednjih 10 aktivnosti (korišćenjem `LTRIM` komande).
- **`global_record` (String):** Čuva najveću pretrčanu distancu zaštićenu lock-om.
- **`fitness_alerts` (Channel):** Pub/Sub kanal za real-time notifikacije.

##  Realizovane operacije
-  **Atomični upis:** Transakciona obrada treninga.
-  **Radar pretraga:** Pronalaženje obližnjih korisnika putem geografskih koordinata.
- **Dynamic Leaderboard:** Automatsko osvežavanje top liste trkača.
- **Distributed Lock:** Sigurno upravljanje deljenim resursima.

## ⚙️ Pokretanje projekta

### 1. Redis (Docker)
```bash
docker run --name nis-fitness-redis -p 6379:6379 -d redis