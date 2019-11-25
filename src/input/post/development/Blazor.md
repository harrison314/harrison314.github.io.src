Published: 25.11.2019
Title: Ako ma zachránil Blazor
Menu: Ako ma zachránil Blazor
Cathegory: Dev
Description: Ako ma zachránil Blazor pred svetom JavaScriptu.
---
**...pred svetom JavaScriptu**

V skratke, [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor) je SPA framework postavený nad _WebAssembly_.
Ako jazyk pre písanie aplikačnej logiky používa C# a pre komponenty používa Razor components, poskytuje DI kontajner, Razor a plno ďalších dobrých vlastností.
Hlavne je v ňom možné použiť takmer akúkoľvek JavaScriptovú a takmer akúkoľvek .Net Standard knižnicu.

Momentálne dokončujem svoj prvý produkčný projekt v Blazore, ide v podstate o pár dynamických formulárov, generovanie _PKCS#10_ žiadosti a _PKCS#12_.
Na riešení cez Blazor bolo super, že mi stačilo napísať okolo sto riadkov JavaScriptu (interop na _WebCrypto_) a všetku ostatnú logiku aj s testami som mal napísanú za dve hodiny.
Keby ju píšem v JavaScripte/TypeScripte trvá to dva mesiace. Ako to viem?

Túto cestu som už prešliapal.

## Krátka história
Na konci roku 2015 som začal pozerať po nejakom SPA frameworku, nasadol som na vlnu hype a začal som robiť v Reacte. No keď som sa ho naučil a spravil si jednoduchú aplikáciu, tak to nadšenie zo mňa vyprchalo.  
([Poznámka 1](#Poznamka1))

V roku 2016 som sa pustil do hoby projektu [PerDia2012](PerDia2012.html), vďaka [DotVVM](https://www.dotvvm.com/) som sa dozvedel o [knockout.js](https://knockoutjs.com/). Páčilo sa mi, že _knockout.js_ má prehľadné šablóny, binding, a v spojení s TypeScriptom som dostal objektový kód.
Neskôr po pridaní podpory _async/await_ do TS sa mi počet riadkov kódu zredukoval takmer o tretinu.

V roku 2017 prišiel projekt, ktorý nebol len pekné GUI k backendu, bolo v ňom dosť šifrovania, HMAC-ovania a podpisovania.
A tu začali problémy. Síce všetky prehliadače podporovali štandard _WebCrypto_ (rozumej všade bolo prístupné rovnaké API) ale podporované algoritmy sa líšili naprieč prehliadačmi a nebolo zaručené, že rovnaký výsledok človek dostane na rovnakom prehliadači ale inej platforme.
Takže na každé použitie _WebCrypto_ bolo potrebné nájsť alebo napísať polyfill, čo v praxi znamenalo, že pre každú metódu bolo potrebné nájsť a vyskúšať niekoľko NPM balíčkov, pričom každý mal vstup a výstup v inom formáte a ťahal zo sebou plno závislostí.
Problémom bola aj kvalita týchto knižníc (nie je AES ako AES).

V roku 2018 prišiel veľmi zaujímavý projekt, ktorý ma podobnú funkcionalitu ako [Azure Key Valut](https://azure.microsoft.com/en-us/services/key-vault/) (sprístupňuje funkcionalitu _HSM_ ako REST službu).  
([Poznámka 2](#Poznamka2))

K tejto službe sa písalo GUI pre bežných používateľov, v ktorom mali byť zobrazené prístupné RSA/EC kľúče, certifikáty a malo poskytovať možnosť generovať žiadosti o certifikát. Na tento projekt bol zvolený aktuálny Agular.

Čo môže byť ťažké na načítaní certifikátu a vypísaní jeho atribútov, veď v NPM na to musí byť plno knižníc?
**Áno je, ale...**
Knižnicu [pkijs](https://pkijs.org/) sa ukázala v tomto projekte nepoužiteľná, [forge](https://github.com/digitalbazaar/forge) vyzerala nádejne, hoci  som musel použiť tri iné knižnice aby som dostal čistatelný výstup.
Nádej sa rozplynula, keď som jej nenechal sparsovať certifikát s EC kľúčom.
Podobne na tom boli ja všetky ostatné, ktoré som našiel.
Tak to dopadlo nakoniec tak, že som si našiel RFC-čko ku X509 certifikátom a napísal som si v TypeScripte vlastný parser.

Generovanie žiadosti o certifikát (_CSR_ alebo _PKCS#10_), pomerne jednoduchá vec, našiel som cez 30 knižníc, no prvých 5 vylúčil podľa dokumentácie (_To je naozaj také ťažké spraviť v javascriptovej knižnici aspoň jeden extension point?_).
Ostaté dokumentáciu nemali, tak som ich po jednej skúšal, študoval ich zdrojáky a tým trávil veľa času.
Niekoľko z nich som vylúčil kvôli jednoúčelovosti (dali sa použiť iba s inou konkrétnou knižnicu alebo popisovačom).
Ostatné sa ani neunúvali dodržiavať štandardy alebo nefungovali vôbec.
Keď som vylúčil aj poslednú knižnicu, ktorá mala komentáre vo vietnamčine, tak som si otvoril príslušné RFC-čko a napísal si vlastný generátor _PKCS#10_ žiadostí.

Ďalší javascriptovský projekt v tomto roku sa týkal podpisovania dokumentov do štandardného európskeho formátu priamo v prehliadači.
Tam bola situácia s NPM balíčkami dosť podobná, nedodržiavanie štandardov, bugy, spájanie a vyberanie veľa knižníc, ktoré navzájom nepasovali, každá mala vstupy a výstupy v inom formáte (vlastný objekt, Uint8Array, Base64, HEX,...), väčšina kódu bola len lepidlo medzi týmito knižnicami a mal som pocit, že sa neustále snažím natlačiť štvorec do kruhového otvoru.

Pokračuje to ďalej skladaním XML-ka, máme cez 30 knižníc serializujúcich alebo pracujúcich z XML-kom, jedna nevie používať atribúty, druhá nevie používať hodnoty, ďalšia nevie pracovať s namespacami...
Zanedlho si píšem vlastný builder XML-ka.

Takto to pokračuje ďalej s XSLT transformáciami, kódovním, a znovu parsovanie certifikátov (síce len RSA, ale bolo potrebné pracovať z rozšíreniami certifikátu),...  
([Poznámka 3](#Poznamka3))

## Záver
Hovorí sa, že v programovaní by nemal človek znovu vynachádzať koleso, no keď má na výber len štvorcové, neotáčajúce sa alebo deravé kolesá, tak na výber príliš nemá. K tomu človek cíti flustráciu ako z [mrogurtu](https://www.youtube.com/watch?v=LXvUhk1ORx0).

Myslím, že Blazor je spása pre .NET programátorov, ktorý v prehliadači chcú robiť niečo viac ako len pekné GUI.
Mne na danom projekte ušetril týždne času a ako bonus som získal velmi pohodlný vývoj.

## Poznámky

#### Poznámka 1 {#Poznamka1}
Pravdepodobne to bolo tým, že mi React príde až pŕliš prehypovaný a mal som skúsenosti s inými technológiami (virtual dom, z programátorských fór mám pocit, že „_Odpoveď na základnú otázku života a vesmíru vôbec_“ nie je _42_ ale React, dokonca táto odpoveď padala aj pri otázke ako konvertovať video do iného formátu...).
Síce sa v prednáškach dozviete, že je to, to najlepšie čo môže byť a všetko ostatné je mŕtve, ale zas každý rok sa mení _best practice_, ako s ním pracovať.
Tiež sa človek dozvie, že je super funkcionálny, ale pre mňa v porovnaní z Haskellom alebo F# príde ako procedurálny kód. A so spojením s manažmentom stavu je zbytočne ukecaný.

#### Poznámka 2 {#Poznamka2}
Išlo o veľmi zaujímavý projekt, pri ktorom som sa naučil ako písať auditovatelný kód, o bezpečnosti založenej na typoch a rozhraniach, ako písať virtuálny driver (pre _PKCS#11_),...

#### Poznámka 3 {#Poznamka3}
Tento projekt mal po dokončení prototypu cez 1900 závislostí, po mesiaci mi NPM v 380-tich hlási kritické bezpečnostné chyby.
V Blazorom projekte, ktorý využíva obdobné technológie mám 3 závislosti. 

#### Poznámka 4 {#Poznamka4}
Mená produktov neuvádzam zámerne.
