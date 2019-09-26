Published: 26.2.2019
Title: SoftHSMv2 (as nuget) for testing
Menu: SoftHSMv2 (as nuget) for testing
---
# SoftHSMv2 (as nuget) for testing
_SoftHSMv2ForTesting_ nuget balík, ktorý zabaľuje [SoftHSMv2](https://github.com/opendnssec/SoftHSMv2)
spolu s minimálnym kódom pre inicializáciu a zmazanie _SoftHSMv2_.
Automaticky inicializuje jeden token, no je možné ich inicializovať viac.

Je určený pre unit testovanie .Net (Core) projektov využívajúcich _PKCS#11_ zariadenia (napríklad čipové karty, eID, HSM,...) v _CI/CD_ prostredí.

Vytvorený [Nuget balíček](https://www.nuget.org/packages/SoftHSMv2ForTesting/) je určený pre _Windows_.

Ukážky použitia a príklady so zdrojovými kódmi sa nachádzajú na [Github stránke projektu](https://github.com/harrison314/SoftHSMv2ForTesting).
