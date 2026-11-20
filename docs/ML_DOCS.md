# ML-alapú kategorizálás (UC-17 – UC-20)

## Működés dióhéjban

Automatikus kategorizálás (tranzakció rögzítésekor és CSV-importnál), ebben a sorrendben:

1. **A felhasználó szabályai** (UC-15, BR-06) – ha illeszkedik, a szabály dönt, a modell meg sem hívódik.
2. **ML-modell** (UC-18) – csak ha van aktív modell. A modell a leírásból (normalizálva) megjósolja a kategóriát és a megbízhatóságot.
3. **Megbízhatósági döntés** (UC-19):
   - magas (alapértelmezés: ≥ 0,80) → automatikus hozzárendelés, állapot: `MachineLearning`;
   - közepes (≥ 0,40) és alacsony (≥ 0,20) → nem kerül mentésre, a **Javaslatok** oldalon jelenik meg megerősítésre;
   - 0,20 alatt → nincs javaslat, a tranzakció kategorizálatlan marad.
4. Ha nincs aktív modell, vagy a modell érvénytelen / a tranzakció típusával összeegyeztethetetlen kategóriát ad: a tranzakció kategorizálatlan marad (a mentés sosem hiúsul meg a modell miatt).

Kézi javítás (UC-17): `POST /api/categorization/feedback` (és a tranzakciólista „Kategória javítása” gombja, valamint a Javaslatok oldal). A tranzakció `ManuallyAssigned` lesz, és egy `CategorizationFeedback` sor őrzi, mit javasolt a rendszer (`PredictedCategoryId`) és mi lett a helyes (`CorrectedCategoryId`). A visszajelzés **nem indít újratanítást**.

## Végpontok

| Végpont | Jogosultság | Leírás |
|---|---|---|
| `POST /api/categorization/feedback` | bejelentkezett | UC-17: kézi javítás / javaslat megerősítése |
| `POST /api/categorization/predict` | bejelentkezett | UC-18/19: javaslat egy leíráshoz (nem módosít) |
| `GET /api/categorization/suggestions` | bejelentkezett | UC-19: megerősítésre váró javaslatok a kategorizálatlan tranzakciókhoz |
| `POST /api/categorization/models/train` | **üzemeltető** | UC-20: tanítás + kiértékelés → új, **inaktív** verzió |
| `GET /api/categorization/models/access` | üzemeltető | 204, ha van jogosultság (a felület ezzel dönti el, megjelenjen-e a menüpont); másnak 403 |
| `GET /api/categorization/models` | üzemeltető | verziók listája |
| `GET /api/categorization/models/{id}` | üzemeltető | verzió + teljes kiértékelési jelentés (JSON) |
| `POST /api/categorization/models/{id}/activate` | üzemeltető | aktiválás (csak ha teljesíti a feltételeket) |
| `POST /api/categorization/models/{id}/deactivate` | üzemeltető | az aktív modell kivezetése |

### Üzemeltetői jogosultság

Az „üzemeltető” az, akinek az e-mail-címe szerepel a `Categorization:Ml:AdminEmails` konfigurációban (alapból üres, vagyis senki). A jogosultság **csak** a modellkezelő végpontokra szól, és a válaszok nem tartalmaznak személyes pénzügyi adatot (csak mutatókat).

```powershell
cd src/FinanceTracker.API
dotnet user-secrets set "Categorization:Ml:AdminEmails:0" "te@example.com"
```

Docker Compose-ban az `.env` fájlba: `ML_ADMIN_EMAIL=te@example.com`.

> Figyelem: a regisztráció nem ellenőrzi az e-mail-cím tulajdonjogát, ezért az üzemeltetői e-mail-címmel előbb regisztrálj te magad, mielőtt a konfigurációba teszed.

## Üzemeltetői admin oldal (`/admin/models`)

Az üzemeltető fiókjával bejelentkezve a bal oldali menüben megjelenik az **Üzemeltetés → Modellkezelés** pont (más felhasználónak nem; az oldalt közvetlen címmel megnyitva is „Nincs jogosultságod” üzenetet kap, és az API is 403-at ad). Az oldalon:

- a modellverziók táblázata (létrehozás ideje, pontosság, F1, tanító/teszt példák száma, az aktiválási feltételek teljesülése, aktív/inaktív állapot);
- **Új modell tanítása** gomb (az eredmény mindig inaktív verzió);
- **Részletek**: pontosság, makro F1, predikciós idő, az aktiválási feltételek soronként, az alapmegoldásokkal és az aktív modellel való összevetés, küszöbelemzés, kategóriánkénti mutatók;
- **Aktiválás** (csak a feltételeket teljesítő verziónál) és **Kikapcsolás**.

Az oldal ugyanazokat az API-végpontokat hívja, mint az alábbi lépések, így az `.http` fájl használata nem szükséges.

## Modell tanítása (UC-20)

1. Gyűjts kézzel kategorizált tranzakciókat (rendszerkategóriákkal). Gyors kipróbálás: az **Import** oldalról letölthető `ml-training-samples.csv` (242 sor).
2. Jelentkezz be üzemeltetőként, és nyisd meg a **Modellkezelés** oldalt, majd kattints az **Új modell tanítása** gombra (vagy API-val: `POST /api/categorization/models/train`, a JWT a bejelentkezés válaszából jön).
3. A válasz egy új, inaktív verzió. A részletes jelentés: `GET /api/categorization/models/{id}` (`evaluation`): pontosság, makro precision/recall/F1, osztályonkénti mutatók, átlagos predikciós idő, szabályalapú alapmegoldás, „legtöbbször előforduló kategória” alapmegoldás, az aktív modellel való összevetés, **küszöbelemzés** (lefedettség és pontosság küszöbönként), és az aktiválási feltételek eredménye.
4. Ha `meetsCriteria = true`: `POST .../{id}/activate`. Az aktiválás mindig tudatos lépés, a tanítás vagy a visszajelzések sosem aktiválnak automatikusan.

### Adatok és szivárgás

- Tanítóadat: a **kézzel** kategorizált tranzakciók, ha a kategória **rendszerkategória** (a modell minden felhasználó között közös). Az automatikusan (szabály/modell) besorolt tételek kimaradnak, így a modell nem a saját tippjein tanul.
- A tanító- és teszthalmaz a tranzakció azonosítójának hash-éből adódik (`Seed`), ezért determinisztikus és stabil: egy tranzakció mindig ugyanabban a halmazban marad, a teszthalmaz nem kerül a tanításba.
- A kiértékelés kizárólag a teszthalmazon történik. A jelentés mutatja a teszt- és tanítószövegek átfedését is (ismétlődő kereskedők miatti optimizmus mérőszáma).
- A tanítónak nem jut el felhasználóazonosító.

### Aktiválási feltételek (alapértelmezés, `Categorization:Ml`)

| Feltétel | Alapérték |
|---|---|
| Tesztadatok száma | ≥ 20 (`MinTestExamples`) |
| Pontosság | ≥ 0,70 (`MinAccuracy`) |
| Makro F1 | ≥ 0,60 (`MinMacroF1`) |
| Átlagos predikciós idő | ≤ 50 ms (`MaxAveragePredictionMs`) |
| Jobb a „legtöbbször előforduló kategória” tippelésénél | mindig |
| Nem rosszabb a szabályalapú alapmegoldásnál | mindig |
| Nem rosszabb az aktív modellnél | `MaxRegressionVsActive` = 0 |

A küszöbök (UC-19: `AutoAssignThreshold`, `MediumThreshold`, `MinSuggestionConfidence`) és az aktiválási feltételek **kezdőértékek**. A végleges értékeket a kiértékelés `thresholds` listája alapján kell beállítani: válaszd azt az automatikus küszöböt, amelynél a pontosság már elfogadható, és a lefedettség még hasznos.

## Modellfájlok

A modellfájlok nem az adatbázisban vannak, hanem a `Categorization:Ml:ModelDirectory` könyvtárban (alapértelmezés: `models` az alkalmazás könyvtárában; Dockerben `/app/models`, külön volume-on). Az adatbázis (`MLModelVersions`) csak a fájl nevét tárolja; a nevet mindig a szerver generálja, felhasználói bemenetből nem származhat, és a feloldott útvonal nem léphet ki a modellkönyvtárból. Régi verziók megmaradnak.

## Ismert korlátok

- Nincs automatikus újratanítás: a modellkezelés üzemeltetői feladat (egyszerű admin oldal vagy API).
- A modell közös, ezért a felhasználók saját kategóriáira nem tanul.
- Egy tanítás szinkron HTTP-kérésként fut (hobbi léptékben másodpercek); egyszerre egy tanítás futhat.
