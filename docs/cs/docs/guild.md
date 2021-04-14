# Správa discord serverů

Správa discord serverů je množina metod, která umožňuje jednodušší správu serveru.

## Příkazy

*Všechny níže uvedené příkazy počítají s výchozím prefixem `$`. Prefix se může lišit v závislosti na vaší konfiguraci.*

### $guild info

Získá kompletní informace o serveru, na kterém byl zavolán příkaz.
Mezi těmito informacemi se vrací:

- Počet kategorií
- Počet textových kanálů
- Počet hlasových kanálů
- Počet rolí
- Počet emotů (běžných/animovaných)
- Počet banů
- Datum a čas vytvoření serveru
- Vlastník serveru
- Stav synchronizace serveru (Ano/Ne)
- Počet uživatelů (z toho kolik je v cache)
- Úroveň vylepšení (Tier)
- Počet boosterů
- Extra funkce
- Počty uživatelů podle stavu (Online, Idle, DoNotDisturb, Offline)
- Limity serveru
    - Maximální počet uživatelů
    - Maximální počet online uživatelů
    - Maximální počet uživatelů v hlasovém kanálu s webkamerou
    - Maximální bitrate

#### Příklad volání

```sh
$guild info
```

### $guild sync

Vyvolá synchronizaci serveru s daty v paměti.

#### Příklad volání

```sh
$guild sync
```

### $guild calcPerms `{verbose}` `{channel}`

Spočítá počet oprávnění na serveru.

#### Parametry

| Název   |  Datový typ  | Popis                                                        |
| ------- | :----------: | ------------------------------------------------------------ |
| verbose |     bool     | Volitelný parametr.Výpis počtu oprávnění po kanálech.        |
| channel | GuildChannel | Volitelný parametr. Výpočet oprávnění pouze v jednom kanálu. |

#### Příklady volání

```sh
$guild calcPerms
$guild calcPerms false
$guild calcPerms true channel_tag
```

### $guild clearPerms `{onlyMod}` `{guildChannel}`

Smaže veškerá uživatelská oprávnění na serveru.

#### Parametry

| Název        |  Datový typ  | Popis                                                                |
| ------------ | :----------: | -------------------------------------------------------------------- |
| onlyMod      |     bool     | Smaže oprávnění pouze uživatelů, kteří mají oprávnění Administrator. |
| guildChannel | GuildChannel | Kanál, ve kterém se mají práva odstranit.                            |

#### Příklad volání

```sh
$guild clearPerms false channel_tag
```

### $guild clearReact `{channel}` `{messageId}` `{react}`

Smaže všechny reakce na daném emote u zprávy.

#### Parametry

| Název     | Datový typ | Popis                                             |
| --------- | :--------: | ------------------------------------------------- |
| channel   |  Channel   | Označení kanálu, ve kterém se zpráva nachází.     |
| messageId |   ulong    | Identifikátor zprávy, na které se reakce nachází. |
| react     |   string   | Název emotu s reakcí.                             |

#### Příklad volání

```sh
$guild clearReact #channel_tag 831937053221453894 emote
```

### $guild clearUselessPerms `{hard}` `{mentionedUser}`

Vyhodnocení uživatelských oprávnění, zda mají nějaký efektivní smysl.

#### Parametry

| Název         | Datový typ | Popis                                                                                                                                                        |
| ------------- | :--------: | ------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| hard          |    bool    | Příznak, který povolí mazání zbytečných oprávnění.                                                                                                           |
| mentionedUser |    User    | Určení uživatele, pro kterého se mají zbytečná oprávnění vyhodnotit. Volitelný parametr. Pokud není zadán, pak se vyhodnocení provádí pro všechny uživatele. |

#### Příklady volání

```sh
$guild clearUselessPerms
$guild clearUselessPerms true
$guild clearUselessPerms true @GrillBot
```

### $guild createEmoteList

Vygeneruje CSV soubor se seznamem emotů na serveru.

#### Příklady volání

```sh
$guild createEmoteList
```

### $guild backupEmotes

**!!! (DEPRECATED) !!!**

Vygeneruje archiv se všemi emoty.

#### Příklady volání

```sh
$guild backupEmotes
```
