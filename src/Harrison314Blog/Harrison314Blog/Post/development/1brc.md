Published: 1.12.2024
Title: Riešenie The One Billion Rows Challenge
Menu: One Billion Rows Challenge
Cathegory: Dev
OgImage: images/1brc/media.png
Description: Riešenie The One Billion Rows Challenge v .NET-e, predstavenie nástrojov na perfomace tuning a rôzne kroky pri riešení tejto úlohy.
---
Nedávno som objavil [The One Billion Rows Challenge](https://1brc.dev/).
Tento rok som ju uprednostnil pred [Advent of code](https://adventofcode.com/), priniesla mi veľa zábavy.

Tento článok je o zaujímavých nástrojoch na perofmace tuning, na ktoré som narazil a niektorých riešeniach, ktoré som použil a prišli mi zaujímavé.

Síce som videl nejaké existujúce riešenia, ale šiel som vlastnou cestou, aby to bolo dobrodružstvo. A sám som si dal podmienku, že nechcem používať unsafe kód.

Je to doslova primitívna úloha, máme súbor v UTF-8, v ktorom je názov miesta a nameraná teplota (ako desatinné číslo).
Úlohou je spraviť program, ktorý tento súbor prejde a na konci, pre každé miesto vypísať minimálnu, priemernú a maximálnu teplotu.

Po internete kolujú rôzne verzie zadaní (odlišný počet desatinných miest, výstupný formát), ale principiálne sú si podobné. 

Príklad formátu vstupného súboru:
```
Hamburg;12.0
Bulawayo;8.9
Palembang;38.8
Hamburg;34.2
Tønsberg;-3.4
St. John's;15.2
Cracow;12.6
... etc. ...
```

To nie je nič ťažké, hádam v každom vyššom jazyku ide táto úloha vyriešiť za 5 minút a ešte si dať kávu.
Tak v čom je tá výzva? V počte riadkov, je ich 10^9^ a testovací súbor v mojom prípade mal cez _13GB_.

Pôvodne táto výzva vznikla pre Javu, keďže sa pri nej naplno prejaví _garbage collector_, ale postupne sa stala obľúbenou aj pre iné jazyky (C#, Rust, Zig, SQL, awk, PHP,...).

Samozrejme na každom stroji budú tie časy iné, dokonca aj niektoré optimalizácie kvôli iným diskom, zberniciam, cache a procesorom. Mo myslím, že pri tejto výzve je dôležitejšia cesta, ako cieľ.

## Nastavenie prostredia a nástroje
Začal som tým, že som si vytvoril implementáciu pomocou asynchrónneho čítania súboru, proste žiadna optimalizácia.
Ku tomu projekt, ktorý generoval testovacie súbory ([napríklad takýto](https://github.com/Vake93/1brc/blob/master/Generator/Program.cs)), unit test projekt a benchmark projekt.

Uvedené programy mi pomáhali skôr orientačne, pretože _The One Billion Rows Challenge_ je úloha, ktorá robí len jedinú vec, navyše alokácie som mal v .NET-e dosť pod kontrolou (kvôli determinizmu do IL a  JIT-u).

### BenchmarkDotNet
<https://benchmarkdotnet.org/index.html>

Na performace testy som použil _BenchmarkDotNet_ a súbor s _10&nbsp;000_ riadkami. Na základnú orientáciu stačil, no neskôr sa ukázal, ako málo presný pri komplexných programoch.
Nie je to chyba _BenchmarkDotNet_, ale toho, ako som túto knižnicu použil, je vhodná na mikro-benchmarking, v čom je aj presná,
no ja som ju použil na beh tela celého programu.

Ku koncu sa mi stalo, že zmena, ktorá vo výsledku na testovacom súbore s 10^9^ riadkami ušetrila 6 sekúnd bola pri použití _BenchmarkDotNet_ pod štatistickou chybou.
No neskôr som ho používal na mikro-bechmarking a na kontrolu alokácii v kóde.

### Hyperfine
<https://github.com/sharkdp/hyperfine>

_Hyperfine_ je malý program, ktorý opakovane spúšťa zadaný program a vyhodnocuje čas jeho behu.

K nemu som si spravil powershell skript, ktorý skompiluje meraný program klasicky a potom pomocou AOT a spustí meranie.

### dotTrace
<https://www.jetbrains.com/profiler/>

_dotTrace_ je asi jeden za najpoužívanejších nástrojov na perfomace tuning a hľadaní úzkeho hrdla pre dotnet aplikácie. Pre jednotlivcov je zadarmo, pre firmy platený (údaj z decembra 2024).

Nakoniec som ho nepoužil.

### Visual Studio Profiler
<https://learn.microsoft.com/en-us/visualstudio/profiling/profiling-feature-tour?view=vs-2022>

Málo ľudí vie, že priamo vo Visual Studiu (v každej verzii) je profiler nabitý funkcionalitou. 

Napríklad pomocou snapshotov pamäte a ich perovaniu ide zistiť, ktoré objekty/triedy spôsobujú pamäťovú náročnosť, alebo memory leaky.

Pri profilovaní CPU zas dokáže ukázať hot paths, teda call stacky na ktorých procesor spálil najviac času a sú vhodné na optimalizáciu.
Podobne je to s alokáciami, async kódom,...

### dotnet-trace
<https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-trace>

_dotnet-trace_ je multiplatformový nástroj pre zbieranie tracov z dotnetpvých aplikácií.

V mojom prípade som používal:
```
dotnet-trace collect --duration 00:00:20 --format Chromium -- dotnet OneBilionChallenge.dll Data\bilion.txt
```

Príkaz zozbiera trace záznamy a následne ich ide otvoriť v Chromiu/Chrome pomocou <chrome:://tracing> alebo v MS&nbsp;Edge pomocou <edge://tracing/>.

Takto ide získať napríklad flame graph, pocty volaní metód a ďalšie udalosti na základe ktorých ide získať podrobnosti o bežaní procesu.

### CodeTrack
<https://www.getcodetrack.com/>

_CodeTrack_ je bezplatná alternatíva pre dotTrace, vizualizuje flame graph, zobrazuje počty volaní a má jednotlivé volania prepojené s dekompilovaným kódom.
No napríklad neobsahuje analýzu alokácií a GC.

Aplikácia zvláda aj .NET Framework, ale je určená len pre Windows.

### ultra
<https://github.com/xoofx/ultra>

_ultra_ je cmd profiler pre Windows dostupný vo forme .NET toolu. Rieši aj JIT, alokácie a GC. Má bohatší výstup ako _dotnet-trace_,
a svoju vizualizáciu rieši cez <https://profiler.firefox.com/>.


## Zaujímavé optimalizácie
Nechcem preberať optimalizácie krok za krokom, na to sú iné blogy uvewdené v zdrojoch, len uvediem zaujímavosti.

Začal som s asychrónnou implementáciou v _.NET 9_, ale tá bola výrazne pomalšia ako synchrónna, čo dáva zmysel pri jednovláknovom programe, takže za naivnú implementáciu považujem synchrónnu, ktorá používa `StreamReader.ReadLine()`.

Na mojom notebooku z _i7-13700H 2.40 GHz, 40GB RAM a SSD disku_ spracovanie súboru z 10^9^ riadkami (13,8GB) trvalo _3 minúty a 43 sekúnd_.

V ďalších optimalizáciách som sa pokúšal znížiť zbytočné alokácie pri čítaní súboru po riadkoch, ale stále som názov miesta (kľúč v dictionary) prevádzal na string.
Pre prístup do dictionary (`Dictionary<string, Data>`) som použil `CollectionsMarshal.GetValueRefOrAddDefault()`. 

Pri čítaní súboru som zo `System.IO.FileReader` prešiel na `System.IO.RandomAccess`.

Následne som celé riešenie prerobil na použitie `Memory<byte>` s tým,
že som si ku dictionary spravil wrapper nad `Memory<byte>` ako štruktúru, ktorá slúži ako kľúč do dictionary.
To malo tú výhodu, že som vedel volať  metódu `Dictionary<Utf8Key, Data>.TryGetValue()` bez zbytočnej alokácie.
No malo to nevýhodu, že som musel pri pridávaní dát spraviť ďalší lookup do dictionary pre `Dictionary<Utf8Key, Data>.Add()`,
to ale až tak nevadí, lebo podľa zadania sa v 10^9^ riadkoch nachádza maximálne 10^4^ unikátnych miest.
V tomto kroku som sa dostal na _1 minútu a 16 sekúnd_.

Ďalšou optimalizáciou bolo neparsovať teplotu ako double (cez `Utf8Parser.TryParse()`),
ale napísať si vlastný parser, ktorý vracal teplotu vynásobenú stovkou, aby som z nej spravil integer (`Hamburg;34.2` → `3420`).
To pomôže hneď v troch veciach:
- parsovanie je rýchlejšie, lebo je prispôsobené na špecifický formát,
- `int Math.Min(int, int)` je výrazne rýchlejšie ako `double Math.Min(double, double)`, 
- `int Math.Max(int, int)` je výrazne rýchlejšie ako `double Math.Max(double, double)`.

Min (a max) pre integer je dokonca rýchlejšie ako `((x < y)? x : y)`, pretože nepoužíva if-y a procesor nemusí riešiť branch-prediction.
Minimum je implementované ako  `y ^ ((x ^ y) & -(x < y))` a maximum `x ^ ((x ^ y) & -(x < y))`.

Vlastné parsovanie a nasledujúce zmeny ma dostali na _54 sekúnd_.

Triedu `Data` som zmenil na štruktúru a namiesto `Dictionary<Utf8Key, Data>.TryGetValue()`
som použil kombináciu metód ` CollectionsMarshal.GetValueRefOrNullRef()` a `Unsafe.IsNullRef()` a tým sa upravil memory layout v dictionary,
čo ušetrilo 5 sekúnd a alokácie triedy `Data`.

```cs
int dataTemperature = Data.Parse(lineData.Temperature.Span);

Utf8Key tmpKey = new Utf8Key(lineData.Name);
ref Data refData = ref CollectionsMarshal.GetValueRefOrNullRef(dictionary, tmpKey);
if (Unsafe.IsNullRef(ref refData))
{
    dictionary.Add(tmpKey.Detach(), new Data(dataTemperature));
}
else
{
    refData.Add(dataTemperature);
}
```

Ďalších 6 sekúnd som získal len tým, že som si napísal vlastnú jednoduchú hash funkciu pre kľúč.

Na 43 sekúnd som sa následne dostal tak, že som si všimol, že vo flame grafoch sa bilónkrát v dictionary volalo vytvorenie defaultného `EqualityComparer<Utf8Key>`, tak som dictionary inicializoval na _10000_ prvkov a implementoval mu `IEqualityComparer<Utf8Key>`.

Nakoniec som súbor rozdelil na segmenty a spracoval ich viacvláknovo podľa počtu jadier procesora a dostal som sa na čas okolo 17 sekúnd. Ale to nie je také zaujímavé. 

## Čo v tomto prípade nefungovalo
Nikde vyššie nespomínam vektorizáciu a SIMD, hoci by sa na parsovanie riadku tieto inštrukcie hodili.
Najskôr som skúsil napísať vlastnú implementáciu na hľadanie nového riadku cez maskovanie `Vector<byte>`
a potom cez `Vector.EqualsAny()`, ale bolo to pomalšie ako `MemoryExtensions.IndexOf()`.  
Prečo? Pretože framework už vektorizáciu masívne používa.

Táto úloha vyzerá ako stvorená pre novinku z _.NET 9_ – _AlternateLookup_, ale ten sa nedá použiť s `CollectionsMarshal.GetValueRefOrAddDefault()` takže voči súčasnému stavu by som si nepomohol.

_AOT kompilácia_ – výsledný beh programu bol vždy o pár sekúnd pomalší ako klasická framework depend kompilácia.

_ReadyToRun_ – výsledný beh programu bol pomalší ako AOT skompilovaného programu.

_[CommunityToolkit.HighPerformance](https://www.nuget.org/packages/CommunityToolkit.HighPerformance)_ – `StringPool` – myslel som, že mi zníži alokácie, čo aj urobil ale pamäť som vymenil za procesorový čas na dodatočné počítanie hashu a lookap do dictionary `StringPoo`-lu.

## Záver
Bolo zaujímavé rozmýšľať o tak jednoduchej úlohe z hľadiska optimalizácie. Bral som to skôr, ako precvičenie práce s nástrojmi naladenie výkonu a nástrojov na optimalizáciu v dotnete.

Pri spracovaní _13GB_ súboru som sa dostal z času _3 minúty 40 sekúnd_ na _17 sekúnd_.

Pravdepodobne (podľa už exitujúcich riešení) by som sa dokázal dostať na oveľa nižší čas ako 17 sekúnd, ale nechcel som si písať vlastnú implementáciu dictionary ani používať unsafe kód.

## Zdroje
1. <https://blog.ndepend.com/faster-dictionary-in-c/>
1. <https://blog.ndepend.com/alternate-lookup-for-dictionary-and-hashset-in-net-9/>
1. <https://www.morling.dev/blog/one-billion-row-challenge/>
1. <https://www.youtube.com/watch?v=LhhGqNAGQHU&list=PL_nMO-wncU0m3V2_2NmetNDrzTyGLJQAr>
1. <https://www.youtube.com/watch?v=HDbtwTsar1Q>
1. <https://www.youtube.com/watch?v=SHGeE_PFA4s>
1. <https://leveluppp.ghost.io/>
1. <https://www.youtube.com/watch?v=aoXSjsJlSp8>
1. <https://github.com/RendleLabs/HiPerfDotNetTalk/tree/main/OneBRC>


