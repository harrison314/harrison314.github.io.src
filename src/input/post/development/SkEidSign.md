Published: 30.7.2018
Title: Podpísanie PDF-ka Slovenským eID-čkom
Menu: Podpisovanie PDF v .Net Core
Cathegory: Dev
Description: Článok o elektronickom podpisovaní PDF SLovenským eID v .Net Core.
---
# Podpísanie PDF-ka Slovenským eID-čkom
V tomto článku sa pokúsim v skratke vysvetliť princípy podpisovania dokumentov pomocou 
_PKCS#11_ a ukážem to a Slovenskom eID-čku, pomocou jednoduchej konzolovej aplikácii v .Net Core.

**Disclaimer**: Podpisová aplikácia má slúžiť len na ukážku princípov,
nevytvára validné podpisy podľa Európskej legislatívy (_eIDAS PAdES_),
aj keď mnohé známe firmy posielajú faktúry podpísané rovnakým spôsobom.
Použitie len na vlastné riziko!

Zdrojové kódy ukážkovej aplikácie sa nachádzajú  na 
[https://github.com/harrison314/SlovakEidSignTool](https://github.com/harrison314/SlovakEidSignTool).

## PKCS#11
_PKCS#11_ je štandard definujúci C-éčkove API pre komunikáciu z crypto tokenmi ako sú HSM (_Hardware security module_),
tokeny a  čipové karty (_smart cards_). Práve Slovenské eID-čko je takáto čipová karta.

Na obrázku nižšie sa nachádza HSM-ko a slovenské eID. 

![HSM (Hardware security module)](images/Programing/SkEidSign/hsm_200.jpg)
![Slovenské eID](images/Programing/SkEidSign/eid_200.jpg)

V projekte využívam knižnicu PKCS#11-interop, ktorá tvorí manažovaný wrapper, nad C-čkovým API.

## Proces podpísania dokumentu

Na začiatku sa načíta a inicializuje _PKCS#11_ knižnica pre eID, ktorá je súčasťou aplikácie [eID klient](https://www.slovensko.sk/sk/na-stiahnutie).

Vyberie sa slot a token, v tomto prípade je slot čítačka kariet a token samotná čipová karta,
eID-čko má slot zo ZEP certifikátom označený labelom *SIG_ZEP*, program ho vyberie.

Načítajú sa informácie o tokene a zistí sa, či je preň potrebná autentifikácia.
Pre eID-čko je to BOK PIN, ktorý umožní prístup k objektom na karte.

Objekty sú napríklad certifikáty, kľúče, dáta, HW features,... Každý objekt je reprezentovaný
definovanou množinou atribútov. Knižnice implementujúce štandard PKCS#11 izolujú operácie pomocou sessions
(operácie v rámci jednej otvorenej session nemajú garantované thread-safe spávanie),
jeho zvláštnosťou je, že keď je autentifikovaná jedna session,
tak sú autentifikované všetky v aplikácii.

Vytvorí sa nová session, ktorá je už autentifikovaná, a vyhľadajú sa všetky certifikáty na tokene.
Vyextrahuje sa certifikát v binárnej podobe (atribút *CKA_VALUE*) a v C# kóde sa zistí,
ktorý z certifikátov je určený na podpis dokumentov (certifikát má extension s flagom _NonRepudiation_).
Následne sa k nemu vyhľadá privátny kľuč, ten má spravidla rovnaké atribúty *CKA_ID* a *CKA_LABEL*
ako príslušný certifikát. Na eID-čku sa dá ZEP/KEP privátny kľúč a certifikát nájsť aj podľa špecifického labelu,
ale na to som sa v aplikácii nechcel spoliehať.

Podpísanie PDF-ka má a starosti knižnica [iTextSharp](https://www.nuget.org/packages/itext7/), 
program implementuje rozhranie [IExternalSignature](https://github.com/harrison314/SlovakEidSignTool/blob/master/src/SlovakEidSignTool/Pkcs11ExternalSignature.cs), 
v ktorom prebieha samotné podpísanie PDF-dokumentu.
Na vstup dostane dáta z PDF-ka, z ktorých sa vytvorí SHA-256 hash. 
Z neho sa vytvorí _PKCS#1 digest info_ (_ASN.1_ dátová štruktúra, ktorá je vhodná na podpísanie RSA kľúčom). 

Samotný podpis musí byť v prípade eID-čka autentifikovaný ZEP PIN-om,
ten sa zadáva po inicializácii podpisovania. Čip v eID-čku podpíše hash a vráti podpis.
Ten vráti v metóde rozhrania [IExternalSignature](https://github.com/harrison314/SlovakEidSignTool/blob/master/src/SlovakEidSignTool/Pkcs11ExternalSignature.cs) a _iTextSharp_ dokončí podpisovanie a uloží PDF-ko na disk.

## Záver
Princíp realizácie podpisu pomocou slovenského eID je podobný ako podpisovanie inými krypto zariadeniami, ku ktorým sa pristupuje pomocou štandardu _PKCS#11_.
 
Ukážkový program využíva knižnice [PKCS#11 interop](https://pkcs11interop.net/) na interakciu z _PKCS#11_ knižnicou, 
[iTextSharp](https://www.nuget.org/packages/itext7/) pre manipuláciu s PDF dokumentom
a [CommandLine](https://github.com/commandlineparser/commandline) pre parsovanie CLI parametrov.

Vytvorenie "surového" RSA alebo EC podpisu je ľahké, ťažšie je to robiť dobre (ochrana PIN-ov, certifikácia softvéru pre podpis od NBU, splnenie špecifikácie pre konkrétne druhy podpisov).
Takto podpísané PDF-ko je v Adobre Readery zelené.

![Signed PDF](images/Programing/SkEidSign/SignedDocument.png){.img-center}

No podpis nie je platným _eIDAS PAdES_ podpisom, chýbajú mu rôzne atribúty, odkaz na podpisovú politiku,...

## Zdroje
 1. [PKCS#11 Interop](https://pkcs11interop.net/)
 1. [PKCS#11 X509Store](https://github.com/Pkcs11Interop/Pkcs11Interop.X509Store/blob/master/src/Pkcs11Interop.X509Store/Pkcs11X509Certificate.cs)
 1. [Signing a PDF File Using Azure Key Vault](https://rahulpnath.com/blog/signing-a-pdf-file-using-azure-key-vault/)
 1. [Slovensko.sk](https://www.slovensko.sk/sk/na-stiahnutie)
