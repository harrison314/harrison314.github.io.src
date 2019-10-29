Published: 19.7.2017
Updated: 9.5.2019
Title: Užitočné programy
Menu: Užitočné programy
Cathegory: Dev
Description: Zoznam programov užitočných pre Windows a .Net vývojára.
---
# Užitočné programy

V tomto článku uvádzam niekoľko jednoduchých užitočných programov pre dotnet vývojára.

## IL Spy / dotPeek 
Ide o .Net dekompilátory. Dokážu zobraziť kód knižníc alebo programov napísaných v C#. Hodia sa hlavne ako rýchla dokumentácia pri riešení obskúrnych problémov, hľadaní závislostí alebo skontrolovaní výsledku optimalizácie kompilátora, ofluskátoru a podobne.

* [IL Spy](http://ilspy.net/)
* [IL Spy Visual Studio Extension](https://marketplace.visualstudio.com/items?itemName=SharpDevelopTeam.ILSpy)
* [dotPeek](https://www.jetbrains.com/decompiler/)

## Nuget package Explorer
Nuget package Explorer je malý program umožňujúci prezeranie obsahu Nuget balíčkov, ich úpravu alebo ručnú tvorbu. Hodí sa pri tvorbe vlastných nuget balíčkov na skontrolovanie zbalených súborov. Navyše je ho možné prepojiť s IL Spy a tým rovno prezerať aj kód vložených knižníc.

* [Nuget package Explorer](https://github.com/NuGetPackageExplorer/NuGetPackageExplorer)

## RoslinPad
Nástroj na rýchle prototypovanie a skúšanie snippletov kódu, s podporou .Net Core a Nugetov. Ide v podstate o bezplatnú alternatívu k obľúbenému [LinqPad-u](https://www.linqpad.net/). Využívam ho hlavne na testovanie kódu, ktorý sa týka reflexie alebo regulárnych výrazov. No hodí sa napríklad aj na spustenie kódu, ktorý treba vykonať len raz (prekódovanie súboru, zmazanie priečinku *node_modules* a podobne).

* [RoslinPad](https://roslynpad.net/)
* [RoslinPad v Microsoft store](https://www.microsoft.com/sk-sk/store/p/roslynpad/9nctj2cqwxv0?ocid=badge&rtc=1)

## Papercut 
Papercut jednoduchý nástroj pre testovanie odosielania a formátovania mialov z aplikácií. Ide o jednoduchý desktopový SMTP email server.

* [Papercut](https://github.com/ChangemakerStudios/Papercut)

## ngrok 
Ngrok je program a služba umožňujúca vytunelovať lokálny port (tcp, udp) do internetu s verejnou doménou druhého rádu. Hodí sa napríklad na predvedenie webu zákazníkovi, bez nutnosti deplonúť ho na verejne prístupný server, testovanie web hookov, testovanie Microsoft bot frameworku. Navyše po spustení poskytuje Web API, pomocou ktorého je z neho možné zistiť dodatočné informácie alebo meniť nastavenia a tým tento proces automatizovať.

* [ngrok](https://ngrok.com/)
* [ngrok - Downloads](https://ngrok.com/download)

## devd 
Devd veľmi primitívny jednosúborový lokálny http server servírujúci statický obsah.

* [devd na Githube](https://github.com/cortesi/devd)
* [devd releases](https://github.com/cortesi/devd/releases/)

## DebugView
Utilita monitorujúca debug a trace výstup programov na lokálnom systéme, hodí sa na debugovanie programov, ktoré výstup nezapisujú do súborov.

* [DebugView](https://technet.microsoft.com/en-us/sysinternals/debugview.aspx)

## Process Explorer
ProcessExplorer je program zobrazujúci bežiace procesy v stromovej štruktúre, využité knižnice a iné veľmi podrobné informácie o bežiacich procesoch v systéme.

* [Process Explorer](https://technet.microsoft.com/en-us/sysinternals/processexplorer.aspx)

## Process Monitor
Process Monitor je program na real-time monitoring procesov vo Windowse, zobrazuje napríklad interakcie zo sieťou, registrami, súborovým systémom...

* [Process Monitor](https://docs.microsoft.com/en-us/sysinternals/downloads/procmon)

## Paint.NET
Paint.NET je populárny editor obrázkov, používam ho hlavne pre to, že mi príde použiteľnejší ako Gimp, má dobrú podporu priehľadných obrázkov a vrstiev. 

* [Paint.NET](https://www.getpaint.net/)