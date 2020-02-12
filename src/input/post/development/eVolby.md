Published: 12.2.2020
Title: Elektronické voľby
Menu: Elektronické voľby
Cathegory: Dev
Description: Prečo sú elektronické voľby v súčasnosti zlý nápad, hoci nám internetbanking funguje?
---
**Prečo sú elektronické voľby v súčasnosti zlý nápad, hoci nám internetbanking funguje?**

_Aby to nebolo veľmi dlhé preskočím reči, čo to voľby sú, aký majú význam pre demokraciu._

Na voľby alebo referendá sú kladené nasledujúce požiadavky:
* voliť by mal len právoplatný volič,
* občan musí mať možnosť voliť slobodne (nesmie byt donútený k voľbe),
* voľba musí byť tajná (aby neskôr nemohol byť človek perzekuovaný totalitným režimom za svoju voľbu).

Tieto body spĺňajú papierové voľby krúžkovaním, plentou, volebnou komisiu jednoduchým a pochopiteľným spôsobom
a to aj pre bežného človeka.

## Prečo elektronické voľby v súčasnosti nie
Elektronické voľby majú mnoho problémov. Medzi tie ľudské patrí:
* hlasovanie papierovými voľbami chápe aj človek so základnou školu, pri elektronických je to horšie
(krúžkovanie papiera verzus _[eID](https://www.slovensko.sk/sk/eid)_ s platným podpisovým certifikátom a správnym softvérom),
* počítaniu papierových hlasov rozumie hocikto a hocikto ho vie skontrolovať,
no skontrolovať elektronické voľby vyžaduje netriviálne znalosti z kryptografie,
kybernetickej bezpečnosti a elektronického podpisu (zakrúžkovaný kandidát za plentou verzus [RSA](https://cs.wikipedia.org/wiki/RSA),
[ECDSA](https://sk.wikipedia.org/wiki/Elliptic_Curve_Digital_Signature_Algorithm),
[AES](https://en.wikipedia.org/wiki/Advanced_Encryption_Standard),
[blind signing](https://en.wikipedia.org/wiki/Blind_signature),
[end-to-end šifrovanie](https://en.wikipedia.org/wiki/End-to-end_encryption), penetračné testy,...),
* bežný programátori nemajú na to aby to spravili dobre, alebo aby v ňom našli chybu ([bežný programátor ani nevie, ako bezpečne pracovať s heslom](https://net.cs.uni-bonn.de/fileadmin/user_upload/naiakshi/Naiakshina_Password_Study.pdf)).

No hlavným problémom je samotný koncept, ako spraviť elektronické voľby dobre:
* volebný systém musí vedieť voliča jednoznačne identifikovať a autorizovať ho na to aby vedel zmeniť svoj hlas (slobodná voľba),
aby sa jeho hlas započítal len raz a aby volil len právoplatný volič,
* zároveň volebný systém nesmie vedieť, ako daný človek hlasoval (zabezpečenie tajnej voľby),
kvôli možnému zneužitiu tejto informácie (od znevýhodnenie pri vybavovaní vecí až zavretie do koncentráku po nástupe totality).

A práve tento rozpor, že **volebný systém musí a zároveň nesmie vedieť, ako hlasoval ktorý občan, nie je v súčasnosti uspokojivo vyriešený**.

### Máme internetbanking, prečo teda nie elektronické voľby?
Internetbanking a elektronické voľby sú diametrálne odlišné veci s odlišnými požiadavkami, rizikami a prostredím.

### Ale veď elektronické voľby v Estónsku fungujú
Áno, fungujú paralelene s papierovými. No spomínaný problém s anonymitou voľby nie je vyriešený uspokojivo.

Veľmi zjednodušene fungujú tak, že aplikácia na strane klienta zoberie hlas voliča a asymetricky ho zašifruje, zašifrovaný hlas podpíše občianskym preukazom.
Takto podpísanú obálku so zašifrovaným hlasom sa odošle na „autentifikačný“ server, ktorý overí či daný občan môže voliť a postará sa, že si pamätá len jeho posledný hlas, no nevie rozšifrovať to ako občan hlasoval.
Pri sčítaní hlasov z nich „autentifikačný“ server odstráni podpisy a zoradené odošle na počítací server, ktorý hlasy vie rozšifrovať a spočítať ich.

Tu je problém v zapečení tajnej voľby, lebo stojí len na tom, že sa nespoja dáta „autentifikačného“ a počítacieho serveru.
No to nestačí.

_(Jestvujú aj sofistikovanejšie volebné schémy napr. [Schémy s využitím homomorfného šifrovania](http://www.dcs.fmph.uniba.sk/diplomovky/obhajene/getfile.php/Danko_DP.pdf?id=334&fid=575&type=application%2Fpdf), no ich náročnosť implementácie je výrazne vyššia.)_

### Dáme tam blockchain
Ľudia radi používajú termíny, ktorým nerozumejú. Blockchain nerieši ani jedenu požiadavku na elektronické voľby spomínané vyššie.

## Záver
Elektronické voľby ešte nie sú vyriešený problém.

No v budúcnosti sa možno objaví volebná schéma, ktorý bude riešiť súčasné problémy elektronických volieb pomocou tvrdých matematických a teoretických princípov, podobne ako v súčastnosti funguje elektronický podpis. 
