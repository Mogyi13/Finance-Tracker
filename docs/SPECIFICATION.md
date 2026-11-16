# Smart Finance
## Végleges rendszer-specifikáció és GitHub fejlesztési backlog

**Verzió:** 1.0  
**Projekt típusa:** Egyetemi szakdolgozati projekt  
**Projektméret:** Közepesen összetett  
**Állapot:** Fejlesztésre előkészítve

---

# I. Rendszer-specifikáció

## 1. Projektáttekintés

### 1.1. A rendszer célja

A Smart Finance egy személyes pénzügyi nyilvántartó és elemző webalkalmazás.

A rendszer lehetővé teszi a felhasználók számára bevételeik és kiadásaik nyilvántartását, pénzügyi szokásaik elemzését, költségkereteik kezelését, valamint jövőbeli kiadásaik becslését.

A hagyományos pénzügyi nyilvántartást intelligens adatfeldolgozási funkciók egészítik ki:

- Automatikus tranzakciókategorizálás.
- Gépi tanulás alapú kategorizálás.
- Szokatlan költések felismerése.
- Ismétlődő kiadások azonosítása.
- Pénzügyi trendek elemzése.
- Kiadások előrejelzése.

A projekt szakdolgozati célja az egyes algoritmusok működésének és hatékonyságának vizsgálata, valamint a szekvenciális és párhuzamos adatfeldolgozás összehasonlítása.

### 1.2. Célközönség

A rendszer magánszemélyek számára készül, akik szeretnék:
- Nyomon követni bevételeiket és kiadásaikat.
- Átlátni költési szokásaikat.
- Költségkereteket meghatározni.
- Felismerni a szokatlan pénzügyi eseményeket.
- Megtervezni jövőbeli kiadásaikat.

### 1.3. A rendszer hatóköre

A rendszer egy többfelhasználós, reszponzív webalkalmazás.

Az első verzió nem tartalmaz:
- Közvetlen banki API-integrációt.
- Automatikus bankszámla-szinkronizációt.
- Natív Android vagy iOS alkalmazást.
- Mikroservice architektúrát.
- Komplex befektetés- és portfóliókezelést.
- Saját neurális hálózat fejlesztését.

---

## 2. Technológiai döntések

| Terület | Véglegesített technológia |
|---|---|
| Programozási nyelv | C# |
| Backend | ASP.NET Core Web API |
| Frontend | Blazor WebAssembly |
| ORM | Entity Framework Core |
| Adatbázis | Microsoft SQL Server |
| Hitelesítés | ASP.NET Core Identity + JWT |
| Gépi tanulás | ML.NET |
| Háttérfeldolgozás | .NET BackgroundService |
| Tesztelés | xUnit |
| Verziókezelés | Git, GitHub |
| API dokumentáció | OpenAPI / Swagger |

**Frontend-megjegyzés:** a Blazor WebAssembly választása illeszkedik a különálló ASP.NET Core Web API architektúrához. A frontend HTTP-kéréseken keresztül kommunikál a backenddel.

---

## 3. Architektúra

### 3.1. Alapelvek

A rendszer az alábbi elvek alapján készül:

- Clean Architecture.
- Domain-Driven Design (DDD).
- Modular Monolith.
- Dependency Injection.
- Egyszerű CQRS-szemlélet.
- Repository és Unit of Work minták ott, ahol indokolt.

A rendszer egyetlen alkalmazásként telepíthető, de a belső modulok felelőssége elkülönül.

Nem cél a mikroservice architektúra kialakítása.

### 3.2. Solution struktúra

```text
SmartFinance/
│
├── src/
│   ├── SmartFinance.Domain/
│   │   ├── Common/
│   │   ├── Users/
│   │   ├── Transactions/
│   │   ├── Categories/
│   │   ├── Budgets/
│   │   ├── Categorization/
│   │   ├── Analytics/
│   │   └── Forecasting/
│   │
│   ├── SmartFinance.Application/
│   │   ├── Common/
│   │   ├── Authentication/
│   │   ├── Transactions/
│   │   ├── Categories/
│   │   ├── Budgets/
│   │   ├── Imports/
│   │   ├── Categorization/
│   │   ├── Analytics/
│   │   └── Forecasting/
│   │
│   ├── SmartFinance.Infrastructure/
│   │   ├── Persistence/
│   │   ├── Identity/
│   │   ├── Repositories/
│   │   ├── MachineLearning/
│   │   ├── ImportProcessing/
│   │   └── BackgroundJobs/
│   │
│   ├── SmartFinance.API/
│   │   ├── Controllers/
│   │   ├── Middleware/
│   │   └── Configuration/
│   │
│   └── SmartFinance.Web/
│       ├── Components/
│       ├── Pages/
│       ├── Layout/
│       └── Services/
│
├── tests/
│   ├── SmartFinance.Domain.Tests/
│   ├── SmartFinance.Application.Tests/
│   ├── SmartFinance.Infrastructure.Tests/
│   └── SmartFinance.API.Tests/
│
├── docs/
│   ├── SPECIFICATION.md
│   ├── ARCHITECTURE.md
│   ├── DATABASE.md
│   └── USE_CASES.md
│
├── README.md
└── .gitignore
```

### 3.3. Rétegek felelőssége

**Domain**
- Entitások.
- Value Objectek.
- Üzleti szabályok.
- Aggregate-ek.
- Domain Service-ek.

**Application**
- Use Case-ek.
- Command és Query műveletek.
- DTO-k.
- Validáció.
- Repository-interfészek.

**Infrastructure**
- EF Core.
- SQL Server.
- Repository implementációk.
- ML.NET.
- CSV-feldolgozás.
- Háttérfolyamatok.

**API**
- REST végpontok.
- Hitelesítés.
- Jogosultságkezelés.
- HTTP kommunikáció.

**Web**
- Blazor felhasználói felület.
- Űrlapok.
- Dashboard.
- Diagramok.
- API-kliens szolgáltatások.

### 3.4. Domain modulok

| Modul | Felelősség |
|---|---|
| Identity | Felhasználói fiókok és hitelesítés |
| Transaction Management | Bevétel és kiadás kezelése |
| Category Management | Kategóriák kezelése |
| Budget Management | Költségkeretek |
| Import Processing | CSV-import és validáció |
| Categorization | Szabályalapú és ML-kategorizálás |
| Financial Analytics | Statisztikák és anomáliák |
| Forecasting | Jövőbeli kiadások becslése |

---

# 4. Felhasználói szerepkörök

## 4.1. Látogató

Jogosultságok:
- Regisztráció.
- Bejelentkezés.

## 4.2. Regisztrált felhasználó

Jogosultságok:
- Saját profil kezelése.
- Saját tranzakciók kezelése.
- Kategóriák kezelése.
- Költségkeretek kezelése.
- CSV-import.
- Kategorizálási szabályok kezelése.
- Pénzügyi elemzések megtekintése.
- Előrejelzések megtekintése.

Az első verzió nem igényel külön adminisztrátori felületet. A rendszerkarbantartási és modellértékelési műveletek fejlesztői vagy üzemeltetői feladatként kezelhetők.

---

# 5. Funkcionális követelmények

## 5.1. Felhasználókezelés

**FR-01 – Regisztráció**

A felhasználó e-mail-cím, jelszó és megjelenítendő név megadásával fiókot hozhat létre.

**FR-02 – Bejelentkezés**

A rendszer hitelesíti a felhasználót, majd hozzáférést biztosít a saját adataihoz.

**FR-03 – Kijelentkezés**

A felhasználó lezárhatja a hitelesített munkamenetét.

**FR-04 – Profilkezelés**

A felhasználó megtekintheti és módosíthatja profiladatait.

## 5.2. Tranzakciókezelés

**FR-05 – Bevétel rögzítése**

A felhasználó bevételt rögzíthet összeggel, pénznemmel, dátummal, kategóriával és leírással.

**FR-06 – Kiadás rögzítése**

A felhasználó kiadást rögzíthet a bevételekkel azonos alapadatokkal.

**FR-07 – Tranzakciók listázása**

A felhasználó megtekintheti saját tranzakcióit, rendezhető és lapozható listában.

**FR-08 – Tranzakció módosítása**

A felhasználó módosíthatja saját tranzakcióit.

**FR-09 – Tranzakció törlése**

A felhasználó törölheti saját tranzakcióit.

**FR-10 – Tranzakciók szűrése**

Szűrési lehetőségek:
- Dátumtartomány.
- Kategória.
- Tranzakciótípus.
- Összeg.
- Leírás.

## 5.3. Kategóriakezelés

**FR-11 – Alapértelmezett kategóriák**

A rendszer előre definiált bevételi és kiadási kategóriákat biztosít.

**FR-12 – Saját kategória létrehozása**

A felhasználó saját kategóriát hozhat létre.

**FR-13 – Kategória módosítása és törlése**

A felhasználó módosíthatja vagy törölheti saját kategóriáit. Használt kategória törlésekor a rendszernek kezelnie kell a kapcsolódó tranzakciókat.

## 5.4. CSV-import

**FR-14 – CSV-fájl feltöltése**

A felhasználó CSV-fájlt tölthet fel tranzakciók importálásához.

**FR-15 – CSV-feldolgozás és validáció**

A rendszer ellenőrzi a fájl formátumát, a kötelező oszlopokat és a sorok adattípusait.

**FR-16 – Hibás sorok kezelése**

A rendszer megjeleníti a hibás sorok sorszámát és a hiba okát.

**FR-17 – Duplikációfelismerés**

A rendszer felismeri a lehetséges ismétlődő importokat, és lehetővé teszi azok kihagyását.

**FR-18 – Importeredmény**

A rendszer összesítést ad a sikeres, hibás és kihagyott tranzakciókról.

## 5.5. Szabályalapú kategorizálás

**FR-19 – Automatikus kategorizálás**

A rendszer tranzakcióleírások alapján kategóriát javasol előre meghatározott szabályok segítségével.

**FR-20 – Egyéni szabályok kezelése**

A felhasználó kulcsszó és kategória alapján saját szabályokat hozhat létre.

**FR-21 – Szabályprioritás**

Több illeszkedő szabály esetén a rendszer prioritás szerint választ.

**FR-22 – Manuális javítás**

A felhasználó módosíthatja az automatikusan kiválasztott kategóriát.

**FR-23 – Kategorizálási visszajelzés**

A rendszer eltárolja a felhasználó javításait későbbi modellfejlesztés céljából.

## 5.6. ML.NET kategorizálás

**FR-24 – ML-alapú kategorizálás**

A rendszer ML.NET segítségével kategóriát javasol a tranzakció leírása alapján.

**FR-25 – Bizonyossági érték**

A rendszer megjeleníti a modell predikciójához kapcsolódó bizonyossági értéket, amennyiben a választott modell ezt támogatja.

**FR-26 – Bizonytalan predikció kezelése**

Alacsony bizonyosság esetén a rendszer manuális ellenőrzést kér.

**FR-27 – Kategorizálási módszerek összehasonlítása**

A szabályalapú és ML-alapú módszer azonos tesztadatokon értékelhető legyen.

**FR-28 – Modell újratanítása**

A tárolt visszajelzések később felhasználhatók új modell tanítására. Az új modell csak értékelés után válthatja le az előzőt.

## 5.7. Pénzügyi elemzések

**FR-29 – Havi összesítés**

A rendszer megjeleníti a kiválasztott hónap bevételeit, kiadásait és nettó pénzáramlását.

**FR-30 – Kategóriánkénti elemzés**

A rendszer kategóriánként összesíti a kiadásokat.

**FR-31 – Időszakok összehasonlítása**

A felhasználó összehasonlíthat különböző hónapokat vagy éveket.

**FR-32 – Pénzügyi trendek**

A rendszer grafikonon jeleníti meg a bevételek és kiadások időbeli alakulását.

**FR-33 – Ismétlődő kiadások felismerése**

A rendszer a tranzakciók alapján ismétlődő kiadásokat javasol.

## 5.8. Anomáliafelismerés

**FR-34 – Szokatlan tranzakció felismerése**

A rendszer statisztikai módszerekkel azonosítja a korábbi szokásoktól eltérő tranzakciókat.

**FR-35 – Kategóriaszintű anomália**

A rendszer felismeri a kategóriák szokásos költési szintjétől való jelentős eltérést.

**FR-36 – Anomáliák megjelenítése**

A rendszer listázza a felismert anomáliákat, és rövid magyarázatot ad.

## 5.9. Költségkeretek

**FR-37 – Költségkeret létrehozása**

A felhasználó időszakhoz és kategóriához kötött költségkeretet hozhat létre.

**FR-38 – Költségkeret követése**

A rendszer megjeleníti a felhasznált és fennmaradó összeget.

**FR-39 – Túllépés jelzése**

A rendszer jelzi a költségkeret elérését vagy túllépését.

## 5.10. Előrejelzés

**FR-40 – Kiadások előrejelzése**

A rendszer korábbi tranzakciók alapján becslést készít a következő időszak kiadásairól.

**FR-41 – Mozgóátlag-alapú előrejelzés**

A rendszer mozgóátlag segítségével becsüli a jövőbeli kiadásokat.

**FR-42 – Lineáris regressziós előrejelzés**

A rendszer lineáris regresszióval is készít becslést.

**FR-43 – Előrejelzési módszerek értékelése**

A rendszer lehetővé teszi a módszerek MAE és RMSE alapján történő összehasonlítását.

**FR-44 – Havi nettó pénzáramlás előrejelzése**

A rendszer becslést készít a hónap végére várható nettó pénzáramlásról.

## 5.11. Dashboard

**FR-45 – Pénzügyi áttekintő**

A dashboard megjeleníti:
- Havi bevétel.
- Havi kiadás.
- Nettó pénzáramlás.
- Kategóriánkénti költés.
- Havi trendek.
- Költségkeretek állapota.
- Anomáliák.
- Előrejelzések.

---

# 6. Üzleti szabályok

| Azonosító | Szabály |
|---|---|
| BR-01 | Minden pénzügyi adat egy felhasználóhoz tartozik. |
| BR-02 | A tranzakció összege pozitív decimal érték. |
| BR-03 | A tranzakció típusa külön mezőben jelzi a bevételt vagy kiadást. |
| BR-04 | Bevétel csak bevételi kategóriához rendelhető. |
| BR-05 | Kiadás csak kiadási kategóriához rendelhető. |
| BR-06 | A felhasználói egyéni szabályok elsőbbséget élveznek az ML-javaslattal szemben. |
| BR-07 | A felhasználó manuálisan felülbírálhatja az automatikus kategorizálást. |
| BR-08 | A kategorizálási visszajelzés nem indít automatikus újratanítást. |
| BR-09 | Különböző pénznemek összegei nem adhatók össze közvetlenül. |
| BR-10 | Az előrejelzés nem tekinthető garantált pénzügyi eredménynek. |
| BR-11 | Anomália felismerése nem jelent automatikusan hibás vagy csalárd tranzakciót. |
| BR-12 | A költségkeret felhasználását az adott időszakba tartozó kiadások határozzák meg. |
| BR-13 | A rendszer csak megfelelő mennyiségű adat esetén készít előrejelzést. |
| BR-14 | Minden felhasználói adatlekérdezésnél ellenőrizni kell az adatok tulajdonosát. |

## 6.1. Pénzügyi számítások értelmezése

Az első verzió nem kezel külön bankszámlákat és nyitóegyenlegeket.

Ezért a rendszerben az egyenleg kifejezés helyett az alábbi számítást használjuk:

**Nettó pénzáramlás = adott időszak bevételei − adott időszak kiadásai.**

Ez nem azonos a bankszámla tényleges egyenlegével.

---

# 7. Adatmodell

## 7.1. Fő entitások

### User
- Id
- Email
- DisplayName
- CreatedAt

A hitelesítési adatokat az ASP.NET Core Identity kezeli.

### Transaction
- Id
- UserId
- Amount
- Currency
- Type
- Description
- TransactionDate
- CategoryId
- Source
- CategorizationStatus
- CreatedAt
- UpdatedAt

### Category
- Id
- UserId
- Name
- Type
- IsSystemCategory
- CreatedAt

A rendszerkategóriák UserId értéke null lehet.

### Budget
- Id
- UserId
- CategoryId
- Amount
- Currency
- PeriodType
- StartDate
- EndDate

### CategorizationRule
- Id
- UserId
- Keyword
- CategoryId
- Priority
- MatchType
- IsActive

### CategorizationFeedback
- Id
- TransactionId
- PredictedCategoryId
- CorrectedCategoryId
- CreatedAt

### ImportBatch
- Id
- UserId
- FileName
- Status
- TotalRows
- SuccessfulRows
- FailedRows
- DuplicateRows
- CreatedAt
- CompletedAt

### ImportError
- Id
- ImportBatchId
- RowNumber
- ErrorCode
- ErrorMessage

### FinancialAnomaly
- Id
- UserId
- TransactionId
- AnomalyType
- Score
- Description
- DetectedAt

### RecurringTransaction
- Id
- UserId
- Description
- CategoryId
- ExpectedAmount
- Frequency
- LastOccurrence
- Confidence

### ForecastResult
- Id
- UserId
- ForecastType
- Method
- PeriodStart
- PeriodEnd
- PredictedAmount
- CreatedAt
- ModelVersion

## 7.2. Value Objectek

**Money**
- Amount: decimal
- Currency: string

**DateRange**
- StartDate
- EndDate

**ConfidenceScore**
- Value

## 7.3. Adatbázis-szabályok

- Elsődleges kulcsok: GUID vagy más egységesen választott azonosító.
- Pénzügyi összegek: decimal, például decimal(18,2).
- Pénznem: háromkarakteres ISO-kód.
- Dátumok: egységes UTC tárolási szabály, a tranzakció dátumának helyes helyi értelmezésével.
- Foreign key kapcsolatok minden releváns entitásnál.
- Egyedi indexek a duplikációk és az ismétlődő rekordok kezelésére.
- Minden felhasználói adathoz UserId alapú tulajdonjog-ellenőrzés.

---

# 8. Kategorizálási architektúra

A kategorizálási folyamat külön Application szolgáltatásként működjön.

Javasolt folyamat:

1. A tranzakció leírásának normalizálása.
2. A felhasználói szabályok kiértékelése.
3. Ha van megfelelő szabály, annak alkalmazása.
4. Ha nincs, az ML.NET modell futtatása.
5. A predikció bizonyosságának értékelése.
6. Magas bizonyosság esetén kategóriajavaslat elfogadása.
7. Alacsony bizonyosság esetén manuális ellenőrzés.
8. A felhasználói javítások eltárolása.

Az ML-modell közös legyen, ne felhasználónként különálló.

A felhasználói visszajelzésekből később új tanítóadatkészlet készíthető.

A modell újratanítása külön folyamatban történjen, és minden új verziót értékelni kell.

---

# 9. API-terv

## Authentication
- POST `/api/auth/register`
- POST `/api/auth/login`
- POST `/api/auth/logout`
- GET `/api/auth/me`
- PUT `/api/auth/profile`

## Transactions
- GET `/api/transactions`
- GET `/api/transactions/{id}`
- POST `/api/transactions`
- PUT `/api/transactions/{id}`
- DELETE `/api/transactions/{id}`

## Categories
- GET `/api/categories`
- POST `/api/categories`
- PUT `/api/categories/{id}`
- DELETE `/api/categories/{id}`

## Budgets
- GET `/api/budgets`
- POST `/api/budgets`
- PUT `/api/budgets/{id}`
- DELETE `/api/budgets/{id}`

## Imports
- POST `/api/imports`
- GET `/api/imports`
- GET `/api/imports/{id}`
- GET `/api/imports/{id}/errors`

## Categorization
- POST `/api/categorization/predict`
- GET `/api/categorization/rules`
- POST `/api/categorization/rules`
- PUT `/api/categorization/rules/{id}`
- DELETE `/api/categorization/rules/{id}`
- POST `/api/categorization/feedback`

## Analytics
- GET `/api/analytics/overview`
- GET `/api/analytics/monthly`
- GET `/api/analytics/categories`
- GET `/api/analytics/trends`
- GET `/api/analytics/anomalies`
- GET `/api/analytics/recurring`

## Forecasting
- GET `/api/forecasting/monthly`
- GET `/api/forecasting/cashflow`
- GET `/api/forecasting/evaluation`

Az API-végpontok az Application réteg Use Case-eit hívják meg. A controllerek nem tartalmazhatnak közvetlen üzleti logikát.

---

# 10. Nem funkcionális követelmények

### NFR-01 – Biztonság
- Biztonságos jelszókezelés.
- JWT-alapú hitelesítés.
- Jogosultság-ellenőrzés minden védett végponton.
- Felhasználói adatok elkülönítése.
- Titkos konfigurációk védelme.

### NFR-02 – Teljesítmény
- Lapozható tranzakciólisták.
- Adatbázisoldali szűrés.
- Nagyobb importok háttérfeldolgozása.
- Párhuzamos feldolgozás mérhetősége.

### NFR-03 – Karbantarthatóság
- Elkülönített rétegek.
- Dependency Injection.
- Automatizált tesztek.
- Egységes hibakezelés.
- Dokumentált API.

### NFR-04 – Használhatóság
- Reszponzív Blazor felület.
- Érthető hibaüzenetek.
- Egységes navigáció.
- Könnyen értelmezhető diagramok.
- Automatikus javaslatok és előrejelzések egyértelmű megkülönböztetése.

### NFR-05 – Megbízhatóság
- Tranzakcióbiztos adatbázisműveletek.
- Konzisztens importfolyamat.
- Hibás adatok kezelése.
- Naplózható háttérfeldolgozás.

---

# 11. Tesztelési stratégia

## Unit tesztek
- Domain szabályok.
- Tranzakcióvalidáció.
- Kategóriatípus ellenőrzése.
- Kategorizálási prioritás.
- Költségkeret számítás.
- Anomáliafelismerés.
- Előrejelzési algoritmusok.

## Integrációs tesztek
- SQL Server adatbázisműveletek.
- Repository implementációk.
- API végpontok.
- Hitelesítés.
- CSV-import.
- Felhasználói jogosultságok.

## Végpontok közötti tesztek
- Regisztráció és bejelentkezés.
- Tranzakció rögzítése.
- Tranzakció módosítása.
- CSV-import.
- Kategorizálás.
- Dashboard frissítése.

## Biztonsági tesztek
- Másik felhasználó tranzakciója nem kérhető le.
- Másik felhasználó kategóriája nem módosítható.
- Másik felhasználó importja nem tekinthető meg.
- Azonosító manipulálásával nem szerezhető jogosulatlan hozzáférés.

---

# 12. Szakdolgozati kutatási feladatok

## 12.1. Kategorizálási módszerek összehasonlítása

**Kutatási kérdés:** Milyen pontossággal képesek a szabályalapú és gépi tanulásos módszerek kategorizálni a pénzügyi tranzakciókat?

Mérőszámok:
- Accuracy.
- Precision.
- Recall.
- F1-score.
- Feldolgozási idő.

Az értékelés azonos, elkülönített teszthalmazon történjen.

## 12.2. Anomáliafelismerés

**Kutatási kérdés:** Mennyire alkalmasak az egyszerű statisztikai módszerek a szokatlan pénzügyi események felismerésére?

Vizsgált módszerek:
- Átlag és szórás.
- Z-score.
- Kategóriaszintű eltérések.

Mérőszámok:
- Felismert anomáliák.
- Téves riasztások.
- Felismerési arány.

## 12.3. Előrejelzési módszerek

**Kutatási kérdés:** Milyen pontossággal becsülhető a jövőbeli kiadás mozgóátlag és lineáris regresszió segítségével?

Mérőszámok:
- MAE.
- RMSE.

A tesztelés időrendben elkülönített adatokkal történjen.

## 12.4. Párhuzamos feldolgozás

**Kutatási kérdés:** Milyen teljesítménykülönbség mérhető szekvenciális és párhuzamos tranzakciófeldolgozás között?

Vizsgált szempontok:
- Feldolgozási idő.
- CPU-használat.
- Memóriahasználat.
- Adathalmaz mérete.
- Párhuzamosság mértéke.

---

# II. GitHub fejlesztési backlog

## 13. Backlog felépítése

A GitHub repositoryban a fejlesztést Epic → Issue → Subtask struktúrában érdemes kezelni.

A GitHub Projects használatával minden Issue követhető egy közös táblán.

### Javasolt címkézési rendszer

| Label | Jelentés |
|---|---|
| `epic` | Nagyobb fejlesztési egység |
| `feature` | Új funkció |
| `backend` | Backend feladat |
| `frontend` | Blazor feladat |
| `database` | Adatbázis |
| `ml` | Gépi tanulás |
| `analytics` | Pénzügyi elemzés |
| `testing` | Tesztelés |
| `documentation` | Dokumentáció |
| `research` | Szakdolgozati mérés |
| `bug` | Hibajavítás |
| `priority-high` | Magas prioritás |
| `priority-medium` | Közepes prioritás |

### GitHub Project státuszok

- Backlog
- Ready
- In Progress
- In Review
- Testing
- Done

---

# 14. Epic lista

| Epic | Név | Cél |
|---|---|---|
| EPIC-01 | Projektalapok és architektúra | A fejlesztési környezet és rétegstruktúra kialakítása |
| EPIC-02 | Felhasználókezelés | Hitelesítés és profilkezelés |
| EPIC-03 | Tranzakció- és kategóriakezelés | Alapvető pénzügyi nyilvántartás |
| EPIC-04 | Dashboard és pénzügyi áttekintés | Pénzügyi adatok vizuális megjelenítése |
| EPIC-05 | CSV-import és kategorizálás | Adatimport és automatikus szabályalapú kategorizálás |
| EPIC-06 | ML.NET kategorizálás | Gépi tanulási modell és összehasonlítás |
| EPIC-07 | Pénzügyi elemzés és költségkeretek | Statisztikák, anomáliák és költségkeretek |
| EPIC-08 | Előrejelzés | Kiadások és pénzáramlás becslése |
| EPIC-09 | Teljesítmény és véglegesítés | Párhuzamos feldolgozás, tesztelés és dokumentáció |

---

# 15. Részletes GitHub Issue backlog

Az Issue-k önálló fejlesztési egységek. A cél az, hogy lehetőleg egy-egy Issue két fejlesztési nap alatt megvalósítható és bemutatható legyen.

A becsült idő nem merev határidő: az összetettebb ML- és elemzési feladatok több időt igényelhetnek.

## EPIC-01 – Projektalapok és architektúra

### SF-001 – .NET solution és projektstruktúra létrehozása
**Cél:** A Smart Finance alapvető solution struktúrájának létrehozása.

Feladatok:
- Domain projekt.
- Application projekt.
- Infrastructure projekt.
- API projekt.
- Blazor WebAssembly projekt.
- Tesztprojektek.

Elfogadási kritériumok:
- A solution sikeresen buildelhető.
- A projektek megfelelően hivatkoznak egymásra.
- Az alkalmazás elindítható.

### SF-002 – Clean Architecture függőségek kialakítása
**Cél:** A rétegek közötti függőségek megfelelő kialakítása.

Elfogadási kritériumok:
- A Domain nem függ Infrastructure vagy API projekttől.
- Az Application a Domain rétegre épül.
- Az Infrastructure implementálja az Application interfészeit.
- Dependency Injection működik.

### SF-003 – SQL Server és EF Core konfiguráció
**Cél:** Adatbázis-kapcsolat kialakítása.

Elfogadási kritériumok:
- Az API csatlakozik SQL Serverhez.
- A DbContext konfigurálva van.
- Az első migráció létrehozható és alkalmazható.
- A connection string biztonságosan konfigurálható.

### SF-004 – Egységes hibakezelés és validációs alapok
**Cél:** Közös hibakezelési és validációs mechanizmus.

Elfogadási kritériumok:
- A backend egységes hibaválaszokat ad.
- A validációs hibák érthető formában jelennek meg.
- A váratlan hibák naplózásra kerülnek.

---

## EPIC-02 – Felhasználókezelés

### SF-005 – Felhasználói regisztráció
**Cél:** Új felhasználói fiók létrehozása.

Elfogadási kritériumok:
- E-mail-cím és jelszó alapján regisztráció lehetséges.
- Duplikált e-mail-cím nem fogadható el.
- A jelszó biztonságosan tárolódik.
- Hibás adatok esetén validációs hiba jelenik meg.

### SF-006 – Bejelentkezés és JWT-hitelesítés
**Cél:** Hitelesített hozzáférés biztosítása.

Elfogadási kritériumok:
- Sikeres bejelentkezés után token készül.
- A tokennel védett API végpontok elérhetők.
- Érvénytelen token esetén a hozzáférés megtagadásra kerül.

### SF-007 – Blazor hitelesítési folyamat
**Cél:** A frontend kapcsolódjon a hitelesítési rendszerhez.

Elfogadási kritériumok:
- Bejelentkezési oldal működik.
- Regisztrációs oldal működik.
- Védett oldalak csak hitelesítés után érhetők el.
- Kijelentkezés megszünteti a hozzáférést.

### SF-008 – Felhasználói profilkezelés
**Cél:** Saját profil megtekintése és módosítása.

Elfogadási kritériumok:
- A felhasználó megtekintheti saját adatait.
- A megjelenítendő név módosítható.
- Más felhasználó adatai nem érhetők el.

---

## EPIC-03 – Tranzakció- és kategóriakezelés

### SF-009 – Transaction domain modell
**Cél:** A pénzügyi tranzakciók domain modelljének kialakítása.

Elfogadási kritériumok:
- A Transaction tartalmazza a szükséges adatokat.
- Az összeg decimal típusú.
- A bevétel és kiadás külön típussal rendelkezik.
- Az alapvető üzleti validációk működnek.

### SF-010 – Alapértelmezett kategóriák létrehozása
**Cél:** Rendszerkategóriák biztosítása.

Elfogadási kritériumok:
- Bevételi és kiadási kategóriák létrejönnek.
- A kategóriák típusa megfelelő.
- A rendszerkategóriák nem módosíthatók más felhasználók által.

### SF-011 – Bevétel rögzítése
**Cél:** Új bevétel létrehozása.

Elfogadási kritériumok:
- A felhasználó megadhatja az összeget, dátumot, kategóriát és leírást.
- Csak bevételi kategória választható.
- A tranzakció a bejelentkezett felhasználóhoz kapcsolódik.

### SF-012 – Kiadás rögzítése
**Cél:** Új kiadás létrehozása.

Elfogadási kritériumok:
- A felhasználó kiadást rögzíthet.
- Csak kiadási kategória választható.
- A mentett tranzakció megjelenik a listában.

### SF-013 – Tranzakciók listázása és részletei
**Cél:** Saját tranzakciók megjelenítése.

Elfogadási kritériumok:
- A lista csak saját tranzakciókat tartalmaz.
- A tranzakció részletei megnyithatók.
- A lista lapozható és rendezhető.

### SF-014 – Tranzakció módosítása és törlése
**Cél:** Korábbi tranzakciók kezelése.

Elfogadási kritériumok:
- A felhasználó módosíthatja saját tranzakcióját.
- Törölheti saját tranzakcióját.
- Más felhasználó tranzakciója nem módosítható.

### SF-015 – Tranzakciók keresése és szűrése
**Cél:** A tranzakciók gyors visszakeresése.

Elfogadási kritériumok:
- Dátum alapján szűrhető.
- Kategória alapján szűrhető.
- Típus és összeg alapján szűrhető.
- Leírás alapján kereshető.
- A szűrések kombinálhatók.

### SF-016 – Saját kategóriák kezelése
**Cél:** Egyéni kategóriák létrehozása és kezelése.

Elfogadási kritériumok:
- A felhasználó saját kategóriát hozhat létre.
- Kategóriát módosíthat és törölhet.
- A kategóriatípus bevétel vagy kiadás.
- Használt kategória törlésekor a kapcsolódó tranzakciók nem veszhetnek el.

---

## EPIC-04 – Dashboard és pénzügyi áttekintés

### SF-017 – Dashboard alapadatok
**Cél:** Pénzügyi összesítő kialakítása.

Elfogadási kritériumok:
- Havi bevétel megjelenik.
- Havi kiadás megjelenik.
- Nettó pénzáramlás kiszámítható.
- Az adatok a bejelentkezett felhasználóhoz tartoznak.

### SF-018 – Kategóriánkénti költési diagram
**Cél:** Kiadások vizuális megjelenítése.

Elfogadási kritériumok:
- Kategóriánkénti összesítés megjelenik.
- A diagram adatai egyeznek az adatbázisból számított értékekkel.
- Az időszak kiválasztható.

### SF-019 – Havi pénzügyi trendek
**Cél:** Havi bevétel és kiadás alakulásának megjelenítése.

Elfogadási kritériumok:
- Több hónap adatai megjelennek.
- Bevétel és kiadás külön követhető.
- Az adatok megfelelő időrendi sorrendben jelennek meg.

---

## EPIC-05 – CSV-import és szabályalapú kategorizálás

### SF-020 – CSV-import alapfolyamat
**Cél:** Tranzakciók beolvasása CSV-fájlból.

Elfogadási kritériumok:
- A felhasználó fájlt tölthet fel.
- A rendszer beolvassa a sorokat.
- Az importfeladat ImportBatch rekordot hoz létre.
- A támogatott CSV-formátum dokumentált.

### SF-021 – CSV-validáció és hibakezelés
**Cél:** Hibás adatok kezelése.

Elfogadási kritériumok:
- Hibás dátum felismerhető.
- Hibás összeg felismerhető.
- Hiányzó kötelező mező felismerhető.
- A hibás sorok részletesen megjelennek.
- A helyes sorok feldolgozhatók.

### SF-022 – Duplikációfelismerés
**Cél:** Ismételten importált tranzakciók kiszűrése.

Elfogadási kritériumok:
- A rendszer képes lehetséges duplikációt azonosítani.
- A duplikált sorok kihagyhatók.
- Az importeredmény tartalmazza a duplikációk számát.
- Az azonos összegű, de különböző tranzakciók nem kerülnek automatikusan biztosan duplikációnak minősítésre.

### SF-023 – Importeredmény megjelenítése
**Cél:** Az importálás eredményének áttekintése.

Elfogadási kritériumok:
- Sikeres sorok száma megjelenik.
- Hibás sorok száma megjelenik.
- Kihagyott sorok száma megjelenik.
- Az import korábbi eredményei visszakereshetők.

### SF-024 – Szabályalapú kategorizáló motor
**Cél:** Kulcsszavak alapján automatikus kategorizálás.

Elfogadási kritériumok:
- A tranzakció leírása normalizálható.
- A rendszer kulcsszó alapján kategóriát választ.
- A szabályok prioritás szerint értékelhetők.
- A kategorizálás eredménye mentésre kerül.

### SF-025 – Egyéni kategorizálási szabályok
**Cél:** Felhasználói szabályok kezelése.

Elfogadási kritériumok:
- A felhasználó új szabályt hozhat létre.
- A szabályhoz kategóriát rendelhet.
- A szabály aktiválható és deaktiválható.
- Más felhasználó szabálya nem módosítható.

### SF-026 – Kategorizálási javítás és visszajelzés
**Cél:** Manuális javítások rögzítése.

Elfogadási kritériumok:
- A felhasználó megváltoztathatja a kategóriát.
- A rendszer eltárolja a korábbi és az új kategóriát.
- A visszajelzés későbbi modellértékeléshez felhasználható.

---

## EPIC-06 – ML.NET kategorizálás

### SF-027 – ML tanítóadatkészlet kialakítása
**Cél:** A gépi tanulási modellhez szükséges adatok előkészítése.

Elfogadási kritériumok:
- A tanítóadatok egységes formátumúak.
- A kategóriák egységesek.
- A tanító- és teszthalmaz elkülönül.
- A tesztadatok nem szivárognak a tanításba.

### SF-028 – ML.NET kategorizáló modell
**Cél:** Tranzakciók gépi tanulás alapú kategorizálása.

Elfogadási kritériumok:
- A modell betölthető.
- A modell tranzakcióleírás alapján kategóriát javasol.
- A predikció eredménye visszaadható az Application réteg számára.

### SF-029 – Kategorizálási bizonyosság és manuális ellenőrzés
**Cél:** Bizonytalan ML-eredmények kezelése.

Elfogadási kritériumok:
- A modell eredményéhez kapcsolódó érték megjeleníthető.
- A döntési küszöb konfigurálható.
- Bizonytalan eredmény manuális ellenőrzésre kerül.
- A felhasználó módosíthatja a javaslatot.

### SF-030 – Kategorizálási módszerek összehasonlítása
**Cél:** Szabályalapú és ML.NET módszer értékelése.

Elfogadási kritériumok:
- Azonos tesztadatok használhatók.
- Accuracy, Precision, Recall és F1-score kiszámítható.
- Feldolgozási idő mérhető.
- Az eredmények exportálhatók vagy dokumentálhatók.

### SF-031 – ML-modell újratanítási folyamat
**Cél:** Javított kategorizálási adatok felhasználása.

Elfogadási kritériumok:
- A visszajelzésekből új tanítóadatkészlet készíthető.
- Új modell tanítható.
- Az új modell értékelhető.
- A korábbi modell megőrizhető.
- Az új modell csak sikeres értékelés után aktiválható.

---

## EPIC-07 – Pénzügyi elemzés és költségkeretek

### SF-032 – Havi és időszakos pénzügyi elemzés
**Cél:** Pénzügyi statisztikák készítése.

Elfogadási kritériumok:
- Havi bevétel és kiadás számítható.
- Időszakok összehasonlíthatók.
- Kategóriánkénti összesítés elérhető.

### SF-033 – Anomáliafelismerés
**Cél:** Szokatlan tranzakciók azonosítása.

Elfogadási kritériumok:
- Statisztikai módszer működik.
- A rendszer képes eltérést jelezni.
- Az eredményhez magyarázat tartozik.
- Kevés adat esetén nem ad félrevezető eredményt.

### SF-034 – Ismétlődő kiadások felismerése
**Cél:** Rendszeresen visszatérő tranzakciók azonosítása.

Elfogadási kritériumok:
- A rendszer időbeli ismétlődéseket keres.
- A felismert kiadások javaslatként jelennek meg.
- A felhasználó ellenőrizheti a javaslatokat.

### SF-035 – Költségkeretek kezelése
**Cél:** Kategóriánkénti költségkeretek létrehozása.

Elfogadási kritériumok:
- Havi keret beállítható.
- A keret egy kategóriához tartozik.
- A felhasználó módosíthatja és törölheti saját keretét.

### SF-036 – Költségkeret felhasználásának követése
**Cél:** Kiadások és költségkeretek összehasonlítása.

Elfogadási kritériumok:
- Felhasznált összeg megjelenik.
- Fennmaradó összeg kiszámítható.
- Túllépés esetén figyelmeztetés jelenik meg.
- A számítás csak az adott időszak kiadásait veszi figyelembe.

---

## EPIC-08 – Előrejelzés

### SF-037 – Előrejelzési adatok előkészítése
**Cél:** Történelmi tranzakciók előkészítése.

Elfogadási kritériumok:
- A tranzakciók időszakokra csoportosíthatók.
- Hiányzó időszakok kezelhetők.
- A bemeneti adatok ellenőrizhetők.

### SF-038 – Mozgóátlag-alapú előrejelzés
**Cél:** Jövőbeli kiadások becslése mozgóátlaggal.

Elfogadási kritériumok:
- Az algoritmus működik.
- Az időablak konfigurálható.
- Az eredmény összehasonlítható a tényleges adatokkal.

### SF-039 – Lineáris regressziós előrejelzés
**Cél:** Alternatív előrejelzési módszer kialakítása.

Elfogadási kritériumok:
- A modell betanítható.
- A jövőbeli kiadás becsülhető.
- Az eredmény összehasonlítható a mozgóátlaggal.

### SF-040 – Előrejelzési módszerek értékelése
**Cél:** Az algoritmusok pontosságának mérése.

Elfogadási kritériumok:
- MAE kiszámítható.
- RMSE kiszámítható.
- Azonos tesztidőszak használható.
- Az eredmények dokumentálhatók.

### SF-041 – Előrejelzések megjelenítése
**Cél:** A becslések megjelenítése a felhasználói felületen.

Elfogadási kritériumok:
- A becsült érték megjelenik.
- A tényleges és becsült értékek elkülönülnek.
- A módszer neve látható.
- A kevés adatból származó becslések figyelmeztetéssel jelennek meg.

---

## EPIC-09 – Teljesítmény és véglegesítés

### SF-042 – Tesztadatgenerátor nagy tranzakcióhalmazokhoz
**Cél:** Teljesítménymérésekhez használható tesztadatok előállítása.

Elfogadási kritériumok:
- Különböző méretű adathalmazok generálhatók.
- A generált adatok reprodukálhatók.
- A tesztadatok elkülönülnek a valódi felhasználói adatoktól.

### SF-043 – Szekvenciális feldolgozás implementálása
**Cél:** Referenciaimplementáció kialakítása.

Elfogadási kritériumok:
- A tranzakciók feldolgozása szekvenciálisan történik.
- A feldolgozás ideje mérhető.
- A kimenet ellenőrizhető.

### SF-044 – Párhuzamos feldolgozás implementálása
**Cél:** Párhuzamos változat kialakítása.

Elfogadási kritériumok:
- A feldolgozás párhuzamosan futtatható.
- A maximális párhuzamosság konfigurálható.
- A kimenet megegyezik a szekvenciális változat eredményével.

### SF-045 – Teljesítménymérés és összehasonlítás
**Cél:** A két feldolgozási módszer összehasonlítása.

Elfogadási kritériumok:
- Azonos tesztadatokon történik a mérés.
- Több adathalmazméret vizsgálható.
- Futási idő rögzíthető.
- CPU- és memóriahasználat mérhető.
- Az eredmények táblázatban vagy diagramon bemutathatók.

### SF-046 – Jogosultsági és integrációs tesztelés
**Cél:** A teljes rendszer biztonsági és funkcionális ellenőrzése.

Elfogadási kritériumok:
- A fő API-végpontok tesztelve vannak.
- A felhasználói adatok elkülönítése ellenőrzött.
- A kritikus üzleti folyamatok tesztelve vannak.
- A hibakezelés ellenőrzött.

### SF-047 – Végleges dashboard és felhasználói élmény
**Cél:** Egységes és bemutatható alkalmazás kialakítása.

Elfogadási kritériumok:
- A fő funkciók egységes felületen elérhetők.
- Reszponzív elrendezés működik.
- Üres állapotok és hibák kezeltek.
- A diagramok és előrejelzések érthetők.

### SF-048 – Szakdolgozati dokumentáció és eredmények
**Cél:** A fejlesztési és kutatási eredmények összefoglalása.

Elfogadási kritériumok:
- Architektúra dokumentálva.
- Adatmodell dokumentálva.
- Algoritmusok működése leírva.
- Mérési eredmények rögzítve.
- Következtetések megfogalmazva.

---

# 16. Fejlesztési függőségek

A fő fejlesztési sorrend:

```text
EPIC-01 Projektalapok
        |
        v
EPIC-02 Felhasználókezelés
        |
        v
EPIC-03 Tranzakció- és kategóriakezelés
        |
        +-------------------+
        |                   |
        v                   v
EPIC-04 Dashboard       EPIC-05 CSV és szabályok
                            |
                            v
                       EPIC-06 ML.NET
                            |
        +-------------------+
        |
        v
EPIC-07 Pénzügyi elemzés
        |
        v
EPIC-08 Előrejelzés
        |
        v
EPIC-09 Teljesítmény és véglegesítés
```

Fontos: a diagram logikai függőséget mutat, nem minden Issue szigorú sorrendjét. Például a dashboard alapjai már a tranzakciókezelés elkészülte után fejleszthetők, miközben az ML-modell előkészítése a CSV-importtal párhuzamosan is megkezdhető.

---

# 17. Issue-k elfogadási szabályai

Minden GitHub Issue esetében az alábbi feltételeket alkalmazzuk:

1. A feladatnak legyen egyértelmű célja.
2. Legyen meghatározva a backend és frontend feladat, ha mindkettőt érinti.
3. Legyenek konkrét elfogadási kritériumok.
4. A kritikus üzleti logikához készüljön teszt.
5. A felhasználói folyamat legyen demonstrálható.
6. A kód kerüljön külön feature branchre.
7. A kész funkció Pull Requesten keresztül kerüljön a fő ágba.
8. Az Issue csak sikeres tesztelés után zárható le.

## Javasolt branch elnevezések

```text
feature/SF-005-user-registration
feature/SF-011-create-income
feature/SF-020-csv-import
feature/SF-028-ml-categorization
feature/SF-033-anomaly-detection
```

## Javasolt commit üzenetek

```text
feat(auth): implement user registration
feat(transactions): add income creation
feat(import): implement csv validation
feat(ml): add transaction categorization model
test(analytics): add anomaly detection tests
docs: update architecture specification
```

---

# 18. Kétnapos fejlesztési ciklusok

A GitHub Issue-k legyenek a fejlesztési egységek. Egy Issue lehetőleg egy bemutatható funkciót tartalmazzon.

A kétnapos ciklus ajánlott felépítése:

**1. nap**
- Domain és adatmodell.
- Application logika.
- API végpont.
- Első működő verzió.

**2. nap**
- Blazor felület.
- Validáció.
- Tesztelés.
- Bemutató.
- Pull Request.

A tisztán infrastrukturális vagy kutatási Issue-knál a bemutató lehet működő technikai demonstráció vagy mérési eredmény.

---

# 19. A projekt végleges elkészülési feltételei

A Smart Finance akkor tekinthető elkészültnek, ha:

- A felhasználók regisztrálhatnak és bejelentkezhetnek.
- A bevételek és kiadások kezelhetők.
- A felhasználói adatok megfelelően elkülönülnek.
- A CSV-import működik.
- A rendszer szabályalapú kategorizálást biztosít.
- Az ML.NET kategorizálás működik.
- A két kategorizálási módszer összehasonlítható.
- A dashboard megjeleníti a fő pénzügyi mutatókat.
- A rendszer anomáliákat és ismétlődő kiadásokat képes jelezni.
- A költségkeretek kezelhetők.
- A rendszer legalább két előrejelzési módszert támogat.
- A párhuzamos és szekvenciális feldolgozás mérhető.
- A fő üzleti folyamatok automatizált tesztekkel ellenőrzöttek.
- A szakdolgozati mérési eredmények dokumentálhatók.

---

# 20. Későbbi továbbfejlesztési lehetőségek

A későbbi verziókban megfontolható:
- Banki API-integráció.
- Több pénznem közötti árfolyamkezelés.
- Több bankszámla kezelése.
- Pénzügyi célok és megtakarítások.
- Értesítések.
- Fejlettebb idősoros előrejelzés.
- Mobilalkalmazás.
- További importformátumok.

Ezek nem részei az első verzió kötelező fejlesztési körének.