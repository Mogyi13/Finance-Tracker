# Finance Tracker – Use-Case -specifikáció

## 1. A dokumentum célja

Ez a dokumentum bemutatja a Finance Tracker alkalmazás szereplőit, használati eseteit, üzleti folyamatait és a rendszertől elvárt működést.

Az alkalmazás célja, hogy segítse a felhasználókat személyes bevételeik és kiadásaik rögzítésében, kategorizálásában, elemzésében és előrejelzésében.

A rendszer támogatja:
- A szabályalapú és gépi tanuláson alapuló tranzakciókategorizálást.
- A statisztikai anomáliadetektálást.
- Az ismétlődő kiadások felismerését.
- A költségkeretek kezelését.
- A pénzügyi előrejelzések készítését.

## 2. Szereplők

### 2.1. Regisztrált felhasználó

A rendszer elsődleges szereplője.

A regisztrált felhasználó az alábbi műveleteket végezheti:
- Saját profiljának kezelése.
- Tranzakciók létrehozása, megtekintése, módosítása és törlése.
- Személyes kategóriák kezelése.
- Tranzakciók importálása CSV-fájlokból.
- Tranzakciókategorizálási szabályok beállítása.
- Automatikusan hozzárendelt kategóriák javítása.
- Pénzügyi statisztikák és trendek megtekintése.
- Költségkeretek létrehozása és nyomon követése.
- Felismert anomáliák és ismétlődő kiadások áttekintése.
- Pénzügyi előrejelzések megtekintése.

### 2.2. Rendszergazda

A rendszergazda az alkalmazás szintű karbantartásért felelős.

Feladatai többek között:
- Rendszerszintű konfigurációk kezelése.
- Alapértelmezett kategóriák kezelése.
- Alkalmazásnaplók áttekintése.
- Gépi tanulási modellek kezelése és aktiválása.
- Háttérfolyamatok felügyelete.

A rendszergazda nem kaphat automatikusan hozzáférést a felhasználók bizalmas pénzügyi adataihoz.

### 2.3. Háttérfeldolgozó szolgáltatás

Belső rendszerszereplő, amely azokat a műveleteket végzi, amelyek nem akadályozhatják a felhasználói kérések kiszolgálását.

Feladatai:
- Nagyobb CSV-importok feldolgozása.
- Ütemezett anomáliadetektálás futtatása.
- Ismétlődő tranzakciók felismerése.
- Előrejelzések létrehozása vagy frissítése szükség esetén.
- Gépi tanulási modellek tanításához szükséges adathalmazok előkészítése.

## 3. Használatieset-áttekintés

| Azonosító | Használati eset | Elsődleges szereplő |
|---|---|---|
| UC-01 | Felhasználói fiók regisztrálása | Felhasználó |
| UC-02 | Bejelentkezés | Felhasználó |
| UC-03 | Kijelentkezés | Felhasználó |
| UC-04 | Profil kezelése | Felhasználó |
| UC-05 | Tranzakció létrehozása | Felhasználó |
| UC-06 | Tranzakciók megtekintése | Felhasználó |
| UC-07 | Tranzakció módosítása | Felhasználó |
| UC-08 | Tranzakció törlése | Felhasználó |
| UC-09 | Tranzakciók szűrése | Felhasználó |
| UC-10 | Kategóriák kezelése | Felhasználó |
| UC-11 | CSV-fájl importálása | Felhasználó |
| UC-12 | Importált tranzakciók ellenőrzése | Rendszer |
| UC-13 | Duplikált tranzakciók felismerése | Rendszer |
| UC-14 | Importálási eredmények megtekintése | Felhasználó |
| UC-15 | Tranzakció kategorizálása szabályok alapján | Rendszer |
| UC-16 | Kategorizálási szabályok kezelése | Felhasználó |
| UC-17 | Tranzakció kategóriájának javítása | Felhasználó |
| UC-18 | Tranzakció kategorizálása ML.NET segítségével | Rendszer |
| UC-19 | Kategorizálási megbízhatóság értékelése | Rendszer |
| UC-20 | Gépi tanulási modell tanítása és értékelése | Rendszergazda |
| UC-21 | Pénzügyi áttekintés megtekintése | Felhasználó |
| UC-22 | Kiadások elemzése kategóriánként | Felhasználó |
| UC-23 | Pénzügyi időszakok összehasonlítása | Felhasználó |
| UC-24 | Ismétlődő kiadások felismerése | Rendszer |
| UC-25 | Pénzügyi anomáliák felismerése | Rendszer |
| UC-26 | Költségkeretek kezelése | Felhasználó |
| UC-27 | Költségkeretek felhasználásának nyomon követése | Felhasználó |
| UC-28 | Kiadási előrejelzések készítése | Rendszer |
| UC-29 | Előrejelzési módszerek összehasonlítása | Rendszergazda |
| UC-30 | Pénzügyi előrejelzések megtekintése | Felhasználó |
| UC-31 | Vezérlőpult megtekintése | Felhasználó |

## 4. Részletes használati esetek

### UC-01 – Felhasználói fiók regisztrálása

**Szereplő:** Felhasználó

**Előfeltételek:**
- A felhasználó nincs hitelesítve.
- A megadott e-mail-címmel még nem létezik felhasználói fiók.

**Alapfolyamat:**
1. A felhasználó megnyitja a regisztrációs oldalt.
2. Megadja az e-mail-címét, a megjelenített nevét és a jelszavát.
3. A frontend ellenőrzi a megadott adatokat.
4. A backend ellenőrzi a beküldött adatokat.
5. A rendszer ellenőrzi, hogy az e-mail-cím már regisztrálva van-e.
6. Az ASP.NET Core Identity létrehozza a felhasználói fiókot.
7. A rendszer inicializálja a felhasználó alapértelmezett kategóriáit.
8. A rendszer visszajelzést ad a sikeres regisztrációról.

**Alternatív folyamatok:**
- Ha az e-mail-cím már regisztrálva van, a rendszer validációs hibát jelez.
- Ha a jelszó nem felel meg a biztonsági követelményeknek, a regisztráció sikertelen.
- Ha a kategóriák inicializálása sikertelen, a rendszernek el kell kerülnie a felhasználói fiók inkonzisztens állapotban történő létrehozását.

**Utófeltételek:**
- Az új felhasználói fiók létrejött.
- A felhasználó hozzáfér az alapértelmezett kategóriáihoz.

### UC-02 – Bejelentkezés

**Szereplő:** Felhasználó

**Előfeltételek:**
- A felhasználó már rendelkezik regisztrált fiókkal.

**Alapfolyamat:**
1. A felhasználó megadja az e-mail-címét és a jelszavát.
2. A backend ellenőrzi a hitelesítési adatokat.
3. Az ASP.NET Core Identity ellenőrzi a jelszót.
4. A rendszer létrehozza a hitelesítéshez szükséges tokent.
5. A frontend beállítja a hitelesítési állapotot.
6. A rendszer átirányítja a felhasználót a vezérlőpultra.

**Alternatív folyamatok:**
- Hibás hitelesítési adatok esetén a rendszer hitelesítési hibát jelez.
- Letiltott vagy zárolt felhasználói fiókkal nem lehet bejelentkezni.

**Utófeltételek:**
- A felhasználó hitelesített állapotba kerül.
- Elérhetővé válnak a hitelesítéshez kötött alkalmazásfunkciók.

### UC-03 – Kijelentkezés

**Szereplő:** Felhasználó

**Alapfolyamat:**
1. A felhasználó kiválasztja a kijelentkezés lehetőségét.
2. A rendszer törli a Web szerveroldali hitelesítési állapotát és a tárolt tokent.
3. A felhasználó átirányításra kerül a bejelentkezési oldalra.

**Utófeltételek:**
- Az aktuális felhasználói munkamenet már nem hitelesített.

### UC-04 – Profil kezelése

**Szereplő:** Felhasználó

**Alapfolyamat:**
1. A felhasználó megnyitja a profilját.
2. A rendszer betölti az aktuális profiladatokat.
3. A felhasználó módosítja a megjelenített nevét vagy más szerkeszthető profiladatait.
4. A backend ellenőrzi a módosításokat.
5. A rendszer elmenti a frissített adatokat.

**Alternatív folyamatok:**
- Érvénytelen adatok esetén a rendszer elutasítja a módosítást.
- Az e-mail-cím módosítása – amennyiben támogatott – a megfelelő Identity-ellenőrzési folyamatot igényli.

### UC-05 – Tranzakció létrehozása

**Szereplő:** Felhasználó

**Előfeltételek:**
- A felhasználó hitelesített állapotban van.

**Alapfolyamat:**
1. A felhasználó megnyitja a tranzakciós űrlapot.
2. Kiválasztja, hogy bevételt vagy kiadást szeretne rögzíteni.
3. Megadja az összeget, a pénznemet, a leírást és a tranzakció dátumát.
4. Opcionálisan kiválaszt egy kategóriát.
5. A backend ellenőrzi a beküldött adatokat.
6. A rendszer ellenőrzi, hogy a kiválasztott kategória a felhasználóhoz tartozik-e, vagy rendszerszintű kategória.
7. Ha az automatikus kategorizálás engedélyezett, és a felhasználó nem választott ki kifejezetten kategóriát, lefut a kategorizálási folyamat.
8. A rendszer elmenti a tranzakciót.
9. A frontend megjeleníti a frissített tranzakciót.

**Alternatív folyamatok:**
- Az érvénytelen vagy nem pozitív összegeket a rendszer elutasítja.
- A tranzakció típusával nem összeegyeztethető kategória nem rendelhető hozzá.
- Másik felhasználóhoz tartozó kategória nem rendelhető hozzá.
- Ha az automatikus kategorizálás nem tud kategóriát meghatározni, a tranzakció kategorizálatlan marad, vagy manuális ellenőrzést igényel.

**Utófeltételek:**
- A tranzakció elmentésre kerül, és a hitelesített felhasználóhoz kapcsolódik.

### UC-06 – Tranzakciók megtekintése

**Szereplő:** Felhasználó

**Alapfolyamat:**
1. A felhasználó megnyitja a tranzakciók listáját.
2. A backend lekéri a hitelesített felhasználóhoz tartozó tranzakciókat.
3. A rendszer lapozható eredményeket ad vissza.
4. A frontend megjeleníti a tranzakciókat.

**Üzleti szabályok:**
- A felhasználók kizárólag a saját tranzakcióikat tekinthetik meg.
- Az eredményeket lapozható formában kell megjeleníteni.
- A tranzakciók dátum és összeg szerint rendezhetők legyenek.

### UC-07 – Tranzakció módosítása

**Szereplő:** Felhasználó

**Alapfolyamat:**
1. A felhasználó kiválaszt egy tranzakciót.
2. A rendszer ellenőrzi a tranzakció tulajdonosát.
3. A felhasználó módosítja a tranzakció adatait.
4. A backend ellenőrzi a módosításokat.
5. A rendszer elmenti a frissített tranzakciót.
6. A kapcsolódó elemzések és származtatott eredmények szükség esetén újraszámításra vagy frissítésre lesznek megjelölve.

**Alternatív folyamatok:**
- Másik felhasználóhoz tartozó tranzakció nem érhető el.
- Az érvénytelen kategória-hozzárendeléseket a rendszer elutasítja.

### UC-08 – Tranzakció törlése

**Szereplő:** Felhasználó

**Alapfolyamat:**
1. A felhasználó kiválaszt egy törlendő tranzakciót.
2. A rendszer ellenőrzi a tranzakció tulajdonosát.
3. A felhasználó megerősíti a törlést.
4. A backend törli a tranzakciót.
5. A kapcsolódó származtatott információk újraszámításra vagy érvénytelenítésre kerülnek.

**Üzleti szabályok:**
- A felhasználó nem törölheti más felhasználó tranzakcióját.
- A kapcsolódó rekordoknak az adatbázisban beállított idegenkulcs-törlési szabályokat kell követniük.

### UC-09 – Tranzakciók szűrése

**Szereplő:** Felhasználó

A felhasználó az alábbi szempontok szerint szűrheti a tranzakciókat:
- Dátumtartomány.
- Tranzakció típusa.
- Kategória.
- Minimális és maximális összeg.
- Leírás vagy kulcsszó.

A rendszer kizárólag a hitelesített felhasználó tranzakcióira alkalmazhatja a szűrőfeltételeket.

### UC-10 – Kategóriák kezelése

**Szereplő:** Felhasználó

**Alapfolyamat:**
1. A felhasználó megnyitja a kategóriakezelést.
2. A rendszer megjeleníti a rendszerszintű és a felhasználó saját kategóriáit.
3. A felhasználó létrehoz, módosít vagy töröl egy személyes kategóriát.
4. A backend ellenőrzi a kategória nevét és típusát.
5. A rendszer elmenti a módosítást.

**Üzleti szabályok:**
- A rendszerszintű kategóriákat a normál felhasználók nem módosíthatják és nem törölhetik.
- Egy kategória nem törölhető, amíg meglévő tranzakciók vagy költségkeretek hivatkoznak rá, kivéve, ha külön átsorolási folyamat kerül megvalósításra.
- A bevételi és kiadási kategóriákat külön kell kezelni.

### UC-11 – CSV-fájl importálása

**Szereplő:** Felhasználó

**Előfeltételek:**
- A felhasználó hitelesített állapotban van.
- A feltöltött fájl támogatott CSV-fájl.

**Alapfolyamat:**
1. A felhasználó megnyitja az importálási oldalt.
2. Kiválaszt egy CSV-fájlt.
3. Beállítja vagy megerősíti a CSV-oszlopok megfeleltetését.
4. A frontend feltölti a fájlt.
5. A backend létrehoz egy importálási köteget.
6. A rendszer ellenőrzi a fájl szerkezetét.
7. A rendszer feldolgozza a sorokat.
8. Az érvényes tranzakciók elmentésre kerülnek.
9. Az érvénytelen sorok importálási hibaként kerülnek rögzítésre.
10. A duplikált sorok kihagyásra kerülnek.
11. A rendszer frissíti az importálási köteg állapotát.

**Alternatív folyamatok:**
- A nem támogatott fájlformátumokat a rendszer elutasítja.
- Hibás CSV-struktúra esetén az importálás sikertelen.
- Az üres fájlokat a rendszer elutasítja.
- Részleges siker engedélyezett, ha egyes sorok hibás adatokat tartalmaznak.

**Utófeltételek:**
- Az importálási köteg és annak feldolgozási eredményei elmentésre kerülnek.

### UC-12 – Importált tranzakciók ellenőrzése

**Szereplő:** Rendszer

A rendszer minden importált sort ellenőriz.

Az ellenőrzés az alábbiakat foglalja magában:
- Kötelező mezők megléte.
- Érvényes dátumformátum.
- Pozitív, numerikus összeg.
- Támogatott pénznem.
- Érvényes tranzakciótípus.
- Érvényes szöveghossz.
- Érvényes kategóriamegfeleltetés, amennyiben meg van adva.

Az érvénytelen sorok nem kerülnek importálásra, és a rendszer megfelelő hibaüzenetet rendel hozzájuk.

### UC-13 – Duplikált tranzakciók felismerése

**Szereplő:** Rendszer

**Alapfolyamat:**
1. A rendszer egységesíti az importált tranzakciók adatait.
2. A releváns tranzakciós mezők alapján determinisztikus ujjlenyomatot számít.
3. Ellenőrzi, hogy ugyanez az ujjlenyomat létezik-e már az adott felhasználónál.
4. Ha talál egyezést, a sort duplikátumként jelöli meg.
5. Ellenkező esetben a tranzakció importálható.

**Üzleti szabályok:**
- A duplikációfelismerést felhasználónként elkülönítve kell végrehajtani.
- Az ujjlenyomatnak olyan releváns mezőket kell tartalmaznia, mint a dátum, az összeg, a pénznem, a tranzakció típusa és az egységesített leírás.
- A duplikációfelismerés gyakorlati heurisztika, nem bizonyíték arra, hogy két tranzakció teljesen azonos.
- A felhasználót tájékoztatni kell a kihagyott duplikátumokról.

### UC-14 – Importálási eredmények megtekintése

**Szereplő:** Felhasználó

A felhasználó az alábbi információkat tekintheti meg:
- Az eredeti fájl neve.
- Az importálás állapota.
- A sorok teljes száma.
- Sikeresen importált sorok száma.
- Sikertelen sorok száma.
- Duplikált sorok száma.
- Az egyes sorokhoz tartozó részletes hibaüzenetek.

### UC-15 – Tranzakció kategorizálása szabályok alapján

**Szereplő:** Rendszer

**Alapfolyamat:**
1. A rendszer megkap egy kategorizálást igénylő tranzakciót.
2. Betölti a felhasználó aktív szabályait.
3. A szabályokat prioritásuk szerint kiértékeli.
4. Kiválasztja az első érvényes egyezést adó szabályt.
5. A tranzakcióhoz rendeli a megfelelő kategóriát.
6. Rögzíti, hogy a kategorizálás szabályalapon történt.

**Üzleti szabályok:**
- A felhasználó által meghatározott szabályok elsőbbséget élveznek a gépi tanulási előrejelzésekkel szemben.
- A szabályoknak illeszkedniük kell a tranzakció típusához és a kategória típusához.
- Az inaktív szabályokat figyelmen kívül kell hagyni.
- A magasabb prioritású szabályokat kell először kiértékelni.

### UC-16 – Kategorizálási szabályok kezelése

**Szereplő:** Felhasználó

A felhasználó az alábbi műveleteket végezheti:
- Új szabály létrehozása.
- Meglévő szabály szerkesztése.
- A szabály prioritásának módosítása.
- A szabály aktiválása vagy deaktiválása.
- A szabály törlése.

Egy szabály az alábbi adatokat tartalmazza:
- Kulcsszó vagy kifejezés.
- Egyezési típus.
- Célkategória.
- Prioritás.
- Aktív állapot.

Kezdetben az alábbi egyezési típusokat kell támogatni:
- Pontos egyezés.
- Tartalmazza.
- Ezzel kezdődik.

### UC-17 – Tranzakció kategóriájának javítása

**Szereplő:** Felhasználó

**Alapfolyamat:**
1. A felhasználó megnyit egy tranzakciót.
2. Kiválaszt egy másik kategóriát.
3. A backend ellenőrzi a kategória érvényességét.
4. A rendszer frissíti a tranzakciót.
5. Létrejön egy kategorizálási visszajelzést rögzítő rekord.
6. A tranzakció manuálisan kategorizáltként lesz megjelölve.

**Üzleti szabályok:**
- A manuális javítás elsőbbséget élvez az automatikus javaslatokkal szemben.
- A visszajelzéseket meg kell őrizni a későbbi modellértékeléshez és -tanításhoz.
- A visszajelzés mentése nem indítja el automatikusan a modell újratanítását.

### UC-18 – Tranzakció kategorizálása ML.NET segítségével

**Szereplő:** Rendszer

**Előfeltételek:**
- Egyetlen alkalmazható, felhasználó által meghatározott szabály sem illeszkedett a tranzakcióra.
- Létezik betanított és elérhető gépi tanulási modell.

**Alapfolyamat:**
1. A rendszer egységesíti a tranzakció leírását.
2. Az ML.NET modell megkapja a tranzakció jellemzőit.
3. A modell előrejelzi a tranzakció kategóriáját.
4. A rendszer kiértékeli az előrejelzés eredményét.
5. Az alkalmazás megkapja az eredményt és a hozzá tartozó megbízhatósági információkat.

**Alternatív folyamatok:**
- Ha nem érhető el modell, a tranzakció kategorizálatlan marad.
- Ha a modell érvénytelen kategóriát ad vissza, az előrejelzést a rendszer elutasítja.
- Ha a tranzakció típusa nem egyeztethető össze az előrejelzett kategóriával, az előrejelzést a rendszer elutasítja.

### UC-19 – Kategorizálási megbízhatóság értékelése

**Szereplő:** Rendszer

A rendszer kiértékeli az előrejelzés megbízhatóságát.

A megbízhatósági küszöbértéknek konfigurálhatónak kell lennie, és kísérleti eredmények alapján kell meghatározni.

**Javasolt kezdeti működés:**
- Magas megbízhatóság esetén a rendszer automatikusan hozzárendelheti az előrejelzett kategóriát.
- Közepes vagy alacsony megbízhatóság esetén a rendszer javaslatot jelenít meg, és manuális megerősítést kér.

A végleges küszöbértékeket validációs eredmények alapján kell kiválasztani, nem önkényes feltételezések szerint.

### UC-20 – Gépi tanulási modell tanítása és értékelése

**Szereplő:** Rendszergazda / Fejlesztő

**Alapfolyamat:**
1. A rendszer előkészíti a tanító adathalmazt a jóváhagyott és javított tranzakciók alapján.
2. Az adathalmaz megtisztításra és egységesítésre kerül.
3. Az adatokat tanító és kiértékelő halmazokra bontja.
4. A rendszer betanítja az ML.NET-folyamatot.
5. A modellt megfelelő osztályozási mérőszámok alapján értékeli.
6. Az eredményeket összehasonlítja a meglévő modellel és a szabályalapú alapmódszerrel.
7. Új modell csak akkor aktiválható, ha megfelel az előre meghatározott értékelési feltételeknek.

**Értékelési mérőszámok:**
- Pontosság (Accuracy).
- Precizitás (Precision).
- Fedés (Recall).
- F1-mérték (F1-score).
- Előrejelzés feldolgozási ideje.

**Üzleti szabályok:**
- A tanító- és tesztadatokat külön kell választani az adatszivárgás elkerülése érdekében.
- A visszajelzések nem aktiválhatnak automatikusan új modellt.
- A modellverziókat és az értékelési eredményeket dokumentálni kell.

### UC-21 – Pénzügyi áttekintés megtekintése

**Szereplő:** Felhasználó

A rendszer a kiválasztott időszakra vonatkozóan megjeleníti:
- Összes bevétel.
- Összes kiadás.
- Nettó pénzforgalom.
- Tranzakciók száma.

A nettó pénzforgalom számítása:

`Összes bevétel - Összes kiadás`

Ez az érték nem azonos a felhasználó tényleges bankszámlaegyenlegével.

### UC-22 – Kiadások elemzése kategóriánként

**Szereplő:** Felhasználó

A rendszer kategóriánként csoportosítja a kiadásokat, és megjeleníti:
- Az összesített összeget.
- A teljes kiadáson belüli százalékos arányt.
- A tranzakciók számát.

Közvetlenül csak az azonos pénznemű tranzakciók összegei összesíthetők.

### UC-23 – Pénzügyi időszakok összehasonlítása

**Szereplő:** Felhasználó

A felhasználó kiválaszt két időszakot.

A rendszer összehasonlítja:
- Az összes bevételt.
- Az összes kiadást.
- A nettó pénzforgalmat.
- A kategóriánkénti kiadásokat.

A rendszer kiszámítja az abszolút és – ahol értelmezhető – a százalékos eltéréseket.

### UC-24 – Ismétlődő kiadások felismerése

**Szereplő:** Rendszer

**Alapfolyamat:**
1. A rendszer elemzi a korábbi kiadási tranzakciókat.
2. A tranzakciókat a valószínű kereskedő vagy egységesített leírás, valamint a kategória alapján csoportosítja.
3. Kiértékeli a hasonló összegeket és az ismétlődő dátumközöket.
4. A rendszer azonosítja a valószínűleg ismétlődő kiadásokat.
5. Megbízhatósági értéket számít.
6. Az eredményeket elmenti és megjeleníti a felhasználónak.

Az ismétlődő tranzakciók felismerése becslésen alapul, ezért az eredményt nem szabad megerősített előfizetésként kezelni.

### UC-25 – Pénzügyi anomáliák felismerése

**Szereplő:** Rendszer

A rendszer elemzi a korábbi tranzakciókat, és azonosítja a szokatlan értékeket.

**Kezdetben alkalmazott módszerek:**
- Az átlagtól való statisztikai eltérés vizsgálata.
- Szórás.
- Z-pontszám (Z-score).
- Kategóriaszintű összehasonlítás.

**Alapfolyamat:**
1. A rendszer lekéri a felhasználó releváns tranzakciós előzményeit.
2. A tranzakciókat kompatibilis kategória és pénznem alapján csoportosítja.
3. Kiszámítja a statisztikai mutatókat.
4. Azonosítja a szokatlan tranzakciókat.
5. Elmenti az anomáliákat tartalmazó rekordokat.
6. A felhasználó áttekintheti az eredményeket.

**Üzleti szabályok:**
- Az anomália nem jelenti automatikusan azt, hogy a tranzakció csalárd vagy hibás.
- A nem elegendő mennyiségű előzményadat kezelését egyértelműen meg kell határozni.
- Az anomáliadetektálás küszöbértékének konfigurálhatónak kell lennie.

### UC-26 – Költségkeretek kezelése

**Szereplő:** Felhasználó

A felhasználó az alábbi műveleteket végezheti:
- Költségkeret létrehozása.
- Kategória kiválasztása.
- Költségkeret összegének beállítása.
- Időszak kiválasztása.
- Meglévő költségkeret módosítása.
- Költségkeret törlése.

A költségkeretek kezdetben kiadási kategóriákhoz kapcsolódnak.

### UC-27 – Költségkeretek felhasználásának nyomon követése

**Szereplő:** Felhasználó

A rendszer kiszámítja:
- A költségkeret összegét.
- Az adott időszak tényleges kiadásait.
- A fennmaradó összeget.
- A felhasználás százalékos arányát.

A rendszer figyelmeztetést jelenít meg, ha a költségkeret felhasználása megközelíti vagy meghaladja a meghatározott limitet.

A költségkeret számításainak a költségkerettel azonos pénznemet kell használniuk.

### UC-28 – Kiadási előrejelzések készítése

**Szereplő:** Rendszer

A rendszer az alábbi módszerekkel készít előrejelzéseket:
- Mozgóátlag.
- Lineáris regresszió.

**Alapfolyamat:**
1. A rendszer lekéri a korábbi havi pénzügyi adatokat.
2. Ellenőrzi, hogy rendelkezésre áll-e elegendő előzményadat.
3. Előkészíti az idősorokat.
4. Az előrejelzési algoritmusok elkészítik a becsléseket.
5. A rendszer elmenti az eredményeket.
6. A frontend megjeleníti a korábbi és az előrejelzett értékeket.

Az előrejelzéseket egyértelműen becslésként kell megjelölni.

### UC-29 – Előrejelzési módszerek összehasonlítása

**Szereplő:** Rendszergazda / Fejlesztő

A rendszer időrendi sorrendbe rendezett múltbeli adatok segítségével értékeli az előrejelzési módszereket.

**Mérőszámok:**
- Átlagos abszolút hiba (Mean Absolute Error – MAE).
- Gyökös átlagos négyzetes hiba (Root Mean Squared Error – RMSE).

Az értékelés során el kell kerülni, hogy a modell tanítása során jövőbeli megfigyeléseket használjon fel.

### UC-30 – Pénzügyi előrejelzések megtekintése

**Szereplő:** Felhasználó

A felhasználó az alábbi adatokat tekintheti meg:
- Előrejelzett havi kiadások.
- Előrejelzett havi nettó pénzforgalom.
- Az előrejelzéshez felhasznált korábbi adatok.
- Az alkalmazott előrejelzési módszer.
- Az előrejelzés időszaka.

Ha nem áll rendelkezésre elegendő adat, a rendszer tájékoztatja a felhasználót arról, hogy miért nem készíthető megbízható előrejelzés.

### UC-31 – Vezérlőpult megtekintése

**Szereplő:** Felhasználó

A vezérlőpult összefoglaló képet ad a felhasználó pénzügyi helyzetéről.

Az alábbi információkat tartalmazhatja:
- Bevételi és kiadási összesítés.
- Nettó pénzforgalom.
- Kiadások kategóriánkénti megoszlása.
- Havi pénzügyi trendek.
- Költségkeretek felhasználása.
- Legutóbbi tranzakciók.
- Felismert anomáliák.
- Ismétlődő kiadások.
- Pénzügyi előrejelzések.

A vezérlőpult kizárólag a hitelesített felhasználó adatait használhatja.

## 5. Általános üzleti szabályok

Az alábbi szabályok az alkalmazás egészére érvényesek:

1. Minden pénzügyi rekord pontosan egy felhasználóhoz tartozik.
2. A felhasználók kizárólag a saját pénzügyi adataikhoz férhetnek hozzá.
3. A rendszerszintű kategóriák több felhasználó között is megoszthatók.
4. A személyes kategóriák kizárólag a tulajdonosaik számára érhetők el.
5. A tranzakciók összegeinek pozitívnak kell lenniük.
6. A tranzakció típusa határozza meg, hogy az összeg bevételt vagy kiadást jelent.
7. A bevételi és kiadási kategóriák nem keverhetők.
8. Különböző pénznemű összegeket nem szabad összeadni átváltási mechanizmus nélkül.
9. A felhasználó által meghatározott kategorizálási szabályok elsőbbséget élveznek a gépi tanulási előrejelzésekkel szemben.
10. A manuális kategorizálás elsőbbséget élvez az automatikus kategorizálással szemben.
11. A gépi tanulási előrejelzések javaslatnak minősülnek, kivéve, ha a beállított megbízhatósági szabályzat engedélyezi az automatikus kategória-hozzárendelést.
12. A visszajelzések nem indítják el automatikusan a modell újratanítását.
13. Az anomáliadetektálás eredményei tájékoztató jellegűek.
14. Az előrejelzéseknél egyértelműen jelezni kell a bizonytalanságot és a korlátokat.
15. Nem elegendő mennyiségű előzményadat esetén nem szabad félrevezető előrejelzéseket készíteni.

## 6. Nem funkcionális elvárások

- A hitelesítést és a jogosultságkezelést a backendnek kell kikényszerítenie.
- Az API-végpontoknak ellenőrizniük kell a kért erőforrások tulajdonjogát.
- A nagyobb importálási és elemzési feladatok nem akadályozhatják szükségtelenül a HTTP-kérések kiszolgálását.
- A listáknak támogatniuk kell a lapozást.
- A felhasználói bemeneteket mind a frontend, mind a backend oldalán ellenőrizni kell.
- A fontos műveleteket automatizált tesztekkel kell lefedni.
- A hibákat naplózni kell anélkül, hogy bizalmas pénzügyi információk kerülnének a naplókba.
- Az alkalmazás felépítésének lehetővé kell tennie további kategorizálási és előrejelzési módszerek későbbi bevezetését.
