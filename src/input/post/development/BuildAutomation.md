Published: 8.7.2023
Updated: 10.7.2023
Title: Prečo používať build automation
Menu: Prečo používať build automation
Cathegory: Dev
Description: Prečo používať build automation tooly a predstavenie Cake Build a NUKE.
OgImage: images/BuildAutomation/media.png
---

Za svoju kariéru som sa stretol s rôznym spôsobom ako buldovať dotnetovské projekty,
od použitia _Visual Studia_, cez _.bat_ súbory, _Powershell_ a nakoniec som skončil pri build automation tooloch (konkrétne _Cake_ a _Nuke_).

Späť by som sa už nevrátil. Prečo? **Lebo tieto nástroje šetria čas a nervy.**

## Čo je to build automation tool
Pri vytvorení artefaktov projektu nejde len o to niečo skompilovať, treba s tým spraviť plno iných drobných vecí.
To zahŕňa odstránenie starých artefaktov, kompiláciu závislých projektov, podpísanie binárok, konverziu dokumentácie do vhodného formátu,
pridanie ďalších súborov do projektu (licencie, vygenerované SQL migrácie, konfiguráciu,...)
a nakoniec vytvorenie balíčkov pre inštaláciu (_zip_, _msi_, _deb_,...) a nezabudnúť ju správne pomenovať.

K tomu celému je často v projekte viac výsledných inštalačných balíkov a nugetov. Taktiež je pred tým potrebné pustiť unit testy,
či zaistiť rozdielne správanie pre release a prerelease build.


A práve tieto úlohy dokáže automatizovať build automation tool. Nie je to kompilátor, je o úroveň vyššie.

![Znázornenie targetov v NUKE.](images/BuildAutomation/plan.png){.img-center} 
Znázornenie závislostí medzi úlohami - NUKE ([zdroj](https://ithrowexceptions.com/2020/06/05/reusable-build-components-with-interface-default-implementations.html)).{: .text-center .font-italic}

## Prečo používať build automation
... pre .NET (aj v porovnaní so štandardami CI/CD pipelinami):

* zdieranie vedomostí v tíme a zastupiteľnosť kolegov,
* build všetkých projektov sa spúšťa rovnako,
* build funguje rovnako lokálne u vývojára ako na build serveri,
* proces buildovania je možné odladiť lokálne (v gite nebude tisíc komitov pre úpravu YAML definície pipeline – do YAML proste breakpoint nedáš) ,
* zníženie vendor-locku (nie je to napísané vo vendor špecifickej YAML pipeline), pri použití CI/CD je potrebný YAML jednoduchý (len spúšťa build automation tool), rovnakú definíciu je možné použiť na väčšinu projektov a napísať túto definíciu pre iného vendora nie je náročné,
* pomocou targetov je možné spustiť rozne úlohy v CI/CD pipleine (napr. _test_, _build_, _publish_,...),
* žiadne magické stringy, žiadne skryté požiadavky a menej prekvapení,
* nie je to mágia, ale známy jazyk a knižnice a dokonca aj IDE.

V podstate celý proces buildovania (vrátane testov a uploadu artefaktov) sme presunuli z „ručnej roboty“ alebo YAML-u do programu,
ktorému rozumieme.

## Cake a NUKE
_Cake_ (C# Make) a _NUKE_ sú oba build automation tooly pre .NET a MS Build.
Oba viac menej dokážu to isté a podobným spôsobom.

Čo majú spoločné:
* pre inicializáciu používajú .NET gloval tool,
* sú multiplatformové,
* majú slušnú komunitu,
* ide o DSL nad jazykom C# (závislosti, úlohy, cleanup, buildovanie, práca so súbormi, globing, version control, notifikácie,...),
* vedia buildovať aj iné ako .NET projekty,
* orientujú sa na úlohy (tasky/targety) so vzájomnými závislosťami,
* ich funkcionalita ide rozšíriť pomocou nugetov (stále je to C#),
* zdieľanie kódu je tiež možné pomocou nugetov,
* majú podporu najznámejších test runnerov,
* dokážu artefakty deploynúť a nasadiť (to sa často nepoužíva),
* majú širokú podporu rozšírení a toolov (_dotnet_, _MS Build_, _node.js_, _EsLint_, _upack_, _Docker_, _OpenCover_, _Sonarqube_, _git_, _Wix_, _Yarn_,...),
* majú dobre vyriešené logovanie.

Čo majú rozdielne:
* _Cake_ je napísaný ako „skript“ (v _.cake_ súbore – vie s ním pracovať _Visual Studio Code_, plus je na to plugin),
* _NUKE_ používa .NET projekt priamo v soliution (takže natívna podpora IDE),
* ku rozšíriteľnosti pristupujú trochu inak,
* príde mi, že _Cake_ má viac extrených toolov/addonov,
* _Cake_ je vhodnejšie aj pre projekty s _.Net Frameworkom_,
* _NUKE_ dokaźe deklaratívne používať dotnet global tools,
* _NUKE_ dokáže parsovať _.sln_ a _.csproj_ súbory,
* _NUKE_ vie generovať YAML súbory pre známe CI/CD riešenia.

### Cake

<table class="table table-borderless">
<tbody>
<tr>
  <td class="col-md-4">Stránka</td>
  <td class="col-md-8"><a href="https://cakebuild.net/" target="_blank">https://cakebuild.net/</a></td>
</td>
<tr>
  <td class="col-md-4">Ako začať</td>
  <td class="col-md-8"><a href="https://cakebuild.net/docs/getting-started/setting-up-a-new-scripting-project" target="_blank">https://cakebuild.net/docs/getting-started/setting-up-a-new-scripting-project</a></td>
</td>
<tr>
  <td class="col-md-4">Github</td>
  <td class="col-md-8"><a href="https://github.com/cake-build/cake" target="_blank">https://github.com/cake-build/cake</a></td>
</td>
<tr>
  <td class="col-md-4">Integrácia na trojstranné baličky</td>
  <td class="col-md-8"><a href="https://cakebuild.net/extensions/" target="_blank">https://cakebuild.net/extensions/</a></td>
</td>
</tbody>
</table>

### NUKE

<table class="table table-borderless">
<tbody>
<tr>
  <td class="col-md-4">Stránka</td>
  <td class="col-md-8"><a href="https://nuke.build/" target="_blank">https://nuke.build/</a></td>
</td>
<tr>
  <td class="col-md-4">Ako začať</td>
  <td class="col-md-8"><a href="https://nuke.build/docs/getting-started/installation/" target="_blank">https://nuke.build/docs/getting-started/installation/</a></td>
</td>
<tr>
  <td class="col-md-4">Github</td>
  <td class="col-md-8"><a href="https://github.com/nuke-build/nuke" target="_blank">https://github.com/nuke-build/nuke</a></td>
</td>
<tr>
  <td class="col-md-4">Integrácia na trojstranné baličky</td>
  <td class="col-md-8"><a href="https://nuke.build/docs/common/cli-tools/" target="_blank">https://nuke.build/docs/common/cli-tools/</a></td>
</td>
</tbody>
</table>

## Ktorý si vybrať?
Typický príklad použitia týchto nástrojov je, že pri release builde sa spustia unit testy,
do buildu sa vrazí git komit a nejaké premenné prostredia, výsledok sa zozipuje,
dopíše sa verzia a niekde sa uploadne artefakt. Na čo bohate stačia oba tieto nástroje.

Oba tieto tooly sú dobré a dokážu podobné veci trochu iným prístupom.
Čokoľvek, čo ide spraviť v C# ide spraviť aj pomocou týchto nástrojov.

_NUKE_ je viac deklaratívne, tým ma človek ľahší začiatok a pokiaľ netreba naozaj špeciálne veci tak bohate stačí.
Taktiež je príjemné mať intelisense vďaka tomu, že ide o konzolovú aplikáciu v rámci soliution.

_Cake_ je viac imperatívne, osobne ho používam o dosť dlhšie
a možno aj pre to mi príde trochu univerzálnejšie, navyše vyhráva v počte integrácii na trojstranné služby a programy.

## Záver
Myslím, že keď si človek vyberie hociktorý nástroj (_Cake_, _NUKE_), tak na začiatku nespraví chybu.
Navyše kód pre build automation nebýva dlhý, tak to ide ľahko prepísať.

A ako som spomínal, už len samotné použitie takéhoto nástroja šetrí čas a nervy.

Osobne mi tieto nástroje priniesli hlavne to, že si viem buildy rýchlo odladiť lokálne.
A keď som na dovolenke, tak mi nemusí nik volať ohľadom buidlovania knižnice XY, ktorú nutne potrebuje na vyriešenie bagu.
