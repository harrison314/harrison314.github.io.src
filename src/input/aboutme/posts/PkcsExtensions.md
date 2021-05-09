Published: 20.5.2020
Title: PkcsExtensions
Menu: PkcsExtensions
Description: 'Malá knižnica pridávajúca chýbajúce veci do .Net-u v oblasti PKCS a PKI.'
---
_PkcsExtensions_ je malá knižnica pridávajúca extensions metódy a typy, ktoré chýbajú v dotnete (_.net core 3.1_) v oblasti PKCS.

Táto knižnica vznikla pre to, lebo som sa často stretával s opakujúcimi sa úlohami. Napríklad bezpečný prevod _SecureString_ na pole bajtov, generátor náhodných čísiel, ktorý dokáže zbierať entropiu, zistenie, či je certifikát možné použiť pri podpisovaní dokumentu,...

Plno z tohto rieši _BauncyCastle_, ale to je okolo dva megabajty veľká knižnica,
ktorá sa nedá dobre linkovať ([IL Linking](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/blazor/configure-linker?view=aspnetcore-3.1)), lebo takmer každá trieda súvisí s každou.
Preto som sa inšpiroval knižnicu _SecurityDriven.Inferno_, ktorá pridáva len tenkú vrstvu a rozšírenia nad štandardné _dotnet_ API.

Mojim cieľom bolo vytvoriť malú knižnicu, vhodnú pre [AOT kompiláciu](https://github.com/dotnet/designs/blob/master/accepted/2020/single-file/design.md), IL Linking a teda aby šla dobre použiť v malých programoch a Blazore.

_PkcsExtensions_ poskytuje funkcionalitu pre:
- export RSA, ECDsa, certifikátu do BER/DER formátu,
- hex konvertor,
- generátor náhodných čísiel,
- rozšírenia pre _HashAlgorithm_ aby bol použiteľnejší,
- KDF algoritmus _SP800-108_ na deriváciu nových kľúčov,
- ASN1 writer a reader z _corefx_,
- pomocné triedy pre vytváranie _SignedCms_,
- rozšírenia pre _X509Certificate2_ na zisťovanie možnosti použitia certifikátu a parsovanie subjectu a issuera,
- ...

Vytvorený [Nuget balíček](https://www.nuget.org/packages/PkcsExtensions/).

Ukážky použitia a príklady so zdrojovými kódmi sa nachádzajú na [Github stránke projektu](https://github.com/harrison314/PkcsExtensions).

## PkcsExtensions.Blazor
_PkcsExtensions.Blazor_ je knižnica pre Blazor WebAssembly, poskytujúca interop pre [WebCrypto](https://developer.mozilla.org/en-US/docs/Web/API/Web_Crypto_API) (kryptografické primitíva vo webovom prehliadači) a ďalšie pomocné triedy a extension metódy pre PKCS v prostredí browsera (_PkcsExtensions_ je jej závislosť). 

Táto knižnica umožňuje:
- generovať náhodné pole bajtov pomocou _WebCrypto_,
- generovať RSA kľúče pomocou _WebCrypto_,
- generovať  EC kľúče pomocou _WebCrypto_,
- Diffie–Hellmanovu výmenu kľúčov a ECIES pomocou _WebCrypto_,
- exportovať RSA kľúče,
- podporu JsonWebKey,
- ...

Táto knižnica zámerne nerobí interop na _WebCrypto_ pre hash funkcie, HMAC, šifrovanie a podpisovanie, pretože ich implementácie a podpora sa môže líšiť medzi prehliadačmi a operačnými systémami, navyše WebCrypto neumožňuje podpísať hash. Preto v súčasnosti považujem za lepšiu alternatívu použiť _dotnet/Blazor_ implementáciu týchto funcionalít.

Vytvorený [Nuget balíček](https://www.nuget.org/packages/PkcsExtensions.Blazor/).

Ukážky použitia a príklady so zdrojovými kódmi sa nachádzajú na [Github stránke projektu](https://github.com/harrison314/PkcsExtensions.Blazor).
