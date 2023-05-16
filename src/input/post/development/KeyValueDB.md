Published: 16.5.2023
Title: Ako na Key-Value databázu
Menu: Ako na Key-Value databázu
Cathegory: Dev
Description: Ukážka modelovania v key-value databázach.
---
Už nejakú dobu sa zaoberám databázami a nejaký čas som riešil aj vlastné nerelačné úložisko. V tomto blogu chcem zosumarizovať postupy,
ako vyriešiť niektoré úlohy pomocou generickej key-value databázy,
aj keď mnohé z nich pôsobia dojmom „samozrejmosti“.

Pre implementáciu key-value databáz sa najčastejšie používajú _B+ stromy_, _sorted string tabulky_ (s _LSM tree_), prípadne _skip-list_. 
Všetky tieto implementácie umožňujú nájsť veľmi rýchlo nasledujúci kľúč (v istom zoradení) alebo hľadať podľa prefixu či rozsahu
a to je v mnohých nasledujúcich riešeniach potrebné. 
Databázy založené na _hash mapách_ túto možnosť zvyčajne nemajú.

_V nasledujúcom texte používam syntax, kde `key => value` znamená kľúčový pár, zátvorky znamenajú N-ticu
(binárne serilzovanú N-ticu hodnôt, kde `(a, b, c)` má prefix `(a, b)`) a v jednoduchých úvodzovkách sú konštanty._

## Použitie prefixov
Použitie prefixov kľúčov je základný koncept na oddeľovanie logických celkov v key-value databázach napríklad na určenie typu záznamu,
prípade k príslušnosti k inej entite.

```handlebars
('data', 'user', user_Id, 'Address') => user address data
```

Takéto usporiadane kľúča hovorí, že sa jedná o dáta (nie index, ani iné pomocné štruktúry) pre používateľa s `user_id` a položku `Address`.

Následne je možné vymazať všetky údaje k danému používateľovi podľa prefixu _id_ používateľa.

Samozrejme veľkosť kľúča je vhodné udržiavať v rozumnej miere.

## Modelovanie tabuliek 
Tabuľky (alebo aj _column family_ či _wide-column_ prístup) ide riešiť dvoma spôsobmi. Prvý je taký, že sa použije primárny kľúč ako kľúč, a riadok tabuľky sa serializuje a uloží,
ako binárna hodnota.

```handlebars
('table', table_name, id) => (id, given name, surname, date of brith)
```

No v tomto prípade nie je možné pracovať so samostatnými hodnotami.

Druhý spôsob je ukladania ukladá hodnoty samostatne a je možné s nimi samostatne pracovať bez deserilizácie a serializácie
(napríklad ich atomicky inkrementovať alebo vyťiahnuť).

```handlebars
('table', table_name, id, 'given name') => given name
('table', table_name, id, 'surname') => surname
('table', table_name, id, 'date of brith') => date of brith
```

## Modelovanie vektorov
S modelovaním vektorov je to podobné ako s modelovaním tabuliek.
Buď ho uložíme serializovaný alebo po jednotlivých hodnotách, aby s nimi bolo možné pracovať samostatne, či vytiahnuť si len časť vektoru.

```handlebars
('vector', vector_name, id) => (value_1, value_2, value_3,...)
```

A druhá varianta.

```handlebars
('vector', vector_name, idm index_i) => value_i
```

## Modelovanie grafu
Graf sa skladá z vrcholov a hrán medzi vrcholmi. V key-value databáze sa modelujú tak, že vrcholy sú klasické hodnoty zo svojim _id_.

```handlebars
('graph', graph_id, 'edge', edge_id) => edge data

```

Orientované hrany majú v kľúči _id_ vrcholu s ktorého vychádzajú do, ktorého vchádzajú a váhy majú v hodnote.

```handlebars
('graph', graph_id, 'vertice', edge_from_id, edge_to_id) => vertice data

```

Ak má graf podporovať viacnásobné hrany je ešte pridané _id_ pre samotnú hranu.

Pri traverzovaní grafom sa ku aktuálnemu uzlu získajú hrany tak, že sa vyhľadajú podľa prefixu z _id_ aktuálneho uzlu.

## Indexovanie
Indexovanie je spôsob ako uložiť dodatočné informácie tak, aby urýchlili vyhľadávanie a nebolo nutné aby databáza prešla cez všetky dáta.
Keďže key-value databázy zvyčajne nemajú sekundárne indexy je ich potrebné modelovať inak.

Princíp modelovania indexov je jednoduchý, tiež využíva to, že je možné hľadať podľa prefixu.

Ak hodnotu, ktorú indexujeme `val1`, ktorá sa nachádza v hodnote s _id_,  vytvoríme takýto záznam:

```handlebars
('ix', index_name, val1) => id

```

Tak získame unikátny index, no ak chceme indexovať opakujúce sa hodnoty, tak musím do kľúča pridať niečo unikátne 
a tu sa rovno ponúka použiť _id_ a hodnotu necháme prázdnu.

```handlebars
('ix', index_name, val1, id) => Ø

```

A tento koncept môžeme ďalej rozširovať. Napríklad do hodnoty vložiť vybrané dáta s indexovaného záznamu (v SQL svete sú to _included columns_).

```handlebars
('ix', index_name, val1, id) => (included columns, ...)

```

Poprípade vytvoriť zložený index, kde sa indexujú viaceré hodnoty.

```handlebars
('ix', index_name, val1, val2, id) => Ø

```

Takýto zložený index má rovnaké obmedzenie ako v relačných databázach a to, že samostatne ide vyhľadávať podľa prvej hodnoty, ale podľa druhej hodnoty už nie.

## Modelovanie časových radov

Časové rady (_time series_) tvoria hodnoty, ktoré sú usporiadané v čase, ich hodnota je tvorená zvyčajne premennými/vlastnosťami.
V key-value databázach idú modelovať rôznymi spôsobmi podľa toho ako ich chceme použiť.

Asi najpriamočiarejšie je ako časový údaj  použiť rovno timestamp (v milisekndách), vtedy ide vyhľadávať podľa konkrétnych časových údajov.
Prípade do kľúča zahrnúť _id_ zdaroja, podľa ktorého chceme filtrovať.

```handlebars
('timeserie', timeserie_name,  timestamp) => (values,...)
```

```handlebars
('timeserie', timeserie_name,  timestamp, source_id) => (values,...)
```

Ak ale chceme vyťahovať hodnoty podľa určitých časových období,
tak si timestamp rozdelíme na „ľudské“ údaje ako rok, kvartál, mesiac, deň,... tak ide vytiahnuť údaje napríklad pre konkrétny mesiac.

```handlebars
('timeserie', timeserie_name,  year, month, day, hout, minute, sec, milisec) => (values,...)
```

Tento model počíta s tým, že v rovnakom čase bude zaznamenaný len jedna hodnota. Ak ich chceme viac musíme kľúč nejakým spôsobom zunikátniť,

## Ukladanie BLOB-ov
_BLOB_ (_Binary large object data_) je proste hocaký blok binárnych dát alebo súbor.
Pri key-value databázach ho teoreticky môžeme uložiť ako jednu hodnotu, no pri takom prístupe narážame na maximálnu veľkosť akú je možné uložiť do jednej hodnoty.
Taktiež je takto problém tieto dáta vytiahnuť. Pre to je lepšie ich uložiť po malých kúskoch, ktoré sa očíslujú.

```handlebars
('blob', blob_name, 'content', 0) => chunk
('blob', blob_name, 'content', 1) => chunk
('blob', blob_name, 'content', 2) => chunk
('blob', blob_name, 'content', 3) => chunk
('blob', blob_name, 'content', 4) => chunk
...
('blob', blob_name, 'content', N-1) => chunk
```

A nakoniec ako poslednú hodnotu pridáme informácie o _BLOB-e_.
```handlebars
('blob', blob_name, 'info') => (chunkCount: N, size: 1024, chunkSize: 512, name: foobar.pdf)`
```

To umožní hľadať len kompletne nahrané _BLOB-y_. Je možné celý zmazať podľa prefixu.
A aj vytiahnuť konkrétnu časť, lebo kúsky (chunk-y) majú rovnakú veľkosť (až na posledný).

## Spatial index
Spatial index, alebo aj priestorový index je index,
pomocou ktorého ide efektívne vyhľadávať v priestorových dátach (body, geometrické a geografické útvary,...).
Typickou úlohou pre tento index je nájsť body do určitej vzdialenosti od iného bodu. 

V _MS SQL_ je spatial index implementovaný pomocou toho, že priestor sa rozdelí na hierarchickú mriežku,
ktorú ide prehľadať podobným princípom ako strom. No v key-value databáze sa používa iný postup –
dvojrozmerné body v priestore sa prevedú na jednorozmernú veličinu,
ktorá má tú vlastnosť, že blízke hodnoty tejto veličiny implikujú blízkosť pôvodných bodov v dvojrozmernom priestore.
Používa sa na to [Mortonov rozklad](https://cs.wikipedia.org/wiki/Morton%C5%AFv_rozklad) (_Morthon Z-curve_, _Z-curve order_).

Pri priestorových útvaroch je treba každý bod tohto útvaru indexovať samostatne.

V key-value databázy budeme tento index modelovať nasledovne (pre objekt s _id_):

```handlebars
('ixs', index_name, z_order_value, id) => (x,y)
```

Hodnotu Mortnovho rozkladu získame použitím vhodného zaokrúhlenia a levelu súradníc a pomocou knižnice vypočítame jeho hodnotu 
(pre .Net to môže byť napríklad [NetTopologySiute](https://nettopologysuite.github.io/NetTopologySuite/api/NetTopologySuite.Shape.Fractal.MortonCode.html)).
V hodnote indexu sú uvedené pôvodné súradnice aby bolo možné jednoducho zistiť, či vyhovujú zadanej podmienke v dopyte.
Pomocou Mortnovho rozkladu sa zistí aký rozsah hodnôt sa bude hľadať zo zdaného bodu a vyhľadávanej oblasti.
Ten sa v key-value databázy vyhľadá a spracujú sa ich pôvodné hodnoty.

## Záver
Pomocou key-value databázy ide modelovať mnoho spomenutých vecí. Ktoré sa môžu hodiť, pretože tieto databázy často umožňujú jednoduchú replikáciu a horizontálne škálovanie.

Pomocou konkrétnych databáz ide implementovať aj iné dátové modely, ale je pre ne potrebná dodatočná funkcionalita, napríklad pre frontu správ je potrebná atomická funkcia na zmenu hodnoty pre kľúč (napríklad implementovaná pomocou _change-vektora_ alebo kontroly verzie hodnoty).

Pri písaní tohto blogu som čerpal hlavne z tutoriálov ku [FoundationDb](https://www.foundationdb.org/),
ktorá sa používa ako "podvozok" pre distribuované databázové systémy.
