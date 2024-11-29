Published: 27.1.2021
Updated: 4.7.2024
Title: Experiment s In-Memory OLTP
OverrideTitle: Experiment s In-Memory OLTP pre high performance registráciu
Menu: Experiment s In-Memory OLTP
Cathegory: Dev
Description: Myšlienkový a praktický experiment, ako riešiť systém na registráciu termínov, ktorá odolá veľkej záťaži.
OgImage: images/InMemoryOltp/InMemorytable.png
---
Po preťažení niektorých informačných systémov v súvislosti s Covid testami a očkovaním som si skúsil najskôr myšlienkový a neskôr aj praktický experiment.
Rozmýšľal som, ako navrhnúť systém na registráciu termínov, ktorý odolá veľkej (nárazovej) záťaži.
Ďalšími podmienkami bolo aby bol systém spoľahlivý (transakčnosť registrácie) a citlivé dáta neukladal niekde, kde by nemali byť.
Po preskúmaní rôznych možností a databáz som sa to pokúsil vyriešiť pomocou technológie _In-Memory OLTP_ v _MS SQL_.

## Inšpirácia
17.1.2021 bol na Slovensku preťažený systém na objednávanie sa na antigénové testy.
Preťaženie vyvolala okamžitá reakcia ľudí na tlačovú konferenciu o celoplošnom skríningu (viac [tu](https://zive.aktuality.sk/clanok/150616/skrining-objednanie-antigenove-testy-testovanie-termin/)).
V Česku mali podobné problémy zo systémom na registráciu očkovania ([problémy s Chytrou karanténou](https://twitter.com/ChytraKarantena/status/1349983604571582466)).

Nechcem tu rozoberať problémy štátneho IT (zhrnutie je možné pozrieť si [tu](https://tech.ihned.cz/c7-66870010-psms7-e882c2115d7c68a)).
No je jasné, že ak službu dizajnujete pre stovky až tisíce aktívnych používateľov, a zrazu sa ich tam nahrnie 150-ktát viac,
tak to proste padne. Jednou z chýb bolo to, že NCZI na danú situáciu nebolo ani len upozornené, a nemalo čas zareagovať.
Toto sa nestáva len v štátnom IT.
Niečo podobné sa stalo len pár dní predtým _Signalu_. Aplikácia _WatsApp_ zmenila podmienky používania a ochrany súkromia,
čo spôsobilo odliv používateľov k _Signalu_ a spôsobili mu [výpadky](https://zive.aktuality.sk/clanok/150597/signal-mal-v-noci-velky-vypadok-problemy-mozu-pretrvavat/).

Následne som v diskusiách a článkoch na internete hľadal, ako tento problém vyriešiť – **ako nadizajnosvať systém, aby vydržal aj veľkú nárazovú záťaž, pričom používatelia systému sa bijú o obmedzené zdieľané prostriedky** (termíny na očkovanie alebo Ag. testy).

Podstatnú časť odpovedí tvorilo _„dať to do cloudu“_.
Áno sila desiatok serverov znesie veľa, no jestvujú dosť pádne dôvody,
prečo štát nedáva citlivé zdravotnícke dáta na počítače niekoho iného
a k tomu v jurisdikcii iného štátu. Plno ďalších odpovedí bolo dokúpiť silné železo, no to zas treba vysúťažiť...

Tak som sa začal zamýšľať, ako navrhnúť architektúru niečoho, ako systém na objednávanie na testy alebo očkovanie tak,
aby som neodovzdával citlivé dáta tretej strane, a vystačil si s klasickými servermi.

## Návrh
Prvé čo treba povedať je, že samotný proces registrácie treba navrhnúť tak,
aby sa procesy, ktoré sú úzke hrdlo vyčlenili na asynchrónne spracovanie (tu to bolo napríklad odosielanie SMS).

Ďalšia kritická oblasť je rezervácia termínu.
Pri veľkej záťaži a použití klasickej relačnej databázy by dochádzalo k degradácii výkonu kvôli zámkom nad riadkami tabuľky.
No tu klasická cache nepomôže. Ako riešenie ma napadlo použiť [In-Memory OLTP v MS SQL](https://docs.microsoft.com/en-us/sql/relational-databases/in-memory-oltp/in-memory-oltp-in-memory-optimization?view=sql-server-ver15).

V niektorých prípadoch je vhodné si napísať vlastnú in-memory databázu
(tak svoje problémy vyriešilo napríklad _Kiwi.com_ - <https://www.youtube.com/watch?v=lbDp8rd9gzU>).
Pri hľadaní riešenia som narazil na niekoľko databázových enginov napísaných v jazyku _Rust_.
Proste sa zobral protokol pre Redis a k nemu sa doplnila databázová časť.
Na niečo takúto sa Rust hodí, lebo už kompilátor jazyk a zabezpečuje pamäťovú bezpečnosť a thread-safe (nemá garbage collector). Odporúčam sa na tento jazyk pozrieť.
(Napríklad [escanor](https://github.com/mambisi/escanor), [MeiliES](https://github.com/meilisearch/MeiliES),...).

Na nasledujúcom diagrame je znázornená architektúra navrhovaného riešenia.

![Architektúra riešenia.](images/InMemoryOltp/Architecture.png){.img-center}

Registračná aplikácia bude _SPA_ aplikácia (alebo _JAM stack_), tak aby bolo možné hostovať frondend ako statické súbory na CDN-ku alebo nezávislom serveri.
Pričom je dôležité, aby klientska aplikácia korektne spracovávala http status _429 Too Many Requests_ retry logikou.

Nasleduje load balancer (napríklad [HAProxy](https://en.wikipedia.org/wiki/HAProxy)),
ktorý rozdeľuje requesty na jednotlivé aplikačné inštancie Web API aplikácií, takisto stráži rate limit aby nedošlo k preťaženiu Web API inštancií a korektne vracať http status _429_. 
(Poprípade je možné _HAProxy_ doplniť o [Consul](https://learn.hashicorp.com/tutorials/consul/load-balancing-haproxy).)

Inštancie Web API slúžia na spracovanie registrácií a obsahujú aplikačnú logiku (v mojom prípade ASP.NET Core 5.0).

Databáza (MS SQL 2019) do ktorej sa ukladajú registrácie.
Pričom tabuľka pre uloženie termínu je duplikovaná na dve – klasická tabuľka a in-memory tabuľka
(netrpí problémami so zamykaním riadkov v tabuľke), kde sú údaje o registrácii pre najvyťaženejšie obdobie (povedzme registrácie na najbližších 30 dni).
Tieto dáta sa synchronizujú v pravidelných intervaloch pomocou stored procedúr,
ktoré volá aplikácia _Sheduler App_.
In-memory tabuľka v režime _SCHEMA_AND_DATA_ ukladá zmeny aj na disk, takže v prípade výpadku serveru nedôjde k ich strate.

A samozrejme nejaké fronta úloh, napríklad pre odosielanie SMS, ktorá môže byť realizovaná rôznymi spôsobmi
(od ukladania v inej databáze, cez RabbitMQ až po exotickejšie riešenia – [FASTER](https://github.com/microsoft/FASTER)).

Čo sa týka samotnej rezervácie termínu, tak by šla vyriešiť tak, že sa naraz odošlú všetky údaje potrebné údaje,
v in-memory tabuľke sa zaregistruje termín, ostatné dáta sa odložia do fronty a spracujú sa neskôr.

## Realizácia
Rozhodol som sa, že si skúsim implementovať časť systému, ktorá sa stará o rezerváciu termínu na test,
aby som porovnal priamočiaru implementáciu, ktorú si človek zvolí, keď rieši bežný systém,
voči implementácii s In-Memory OLTP tabuľkou.
Zameral som sa hlavne na databázovú časť, preto aplikačná logika zostala bez optimalizácií.

Aplikačný server je Web API realizované pomocou _ASP.NET Core 5.0_.
Aplikačná logika je implementovaná pomocou _Entity Framewrk Core 5.0_ a v prípade In-Memory OLTP tabuľky je volaná jednoduchá stored procedúra cez SQL klienta,
implementácia je priamo v kontroleroch.
Inak je použitá defaultná šablóna pre vytvorenie projektu, takže neboli robené žiadne iné optimalizácie na strane aplikácie. A na Web API boli vyvedené ešte metódy na generovanie testovacích dát (knižnicou [Bogus](https://github.com/bchavez/Bogus)).

Databáza je MS SQL 2019.

Systém si do databázy predgeneruje termíny na registráciu (karteziánsky súčin medzi odberovými/očkovacími miestami, dátumom a časom v rámci dňa).

DDL pre registračnú in-memory tabuľku vyzerá takto:

```sql
CREATE TABLE [dbo].[RegistrationInMemory]
(
	[Day] [DATETIME2](7) NOT NULL,
	[TimeSlotId] [INT] NOT NULL,
	[PlaceId] [INT] NOT NULL,
	[CovidClientId] [INT] NULL,
	[Register] [DATETIME2](7) NULL,

    CONSTRAINT [PK_RegistrationInMemory] PRIMARY KEY NONCLUSTERED 
    (
    	[Day] ASC,
    	[TimeSlotId] ASC,
    	[PlaceId] ASC
    ),
    INDEX [RegistrationInMemory_Sorting_IX] NONCLUSTERED 
    (
    	[Day] ASC,
    	[TimeSlotId] ASC,
    	[PlaceId] ASC
    )
) WITH (MEMORY_OPTIMIZED = ON, DURABILITY = SCHEMA_AND_DATA);
```

Primárny kľúč tvorí kombinácia dátumu, času v rámci dňa a odberového/očkovacieho miesta, pre rýchle vyhľadanie pri registrácii.

Ďalší index slúži pri vyhľadávaní voľných termínov a ich zobrazení v rámci dňa alebo týždňa.

Natívne kompilovaná stored procedúra pre registráciu termínu vyzerá nasledovne:

```sql
CREATE PROCEDURE [dbo].[RegistrationInMemory_TryRegister]
	@date DATETIME2,
	@slotId INT,
	@placeId INT,
	@covidClientId INT
WITH NATIVE_COMPILATION, SCHEMABINDING
AS BEGIN ATOMIC WITH
(
 TRANSACTION ISOLATION LEVEL = SNAPSHOT, LANGUAGE = N'us_english'
)
  
  DECLARE @isRegistered BIT = 0;

  UPDATE [dbo].[RegistrationInMemory]
      SET @isRegistered = 1,
          [CovidClientId] = @covidClientId,
          [Register] = GETDATE()
      WHERE [Day] = @date AND [TimeSlotId] = @slotId AND [PlaceId] = @placeId AND [CovidClientId] IS NULL;

  SELECT @isRegistered AS [RegistrationSuccessfull];
END
```
Vo výsledku vráti, či sa podarilo používateľa registrovať na zvolený termín.

Výhody použitia In-Memory OLTP voči napríklad Redisu sú hlavne v tranzakčnosti presúvania dát do klasických tabuliek,
možnosť písať natívne kompilované procedúry a dodatočné indexy (v Redise by ich bolo nutné implementovať aplikačne).

## Výsledky
Záťažové testovanie som robil na notebooku s procesorom _Inter Core i7-8550U 1.80GHz_, _8GB_ RAM a SSD diskom na operačnom systéme _Windows 10 Pro verzia 2004_.

Použil som naň nástroj [Netling](https://github.com/hallatore/Netling) a testovacie GET metódy kontroleru vyberali náhodné termíny na náhodných miestach pre registráciu.

Zaujímavé zistenie bolo, že v in-memory tabuľke dáta aj s indexami pre 5&nbsp;000&nbsp;000 termínov na registráciu zaberali približne 1GB z RAM.

Pri klasickom použití tabuľky a Entity Frameworku (načítanie termínu a jeho modifikácia) som sa dostal na priepustnosť 1&nbsp;386 requestov za sekundu. (Snímka okna nižšie.)

![Výsledky použitia klasickej tabuľky.](images/InMemoryOltp/SqlTable.png){.img-center}

Pri použití In-Memory tabuľky volanej cez SQL klienta som sa dostal na priepustnosť 10&nbsp;069 requestov za sekundu. Čo už je slušné číslo na aplikáciu bez akýchkoľvek optimalizácií. Pri in-memory tabuľke bolo úzke hrdlo procesor. (Snímka okna nižšie.)

![Výsledky použitia klasickej tabuľky.](images/InMemoryOltp/InMemorytable.png){.img-center}

**Poznámky**:
Rozdiel v rýchlosti medzi _Entity Frameowrkom Core 5_ a priamom prístupom (SQL klient) je zanedbateľný.
Vysoké čísla a nízku latenciu považujem za výsledok toho, že som testoval na rovnakom notebooku ako bežal server a teda odpadáva sieťová latencia.
Relatívne malý bandwidth je spôsobený testovaním pomocou NETLING-u, využíva len GET požiadavky.

## Záver
Dlho som hľadal nejaký reálny use-case pre technológiu In-Memory OLTP, konečne sa mi ju podarilo vyskúšať.
Dosť príjemne ma prekvapili čísla performace aj množstvo zabranej pamäte.
**Desaťtisíc spracovaných requestov za sekundu som nečakal.**

No musím podotknúť, že som prakticky riešil len malú časť systému a taktiež môj experiment ignoruje sieťovú latenciu.
Takže vo výsledku by na strane aplikácie tých requetsov bolo spracovaných menej.

Do pozornosti ešte dávam prednášku o tom, ako spracovávať Big Data v MS SQL - <https://wug.cz/zaznamy/503-SQL-Server-Bootcamp-2018-Zpracovavejte-velka-data-v-SQL-Serveru-rychlosti-blesku>
a architektúru [CQRS](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs).

### Neskoršia optimalizácia
Po napísaní tohto blogu (január 2021) som sa k problematike o mesiac vrátil. Skúšal som iné optimalizácie, niektoré úspešne
a iné menej úspešne. Uvádzam ich takto mimo hlavného obsahu,
lebo nespĺňajú požiadavky zadefinované v úvode.

Menej úspešne dopadol pokus o použitie prístupu _Event store_, hlavne pre to,
že kvôli požiadavkám som musel obchádzať niektoré jeho princípy a taktiež som naň nevedel rýchlo napísať vhodné transakčné úložisko a notifikáciu používateľa.  
(Mal som obmedzený čas a databázu [EventStore](https://www.eventstore.com/) som neskúšal.)

Priamy zápis údajov do Redis-u cez ASP.NET Core kontroller mi dal približne 14&nbsp;500 requetsov za sekundu.

Priamy zápis do Redis-u cez vlastný ASP.NET Core middleware, ktorý zastupoval REST endpoint, dokázal spracovať približne 36&nbsp;000 requestov za sekundu.

Priamy zápis do Redisu pomocou natívnej aplikácii napísanej v Rust-e pomocou frameworku 
[Actix](https://actix.rs/)
a knižnici [actix-storage-redis](https://github.com/pooyamb/actix-storage/) mi dokázalo spracovať tiež okolo 36&nbsp;000 requestov za sekundu.
Rust bol rýchlejší o niekoľko málo stoviek requestov.

Priamy zápis do MS SQL In-Memory OLTP tabuľky cez vlastný ASP.NET Core middleware, ktorý zastupoval REST endpoint, dokázal spracovať 15&nbsp;000 requestov za sekundu.
To je v tomto prípade o _50%_ viac ako cez kontroller. A ako jediné riešenie z tejto kapitoly spĺňa požiadavky zadefinované v úvode,
no nevýhoda je, že sa takýto endpoint neobjaví v OpenAPI špecifikácii.
