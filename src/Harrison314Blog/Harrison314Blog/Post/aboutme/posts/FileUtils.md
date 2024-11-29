Published: 28.5.2016
Title: FileUtils
Menu: FileUtils
Description: 'Aplikácia FileUtils pre prácu zo súbormi.'
Cathegory: Portofolio
---
_FileUtils_ je jednoduchá aplikácia, ktorej hlavnou funkcionalitou je hľadanie duplicitných súborov
na disku podľa ich obsahu. Je vhodná pri veľkom množstve PDF-iek alebo fotiek na disku. Medzi ďalšie 
funkcie a vlastnosti patrí:

* spájanie a rozdeľovanie súborov,
* skartovača súborov,
* lokalizácia aplikácie,
* systém pluginov.

Zaujímavosťou tohto programu je spôsob hľadania duplicitných súborov,
využíva sa na to _HashSet_ objektov reprezentujúcich súbor.
Tieto objekty si uchovávajú cestu k súboru a jeho veľkosť. Veľkosť súboru sa používa v metóde _GetHashCode_ , 
metóda _Equals_ najskôr porovná veľkosti súborov, ak sa zhodujú prečíta ich a vypočíta ich MD5 hashe,
ktoré následne porovná. Vďaka tomu je hľadanie duplicitných súborov rýchle (na bežnom notebooku z Intel i5 a točivým diskom cca. 1GB priemerne 300-500KB súborov, prehľadá za 30 sekúnd) a pamäťovo nenáročné.

## Použité technológie

C# 4.0, WPF, lokalizácia pomocou T4 generátora resx súborov a vlastný _WPF MarkupExtension_ pre dynamické načítavanie lokalizácie.

![Hľadanie zhodných súborov](images/About/Portfolio/1.png){.img-center}

![Nastavenia](images/About/Portfolio/2.png){.img-center}
