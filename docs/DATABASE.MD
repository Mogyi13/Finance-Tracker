# Smart Finance – Adatbázis-specifikáció

## 1. A dokumentum célja

Ez a dokumentum meghatározza a Smart Finance alkalmazás relációs adatbázisának tervezett szerkezetét, az entitásokat, azok mezőit, kapcsolatait, megszorításait és indexelési stratégiáját.

Az adatbázis-kezelő rendszer:

- Microsoft SQL Server

Az adatbázis eléréséért és kezeléséért az Entity Framework Core felel.

A felhasználók hitelesítését az ASP.NET Core Identity biztosítja.

## 2. Adatbázis-tervezési alapelvek

### 2.1. Felhasználói adatok elkülönítése

A rendszer többfelhasználós működésre készül.

Minden felhasználói pénzügyi adatnak egyértelmű tulajdonosa van.

Az alábbi adatokhoz kötelező a felhasználóhoz rendelés:

- Tranzakciók.
- Saját kategóriák.
- Költségkeretek.
- Kategorizálási szabályok.
- Importfolyamatok.
- Anomáliák.
- Ismétlődő kiadások.
- Előrejelzési eredmények.

A rendszerkategóriák közös adatok, ezért ezeknél a `UserId` mező értéke `NULL`.

### 2.2. Azonosítók

Az elsődleges kulcsok alapértelmezett típusa:

`uniqueidentifier`

Az ASP.NET Core Identity felhasználói azonosítójának típusa szintén `string` vagy a választott Identity-konfiguráció szerinti típus lehet.

A projektben célszerű egységesen `Guid` típusú felhasználói azonosítót alkalmazni, például `IdentityUser<Guid>` használatával.

Ennek megfelelően a pénzügyi táblák `UserId` mezője `uniqueidentifier`.

### 2.3. Pénzügyi adatok tárolása

Az összegek típusa:

`decimal(18,2)`

A pénznem tárolása:

`char(3)`

Példák:

- HUF
- EUR
- USD

A rendszer nem végez automatikus devizaátváltást az első verzióban.

Különböző pénznemű tranzakciókat nem szabad közvetlenül összegezni.

### 2.4. Dátumok tárolása

A tranzakciók üzleti dátuma:

`date`

A létrehozási és módosítási időpontok:

`datetime2`

A rendszer által generált időpontokat UTC időzónában kell tárolni.

A tranzakció dátuma nem feltétlenül azonos a rekord létrehozásának időpontjával.

### 2.5. Törlési szabályok

A felhasználói adatokhoz kapcsolódó idegen kulcsoknál körültekintően kell megválasztani a törlési viselkedést.

Javaslat:

- Kategória nem törölhető, ha tranzakció vagy költségkeret hivatkozik rá.
- Importfolyamat törlésekor a hozzá tartozó importhibák együtt törölhetők.
- Tranzakció törlésekor a kapcsolódó visszajelzések és anomáliarekordok kezelése explicit módon történjen.
- Felhasználói fiók törlésekor a kapcsolódó pénzügyi adatok törlése vagy anonimizálása külön üzleti folyamat legyen.

## 3. Táblák áttekintése

| Tábla | Feladat |
|---|---|
| AspNetUsers | Felhasználói fiókok és profiladatok |
| AspNetRoles | Szerepkörök |
| AspNetUserRoles | Felhasználók szerepkörei |
| Transactions | Bevételek és kiadások |
| Categories | Rendszer- és saját kategóriák |
| Budgets | Költségkeretek |
| CategorizationRules | Felhasználói kategorizálási szabályok |
| CategorizationFeedback | Kategorizálási javítások |
| ImportBatches | CSV-importfolyamatok |
| ImportErrors | Importálási hibák |
| FinancialAnomalies | Felismert pénzügyi anomáliák |
| RecurringTransactions | Felismert ismétlődő kiadások |
| ForecastResults | Előrejelzési eredmények |
| MLModelVersions | Gépi tanulási modellek verziói |

Az `AspNetUsers` és a kapcsolódó Identity-táblákat az ASP.NET Core Identity kezeli.

## 4. Részletes tábladefiníciók

### 4.1. AspNetUsers – Felhasználók

Az ASP.NET Core Identity által kezelt felhasználói tábla.

A projektben javasolt egy saját `ApplicationUser` osztály létrehozása, amely az `IdentityUser<Guid>` osztályból származik.

| Mező | Típus | Kötelező | Leírás |
|---|---|---|---|
| Id | uniqueidentifier | Igen | Elsődleges kulcs |
| UserName | nvarchar(256) | Igen | Felhasználónév |
| Email | nvarchar(256) | Igen | E-mail-cím |
| DisplayName | nvarchar(100) | Igen | Megjelenítendő név |
| PasswordHash | nvarchar(max) | Nem | Az Identity által kezelt jelszóhash |
| CreatedAt | datetime2 | Igen | Regisztráció időpontja |

A felsorolt Identity-mezők egy részét maga az Identity biztosítja, a `DisplayName` és `CreatedAt` saját bővítés.

Az Identity további mezői, például a `NormalizedEmail`, `NormalizedUserName`, `SecurityStamp` és `ConcurrencyStamp` szintén megmaradnak.

**Megjegyzés:** jelszót soha nem tárolunk olvasható formában.

### 4.2. Categories – Kategóriák

A tranzakciók csoportosítására szolgáló kategóriák.

| Mező | Típus | Kötelező | Leírás |
|---|---|---|---|
| Id | uniqueidentifier | Igen | Elsődleges kulcs |
| UserId | uniqueidentifier | Nem | Tulajdonos, NULL esetén rendszerkategória |
| Name | nvarchar(100) | Igen | Kategória neve |
| Type | tinyint | Igen | Income vagy Expense |
| IsSystemCategory | bit | Igen | Rendszerkategória-e |
| CreatedAt | datetime2 | Igen | Létrehozás időpontja |

A `Type` értékei:

- `1` – Income
- `2` – Expense

Üzleti szabályok:

- A rendszerkategóriák `UserId` értéke NULL.
- A saját kategóriák `UserId` értéke kötelező.
- A rendszerkategóriák nem módosíthatók normál felhasználó által.
- Egy felhasználónál azonos típusú, azonos nevű kategória nem hozható létre többször.

### 4.3. Transactions – Tranzakciók

A rendszer egyik legfontosabb táblája. Minden bevétel és kiadás ebben a táblában szerepel.

| Mező | Típus | Kötelező | Leírás |
|---|---|---|---|
| Id | uniqueidentifier | Igen | Elsődleges kulcs |
| UserId | uniqueidentifier | Igen | Tulajdonos |
| CategoryId | uniqueidentifier | Nem | Hozzárendelt kategória |
| Amount | decimal(18,2) | Igen | Tranzakció összege |
| Currency | char(3) | Igen | Pénznem |
| Type | tinyint | Igen | Bevétel vagy kiadás |
| Description | nvarchar(500) | Nem | Tranzakció leírása |
| TransactionDate | date | Igen | Tranzakció dátuma |
| Source | tinyint | Igen | Rögzítés forrása |
| CategorizationStatus | tinyint | Igen | Kategorizálás állapota |
| CreatedAt | datetime2 | Igen | Létrehozás időpontja |
| UpdatedAt | datetime2 | Nem | Utolsó módosítás időpontja |

A `Type` értékei:

- `1` – Income
- `2` – Expense

A `Source` értékei:

- `1` – Manual
- `2` – CsvImport

A `CategorizationStatus` értékei:

- `0` – Uncategorized
- `1` – RuleBased
- `2` – MachineLearning
- `3` – ManuallyAssigned

Fontos szabályok:

- Az összeg csak pozitív lehet.
- A bevétel vagy kiadás jellegét a `Type` határozza meg, nem az összeg előjele.
- A kategória típusa meg kell egyezzen a tranzakció típusával.
- A kategória csak a felhasználó saját kategóriája vagy rendszerkategória lehet.
- A tranzakció csak a saját felhasználója számára hozzáférhető.

### 4.4. Budgets – Költségkeretek

A felhasználók kategóriánkénti kiadási korlátait tárolja.

| Mező | Típus | Kötelező | Leírás |
|---|---|---|---|
| Id | uniqueidentifier | Igen | Elsődleges kulcs |
| UserId | uniqueidentifier | Igen | Tulajdonos |
| CategoryId | uniqueidentifier | Igen | Kategória |
| Amount | decimal(18,2) | Igen | Költségkeret összege |
| Currency | char(3) | Igen | Pénznem |
| PeriodType | tinyint | Igen | Időszak típusa |
| StartDate | date | Igen | Időszak kezdete |
| EndDate | date | Nem | Időszak vége |
| CreatedAt | datetime2 | Igen | Létrehozás időpontja |
| UpdatedAt | datetime2 | Nem | Módosítás időpontja |

A `PeriodType` értékei:

- `1` – Monthly
- `2` – Weekly
- `3` – Custom

Üzleti szabályok:

- A költségkeret összege pozitív.
- A költségkeret csak kiadási kategóriához kapcsolódhat.
- A kategóriának a felhasználóhoz vagy a rendszerhez kell tartoznia.
- A költségkeret és a tranzakciók pénznemének egyeznie kell az összesítés során.
- A `StartDate` nem lehet későbbi az `EndDate` értékénél.

### 4.5. CategorizationRules – Kategorizálási szabályok

A felhasználók által létrehozott automatikus kategorizálási szabályok.

| Mező | Típus | Kötelező | Leírás |
|---|---|---|---|
| Id | uniqueidentifier | Igen | Elsődleges kulcs |
| UserId | uniqueidentifier | Igen | Tulajdonos |
| CategoryId | uniqueidentifier | Igen | Célkategória |
| Keyword | nvarchar(200) | Igen | Keresett kifejezés |
| MatchType | tinyint | Igen | Egyezés típusa |
| Priority | int | Igen | Szabály prioritása |
| IsActive | bit | Igen | Aktív állapot |
| CreatedAt | datetime2 | Igen | Létrehozás időpontja |
| UpdatedAt | datetime2 | Nem | Módosítás időpontja |

A `MatchType` értékei:

- `1` – Exact
- `2` – Contains
- `3` – StartsWith

Üzleti szabályok:

- A szabály csak a tulajdonos tranzakcióira alkalmazható.
- A célkategória típusának kompatibilisnek kell lennie a tranzakció típusával.
- A magasabb prioritású szabályok kerülnek előbb kiértékelésre.
- A kikapcsolt szabályokat a rendszer figyelmen kívül hagyja.

### 4.6. CategorizationFeedback – Kategorizálási visszajelzések

A felhasználó által javított kategorizálások tárolására szolgál.

| Mező | Típus | Kötelező | Leírás |
|---|---|---|---|
| Id | uniqueidentifier | Igen | Elsődleges kulcs |
| TransactionId | uniqueidentifier | Igen | Érintett tranzakció |
| PredictedCategoryId | uniqueidentifier | Nem | Korábban javasolt kategória |
| CorrectedCategoryId | uniqueidentifier | Igen | Felhasználó által megadott kategória |
| CreatedAt | datetime2 | Igen | Visszajelzés időpontja |

A visszajelzések felhasználhatók:

- Későbbi tanítóadatkészletek előállítására.
- A kategorizálási módszerek összehasonlítására.
- A modell pontosságának elemzésére.

Fontos szabály: a visszajelzés mentése önmagában nem indít modell-újratanítást.

### 4.7. ImportBatches – Importfolyamatok

Egy CSV-fájl feltöltésének és feldolgozásának eredményét tárolja.

| Mező | Típus | Kötelező | Leírás |
|---|---|---|---|
| Id | uniqueidentifier | Igen | Elsődleges kulcs |
| UserId | uniqueidentifier | Igen | Tulajdonos |
| FileName | nvarchar(255) | Igen | Feltöltött fájl neve |
| Status | tinyint | Igen | Import állapota |
| TotalRows | int | Igen | Összes feldolgozott sor |
| SuccessfulRows | int | Igen | Sikeres sorok |
| FailedRows | int | Igen | Hibás sorok |
| DuplicateRows | int | Igen | Duplikált sorok |
| CreatedAt | datetime2 | Igen | Import indítása |
| CompletedAt | datetime2 | Nem | Befejezés időpontja |

A `Status` értékei:

- `1` – Pending
- `2` – Processing
- `3` – Completed
- `4` – PartiallyCompleted
- `5` – Failed

A fájl tartalmát nem szükséges tartósan az adatbázisban tárolni. A feldolgozás után csak az importfolyamat eredményeit és a szükséges hibainformációkat őrizzük meg.

### 4.8. ImportErrors – Importálási hibák

Az import során előforduló hibák részletes nyilvántartása.

| Mező | Típus | Kötelező | Leírás |
|---|---|---|---|
| Id | uniqueidentifier | Igen | Elsődleges kulcs |
| ImportBatchId | uniqueidentifier | Igen | Kapcsolódó importfolyamat |
| RowNumber | int | Igen | Hibás sor sorszáma |
| ErrorCode | nvarchar(50) | Igen | Hibatípus azonosítója |
| ErrorMessage | nvarchar(500) | Igen | Hiba leírása |

Példák a hibakódokra:

- `INVALID_DATE`
- `INVALID_AMOUNT`
- `MISSING_DESCRIPTION`
- `INVALID_CURRENCY`
- `INVALID_TYPE`
- `INVALID_CATEGORY`

### 4.9. FinancialAnomalies – Pénzügyi anomáliák

A rendszer által felismert szokatlan tranzakciókat tárolja.

| Mező | Típus | Kötelező | Leírás |
|---|---|---|---|
| Id | uniqueidentifier | Igen | Elsődleges kulcs |
| UserId | uniqueidentifier | Igen | Tulajdonos |
| TransactionId | uniqueidentifier | Igen | Érintett tranzakció |
| AnomalyType | tinyint | Igen | Anomália típusa |
| Score | decimal(10,4) | Nem | Számított pontszám |
| Description | nvarchar(500) | Igen | Értelmezhető magyarázat |
| DetectedAt | datetime2 | Igen | Felismerés időpontja |
| IsReviewed | bit | Igen | Felhasználó megtekintette-e |

Az `AnomalyType` kezdeti értékei:

- `1` – UnusuallyHighAmount
- `2` – CategoryDeviation

A táblában tárolt anomália nem jelenti azt, hogy a tranzakció hibás vagy csalárd.

### 4.10. RecurringTransactions – Ismétlődő kiadások

A rendszer által felismert ismétlődő tranzakciós mintákat tárolja.

| Mező | Típus | Kötelező | Leírás |
|---|---|---|---|
| Id | uniqueidentifier | Igen | Elsődleges kulcs |
| UserId | uniqueidentifier | Igen | Tulajdonos |
| Description | nvarchar(200) | Igen | Felismert tranzakciócsoport leírása |
| CategoryId | uniqueidentifier | Nem | Kapcsolódó kategória |
| ExpectedAmount | decimal(18,2) | Igen | Becsült összeg |
| Currency | char(3) | Igen | Pénznem |
| Frequency | tinyint | Igen | Ismétlődés gyakorisága |
| LastOccurrence | date | Igen | Utolsó előfordulás |
| Confidence | decimal(5,4) | Igen | Felismerési bizonyosság |
| CreatedAt | datetime2 | Igen | Felismerés időpontja |

A `Frequency` értékei:

- `1` – Weekly
- `2` – Monthly
- `3` – Quarterly
- `4` – Yearly

Az ismétlődő kiadások felismerése becslésen alapul, ezért az eredményeket nem szabad automatikusan megerősített előfizetésként kezelni.

### 4.11. ForecastResults – Előrejelzési eredmények

A rendszer által készített pénzügyi előrejelzéseket tárolja.

| Mező | Típus | Kötelező | Leírás |
|---|---|---|---|
| Id | uniqueidentifier | Igen | Elsődleges kulcs |
| UserId | uniqueidentifier | Igen | Tulajdonos |
| ForecastType | tinyint | Igen | Előrejelzés típusa |
| Method | tinyint | Igen | Alkalmazott módszer |
| Currency | char(3) | Igen | Pénznem |
| PeriodStart | date | Igen | Előrejelzési időszak kezdete |
| PeriodEnd | date | Igen | Előrejelzési időszak vége |
| PredictedAmount | decimal(18,2) | Igen | Előrejelzett összeg |
| CreatedAt | datetime2 | Igen | Előállítás időpontja |
| ModelVersionId | uniqueidentifier | Nem | Használt modellverzió, ha releváns |

A `ForecastType` értékei:

- `1` – MonthlyExpenses
- `2` – MonthlyNetCashFlow

A `Method` értékei:

- `1` – MovingAverage
- `2` – LinearRegression

A mozgóátlag és a lineáris regresszió eredményeit külön rekordokban kell tárolni, hogy összehasonlíthatók legyenek.

### 4.12. MLModelVersions – Gépi tanulási modellverziók

A kategorizáláshoz használt ML.NET-modellek verzióit és értékelési eredményeit tárolja.

| Mező | Típus | Kötelező | Leírás |
|---|---|---|---|
| Id | uniqueidentifier | Igen | Elsődleges kulcs |
| Version | nvarchar(50) | Igen | Modellverzió azonosítója |
| ModelPath | nvarchar(500) | Igen | Modellfájl elérési útja |
| Accuracy | decimal(8,6) | Nem | Pontosság |
| Precision | decimal(8,6) | Nem | Precision |
| Recall | decimal(8,6) | Nem | Recall |
| F1Score | decimal(8,6) | Nem | F1-score |
| IsActive | bit | Igen | Aktív modell-e |
| TrainingDataCount | int | Igen | Tanítóadatok száma |
| CreatedAt | datetime2 | Igen | Létrehozás időpontja |

Üzleti szabályok:

- Egyszerre csak egy aktív kategorizáló modell használható.
- Egy korábbi modellverzió megőrizhető összehasonlítás céljából.
- Az új modell csak sikeres értékelés után aktiválható.
- A modellfájlokat nem szükséges közvetlenül az adatbázisban tárolni.
- A `ModelPath` egy biztonságosan kezelt szerveroldali elérési út vagy azonosító legyen, ne felhasználó által megadható fájlútvonal.

## 5. Táblák közötti kapcsolatok

### 5.1. Kapcsolatok áttekintése

| Szülőtábla | Gyermektábla | Kapcsolat | Leírás |
|---|---|---|---|
| AspNetUsers | Transactions | 1:N | Egy felhasználónak több tranzakciója lehet |
| AspNetUsers | Categories | 1:N | Egy felhasználónak több saját kategóriája lehet |
| AspNetUsers | Budgets | 1:N | Egy felhasználónak több költségkerete lehet |
| AspNetUsers | CategorizationRules | 1:N | Egy felhasználónak több szabálya lehet |
| AspNetUsers | ImportBatches | 1:N | Egy felhasználónak több importja lehet |
| AspNetUsers | FinancialAnomalies | 1:N | Egy felhasználónak több anomáliája lehet |
| AspNetUsers | RecurringTransactions | 1:N | Egy felhasználónak több ismétlődő kiadása lehet |
| AspNetUsers | ForecastResults | 1:N | Egy felhasználónak több előrejelzése lehet |
| Categories | Transactions | 1:N | Egy kategóriához több tranzakció tartozhat |
| Categories | Budgets | 1:N | Egy kategóriához több költségkeret tartozhat |
| Categories | CategorizationRules | 1:N | Egy kategóriához több szabály tartozhat |
| Categories | CategorizationFeedback | 1:N | Egy kategória több visszajelzésben szerepelhet |
| Categories | RecurringTransactions | 1:N | Egy kategóriához több ismétlődő kiadás tartozhat |
| Transactions | CategorizationFeedback | 1:N | Egy tranzakcióhoz több javítási esemény tartozhat |
| Transactions | FinancialAnomalies | 1:N | Egy tranzakcióhoz több anomáliaészlelés tartozhat |
| ImportBatches | ImportErrors | 1:N | Egy importhoz több hiba tartozhat |
| MLModelVersions | ForecastResults | 1:N | Egy modellverzióhoz több előrejelzés kapcsolódhat, ha releváns |

A kategorizálási visszajelzések esetében a `PredictedCategoryId` opcionális, a `CorrectedCategoryId` kötelező.

## 6. Fontos adatbázis-megszorítások

### 6.1. Tranzakciók

- `Amount > 0`
- `Currency` nem lehet üres.
- `Type` csak támogatott érték lehet.
- A `CategoryId` csak megfelelő típusú kategóriára mutathat.
- A `UserId` nem lehet NULL.

### 6.2. Kategóriák

- A kategórianév nem lehet üres.
- A kategóriatípus csak Income vagy Expense lehet.
- Rendszerkategóriánál `UserId = NULL`.
- Saját kategóriánál `UserId` kötelező.

### 6.3. Költségkeretek

- `Amount > 0`
- `StartDate <= EndDate`, ha az EndDate meg van adva.
- Csak kiadási kategória használható.

### 6.4. Kategorizálási szabályok

- A prioritás egész szám.
- A kulcsszó nem lehet üres.
- A szabály aktív állapota logikai érték.
- A célkategóriának a tulajdonoshoz vagy a rendszerhez kell tartoznia.

### 6.5. Előrejelzések

- `PeriodStart <= PeriodEnd`
- A pénznem kötelező.
- Az előrejelzési módszer csak támogatott érték lehet.
- Az előrejelzéshez elegendő történeti adatnak kell rendelkezésre állnia.

## 7. Indexelési stratégia

A gyakori lekérdezések gyorsítása érdekében az alábbi indexek javasoltak.

| Tábla | Indexelt mezők | Cél |
|---|---|---|
| Transactions | UserId, TransactionDate | Felhasználói tranzakciók időszak szerinti listázása |
| Transactions | UserId, CategoryId, TransactionDate | Kategóriánkénti elemzés |
| Transactions | UserId, Type, TransactionDate | Bevétel és kiadás szűrése |
| Categories | UserId, Type, Name | Kategóriák gyors lekérdezése |
| Budgets | UserId, CategoryId | Költségkeretek lekérdezése |
| CategorizationRules | UserId, IsActive, Priority | Aktív szabályok prioritás szerinti lekérése |
| ImportBatches | UserId, CreatedAt | Importelőzmények listázása |
| ImportErrors | ImportBatchId, RowNumber | Importhibák lekérdezése |
| FinancialAnomalies | UserId, DetectedAt | Anomáliák listázása |
| ForecastResults | UserId, ForecastType, PeriodStart | Előrejelzések lekérdezése |

Az indexeket az EF Core Fluent API segítségével célszerű konfigurálni.

## 8. Duplikációfelismerés adatbázis-szinten

A CSV-import során duplikációk felismerésére tranzakciós ujjlenyomat használható.

Javasolt kiegészítő mező a `Transactions` táblában:

| Mező | Típus | Kötelező | Leírás |
|---|---|---|---|
| Fingerprint | char(64) | Nem | SHA-256 tranzakciós ujjlenyomat |

Az ujjlenyomat számítása a következő normalizált mezők alapján történhet:

- UserId.
- TransactionDate.
- Amount.
- Currency.
- Type.
- Normalizált Description.

Az ujjlenyomat egyedisége felhasználónként ellenőrizhető.

Javasolt szűrt egyedi index:

`(UserId, Fingerprint)`

A `Fingerprint` mező lehet NULL a kézzel létrehozott vagy még nem ellenőrzött tranzakcióknál.

Fontos: az ujjlenyomat alapú felismerés nem tökéletes. Két valós tranzakció adatai is lehetnek azonosak, ezért a duplikációkezelést a felhasználói felületen is egyértelművé kell tenni.

## 9. EF Core megvalósítási javaslat

Az adatbázis kezelésére az Entity Framework Core használata javasolt.

### 9.1. DbContext

Javasolt egy központi adatbázis-kontextus:

`ApplicationDbContext`

Az `ApplicationDbContext` az `IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>` osztályból származzon.

A kontextusban külön DbSet-ek legyenek:

- Transactions
- Categories
- Budgets
- CategorizationRules
- CategorizationFeedback
- ImportBatches
- ImportErrors
- FinancialAnomalies
- RecurringTransactions
- ForecastResults
- MLModelVersions

### 9.2. Fluent API konfiguráció

A táblák konfigurációját külön konfigurációs osztályokba érdemes szervezni.

Példák:

- TransactionConfiguration
- CategoryConfiguration
- BudgetConfiguration
- CategorizationRuleConfiguration
- ImportBatchConfiguration
- FinancialAnomalyConfiguration
- ForecastResultConfiguration

A konfigurációk az `IEntityTypeConfiguration<T>` interfészt valósítsák meg.

A Fluent API segítségével kell meghatározni:

- Elsődleges kulcsok.
- Idegen kulcsok.
- Kötelező mezők.
- Maximális szöveghosszak.
- Decimal pontosság.
- Egyedi megszorítások.
- Indexek.
- Törlési viselkedés.

### 9.3. Migrációk

Az adatbázis létrehozása és módosítása EF Core migrációkkal történjen.

A migrációk forráskód-kezelés alatt maradjanak.

Az adatbázis módosításait nem kézzel, SQL Server Management Studio segítségével kell véglegesen elvégezni, hanem migrációkon keresztül.

Példa parancsok:

```bash
dotnet ef migrations add InitialCreate \
  --project src/FinanceTracker.Infrastructure \
  --startup-project src/FinanceTracker.API

dotnet ef database update \
  --project src/FinanceTracker.Infrastructure \
  --startup-project src/FinanceTracker.API
```

A pontos projektbeállításokhoz a migrációk futtatásakor az Infrastructure és API projektek közötti hivatkozásokat is megfelelően konfigurálni kell.

## 10. Adatbiztonság és jogosultságkezelés

A felhasználói adatok védelme kiemelt követelmény.

Kötelező szabályok:

1. A felhasználó azonosítóját hitelesített szerveroldali kontextusból kell meghatározni.
2. Nem szabad megbízni a frontend által küldött `UserId` értékben.
3. Minden felhasználói rekord lekérdezését tulajdonosra kell szűrni.
4. Módosítás és törlés előtt is ellenőrizni kell a tulajdonjogot.
5. Kategória-hozzárendeléskor ellenőrizni kell a kategória tulajdonosát.
6. Az adminisztrátori jogosultság nem jelent automatikus hozzáférést a személyes pénzügyi adatokhoz.
7. A jelszavakat kizárólag az ASP.NET Core Identity kezelheti.
8. A kapcsolatok és az adatbázis-hozzáférés biztonságosan konfigurálandók.

## 11. Tervezési döntések és tudatos egyszerűsítések

A projekt közepes összetettségű szakdolgozati alkalmazás. Az adatbázisnak támogatnia kell a kutatási funkciókat, de nem szükséges minden későbbi lehetőséget előre megvalósítani.

Az első verzióban:

- Nincs bankszámla-nyilvántartás.
- Nincs banki API-integráció.
- Nincs automatikus devizaátváltás.
- Nincs több pénznemet összesítő egyenleg.
- Nincs natív mobilalkalmazás.
- Nincs automatikus ML-modell újratanítás.
- Nincs összetett előfizetéskezelő modul.
- Nincs külön adattárház.

A pénzügyi összesítések alapját a tranzakciók adják.

A dashboardon megjelenített nettó pénzáramlás nem bankszámla-egyenleg.

A statisztikai elemzések és előrejelzések eredményeit a rendszer szükség szerint újraszámíthatja.

A végleges adatbázis-struktúra kialakításakor a domain modellek és az EF Core konfigurációk legyenek az elsődleges források, a jelen dokumentum pedig az adatmodell tervezési és dokumentációs alapja.