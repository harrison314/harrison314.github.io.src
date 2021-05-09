Published: 20.2.2020
Title: eID nie je platobná karta
Menu: eID nie je platobná karta
Cathegory: Dev
Description: Vysvetlenie častých omylov okolo elektronického občianskeho preukazu.
---
Často sa stretávam s tým, že ľudia chcú od elektronického občianskeho preukazu veci, ktoré sú nezmyslené alebo vyslovene nebezpečné.

**Tento blog je reakciou na veci, na ktoré narážam často na diskusných fórach a nechce sa mi to dookola vysvetľovať.**

## Nie je karta ako karta
> Dokelu na čo máme techniku?
A načo si to mam ako občan aktivovať sama, veď keď mi vydajú občiansky, nech mi ho na polícii rovno aktivujú,
pošlú mi prihlasovacie údaje domou v obálke zabezpečeným spôsobom ako banka posiela aktivačné nastavenie na kartu.
A nech nemusím riešiť debiliny.

<!--Elektronický občiansky preukaz nie je platobná karta, rieši iné problémy a procesy okolo neho fungujú inak z dobrého dôvodu.-->

[eID](https://www.slovensko.sk/sk/eid/_eid-karta/) sa síce podobá na platobnú kartu, má rovnako vyzerajúci čip, ale rieši úplné problémy a procesy okolo neho fungujú inak z dobrého dôvodu.

Pin-y na akejkoľvek čipovej karte sprístupňujú certifikáty a privátne kľúče na použitie.
Takže „aktivácia“ (vydanie certifikátov) by vytvorila situáciu, keď niekto iný má prístup naraz ku karte a jej PIN-u.
Vďaka čomu môže podpísať ľubovoľný dokument bez Vášho vedomia.
Okrem toho, na Slovensku má elektronický podpis s časovou pečiatkou právnu platnosť notársky overeného podpisu.

Pri platobnej karte je situácia iná.
Platobná karta sa viaže k účtu a jeho majiteľ v internet bankingu vidí všetky operácie s kartou
a tie podozrivé môže stornovať alebo nahlásiť ako podvod. No pri elektronickom podpise nič také nejestvuje
(podpis sa technicky vykonáva na čipe karty).
Nejestvuje zoznam zmlúv podpísaný konkrétnym eID,
navyše už podpísaný dokument v čase platnosti certifikátu (pred revokáciou alebo skončením platnosti) nie je možné stornovať.

Preto musia byť PIN-y od eID tajomstvo jej držiteľa.

## Nie je certifikát ako certifikát
> Ja chcem podpísať dokument autentifikačným certifikátom!

Technicky je možné podpísať dokument ľubovoľným [certifikátom](https://en.wikipedia.org/wiki/X.509) (spolu s privátnym kľúčom),
ale nie je dobrý nápad podpisovať dokumenty certifikátmi, ktoré neboli na to určené.

Certifikačná autorita vydáva certifikát na nejaké použitie, ktoré je označené v certifikáte
(podpis dokumentov, šifrovanie, autentifikácia, code signing, ...).
Od toho na čo je certifikát určený sa odvíjajú aj iné veci, ako požiadavky na uloženie (_[QSCD](https://www.nbu.gov.sk/doveryhodne-sluzby/certifikacia-produktov/certifikovane-produkty/zariadenia-pre-podpis-a-pecat-qscd/index.html)_) a zabezpečenie privátnych kľúčov, doba platnosti, veľkosť a typ kľúčov...

Žiaden seriózny softvér na vytváranie elektronických podpisov nedovolí podpísať dokument SSL, auetntifikačným alebo šifrovacím certifikátom
a žiaden seriózny softvér niečo také nevyhlási za validný dokument. Sú na to reálne dôvody.

## Otvorený kód je super, hlavne, keď ho spraví niekto iný
> eID treba heknúť na heketone. Nemáme open source podpisovač.

Netreba, slovenské eID poskytuje štandardné a rozhranie čipových kariet a kryptozariadení _[PKCS#11](http://docs.oasis-open.org/pkcs11/pkcs11-base/v2.40/os/pkcs11-base-v2.40-os.html)_.
Pomocou neho ide pristupovať k certifikátom, prihlásiť sa na kartu, či spraviť podpis.

A ak niekto chce mať podpisovač s otvoreným kódom, tak nemusí fňukať.
Náš občiansky preukaz používa štandardné rozhranie (_PKCS#11_),
štandardy pre formáty európskych elektronických podpisov sú tiež otvorené (_ETSI_ a _[eIDAS](https://en.wikipedia.org/wiki/EIDAS)_),
tak už len stačí spraviť projekt na githube a nakódiť to.
