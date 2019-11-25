Published: 20.5.2018
Updated: 12.9.2019
Title: Golang a alternatívy
Menu: Golang
Cathegory: Dev
Description: Go očami dotnet developera.
---
Haipovaný jazyk, no len na špeciálne použitie. (Blog je písaný k verzii _Go 1.10_ a hodnotí defaultný jazyk a kompilátor.)

**Výhody:**

* Výsledkom kompilácie je jeden natívny spustiteľný súbor bez závislostí.
* Má štandardnú knižnica pre moderné použitie (JSON, REST Api, Http klient, certifikáty,...).
* Jednoduchá cross-kompilácia bez nutnosti mať k dispozícii cieľovú platformu (napr. Mac OS).
* Primitívny jazyk so statickým typovaním.
* Jednoduchá správa závislostí.
* Podpora paralelizácie v jazyku.
* Vhodné na mikro (piko) služby ako load balancer, proxy,...

**Nevýhody:**

* Až moc primitívny jazyk, pre mňa to je kombinácia Pascalu a JavaScriptu.
* Absencia výnimiek, chybu treba vždy vracať a po každej operácii kontrolovať, či nenastala.
* Absencia generík, či makier a overloadov funkcií. To vedie k opakovaniu kódu a vymýšľaniu mien pre overloady a sťažuje implementáciu algoritmov a infraštruktúrneho kódu.
* Absencia resoursov, keď je treba zakompilovať nejaký súbor do binárky je ho tam nutné napastovať ako string alebo bytové pole.
* Nemožnosť kors-kompilácie ak sa použije v kombinácii s C/C++.
* Defaultne chýbajú verzie závislostí a ich automatický restor.
* Na natívne kompilovaný jazyk určený pre vysoký výkon mi chýba možnosť explicitne vytvárať a deštruovať dátové štruktúry.

## Alternatívy

### C++ 11 a vyšie

Od C\+\+11 považujem C\+\+ za modrený jazyk, ktorému nechýbajú veci ako lambdy, funkcie vyššieho rádu, immutable, polymorfizmus... Plus má vďaka _Opem MP_ priamo v jazyku podporu paralelizácie.

Z neparalného programu:


```cpp
  #include <cmath>
  int main(int argc, char *argv[])
  {
    const int size = 256;
    double sinTable[size];
    
    for(int n=0; n < size; ++n)
      sinTable[n] = std::sin(2 * M_PI * n / size);
  
    // the table is now initialized
  }
```

Na paralelný:

```cpp
  #include <cmath>
  int main(int argc, char *argv[])
  {
    const int size = 256;
    double sinTable[size];
    
    #pragma omp parallel for
    for(int n=0; n < size; ++n)
      sinTable[n] = std::sin(2 * M_PI * n / size);
  
    // the table is now initialized
  }
```

Viac ukážok na: [https://bisqwit.iki.fi/story/howto/openmp/](https://bisqwit.iki.fi/story/howto/openmp/).

### Mono
Mono umožňuje cross-kompiláciu do natívneho kódu s tým, že možné všetko zbaliť do jednej binárky pomocou [mkbundle](http://www.mono-project.com/docs/tools+libraries/tools/mkbundle/). Z pokusov, ktoré som robil (konzolové aplikácie s HTTP klientom) mi vyšlo, že výsledný binárny súbor je približne o megabajt väčší ako alternatíva v _Go_. Navyše je takto možné spraviť programy, ktoré majú jednoduché GUI.

### .Net Core
.Net Core umožňuje korss-kompiláciu pre jednotlivé platformy s pribalením všetkých závislostí (nie je potrebné inštalovať runtime). S výkonom sa ide pohrať cez _unsafe_ kód. Nevýhodou je, že aj ku konzolovej aplikácii s HTTP klientom pribalí 30 až 50 MB (podľa výslednej platformy), čo pri serveri nevadí, no pri utilitke pre bežných ľudí hej.

**Edit**: Aj .Net Core ide kompilovať do natívnej binárky, ktorá má obdobnú veľkosť ako program v _Go_ pomocou
* [ILCompoler-u](https://github.com/dotnet/corert/blob/master/Documentation/how-to-build-and-run-ilcompiler-in-console-shell-prompt.md),
* [oficiálneho nástroja crossgen](http://www.jackdermody.net/article/Compiling_NET_Core_to_Native),
* [návodu na blogu Secana](https://secanablog.wordpress.com/2018/06/08/compile-a-net-core-app-to-a-single-native-binary/).

**Edit**: Od .Net Core 3.0 ide kompilovať projekt do natívnej jednosúborovej optimalizovanej natívnej binárky.
Viac sa dozviete na [blogu Scotta Hanselmana](https://www.hanselman.com/blog/MakingATinyNETCore30EntirelySelfcontainedSingleExecutable.aspx).
Napríklad pre Windows 10 stačí spraviť publish s parametrami:
`dotnet publish -c Release -r win10-x64 -p:PublishTrimmed=true -p:PublishSingleFile=true`.

## Záver
_Go_ je veľmi primitívny jazyk, jeho syntax a premisy sa človek naučí zvládnuť za niekoľko hodín a za pár dní dokáže v ňom písať programy. Ale z charakteru jazyka, by som v ňom nepísal veľké systémy, skôr sa hodí na malé utilitky, mirkoslužby (ktoré sú ale fakt mikro) alebo pre použitie v _Raspberry Pi_.

Prosto všade tam, kde je výhodné mať malý procedurálny program (max. niekoľko stoviek riadkov), ktorý je ale pomerne rýchly a je bez závislostí na externom runtime, či knižniciach (netreba nič inštalovať na operačný systém). Pekným príkladom je napríklad utilitka 
 [ngrok](https://ngrok.com/).