Published: 16.4.2016
Title: Monády
Menu: Monády
Cathegory: Dev
Description: Stručný popis monád (nachádzajúcich sa napríklad v Haskelli).
---
# Monády
Monáda je trojica _(M, return, then)_ pozostávajúca z typového konštruktora _M_ a dvojice polymorfných funkcií.

<blockquote>
    <i>return</i> :: a &rarr; M a<br />
    <i>then</i> :: M a &rarr; (a &rarr; M b) &rarr; M b
</blockquote>

Tieto funkcie musia spĺňať nasledujúce pravidlá:

<blockquote>
    <i>then</i> (<i>return</i> a) k = k a<br />
    <i>then</i> m <i>return</i> = m<br />
    <i>then</i> (&lambda; a &sdot; <i>then</i> (k a) (&lambda; b &sdot; k b ))
    = <i>then</i> (<i>then</i> m (&lambda; a &sdot; k a))(&lambda; b &sdot; k b)<br />
</blockquote>
